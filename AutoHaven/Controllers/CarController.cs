using AutoHaven.IRepository;
using AutoHaven.Models;
using AutoHaven.Services;
using AutoHaven.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static AutoHaven.Models.CarListingModel;

namespace AutoHaven.Controllers
{
    public class CarController : Controller
    {
        private readonly ICarListingModelRepository _carListingRepo;
        private readonly ICarModelRepository _carRepo;
        private readonly IUserSubscriptionModelRepository _userSubscriptionRepo;
        private readonly IReviewModelRepository _reviewRepo;
        private readonly IFavouriteModelRepository _favouriteRepo;
        private readonly ICarViewHistoryRepository _historyRepo;

        public CarController(
            ICarListingModelRepository carListingRepo,
            ICarModelRepository carRepo,
            IUserSubscriptionModelRepository userSubscriptionRepo,
            IReviewModelRepository reviewRepo,
            IFavouriteModelRepository favouriteRepo,
            ICarViewHistoryRepository historyRepo)
        {
            _carListingRepo = carListingRepo;
            _carRepo = carRepo;
            _userSubscriptionRepo = userSubscriptionRepo;
            _reviewRepo = reviewRepo;
            _favouriteRepo = favouriteRepo;
            _historyRepo = historyRepo;
        }

        // ==================== GET: Browse All Listings ====================
        [HttpGet]
        public IActionResult Index(string searchTerm = "", string[] makes = null, int? minPrice = null, int? maxPrice = null,
            int? selectedYear = null, int? transmission = null, int? fuel = null,
            int listingType = 0, string sortBy = "newest", int page = 1)
        {
            try
            {
                const int pageSize = 12;

                // Get all listings first
                var allListings = _carListingRepo.Get().ToList();

                // If no listings exist, return empty with defaults
                if (!allListings.Any())
                {
                    ViewBag.SearchTerm = searchTerm;
                    ViewBag.MinPrice = minPrice;
                    ViewBag.MaxPrice = maxPrice;
                    ViewBag.SelectedYear = selectedYear;
                    ViewBag.Transmission = transmission;
                    ViewBag.Fuel = fuel;
                    ViewBag.ListingType = listingType;
                    ViewBag.SortBy = sortBy;
                    ViewBag.CurrentPage = page;
                    ViewBag.TotalPages = 1;
                    ViewBag.TotalCount = 0;
                    ViewBag.AvailableMakes = new List<string>();
                    ViewBag.SelectedMakes = makes ?? new string[] { };
                    ViewBag.Years = new List<int>();
                    ViewBag.Transmissions = Enum.GetValues(typeof(CarModel.Transmission)).Cast<CarModel.Transmission>().ToList();
                    ViewBag.Fuels = Enum.GetValues(typeof(CarModel.FuelType)).Cast<CarModel.FuelType>().ToList();
                    ViewBag.Reviews = new List<ReviewModel>();

                    return View(new List<CarListingModel>());
                }

                var query = allListings.AsQueryable();

                // Search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(cl =>
                        cl.Car.Manufacturer.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        cl.Car.Model.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (cl.Description != null && cl.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).AsQueryable();
                }

                // Make filter
                if (makes != null && makes.Length > 0)
                {
                    var makesSet = new HashSet<string>(makes, StringComparer.OrdinalIgnoreCase);
                    query = query.Where(cl =>
                        cl.Car != null && makesSet.Contains(cl.Car.Manufacturer)
                    ).AsQueryable();
                }

                // Listing Type filter
                if (listingType == 1)
                    query = query.Where(cl => cl.Type == CarListingModel.ListingType.ForSelling).AsQueryable();
                else if (listingType == 2)
                    query = query.Where(cl => cl.Type == CarListingModel.ListingType.ForRenting).AsQueryable();

                // Price filter
                if (minPrice.HasValue)
                {
                    query = query.Where(cl =>
                        (cl.Type == CarListingModel.ListingType.ForSelling && cl.NewPrice >= minPrice) ||
                        (cl.Type == CarListingModel.ListingType.ForRenting && cl.RentPrice >= minPrice)
                    ).AsQueryable();
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(cl =>
                        (cl.Type == CarListingModel.ListingType.ForSelling && cl.NewPrice <= maxPrice) ||
                        (cl.Type == CarListingModel.ListingType.ForRenting && cl.RentPrice <= maxPrice)
                    ).AsQueryable();
                }

                // Year filter
                if (selectedYear.HasValue)
                    query = query.Where(cl => cl.Car.ModelYear == selectedYear).AsQueryable();

                // Transmission filter
                if (transmission.HasValue)
                    query = query.Where(cl => (int)cl.Car.CurrentTransmission == transmission).AsQueryable();

                // Fuel type filter
                if (fuel.HasValue)
                    query = query.Where(cl => (int)cl.Car.CurrentFuel == fuel).AsQueryable();

                // Sorting
                var listings = sortBy switch
                {
                    "price_asc" => query.OrderBy(cl =>
                        cl.Type == CarListingModel.ListingType.ForSelling ? cl.NewPrice : cl.RentPrice).ToList(),
                    "price_desc" => query.OrderByDescending(cl =>
                        cl.Type == CarListingModel.ListingType.ForSelling ? cl.NewPrice : cl.RentPrice).ToList(),
                    "highest_rated" => query.OrderByDescending(cl =>
                        _reviewRepo.Get().Where(r => r.ListingId == cl.ListingId).Average(r => (double?)r.Rating) ?? 0).ToList(),
                    "most_viewed" => query.OrderByDescending(cl => cl.Views).ToList(),
                    _ => query.OrderByDescending(cl => cl.CreatedAt).ToList()
                };

                // Pagination
                int totalCount = listings.Count();
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var paginatedListings = listings.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Get filter options for dropdowns
                var availableMakes = allListings.Where(cl => cl.Car != null && !string.IsNullOrEmpty(cl.Car.Manufacturer))
                    .Select(cl => cl.Car.Manufacturer)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                var years = allListings.Where(cl => cl.Car != null)
                    .Select(cl => cl.Car.ModelYear)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList();

                var transmissions = Enum.GetValues(typeof(CarModel.Transmission))
                    .Cast<CarModel.Transmission>()
                    .ToList();

                var fuels = Enum.GetValues(typeof(CarModel.FuelType))
                    .Cast<CarModel.FuelType>()
                    .ToList();

                // Get all reviews for ratings
                var allReviews = _reviewRepo.Get() ?? new List<ReviewModel>();

                // ViewBag assignments
                ViewBag.SearchTerm = searchTerm;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.SelectedYear = selectedYear;
                ViewBag.Transmission = transmission;
                ViewBag.Fuel = fuel;
                ViewBag.ListingType = listingType;
                ViewBag.SortBy = sortBy;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;
                ViewBag.AvailableMakes = availableMakes ?? new List<string>();
                ViewBag.SelectedMakes = makes ?? new string[] { };
                ViewBag.Years = years ?? new List<int>();
                ViewBag.Transmissions = transmissions ?? new List<CarModel.Transmission>();
                ViewBag.Fuels = fuels ?? new List<CarModel.FuelType>();
                ViewBag.Reviews = allReviews;

                return View(paginatedListings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("INDEX ERROR: " + ex.ToString());
                ViewBag.Error = $"Error loading listings: {ex.Message}";
                ViewBag.SearchTerm = searchTerm;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.SelectedYear = selectedYear;
                ViewBag.Transmission = transmission;
                ViewBag.Fuel = fuel;
                ViewBag.ListingType = listingType;
                ViewBag.SortBy = sortBy;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.AvailableMakes = new List<string>();
                ViewBag.SelectedMakes = new string[] { };
                ViewBag.Years = new List<int>();
                ViewBag.Transmissions = Enum.GetValues(typeof(CarModel.Transmission)).Cast<CarModel.Transmission>().ToList();
                ViewBag.Fuels = Enum.GetValues(typeof(CarModel.FuelType)).Cast<CarModel.FuelType>().ToList();
                ViewBag.Reviews = new List<ReviewModel>();

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

                // ✅ INCREMENT VIEW COUNT
                _carListingRepo.IncrementViews(id.Value);

                // ✅ HISTORY TRACKING
                try
                {
                    int uid = GetCurrentUserId();

                    var history = new CarViewHistoryModel
                    {
                        ListingId = id.Value,
                        UserId = uid > 0 ? uid : null,
                        ViewedAt = DateTime.UtcNow,
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                    };

                    if (uid > 0)
                    {
                        var old = _historyRepo
                            .GetByUserId(uid)
                            .FirstOrDefault(h => h.ListingId == id.Value);

                        if (old != null)
                        {
                            _historyRepo.Delete(old.Id);
                        }
                    }

                    _historyRepo.Insert(history);
                }
                catch { }

                // Get reviews for this listing
                var reviews = _reviewRepo.GetByListingId(id.Value);
                ViewBag.Reviews = reviews;
                ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                ViewBag.ReviewCount = reviews.Count();

                // ✅ CHECK IF CURRENT USER IS OWNER
                int userId = GetCurrentUserId();
                bool isOwner = (userId > 0 && listing.UserId == userId);
                ViewBag.IsOwner = isOwner;
                ViewBag.CurrentUserId = userId;

                // Check if user has favorited this listing
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
                string errorMessage = ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;

                ModelState.AddModelError("", $"Error loading listing: {errorMessage}");
                System.Diagnostics.Debug.WriteLine("FULL ERROR: " + ex.ToString());

                return View("Error");
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
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateCarListingViewModel viewModel, IEnumerable<IFormFile> imageFiles)
        {
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

                // ✅ VALIDATE SUBSCRIPTION
                var subscription = _userSubscriptionRepo.GetActiveForUser(userId);

                if (subscription == null)
                {
                    ModelState.AddModelError("", "❌ You need an active subscription to list cars. Please purchase a plan.");
                    return View(viewModel);
                }

                System.Diagnostics.Debug.WriteLine($"✨ DEBUG: User {userId} wants to feature: {viewModel.WantsFeatured}");
                System.Diagnostics.Debug.WriteLine($"📋 Subscription: {subscription.SubscriptionPlan.SubscriptionName}, Featured slots: {subscription.SubscriptionPlan.FeatureSlots}");

                // ✅ CHECK CAR LISTING COUNT
                var carCount = _carListingRepo.Get()
                    .Count(cl => cl.UserId == userId &&
                                 cl.CurrentState != CarListingModel.State.Sold &&
                                 cl.CurrentState != CarListingModel.State.Rented);

                if (carCount >= subscription.SubscriptionPlan.MaxCarListing)
                {
                    ModelState.AddModelError("",
                        $"❌ You've reached your car listing limit ({carCount}/{subscription.SubscriptionPlan.MaxCarListing})");
                    return View(viewModel);
                }

                // ✅ CHECK FEATURED SLOTS (if user wants to feature)
                if (viewModel.WantsFeatured)
                {
                    var featuredCount = _carListingRepo.Get()
                        .Count(cl => cl.UserId == userId && cl.IsFeatured == true);

                    if (featuredCount >= subscription.SubscriptionPlan.FeatureSlots)
                    {
                        ModelState.AddModelError("",
                            $"❌ You've used all featured slots ({featuredCount}/{subscription.SubscriptionPlan.FeatureSlots})");
                        return View(viewModel);
                    }
                }

                // ✅ CREATE CAR
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

                // ✅ CREATE LISTING WITH FEATURED OPTION
                var listing = new CarListingModel
                {
                    CarId = car.CarId,
                    UserId = userId,
                    Type = viewModel.ListingType,
                    NewPrice = viewModel.NewPrice ?? 0,
                    RentPrice = viewModel.RentPrice ?? 0,
                    Description = viewModel.Description ?? string.Empty,
                    Color = viewModel.Color ?? string.Empty,
                    CurrentState = CarListingModel.State.Available, // ✅ MUST BE AVAILABLE
                    IsFeatured = viewModel.WantsFeatured, // ✅ SET FROM CHECKBOX
                    Discount = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                System.Diagnostics.Debug.WriteLine($"💾 Saving listing: Featured={listing.IsFeatured}, State={listing.CurrentState}");

                // Insert listing with images
                var validImages = imageFiles?
                    .Where(f => f != null && f.Length > 0)
                    .Take(7)
                    .ToList() ?? new List<IFormFile>();

                _carListingRepo.Insert(listing, validImages);

                System.Diagnostics.Debug.WriteLine($"✅ Listing saved with ID {listing.ListingId}, Featured={listing.IsFeatured}");

                string message = viewModel.WantsFeatured
                    ? "Listing created successfully and featured! 🌟"
                    : "Listing created successfully!";

                TempData["Notification.Message"] = message;
                TempData["Notification.Type"] = "success";
                return RedirectToAction(nameof(Details), new { id = listing.ListingId });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;

                System.Diagnostics.Debug.WriteLine("FULL ERROR: " + ex.ToString());

                ModelState.AddModelError("", $"Error creating listing: {errorMessage}");
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
        public IActionResult Update(int id, CreateCarListingViewModel viewModel, IEnumerable<IFormFile> imageFiles, int[] imageIdsToKeep)
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
                listing.UpdatedAt = DateTime.Now;

                // Handle images
                var newImages = imageFiles?.Where(f => f != null && f.Length > 0).ToList();
                _carListingRepo.Update(listing, imageIdsToKeep ?? new int[0], newImages);

                TempData["Notification.Message"] = "Listing updated successfully!";
                TempData["Notification.Type"] = "success";
                return RedirectToAction(nameof(Details), new { id = listing.ListingId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UPDATE ERROR: " + ex.ToString());
                ModelState.AddModelError("", $"Error updating listing: {ex.Message}");
                return View("Create", viewModel);
            }
        }
        // ====================  Feature/Unfeature Feature ====================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFeatured(int listingId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var listing = _carListingRepo.GetById(listingId);

                if (listing == null)
                    return NotFound();

                if (listing.UserId != userId)
                    return Forbid();

                // If trying to FEATURE
                if (!listing.IsFeatured)
                {
                    var validationService = new CarListingValidationService(_userSubscriptionRepo, _carListingRepo);
                    var (isValid, message) = validationService.ValidateCarCreation(userId, wantsFeatured: true);

                    if (!isValid)
                    {
                        return BadRequest(new { success = false, message });
                    }
                }

                // Toggle featured
                listing.IsFeatured = !listing.IsFeatured;
                listing.UpdatedAt = DateTime.Now;
                _carListingRepo.Update(listing, new int[0], null);

                return Ok(new
                {
                    success = true,
                    message = listing.IsFeatured ? "Listed as featured!" : "Removed from featured.",
                    isFeatured = listing.IsFeatured
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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

                TempData["Notification.Message"] = "Listing deleted successfully!";
                TempData["Notification.Type"] = "success";
                return RedirectToAction(nameof(MyListings));
            }
            catch (Exception ex)
            {
                TempData["Notification.Message"] = $"Error deleting listing: {ex.Message}";
                TempData["Notification.Type"] = "error";
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
                {
                    TempData["Notification.Message"] = "Listing isn't existed";
                    TempData["Notification.Type"] = "error";
                    return RedirectToAction("MyHistory");
                }

                var already = _favouriteRepo.Get()
                     .Any(f => f.UserId == userId && f.ListingId == listingId);
                if (already)
                {
                    TempData["Notification.Message"] = "Already in your favourites.";
                    TempData["Notification.Type"] = "info";
                    return RedirectToAction("MyHistory");
                }

                var favorite = new FavouriteModel
                {
                    UserId = userId,
                    ListingId = listingId,
                    CreatedAt = DateTime.Now
                };

                _favouriteRepo.Insert(favorite);

                TempData["Notification.Message"] = "Added To Favourites!";
                TempData["Notification.Type"] = "success";
                return RedirectToAction("MyHistory");
            }
            catch (Exception ex)
            {
                TempData["Notification.Message"] = "Error: " + ex.Message;
                TempData["Notification.Type"] = "error";
                return RedirectToAction("MyHistory");
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
        // ================== History Actions =======================
        [Authorize]
        [HttpGet]
        public IActionResult MyHistory(string q = "", string sortBy = "newest", int page = 1)
        {
            int uid = GetCurrentUserId();
            if (uid == 0) return Unauthorized();

            const int pageSize = 4;               // <-- max 4 posts per page
            if (page < 1) page = 1;

            // Get all history rows for user
            var userHistQuery = _historyRepo.Get()
                .Where(h => h.UserId == uid)
                .AsQueryable();

            // Optional search (manufacturer, model, description)
            if (!string.IsNullOrWhiteSpace(q))
            {
                var qTrim = q.Trim();
                userHistQuery = userHistQuery.Where(h =>
                    (h.CarListing != null && h.CarListing.Car != null &&
                        ((h.CarListing.Car.Manufacturer ?? "").Contains(qTrim, StringComparison.OrdinalIgnoreCase) ||
                         (h.CarListing.Car.Model ?? "").Contains(qTrim, StringComparison.OrdinalIgnoreCase)))
                    || ((h.CarListing != null ? h.CarListing.Description : "") ?? "").Contains(qTrim, StringComparison.OrdinalIgnoreCase)
                );
            }

            // Group by listing and take latest view per listing
            var latestPerListing = userHistQuery
                .GroupBy(h => h.ListingId)
                .Select(g => g.OrderByDescending(x => x.ViewedAt).FirstOrDefault());

            // Materialize to list so we can sort by computed values (ratings, views, price)
            var latestList = latestPerListing
                .Where(x => x != null)
                .Select(x => x!) // non-null
                .ToList();

            // Apply sorting
            IEnumerable<CarViewHistoryModel> ordered = latestList;

            switch ((sortBy ?? "newest").ToLowerInvariant())
            {
                case "price_asc":
                    ordered = latestList.OrderBy(h =>
                        h.CarListing != null && h.CarListing.Type == CarListingModel.ListingType.ForSelling
                            ? h.CarListing.NewPrice
                            : h.CarListing != null ? h.CarListing.RentPrice : decimal.MaxValue);
                    break;

                case "price_desc":
                    ordered = latestList.OrderByDescending(h =>
                        h.CarListing != null && h.CarListing.Type == CarListingModel.ListingType.ForSelling
                            ? h.CarListing.NewPrice
                            : h.CarListing != null ? h.CarListing.RentPrice : 0m);
                    break;

                case "highest_rated":
                    ordered = latestList.OrderByDescending(h =>
                        _reviewRepo.Get().Where(r => r.ListingId == h.ListingId).Average(r => (double?)r.Rating) ?? 0);
                    break;

                case "most_viewed":
                    ordered = latestList.OrderByDescending(h => h.CarListing?.Views ?? 0);
                    break;

                case "newest":
                default:
                    // newest = most recently viewed
                    ordered = latestList.OrderByDescending(h => h.ViewedAt);
                    break;
            }

            // Pagination
            var totalCount = ordered.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Fill ViewBag for view (the view expects these)
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.SortBy = sortBy ?? "newest";
            ViewBag.SearchTerm = q ?? string.Empty;

            // Optional filter lists (used by your view)
            var allListings = _carListingRepo.Get().ToList();
            ViewBag.Makes = allListings.Where(cl => cl.Car != null && !string.IsNullOrEmpty(cl.Car.Manufacturer))
                                      .Select(cl => cl.Car.Manufacturer).Distinct().OrderBy(x => x).ToList();
            ViewBag.Years = allListings.Where(cl => cl.Car != null).Select(cl => cl.Car.ModelYear).Distinct().OrderByDescending(y => y).ToList();
            ViewBag.Transmissions = Enum.GetValues(typeof(CarModel.Transmission)).Cast<CarModel.Transmission>().ToList();
            ViewBag.Fuels = Enum.GetValues(typeof(CarModel.FuelType)).Cast<CarModel.FuelType>().ToList();

            return View("MyHistory", items);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearHistory()
        {
            int uid = GetCurrentUserId();
            if (uid == 0) return Unauthorized();

            _historyRepo.DeleteByUser(uid);

            TempData["Notification.Message"] = "History cleared.";
            TempData["Notification.Type"] = "success";
            return RedirectToAction(nameof(MyHistory));
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromHistory(int id)
        {
            int uid = GetCurrentUserId();
            if (uid == 0) return Unauthorized();

            var row = _historyRepo.Get().FirstOrDefault(h => h.Id == id && h.UserId == uid);
            if (row != null)
            {
                _historyRepo.Delete(id);
                TempData["Notification.Message"] = "Removed from history.";
                TempData["Notification.Type"] = "success";
            }
            else
            {
                TempData["Notification.Message"] = "History entry not found.";
                TempData["Notification.Type"] = "error";
            }

            return RedirectToAction(nameof(MyHistory));
        }


    }
}