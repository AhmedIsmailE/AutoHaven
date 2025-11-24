using AutoHaven.IRepository;
using AutoHaven.Models;
using AutoHaven.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoHaven.Controllers
{
    public class CarListingController : Controller
    {
        private readonly ICarListingModelRepository _carListingRepo;
        private readonly ICarModelRepository _carRepo;
        private readonly IUserSubscriptionModelRepository _userSubscriptionRepo;
        private readonly IReviewModelRepository _reviewRepo;
        private readonly IFavouriteModelRepository _favouriteRepo;

        public CarListingController(
            ICarListingModelRepository carListingRepo,
            ICarModelRepository carRepo,
            IUserSubscriptionModelRepository userSubscriptionRepo,
            IReviewModelRepository reviewRepo,
            IFavouriteModelRepository favouriteRepo)
        {
            _carListingRepo = carListingRepo;
            _carRepo = carRepo;
            _userSubscriptionRepo = userSubscriptionRepo;
            _reviewRepo = reviewRepo;
            _favouriteRepo = favouriteRepo;
        }

        // ==================== GET: Browse All Listings ====================
        [HttpGet]
        public IActionResult Index(string searchTerm = "", int listingType = -1, string sortBy = "newest")
        {
            try
            {
                var listings = _carListingRepo.Get();

                // Filter by listing type
                if (listingType >= 0)
                {
                    listings = listings
                        .Where(cl => (int)cl.Type == listingType)
                        .ToList();
                }

                // Search by manufacturer, model, or description
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    listings = listings
                        .Where(cl =>
                            cl.Car.Manufacturer.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            cl.Car.Model.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            (cl.Description != null && cl.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                // Sort
                listings = sortBy switch
                {
                    "price_asc" => listings
                        .OrderBy(cl => cl.Type == CarListingModel.ListingType.ForSelling ? cl.NewPrice : cl.RentPrice)
                        .ToList(),
                    "price_desc" => listings
                        .OrderByDescending(cl => cl.Type == CarListingModel.ListingType.ForSelling ? cl.NewPrice : cl.RentPrice)
                        .ToList(),
                    "oldest" => listings.OrderBy(cl => cl.CreatedAt).ToList(),
                    _ => listings.OrderByDescending(cl => cl.CreatedAt).ToList()
                };

                ViewBag.SearchTerm = searchTerm;
                ViewBag.ListingType = listingType;
                ViewBag.SortBy = sortBy;

                return View(listings);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading listings: {ex.Message}";
                return View(new List<CarListingModel>());
            }
        }

        // ==================== GET: Listing Details ====================
        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound("Listing ID is required.");

            try
            {
                var listing = _carListingRepo.GetById(id.Value);
                if (listing == null)
                    return NotFound("Listing not found.");

                // Get reviews for this listing
                var reviews = _reviewRepo.GetByListingId(id.Value);
                ViewBag.Reviews = reviews;
                ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                ViewBag.ReviewCount = reviews.Count();

                // Check if user has favorited this listing
                int userId = GetCurrentUserId();
                if (userId > 0)
                {
                    var isFavorited = _favouriteRepo.Get()
                        .Any(f => f.UserId == userId && f.ListingId == id.Value);
                    ViewBag.IsFavorited = isFavorited;
                }

                return View(listing);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading listing: {ex.Message}";
                return NotFound();
            }
        }

        // ==================== GET: Create Listing Form ====================
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCarListingViewModel());
        }

        // ==================== POST: Create Listing ====================
        //[Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateCarListingViewModel viewModel, IEnumerable<IFormFile> imageFiles)
        {
            // Validate ViewModel
            if (!viewModel.IsValid())
            {
                ModelState.AddModelError("", "Please enter a valid price for the selected listing type.");
                return View(viewModel);
            }

            if (!ModelState.IsValid)
                return View(viewModel);

            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized();

                // Check subscription limits
                var userSubscription = _userSubscriptionRepo.GetActiveForUser(userId);
                var currentListingCount = _carListingRepo.Get()
                    .Count(cl => cl.UserId == userId);

                if (userSubscription != null && currentListingCount >= userSubscription.SubscriptionPlan.MaxCarListing)
                {
                    ModelState.AddModelError("", "You've reached your listing limit. Please upgrade your subscription.");
                    return View(viewModel);
                }

                // Create Car
                var car = new CarModel
                {
                    Manufacturer = viewModel.Manufacturer,
                    Model = viewModel.Model,
                    ModelYear = viewModel.ModelYear,
                    BodyStyle = viewModel.BodyStyle ?? string.Empty,
                    CurrentTransmission = viewModel.Transmission,
                    CurrentFuel = viewModel.Fuel,
                    Power = viewModel.Power,
                    Doors = viewModel.Doors
                };

                _carRepo.Insert(car);

                // Create Listing
                var listing = new CarListingModel
                {
                    CarId = car.CarId,
                    UserId = userId,
                    Type = viewModel.ListingType,
                    NewPrice = viewModel.NewPrice ?? 0,
                    RentPrice = viewModel.RentPrice ?? 0,
                    Description = viewModel.Description ?? string.Empty,
                    Color = viewModel.Color ?? string.Empty,
                    CurrentState = CarListingModel.State.Available,
                    IsFeatured = false,
                    Discount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Insert listing with images
                var validImages = imageFiles?
                .Where(f => f != null && f.Length > 0)
                .Take(7)
                .ToList() ?? new List<IFormFile>();

                _carListingRepo.Insert(listing, validImages);


                TempData["Success"] = "Listing created successfully!";
                return RedirectToAction(nameof(Details), new { id = listing.ListingId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating listing: {ex.Message}");
                return View(viewModel);
            }
        }

        // ==================== GET: Edit Listing ====================
        [Authorize]
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound("Listing ID is required.");

            try
            {
                var listing = _carListingRepo.GetById(id.Value);
                if (listing == null)
                    return NotFound("Listing not found.");

                int userId = GetCurrentUserId();
                if (listing.UserId != userId)
                    return Forbid("You don't have permission to edit this listing.");

                var viewModel = new CreateCarListingViewModel
                {
                    Manufacturer = listing.Car.Manufacturer,
                    Model = listing.Car.Model,
                    ModelYear = listing.Car.ModelYear,
                    BodyStyle = listing.Car.BodyStyle,
                    Color = listing.Color,
                    Transmission = listing.Car.CurrentTransmission,
                    Fuel = listing.Car.CurrentFuel,
                    Power = listing.Car.Power,
                    Doors = listing.Car.Doors,
                    ListingType = listing.Type,
                    NewPrice = listing.NewPrice,
                    RentPrice = listing.RentPrice,
                    Description = listing.Description
                };

                ViewBag.ListingId = id;
                ViewBag.CurrentImages = listing.CarImages;
                return View("Create", viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading listing: {ex.Message}";
                return NotFound();
            }
        }

        // ==================== POST: Update Listing ====================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, CreateCarListingViewModel viewModel, IEnumerable<IFormFile> newImages, int[] imageIdsToKeep)
        {
            if (!viewModel.IsValid())
            {
                ModelState.AddModelError("", "Please enter a valid price for the selected listing type.");
                return View("Create", viewModel);
            }

            if (!ModelState.IsValid)
                return View("Create", viewModel);

            try
            {
                var listing = _carListingRepo.GetById(id);
                if (listing == null)
                    return NotFound("Listing not found.");

                int userId = GetCurrentUserId();
                if (listing.UserId != userId)
                    return Forbid("You don't have permission to edit this listing.");

                // Update car info
                listing.Car.Manufacturer = viewModel.Manufacturer;
                listing.Car.Model = viewModel.Model;
                listing.Car.ModelYear = viewModel.ModelYear;
                listing.Car.BodyStyle = viewModel.BodyStyle ?? string.Empty;
                listing.Car.CurrentTransmission = viewModel.Transmission;
                listing.Car.CurrentFuel = viewModel.Fuel;
                listing.Car.Power = viewModel.Power;
                listing.Car.Doors = viewModel.Doors;

                // Update listing info
                listing.Type = viewModel.ListingType;
                listing.NewPrice = viewModel.NewPrice ?? 0;
                listing.RentPrice = viewModel.RentPrice ?? 0;
                listing.Description = viewModel.Description ?? string.Empty;
                listing.Color = viewModel.Color ?? string.Empty;
                listing.UpdatedAt = DateTime.UtcNow;

                // Update listing and images
                var filesToUpload = newImages?.Where(f => f.Length > 0).ToList();
                _carListingRepo.Update(listing, imageIdsToKeep ?? new int[0], filesToUpload);

                TempData["Success"] = "Listing updated successfully!";
                return RedirectToAction(nameof(Details), new { id = listing.ListingId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating listing: {ex.Message}");
                return View("Create", viewModel);
            }
        }

        // ==================== POST: Delete Listing ====================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var listing = _carListingRepo.GetById(id);
                if (listing == null)
                    return NotFound("Listing not found.");

                int userId = GetCurrentUserId();
                if (listing.UserId != userId)
                    return Forbid("You don't have permission to delete this listing.");

                _carListingRepo.Delete(id);

                TempData["Success"] = "Listing deleted successfully!";
                return RedirectToAction(nameof(MyListings));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting listing: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // ==================== GET: User's Own Listings ====================
        [Authorize]
        [HttpGet]
        public IActionResult MyListings()
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized();

                var listings = _carListingRepo.Get()
                    .Where(cl => cl.UserId == userId)
                    .OrderByDescending(cl => cl.CreatedAt)
                    .ToList();

                return View(listings);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading your listings: {ex.Message}";
                return View(new List<CarListingModel>());
            }
        }

        // ==================== POST: Add to Favorites ====================
        [Authorize]
        [HttpPost]
        public IActionResult AddToFavorite(int listingId)
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized();

                var listing = _carListingRepo.GetById(listingId);
                if (listing == null)
                    return NotFound("Listing not found.");

                var favorite = new FavouriteModel
                {
                    UserId = userId,
                    ListingId = listingId,
                    CreatedAt = DateTime.UtcNow
                };

                _favouriteRepo.Insert(favorite);

                return Ok(new { message = "Added to favorites", success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
        }

        // ==================== POST: Remove from Favorites ====================
        [Authorize]
        [HttpPost]
        public IActionResult RemoveFromFavorite(int listingId)
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized();

                _favouriteRepo.DeleteByUserAndListing(userId, listingId);

                return Ok(new { message = "Removed from favorites", success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
        }

        // ==================== GET: User's Favorites ====================
        [Authorize]
        [HttpGet]
        public IActionResult Favorites()
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized();

                var favorites = _favouriteRepo.Get()
                    .Where(f => f.UserId == userId)
                    .Select(f => f.CarListing)
                    .OrderByDescending(cl => cl.UpdatedAt)
                    .ToList();

                return View(favorites);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading favorites: {ex.Message}";
                return View(new List<CarListingModel>());
            }
        }

        // ==================== POST: Add Review ====================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(int listingId, int rating, string comment)
        {
            try
            {
                if (rating < 1 || rating > 5)
                    return BadRequest("Rating must be between 1 and 5.");

                if (string.IsNullOrWhiteSpace(comment))
                    return BadRequest("Comment is required.");

                int userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized();

                var listing = _carListingRepo.GetById(listingId);
                if (listing == null)
                    return NotFound("Listing not found.");

                var review = new ReviewModel
                {
                    UserId = userId,
                    ListingId = listingId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _reviewRepo.Insert(review);

                TempData["Success"] = "Review added successfully!";
                return RedirectToAction(nameof(Details), new { id = listingId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding review: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = listingId });
            }
        }

        // ==================== POST: Delete Review ====================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteReview(int reviewId, int listingId)
        {
            try
            {
                var review = _reviewRepo.GetById(reviewId);
                if (review == null)
                    return NotFound("Review not found.");

                int userId = GetCurrentUserId();
                if (review.UserId != userId)
                    return Forbid("You don't have permission to delete this review.");

                _reviewRepo.Delete(reviewId);

                TempData["Success"] = "Review deleted successfully!";
                return RedirectToAction(nameof(Details), new { id = listingId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting review: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = listingId });
            }
        }

        // ==================== GET: Filter by Type ====================
        [HttpGet]
        public IActionResult FilterByType(int listingType)
        {
            return RedirectToAction(nameof(Index), new { listingType, sortBy = "newest" });
        }

        // ==================== GET: Provider Listings ====================
        [HttpGet]
        public IActionResult ProviderListings(int userId)
        {
            try
            {
                var listings = _carListingRepo.Get()
                    .Where(cl => cl.UserId == userId)
                    .OrderByDescending(cl => cl.CreatedAt)
                    .ToList();

                ViewBag.ProviderId = userId;
                ViewBag.ProviderName = listings.FirstOrDefault()?.User?.Name ?? "Provider";

                return View("Index", listings);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading provider listings: {ex.Message}";
                return View("Index", new List<CarListingModel>());
            }
        }

        // ==================== HELPER: Get Current User ID ====================
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(nameIdentifier) && int.TryParse(nameIdentifier, out int id))
                return id;

            return 0;
        }
    }
}