using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoHaven.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICarListingModelRepository _carListingRepo;
        private readonly IUserSubscriptionModelRepository _userSubRepo;

        public HomeController(
            ICarListingModelRepository carListingRepo,
            IUserSubscriptionModelRepository userSubRepo)
        {
            _carListingRepo = carListingRepo;
            _userSubRepo = userSubRepo;
        }
        public IActionResult AccessDenied()
        {
            TempData["Notification.Message"] = "Access Denied: You do not have permission to access this resource.";
            TempData["Notification.Type"] = "error";

            return RedirectToAction("Home");
        }
        public IActionResult Home()
        {
            try
            {
                // 1️⃣ GET ALL LISTINGS
                var allListings = _carListingRepo.Get();
                System.Diagnostics.Debug.WriteLine($"📋 Total listings loaded: {allListings.Count}");

                // 2️⃣ FILTER FEATURED CARS
                var featuredListings = allListings
                    .Where(cl => cl.IsFeatured == true)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"⭐ Featured cars marked: {featuredListings.Count}");

                // 3️⃣ VALIDATE SUBSCRIPTION - ONLY SHOW IF ACTIVE
                var validFeaturedCars = new List<CarListingModel>();

                foreach (var car in featuredListings)
                {
                    // Get provider's active subscription
                    var activeSubscription = _userSubRepo.GetActiveForUser(car.UserId);

                    if (activeSubscription != null && activeSubscription.EndDate > DateTime.Now)
                    {
                        // ✅ VALID: Provider has active subscription
                        validFeaturedCars.Add(car);
                        System.Diagnostics.Debug.WriteLine(
                            $"✅ Car {car.ListingId} shown - Provider {car.UserId} subscription valid until {activeSubscription.EndDate:yyyy-MM-dd}");
                    }
                    else
                    {
                        // ❌ EXPIRED: Hide from public view
                        System.Diagnostics.Debug.WriteLine(
                            $"❌ Car {car.ListingId} HIDDEN - Provider {car.UserId} subscription expired");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"🎯 Final featured cars to display: {validFeaturedCars.Count}");

                return View("~/Views/Shared/Home.cshtml", validFeaturedCars);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in HomeController.Index: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                return View(new List<CarListingModel>());
            }
        }
    }
}
