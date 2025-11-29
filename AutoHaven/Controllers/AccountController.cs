using AutoHaven.IRepository;
using AutoHaven.Models;
using AutoHaven.Repository;
using AutoHaven.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace AutoHaven.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly ProjectDbContext _projectDbContext;
        private readonly IFavouriteModelRepository _favouriteRepo;
        private readonly ICarListingModelRepository _carListingRepo;
        private readonly IReviewModelRepository _reviewRepo;
        private readonly ICarViewHistoryRepository _historyRepo;
        private const long MaxFileBytes = 2 * 1024 * 1024;
        private const int MaxWidth = 1024;
        private const int MaxHeight = 1024;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ProjectDbContext projectDbContext,
            IWebHostEnvironment env,
            IFavouriteModelRepository favouriteRepo,
            ICarListingModelRepository carListingRepo,
            IReviewModelRepository reviewRepo,
            ICarViewHistoryRepository historyRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _projectDbContext = projectDbContext;
            _env = env;
            _favouriteRepo = favouriteRepo;
            _carListingRepo = carListingRepo;
            _reviewRepo = reviewRepo;
            _historyRepo = historyRepo;
        }

        // ==================== GET: Register ====================
        [HttpGet]
        public IActionResult Register() => View();

        // ==================== POST: Register ====================
        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserViewModel userViewModel)
        {
            if (!ModelState.IsValid) return View(userViewModel);

            var applicationUser = new ApplicationUser
            {
                UserName = userViewModel.UserName,
                Email = userViewModel.Email,
                PhoneNumber = userViewModel.PhoneNumber,
                Role = userViewModel.Role,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            IdentityResult result = await _userManager.CreateAsync(applicationUser, userViewModel.Password);
            if (result.Succeeded) return RedirectToAction("Login");

            foreach (IdentityError error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(userViewModel);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginUserViewModel loginUserViewModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid Credentials");
                return View(loginUserViewModel);
            }

            // Search user by Email or Phone
            ApplicationUser user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == loginUserViewModel.EmailOrPhone
                                       || u.PhoneNumber == loginUserViewModel.EmailOrPhone);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginUserViewModel.Password))
            {
                user.UpdatedAt = DateTime.Now;
                await _userManager.UpdateAsync(user);

                var claims = new List<Claim> { new Claim("Role", user.Role.ToString()) };
                await _signInManager.SignInWithClaimsAsync(user, isPersistent: loginUserViewModel.RememberMe, claims);

                return RedirectToAction("Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid Credentials");
            return View(loginUserViewModel);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile(string? edit)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = ProfileViewModel.MapToModel(user);
            if (edit == "1") ViewData["EditMode"] = true;
            return View("Profile", model);
        }

        [HttpGet]
        public IActionResult Edit() => RedirectToAction(nameof(Profile), new { edit = 1 });

        // ==================== POST: Edit (errors converted to TempData notifications) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model, IFormFile? avatar)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewData["EditMode"] = true;
            if (!ModelState.IsValid)
            {
                TempData["Notification.Message"] = "Unable to determine user id.";
                TempData["Notification.Type"] = "error";
                return View("Profile", model);
            }

            user.Name = model.Name;
            user.CompanyName = model.CompanyName;
            user.Street = model.Street;
            user.City = model.City;
            user.State = model.State;
            user.UpdatedAt = DateTime.Now;
            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmail = await _userManager.SetEmailAsync(user, model.Email ?? string.Empty);
                if (!setEmail.Succeeded)
                {
                    TempData["Notification.Message"] = "Unable to determine user id.";
                    TempData["Notification.Type"] = "error";
                    return View("Profile", model);
                }
            }

            if (!string.Equals(user.PhoneNumber, model.PhoneNumber, StringComparison.OrdinalIgnoreCase))
            {
                var setPhone = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhone.Succeeded)
                {
                    TempData["Notification.Message"] = "Unable to determine user id.";
                    TempData["Notification.Type"] = "error";
                    return View("Profile", model);
                }
            }

            // ✅ HANDLE AVATAR UPLOAD
            if (avatar != null && avatar.Length > 0)
            {
                var allowed = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif" };
                if (!allowed.Contains(avatar.ContentType.ToLower()))
                {
                    TempData["Notification.Message"] = "Unable to determine user id.";
                    TempData["Notification.Type"] = "error";
                    return View("Profile", model);
                }

                if (avatar.Length > MaxFileBytes)
                {
                    TempData["Notification.Message"] = "Unable to determine user id.";
                    TempData["Notification.Type"] = "error";
                    return View("Profile", model);
                }

                try
                {
                    using var mem = new MemoryStream();
                    await avatar.CopyToAsync(mem);
                    mem.Position = 0;

                    try
                    {
                        using var img = System.Drawing.Image.FromStream(mem);
                        if (img.Width > MaxWidth || img.Height > MaxHeight)
                        {
                            TempData["Notification.Message"] = "Unable to determine user id.";
                            TempData["Notification.Type"] = "error";
                            return View("Profile", model);
                        }
                    }
                    catch { }

                    mem.Position = 0;

                    var usernameSafe = string.IsNullOrWhiteSpace(user.UserName)
                        ? $"user_{user.Id}"
                        : string.Concat(user.UserName.Split(Path.GetInvalidFileNameChars()));

                    var webRoot = _env.WebRootPath;
                    var userPfpDir = Path.Combine(webRoot, "images", usernameSafe, "PFP");
                    Directory.CreateDirectory(userPfpDir);

                    var ext = Path.GetExtension(avatar.FileName);
                    if (string.IsNullOrWhiteSpace(ext)) ext = ".png";

                    var newFileName = $"p{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(userPfpDir, newFileName);

                    using var fs = new FileStream(filePath, FileMode.Create);
                    mem.CopyTo(fs);

                    var prev = user.AvatarUrl;
                    if (!string.IsNullOrWhiteSpace(prev) &&
                        prev.StartsWith($"/images/{usernameSafe}/PFP/"))
                    {
                        var prevPath = Path.Combine(webRoot, prev.TrimStart('/'));
                        if (System.IO.File.Exists(prevPath)) System.IO.File.Delete(prevPath);
                    }

                    user.AvatarUrl = $"/images/{usernameSafe}/PFP/{newFileName}";
                }
                catch
                {
                    TempData["Notification.Message"] = "Unable to determine user id.";
                    TempData["Notification.Type"] = "error";
                    return View("Profile", model);
                }
            }

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                TempData["Notification.Message"] = "Unable to determine user id.";
                TempData["Notification.Type"] = "error";
                return View("Profile", model);
            }

            TempData["Notification.Message"] = "Profile updated successfully!";
            TempData["Notification.Type"] = "success";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            int uid = GetCurrentUserId();
            if (uid == 0)
            {
                TempData["Notification.Message"] = "Unable to determine user id.";
                TempData["Notification.Type"] = "error";
                return RedirectToAction(nameof(Profile));
            }

            using var tx = await _projectDbContext.Database.BeginTransactionAsync();
            try
            {
                // 1) Remove favourites
                try
                {
                    var favs = _favouriteRepo.Get().Where(f => f.UserId == uid).ToList();
                    foreach (var f in favs) _favouriteRepo.Delete(f.FavouriteId);
                }
                catch { /* ignore individual errors - best effort */ }

                // 2) Remove view history
                try { _historyRepo.DeleteByUser(uid); } catch { }

                // 3) Remove reviews by this user
                try
                {
                    var reviews = _reviewRepo.Get().Where(r => r.UserId == uid).ToList();
                    foreach (var r in reviews) _reviewRepo.Delete(r.ReviewId);
                }
                catch { }

                // 4) Remove listings owned by user (and any repo-handled children)
                try
                {
                    var listings = _carListingRepo.Get().Where(c => c.UserId == uid).ToList();
                    foreach (var l in listings) _carListingRepo.Delete(l.ListingId);
                }
                catch { }

                // 5) Persist deletes (works if repos share _projectDbContext)
                await _projectDbContext.SaveChangesAsync();

                // 6) Delete avatar files if they exist and are inside user's PFP folder
                try
                {
                    var usernameSafe = string.IsNullOrWhiteSpace(user.UserName)
                        ? $"user_{user.Id}"
                        : string.Concat(user.UserName.Split(System.IO.Path.GetInvalidFileNameChars()));

                    var webRoot = _env.WebRootPath;
                    if (!string.IsNullOrWhiteSpace(user.AvatarUrl) &&
                        user.AvatarUrl.StartsWith($"/images/{usernameSafe}/PFP/", StringComparison.OrdinalIgnoreCase))
                    {
                        var prevPath = Path.Combine(webRoot, user.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(prevPath))
                        {
                            System.IO.File.Delete(prevPath);
                        }

                        // try to remove the PFP directory if empty
                        var pfpDir = Path.Combine(webRoot, "images", usernameSafe, "PFP");
                        if (Directory.Exists(pfpDir) && !Directory.EnumerateFileSystemEntries(pfpDir).Any())
                        {
                            Directory.Delete(pfpDir);
                        }
                    }
                }
                catch { /* ignore cleanup errors */ }

                // 7) Delete Identity user
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    await tx.RollbackAsync();
                    var err = string.Join("; ", deleteResult.Errors.Select(e => e.Description));
                    TempData["Notification.Message"] = "Failed to delete account: " + err;
                    TempData["Notification.Type"] = "error";
                    return RedirectToAction(nameof(Profile));
                }

                await tx.CommitAsync();

                // Sign out and redirect
                await _signInManager.SignOutAsync();
                TempData["Notification.Message"] = "Your account has been removed.";
                TempData["Notification.Type"] = "success";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                try { await tx.RollbackAsync(); } catch { }
                TempData["Notification.Message"] = "Error deleting account: " + ex.Message;
                TempData["Notification.Type"] = "error";
                return RedirectToAction(nameof(Profile));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAvatar()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var webRoot = _env.WebRootPath;

            try
            {
                var usernameSafe = string.Concat(user.UserName.Split(Path.GetInvalidFileNameChars()));

                if (!string.IsNullOrWhiteSpace(user.AvatarUrl) &&
                    user.AvatarUrl.StartsWith($"/images/{usernameSafe}/PFP/"))
                {
                    var prevPath = Path.Combine(webRoot, user.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(prevPath)) System.IO.File.Delete(prevPath);
                }

                user.AvatarUrl = ProfileViewModel.DevFallbackLocalPath;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }
            catch { }

            TempData["Notification.Message"] = "Avatar reset!";
            TempData["Notification.Type"] = "success";
            return RedirectToAction(nameof(Profile));
        }

        public IActionResult Home() => View();
        public IActionResult Admin() => View("AdminDashboard");

        // ==================== HELPER: Get Current User ID ====================
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId)) return userId;

            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(nameIdentifier) && int.TryParse(nameIdentifier, out int id)) return id;

            return 0;
        }

        // DTO for JSON binding
        public class RemoveFavoriteRequest { public int ListingId { get; set; } }

        // GET: /Account/Favorites
        [Authorize]
        [HttpGet]
        public IActionResult Favorites(int page = 1, int pageSize = 4)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return Unauthorized();

                // Query favourites -> CarListing projection
                var favsQuery = _favouriteRepo.Get()
                    .Where(f => f.UserId == userId)
                    .Select(f => f.CarListing)
                    .AsQueryable();

                // total count (for header & pagination)
                var totalCount = favsQuery.Count();

                // clamp page
                if (page < 1) page = 1;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                if (totalPages == 0) totalPages = 1;
                if (page > totalPages) page = totalPages;

                // get page items
                var pageItems = favsQuery
                    .OrderByDescending(c => c.UpdatedAt)   // keep consistent ordering (most recent first)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // expose pagination data to view
                ViewBag.TotalCount = totalCount;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;

                return View("~/Views/Account/Favorites.cshtml", pageItems);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading favorites: " + ex.Message;
                TempData["Notification.Message"] = "Error loading favorites: " + ex.Message;
                TempData["Notification.Type"] = "error";
                ViewBag.TotalCount = 0;
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 1;
                return View("~/Views/Account/Favorites.cshtml", new List<AutoHaven.Models.CarListingModel>());
            }
        }


        [Authorize]
        [HttpGet]
        public IActionResult History(string q = "", string sortBy = "newest", int page = 1)
        {
            int uid = GetCurrentUserId();
            if (uid == 0) return Unauthorized();

            const int pageSize = 4;
            if (page < 1) page = 1;

            // base query for user's history
            var userHistQuery = _historyRepo.Get().Where(h => h.UserId == uid).AsQueryable();

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

            // take latest view per listing
            var latestPerListing = userHistQuery
                .GroupBy(h => h.ListingId)
                .Select(g => g.OrderByDescending(x => x.ViewedAt).FirstOrDefault());

            var latestList = latestPerListing.Where(x => x != null).Select(x => x!).ToList();

            // sort
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
                    ordered = latestList.OrderByDescending(h => h.ViewedAt);
                    break;
            }

            // pagination
            var totalCount = ordered.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;

            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // --- NEW: authoritative favourites lookup for the listings on current page ---
            var listingIdsOnPage = items.Select(x => x.ListingId).Where(id => id > 0).Distinct().ToList();

            // Query favourites for current user for just these listings
            var favouriteListingIds = _favouriteRepo.Get()
                .Where(f => f.UserId == uid && f.ListingId != null && listingIdsOnPage.Contains(f.ListingId.Value))
                .Select(f => f.ListingId!.Value)
                .ToHashSet();


            ViewBag.FavouritedIds = favouriteListingIds;
            // -------------------------------------------------------------------------

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.SortBy = sortBy ?? "newest";
            ViewBag.SearchTerm = q ?? string.Empty;

            var allListings = _carListingRepo.Get().ToList();
            ViewBag.Makes = allListings.Where(cl => cl.Car != null && !string.IsNullOrEmpty(cl.Car.Manufacturer))
                                      .Select(cl => cl.Car.Manufacturer).Distinct().OrderBy(x => x).ToList();
            ViewBag.Years = allListings.Where(cl => cl.Car != null).Select(cl => cl.Car.ModelYear).Distinct().OrderByDescending(y => y).ToList();
            ViewBag.Transmissions = Enum.GetValues(typeof(CarModel.Transmission)).Cast<CarModel.Transmission>().ToList();
            ViewBag.Fuels = Enum.GetValues(typeof(CarModel.FuelType)).Cast<CarModel.FuelType>().ToList();

            return View("History", items);
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
            return RedirectToAction(nameof(History));
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

            return RedirectToAction(nameof(History));
        }

        // ------------------- Non-AJAX favorites endpoints (kept for server-side forms/fallback) -------------------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToFavorite(int listingId, string url)
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0) return Unauthorized();

                var listing = _carListingRepo.GetById(listingId);
                if (listing == null)
                {
                    TempData["Notification.Message"] = "Listing isn't existed";
                    TempData["Notification.Type"] = "error";
                    return RedirectToActionSafe(url);
                }

                var already = _favouriteRepo.Get().Any(f => f.UserId == userId && f.ListingId == listingId);
                if (already)
                {
                    TempData["Notification.Message"] = "Already in your favourites.";
                    TempData["Notification.Type"] = "info";
                    return RedirectToActionSafe(url);
                }

                var favorite = new FavouriteModel
                {
                    UserId = userId,
                    ListingId = listingId,
                    CreatedAt = DateTime.UtcNow
                };

                _favouriteRepo.Insert(favorite);

                TempData["Notification.Message"] = "Added To Favourites!";
                TempData["Notification.Type"] = "success";
                return RedirectToActionSafe(url);
            }
            catch (Exception ex)
            {
                TempData["Notification.Message"] = "Error: " + ex.Message;
                TempData["Notification.Type"] = "error";
                return RedirectToActionSafe(url);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromFavorite(int listingId, string url)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var fav = _favouriteRepo.Get().FirstOrDefault(f => f.UserId == userId && f.ListingId == listingId);
            if (fav != null) _favouriteRepo.Delete(fav.FavouriteId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok(new { success = true });

            TempData["Notification.Message"] = "Removed from favourites";
            TempData["Notification.Type"] = "info";
            return RedirectToActionSafe(url);
        }

        // ------------------- AJAX endpoints used by your JS (POST + CSRF via header) - returns JSON -------------------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToFavoriteInHistory([FromForm] int listingId)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new { success = false, message = "Unauthorized" });

            var listing = _carListingRepo.GetById(listingId);
            if (listing == null) return BadRequest(new { success = false, message = "Listing not found" });

            var already = _favouriteRepo.Get().Any(f => f.UserId == userId && f.ListingId == listingId);
            if (already) return Ok(new { success = true, message = "Already in favourites" });

            var favorite = new FavouriteModel
            {
                UserId = userId,
                ListingId = listingId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _favouriteRepo.Insert(favorite);
                return Ok(new { success = true, message = "Added to favourites" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to add favourite: " + ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromFavoriteInHistory([FromForm] int listingId)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new { success = false, message = "Unauthorized" });

            var fav = _favouriteRepo.Get().FirstOrDefault(f => f.UserId == userId && f.ListingId == listingId);
            if (fav == null) return Ok(new { success = true, message = "Already removed" });

            try
            {
                _favouriteRepo.Delete(fav.FavouriteId);
                return Ok(new { success = true, message = "Removed from favourites" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to remove favourite: " + ex.Message });
            }
        }

        // Keep these adapter methods for other pages (Favorites view etc.)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromFavoriteInFavorites(int listingId)
        {
            // non-AJAX fallback: redirect to favorites page after delete
            return RemoveFromFavorite(listingId, "Account/Favorites");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToFavoriteInFavorites(int listingId)
        {
            return AddToFavorite(listingId, "Account/Favorites");
        }

        // Helper: parse "Controller/Action" or simple action into RedirectToAction
        private IActionResult RedirectToActionSafe(string actionOrControllerAndAction)
        {
            if (string.IsNullOrWhiteSpace(actionOrControllerAndAction)) return RedirectToAction("Home");

            // If format is "Controller/Action"
            if (actionOrControllerAndAction.Contains('/'))
            {
                var parts = actionOrControllerAndAction.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var controller = parts[0];
                    var action = parts[1];
                    return RedirectToAction(action, controller);
                }
            }

            // otherwise treat as an action name
            return RedirectToAction(actionOrControllerAndAction);
        }
    }
}