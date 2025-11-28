using AutoHaven.IRepository;
using AutoHaven.Models;
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

        public CarController (
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
        public IActionResult Index(string searchTerm = "", int? minPrice = null, int? maxPrice = null,
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
                    ViewBag.Makes = new List<string>();
                    ViewBag.Years = new List<int>();
                    ViewBag.Transmissions = Enum.GetValues(typeof(CarModel.Transmission)).Cast<CarModel.Transmission>().ToList();
                    ViewBag.Fuels = Enum.GetValues(typeof(CarModel.FuelType)).Cast<CarModel.FuelType>().ToList();

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
                var makes = allListings.Where(cl => cl.Car != null && !string.IsNullOrEmpty(cl.Car.Manufacturer))
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
                ViewBag.Makes = makes ?? new List<string>();
                ViewBag.Years = years ?? new List<int>();
                ViewBag.Transmissions = transmissions ?? new List<CarModel.Transmission>();
                ViewBag.Fuels = fuels ?? new List<CarModel.FuelType>();

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
                ViewBag.Makes = new List<string>();
                ViewBag.Years = new List<int>();
                ViewBag.Transmissions = Enum.GetValues(typeof(CarModel.Transmission)).Cast<CarModel.Transmission>().ToList();
                ViewBag.Fuels = Enum.GetValues(typeof(CarModel.FuelType)).Cast<CarModel.FuelType>().ToList();

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
                // ================= For History Part =================
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
                // 🔍 SHOW THE REAL ERROR
                string errorMessage = ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;

                ModelState.AddModelError("", $"Error creating listing: {errorMessage}");

                // Also log it
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

             /*   // Check subscription limits
                var userSubscription = _userSubscriptionRepo.GetActiveForUser(userId);
                var currentListingCount = _carListingRepo.Get()
                    .Count(cl => cl.UserId == userId);

                if (userSubscription != null && currentListingCount >= userSubscription.MaxCarListing)
                {
                    ModelState.AddModelError("", "You've reached your listing limit. Please upgrade your subscription.");
                    return View(viewModel);
                }*/

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
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Insert listing with images
                var validImages = imageFiles?
                .Where(f => f != null && f.Length > 0)
                .Take(7)
                .ToList() ?? new List<IFormFile>();

                _carListingRepo.Insert(listing, validImages);


                TempData["Notification.Message"] = "List Got Created!";
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
                listing.UpdatedAt = DateTime.Now;

                // Update listing and images
                var filesToUpload = newImages?.Where(f => f.Length > 0).ToList();
                _carListingRepo.Update(listing, imageIdsToKeep ?? new int[0], filesToUpload);

                TempData["Notification.Message"] = "List Updated Successfully!";
                TempData["Notification.Type"] = "success";
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

                TempData["Notification.Message"] = "Listing deleted successfully!";
                TempData["Notification.Type"] = "success";
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