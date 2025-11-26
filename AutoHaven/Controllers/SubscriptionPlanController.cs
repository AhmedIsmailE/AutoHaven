using Microsoft.AspNetCore.Mvc;

namespace AutoHaven.Controllers
{
    public class SubscriptionPlanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
