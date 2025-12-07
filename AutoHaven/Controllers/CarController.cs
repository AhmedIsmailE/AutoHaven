using AutoHaven.IRepository;
using AutoHaven.Models;
using AutoHaven.Services;
using AutoHaven.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUserModel> _userManager;

        public CarController (
            ICarListingModelRepository carListingRepo,
            ICarModelRepository carRepo,
            IUserSubscriptionModelRepository userSubscriptionRepo,
            IReviewModelRepository reviewRepo,
            IFavouriteModelRepository favouriteRepo,
            ICarViewHistoryRepository historyRepo,
            UserManager<ApplicationUserModel> userManager)
        {
            _carListingRepo = carListingRepo;
            _carRepo = carRepo;
            _userSubscriptionRepo = userSubscriptionRepo;
            _reviewRepo = reviewRepo;
            _favouriteRepo = favouriteRepo;
            _historyRepo = historyRepo;
            _userManager = userManager;
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
        [Authorize]
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

                // 🎯 GET USER INFO
                int userId = GetCurrentUserId();
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                // 🎯 CHECK IF USER ALREADY VIEWED THIS LISTING (ever)
                bool hasViewedBefore = _historyRepo.HasViewedBefore(
                    listingId: id.Value,
                    userId: userId > 0 ? userId : null,
                    ipAddress: ipAddress
                );

                // ✅ ONLY INCREMENT IF NEVER VIEWED BEFORE
                if (!hasViewedBefore)
                {
                    System.Diagnostics.Debug.WriteLine($"📈 Incrementing view for listing {id.Value}");
                    _carListingRepo.IncrementViews(id.Value);

                    // ✅ INSERT VIEW RECORD
                    var history = new CarViewHistoryModel
                    {
                        ListingId = id.Value,
                        UserId = userId > 0 ? userId : null,
                        ViewedAt = DateTime.UtcNow,
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        IpAddress = ipAddress
                    };

                    try
                    {
                        _historyRepo.Insert(history);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Error inserting view history: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 User already viewed listing {id.Value} - no increment");
                }

                // Get reviews for this listing
                var reviews = _reviewRepo.GetByListingId(id.Value);
                ViewBag.Reviews = reviews;
                ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                ViewBag.ReviewCount = reviews.Count();

                // ✅ CHECK IF CURRENT USER IS OWNER
                 userId = GetCurrentUserId();
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
        [Authorize(Policy = "AdminOrProvider")]
        [HttpGet]
        public IActionResult Create()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Admins bypass subscription check
            if (!CurrentUserIsAdmin())
            {
                var subscription = _userSubscriptionRepo.GetActiveForUser(userId);
                if (subscription == null)
                {
                    TempData["Notification.Message"] = "You need an active subscription to create listings.";
                    TempData["Notification.Type"] = "error";
                    return RedirectToAction("Index", "Subscription");
                }
            }

            return View(new CreateCarListingViewModel());
        }

        // ==================== POST: Create Listing ====================
        [Authorize(Policy = "AdminOrProvider")]
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
                UserSubscriptionModel? subscription = null;

                if (!CurrentUserIsCustomer())
                {
                    subscription = _userSubscriptionRepo.GetActiveForUser(userId);
                    if (subscription == null)
                    {
                        TempData["Notification.Message"] = "You need an active subscription to list cars. Please purchase a plan.";
                        TempData["Notification.Type"] = "error";
                        return RedirectToAction("Index", "Subscription");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"✨ DEBUG: User {userId} wants to feature: {viewModel.WantsFeatured}");
                System.Diagnostics.Debug.WriteLine($"📋 Subscription: {subscription.SubscriptionPlan.SubscriptionName}, Featured slots: {subscription.SubscriptionPlan.FeatureSlots}");

                // ✅ CHECK CAR LISTING COUNT
                var carCount = _carListingRepo.Get()
                    .Count(cl => cl.UserId == userId);

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
                if (!validImages.Any())
                {
                    ModelState.AddModelError("", "At least one photo is required to publish a listing.");
                    return View(viewModel);
                }
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
        [Authorize(Policy = "AdminOrProvider")]
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
                    Description = listing.Description,
                    WantsFeatured = listing.IsFeatured,
                    CurrentState = listing.CurrentState
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
        [Authorize(Policy = "AdminOrProvider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Car/Update/{id}")]  // ✅ ADD EXPLICIT ROUTE
        public IActionResult Update(int id, CreateCarListingViewModel viewModel,
                IEnumerable<IFormFile> imageFiles, string imageIdsToKeep)
        {
            System.Diagnostics.Debug.WriteLine($"🔄 UPDATE called for listing ID: {id}");
            System.Diagnostics.Debug.WriteLine($"📊 CurrentState from form: {viewModel.CurrentState}");


            if (string.IsNullOrEmpty(imageIdsToKeep))
            {
                ModelState.Remove("imageIdsToKeep");
            }
            if (!viewModel.IsValid())
            {
                ModelState.AddModelError("", "Please enter a valid price for the selected listing type.");
                ViewBag.ListingId = id;
                ViewBag.CurrentImages = _carListingRepo.GetById(id)?.CarImages ?? new List<CarImageModel>();
                return View("Create", viewModel);
            }

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("❌ ModelState is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"   Error: {error.ErrorMessage}");
                }

                ViewBag.ListingId = id;
                ViewBag.CurrentImages = _carListingRepo.GetById(id)?.CarImages ?? new List<CarImageModel>();
                return View("Create", viewModel);
            }

            try
            {
                var listing = _carListingRepo.GetById(id);
                if (listing == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Listing {id} not found");
                    return NotFound("Listing not found.");
                }

                int userId = GetCurrentUserId();

                // Allow admin to edit any listing, providers can only edit their own
                if (!CurrentUserIsAdmin() && listing.UserId != userId)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ User {userId} not authorized to edit listing {id}");
                    return Forbid("You don't have permission to edit this listing.");
                }

                // ✅ Parse imageIdsToKeep from comma-separated string to int[]
                int[] imageIdsArray = new int[0];

                if (!string.IsNullOrEmpty(imageIdsToKeep))
                {
                    try
                    {
                        imageIdsArray = imageIdsToKeep
                            .Split(',')
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Select(int.Parse)
                            .ToArray();
                    }
                    catch
                    {
                        imageIdsArray = new int[0];
                    }
                }

                System.Diagnostics.Debug.WriteLine($"📋 Image IDs to keep: {(imageIdsArray.Length > 0 ? string.Join(", ", imageIdsArray) : "NONE")}");

                // ✅ Update car info
                listing.Car.Manufacturer = viewModel.Manufacturer;
                listing.Car.Model = viewModel.Model;
                listing.Car.ModelYear = viewModel.ModelYear;
                listing.Car.BodyStyle = viewModel.BodyStyle ?? string.Empty;
                listing.Car.CurrentTransmission = viewModel.Transmission;
                listing.Car.CurrentFuel = viewModel.Fuel;
                listing.Car.Power = viewModel.Power;
                listing.Car.Doors = viewModel.Doors;

                // ✅ Update listing info
                listing.Type = viewModel.ListingType;
                listing.NewPrice = viewModel.NewPrice ?? 0;
                listing.RentPrice = viewModel.RentPrice ?? 0;
                listing.Description = viewModel.Description ?? string.Empty;
                listing.Color = viewModel.Color ?? string.Empty;
                listing.UpdatedAt = DateTime.Now;
                listing.IsFeatured = viewModel.WantsFeatured;
                listing.CurrentState = viewModel.CurrentState;  // ✅ UPDATE STATUS

                System.Diagnostics.Debug.WriteLine($"💾 Updating listing: Featured={listing.IsFeatured}, State={listing.CurrentState}");

                // Handle images
                var newImages = imageFiles?
                    .Where(f => f != null && f.Length > 0)
                    .Take(7)
                    .ToList() ?? new List<IFormFile>();

                int keptOldCount = listing.CarImages?
                    .Count(ci => imageIdsArray.Contains(ci.CarImageId)) ?? 0;

                bool hasNewImages = newImages.Any();

                // RULE: Must keep at least 1 image or upload new ones
                if (!hasNewImages && keptOldCount == 0)
                {
                    ModelState.AddModelError("", "You must keep at least one existing photo or upload a new one.");
                    ViewBag.ListingId = id;
                    ViewBag.CurrentImages = listing.CarImages;
                    return View("Create", viewModel);
                }
                if (!hasNewImages && keptOldCount == 0)
                {
                    ModelState.AddModelError("", "You must keep at least one existing photo or upload a new one.");
                    ViewBag.ListingId = id;
                    ViewBag.CurrentImages = listing.CarImages;
                    return View("Create", viewModel);
                }

                // ✅ CALL REPOSITORY UPDATE
                _carListingRepo.Update(listing, imageIdsArray, newImages);

                System.Diagnostics.Debug.WriteLine($"✅ Listing {id} updated successfully!");

                TempData["Notification.Message"] = "Listing updated successfully!";
                TempData["Notification.Type"] = "success";
                return RedirectToAction(nameof(Details), new { id = listing.ListingId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ UPDATE ERROR: " + ex.ToString());
                ModelState.AddModelError("", $"Error updating listing: {ex.Message}");

                ViewBag.ListingId = id;
                ViewBag.CurrentImages = _carListingRepo.GetById(id)?.CarImages ?? new List<CarImageModel>();
                return View("Create", viewModel);
            }
        }
        // ====================  Feature/Unfeature Feature ====================
        [Authorize(Policy = "AdminOrProvider")]
        //[Authorize(Policy = "ProviderOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFeatured(int listingId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var listing = _carListingRepo.GetById(listingId);

                if (listing == null)
                    return NotFound(new { success = false, message = "Listing not found" });

                if (listing.UserId != userId)
                    return StatusCode(403, new { success = false, message = "Not authorized" });

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

                // ✅ KEEP ALL EXISTING IMAGES - Extract their IDs
                int[] existingImageIds = listing.CarImages
                    ?.Select(img => img.CarImageId)
                    .ToArray() ?? new int[0];

                System.Diagnostics.Debug.WriteLine($"📋 Keeping {existingImageIds.Length} images when toggling featured");

                // Update without modifying images
                _carListingRepo.Update(listing, existingImageIds, null);

                return Ok(new
                {
                    success = true,
                    message = listing.IsFeatured ? "Listed as featured! 🌟" : "Removed from featured.",
                    isFeatured = listing.IsFeatured
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ToggleFeatured Error: {ex.ToString()}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ==================== POST: Delete Listing ====================
        [Authorize(Policy = "AdminOrProvider")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                // ✅ Check authentication FIRST
                int userId = GetCurrentUserId();
                if (userId == 0)
                {
                    TempData["Notification.Message"] = "You must be logged in to delete listings.";
                    TempData["Notification.Type"] = "error";
                    return RedirectToAction(nameof(Index), "Car");
                }

                var listing = _carListingRepo.GetById(id);
                if (listing == null)
                {
                    TempData["Notification.Message"] = "Listing not found.";
                    TempData["Notification.Type"] = "error";
                    return RedirectToAction(nameof(Index), "Car");
                }

                // ✅ Check ownership
                if (listing.UserId != userId)
                {
                    TempData["Notification.Message"] = "You don't have permission to delete this listing.";
                    TempData["Notification.Type"] = "error";
                    return RedirectToAction(nameof(Index), "Car");
                }

                // ✅ Delete the listing
                _carListingRepo.Delete(id);

                System.Diagnostics.Debug.WriteLine($"✅ Listing {id} deleted by user {userId}");

                TempData["Notification.Message"] = "Listing deleted successfully!";
                TempData["Notification.Type"] = "success";
                return RedirectToAction(nameof(Index), "Car");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Delete Error: {ex.ToString()}");

                TempData["Notification.Message"] = $"Error deleting listing: {ex.Message}";
                TempData["Notification.Type"] = "error";
                return RedirectToAction(nameof(Index), "Car");
            }
        }  

        //================ Favourites Methods For Details ==================
        // GET: /Car/IsFavorite/{id}
        [HttpGet]
        public async Task<IActionResult> IsFavorite(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { isFavorite = false });

            // Using repo (synchronous Any is fine here)
            bool exists = _favouriteRepo.Get().Any(f => f.UserId == user.Id && f.ListingId == id);

            return Json(new { isFavorite = exists });
        }

        // POST: /Car/AddToFavorite/{id}  <- toggle / add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToFavorite(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { isFavorite = false, notification = new { message = "Login Required", type = "error" } });

            // Check existing favourite using repo
            var existing = _favouriteRepo.Get().FirstOrDefault(f => f.UserId == user.Id && f.ListingId == id);

            if (existing != null)
            {
                // Remove (toggle off)
                _favouriteRepo.Delete(existing.FavouriteId);
                return Json(new { isFavorite = false });
            }

            // Add (toggle on)
            var fav = new FavouriteModel
            {
                UserId = user.Id,
                ListingId = id
            };

            _favouriteRepo.Insert(fav);

            return Json(new { isFavorite = true, notification = new { message = "Added To Favorite List.", type = "success" } });
        }

        // Optional explicit Remove endpoint (if you prefer separate endpoints)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFavorite(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var fav = _favouriteRepo.Get().FirstOrDefault(f => f.UserId == user.Id && f.ListingId == id);
            if (fav != null)
            {
                _favouriteRepo.Delete(fav.FavouriteId);
            }

            return Ok();
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
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _reviewRepo.Insert(review);

                TempData["Notification.Message"] = "Review added successfully!";
                TempData["Notification.Type"] = "success";
                return RedirectToAction(nameof(Details), new { id = listingId });
            }
            catch (Exception ex)
            {
                TempData["Notification.Message"] = $"Error adding review: {ex.Message}";
                TempData["Notification.Type"] = "error";
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
        private bool CurrentUserIsAdmin()
        {
            var roleClaim = User.FindFirst("Role")?.Value;
            return roleClaim == ApplicationUserModel.RoleEnum.Admin.ToString();
        }
        private bool CurrentUserIsCustomer()
        {
            var roleClaim = User.FindFirst("Role")?.Value;
            return roleClaim == ApplicationUserModel.RoleEnum.Customer.ToString();
        }


    }
}