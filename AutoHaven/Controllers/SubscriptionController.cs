using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json.Nodes;

namespace AutoHaven.Controllers
{
    
    public class SubscriptionController : Controller
    {
        private readonly UserManager<ApplicationUserModel> _userManager;
        private readonly ISubscriptionPlanModelRepository _subscriptionPlanRepo;
        private readonly IUserSubscriptionModelRepository _userSubscriptionRepo;

        public SubscriptionController(
            UserManager<ApplicationUserModel> userManager,
            ISubscriptionPlanModelRepository subscriptionPlanRepo,
            IUserSubscriptionModelRepository userSubscriptionRepo,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _subscriptionPlanRepo = subscriptionPlanRepo;
            _userSubscriptionRepo = userSubscriptionRepo;
            PaypalClientId = configuration["PayPalOptions:ClientId"]!;
            PaypalSecret = configuration["PayPalOptions:ClientSecret"]!;
            PaypalUrl = configuration["PayPalOptions:Url"]!;
        }
        private string PaypalClientId { get; set; } = "";
        private string PaypalSecret { get; set; } = "";
        private string PaypalUrl { get; set; } = "";
        //[Authorize]
        [Authorize(Policy = "AdminOrProvider")]
        private async Task<string> GetPaypalAccessToken()
        {

            string accessToken = "";
            string url = PaypalUrl + "/v1/oauth2/token";
            using (var client = new HttpClient())
            {
                string credentials64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(PaypalClientId + ":" + PaypalSecret));

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials64);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                requestMessage.Content = new StringContent("grant_type=client_credentials", null, "application/x-www-form-urlencoded");

                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse != null)
                    {
                        accessToken = jsonResponse["access_token"]?.ToString() ?? "";
                    }
                }
            }
            return accessToken;
        }

        //[Authorize]
        [Authorize(Policy = "AdminOrProvider")]
        [HttpPost]
        public async Task<JsonResult> CreateOrder([FromBody] JsonObject data)
        {
            var totalAmount = data?["amount"]?.ToString();
            if (totalAmount == null)
            {
                return new JsonResult(new { Id = "" });
            }

            // create the request body
            JsonObject createOrderRequest = new JsonObject();
            createOrderRequest.Add("intent", "CAPTURE");

            JsonObject amount = new JsonObject();
            amount.Add("currency_code", "USD");
            amount.Add("value", totalAmount);

            JsonObject purchaseUnit1 = new JsonObject();
            purchaseUnit1.Add("amount", amount);

            JsonArray purchaseUnits = new JsonArray();
            purchaseUnits.Add(purchaseUnit1);

            createOrderRequest.Add("purchase_units", purchaseUnits);


            string accessToken = await GetPaypalAccessToken();
            // send request
            string url = PaypalUrl + "/v2/checkout/orders";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent(createOrderRequest.ToString(), Encoding.UTF8, "application/json");
                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync(); var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse != null)
                    {
                        string paypalOrderId = jsonResponse["id"]?.ToString() ?? "";
                        return new JsonResult(new { Id = paypalOrderId });
                    }
                }

            }
            return new JsonResult(new { Id = "" });
        }

        //[Authorize]
        [Authorize(Policy = "AdminOrProvider")]
        [HttpPost]
        public async Task<JsonResult> CompleteOrder([FromBody] JsonObject data)
        {
            // 1. Get Data from Frontend
            var orderId = data?["orderID"]?.ToString();
            var planIdStr = data?["planId"]?.ToString();

            if (orderId == null || planIdStr == null)
            {
                return new JsonResult("error");
            }

            // 2. Capture Payment via PayPal
            string accessToken = await GetPaypalAccessToken();
            string url = PaypalUrl + "/v2/checkout/orders/" + orderId + "/capture";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("", Encoding.UTF8, "application/json");

                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);

                    if (jsonResponse != null)
                    {
                        string paypalOrderStatus = jsonResponse["status"]?.ToString() ?? "";

                        // 3. IF PAYMENT SUCCESSFUL -> SAVE TO DB IMMEDIATELY
                        if (paypalOrderStatus == "COMPLETED")
                        {
                            var userIdString = _userManager.GetUserId(User);

                            if (int.TryParse(userIdString, out int userId) && int.TryParse(planIdStr, out int planId))
                            {
                                // Save to Database here
                                _userSubscriptionRepo.Create(userId, planId);
                                TempData["Purchased"] = true;
                                return new JsonResult("success");
                            }
                        }
                    }
                }
            }

            return new JsonResult("error");
        }

        [Authorize]
        public IActionResult PurchaseSuccess()
        {
            bool allowed = TempData.Peek("Purchased") as bool? == true;

            if (allowed)
            {
                TempData["Purchased"] = false;
                return View("PurchaseView");
            }

            return RedirectToAction("Home", "Home");
        }
        [Authorize(Policy = "AdminOrProvider")]
        [HttpGet]
        public IActionResult Index()
        {
            var subscriptions = _subscriptionPlanRepo.Get().Skip(1).ToList();
            return View("IndexView",subscriptions);
        }
        [Authorize(Policy = "AdminOrProvider")]
        [HttpGet]
        public IActionResult Payment(int planId)
        {
            var plan = _subscriptionPlanRepo.GetById(planId);
            if (plan == null || plan.SubscriptionPlanId == 1)
                return RedirectToAction("Index");
            ViewBag.PaypalClientId = PaypalClientId;
            return View("PaymentView",plan);
        }
        }
}
