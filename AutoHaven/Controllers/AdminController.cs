using AutoHaven.IRepository;
using AutoHaven.Models;
using AutoHaven.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoHaven.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUserModel> _userManager;
        private readonly ProjectDbContext _projectDbContext;
        private readonly ICarListingModelRepository _carListingRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFavouriteModelRepository _favouriteRepo;
        private readonly IReviewModelRepository _reviewRepo;
        private readonly ICarViewHistoryRepository _historyRepo;
        private readonly ICarListingModelRepository _carListingRepo;

        public AdminController(
            UserManager<ApplicationUserModel> userManager,
            ICarListingModelRepository carListingRepository,
            IFavouriteModelRepository favouriteRepo,
            IReviewModelRepository reviewRepo,
            ProjectDbContext projectDbContext,
            ICarViewHistoryRepository historyRepo,
            ICarListingModelRepository carListingRepo,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _favouriteRepo = favouriteRepo;
            _reviewRepo = reviewRepo;
            _projectDbContext = projectDbContext;
            _carListingRepository = carListingRepository;
            _historyRepo = historyRepo;
            _carListingRepo = carListingRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        //=====================          Admin Dashboard View          ====================

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // 1) Cars counts
            var carsForSale = await _projectDbContext.CarListings
                .CountAsync(c => c.Type == CarListingModel.ListingType.ForSelling);

            var carsForRent = await _projectDbContext.CarListings
                .CountAsync(c => c.Type == CarListingModel.ListingType.ForRenting);

            // 2) Users count (exclude app-level Admins)
            var totalUsers = await _userManager.Users
                .Where(u => u.Role != ApplicationUserModel.RoleEnum.Admin)
                .CountAsync();

            // 3) Total revenue FOR PROVIDERS ONLY: sum PricePerMonth for subscriptions whose user is Provider
            decimal totalRevenueMonthly = 0m;
            try
            {
                // LINQ query: join subscriptions -> plans -> users, filter by Provider role, sum plan.PricePerMonth
                totalRevenueMonthly = await (
                    from us in _projectDbContext.UserSubscriptions
                    join sp in _projectDbContext.SubscriptionPlans on us.PlanId equals sp.SubscriptionPlanId
                    join u in _userManager.Users on us.UserId equals u.Id
                    where u.Role == ApplicationUserModel.RoleEnum.Provider
                    select sp.PricePerMonth
                ).SumAsync();
            }
            catch (Exception)
            {
                // Fallback: materialize the projection then sum (slower but reliable)
                try
                {
                    var prices = await (
                        from us in _projectDbContext.UserSubscriptions
                        join sp in _projectDbContext.SubscriptionPlans on us.PlanId equals sp.SubscriptionPlanId
                        join u in _userManager.Users on us.UserId equals u.Id
                        where u.Role == ApplicationUserModel.RoleEnum.Provider
                        select sp.PricePerMonth
                    ).ToListAsync();

                    totalRevenueMonthly = prices.Sum();
                }
                catch
                {
                    // if everything fails, keep 0 and optionally log
                    totalRevenueMonthly = 0m;
                }
            }

            // 4) expose to view
            ViewBag.CarsForSale = carsForSale;
            ViewBag.CarsForRent = carsForRent;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRevenueMonthly = totalRevenueMonthly;
            ViewBag.TotalRevenueMonthlyFormatted = totalRevenueMonthly.ToString(); // e.g. $1,200

            var pending = await _userManager.Users
                          .Where(u => !u.IsApproved)
                          .OrderBy(u => u.CreatedAt)
                          .Select(u => new AutoHaven.ViewModel.PendingUserViewModel
                          {
                              Id = u.Id,
                              UserName = u.UserName,
                              Email = u.Email,
                              PhoneNumber = u.PhoneNumber,
                              Name = u.Name,
                              CreatedAt = u.CreatedAt,
                              Role = u.Role.ToString(),
                              NationalId = u.NationalId,
                              IdImagePath = u.IdImagePath
                          })
                          .ToListAsync();

            return View("Dashboard", pending);
        }

        // ====================      Admin Dahsboard Functionality     ====================

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser([FromForm] int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound(new { success = false, message = "User not found.", type = "error" });

            user.IsApproved = true;
            user.UpdatedAt = DateTime.Now;
            var up = await _userManager.UpdateAsync(user);
            if (!up.Succeeded)
            {
                var errors = string.Join("; ", up.Errors.Select(e => e.Description));
                return BadRequest(new { success = false, message = "Failed to approve user: " + errors, type = "error" });
            }

            // optional: send email/notification

            return Ok(new { success = true, message = "User approved successfully.", type = "success" });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUser([FromForm] int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound(new { success = false, message = "User not found.", type = "error" });

            using var tx = await _projectDbContext.Database.BeginTransactionAsync();
            try
            {
                var uid = user.Id;

                try
                {
                    var favs = _favouriteRepo.Get().Where(f => f.UserId == uid).ToList();
                    foreach (var f in favs) _favouriteRepo.Delete(f.FavouriteId);
                }
                catch { }

                try { _historyRepo.DeleteByUser(uid); } catch { }

                try
                {
                    var reviews = _reviewRepo.Get().Where(r => r.UserId == uid).ToList();
                    foreach (var r in reviews) _reviewRepo.Delete(r.ReviewId);
                }
                catch { }

                try
                {
                    var listings = _carListingRepo.Get().Where(c => c.UserId == uid).ToList();
                    foreach (var l in listings) _carListingRepo.Delete(l.ListingId);
                }
                catch { }

                await _projectDbContext.SaveChangesAsync();

                var delRes = await _userManager.DeleteAsync(user);
                if (!delRes.Succeeded)
                {
                    await tx.RollbackAsync();
                    var err = string.Join("; ", delRes.Errors.Select(e => e.Description));
                    return BadRequest(new { success = false, message = "Failed to delete user: " + err, type = "error" });
                }

                await tx.CommitAsync();
                return Ok(new { success = true, message = "User rejected and removed.", type = "success" });
            }
            catch (Exception ex)
            {
                try { await tx.RollbackAsync(); } catch { }
                return StatusCode(500, new { success = false, message = "Error: " + ex.Message, type = "error" });
            }
        }

        // ==================== HELPER: Check if current user is admin ====================
        private bool IsAdminUser()
        {
            var roleClaimValue = User.FindFirst("Role")?.Value;
            return roleClaimValue == ApplicationUserModel.RoleEnum.Admin.ToString();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int id) ? id : 0;
        }

        public async Task<IActionResult> PostListing(
            string search = "",
            string status = "All",
            string type = "All",
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // Prepare VM
            var vm = new AdminPostsViewModel
            {
                Search = search ?? string.Empty,
                Status = status ?? "All",
                Type = type ?? "All",
                DateFrom = dateFrom,
                DateTo = dateTo,
                Page = page,
                PageSize = pageSize
            };

            // Base query with required navigation properties
            IQueryable<CarListingModel> q = _projectDbContext.CarListings
                .AsNoTracking()
                .Include(x => x.Car)
                .Include(x => x.User)
                .Include(x => x.CarImages);

            // 1) Search: matches manufacturer, model, model year, description, or user fields.
            if (!string.IsNullOrWhiteSpace(vm.Search))
            {
                var s = vm.Search.Trim().ToLower();

                // Try parsing as year (for ModelYear search)
                bool hasYear = int.TryParse(vm.Search.Trim(), out int yearValue);

                q = q.Where(x =>
                    // Car fields
                    (x.Car != null &&
                        (
                            (!string.IsNullOrEmpty(x.Car.Manufacturer) && x.Car.Manufacturer.ToLower().Contains(s)) ||
                            (!string.IsNullOrEmpty(x.Car.Model) && x.Car.Model.ToLower().Contains(s)) ||
                            (hasYear && x.Car.ModelYear == yearValue)      // ✅ instead of ToString().Contains(...)
                        ))
                    // Listing description
                    || (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(s))
                    // User (seller) fields
                    || (x.User != null && (
                           (!string.IsNullOrEmpty(x.User.UserName) && x.User.UserName.ToLower().Contains(s)) ||
                           (!string.IsNullOrEmpty(x.User.Email) && x.User.Email.ToLower().Contains(s))
                       ))
                );
            }

            // Status filter (robust to casing, numeric id, and a couple of known typos)
            if (!string.IsNullOrWhiteSpace(vm.Status) && !vm.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                // Try direct enum parse first
                if (!Enum.TryParse<CarListingModel.State>(vm.Status, true, out var parsedState))
                {
                    // Try numeric parse (e.g. ?status=3)
                    if (int.TryParse(vm.Status, out var idx) && Enum.IsDefined(typeof(CarListingModel.State), idx))
                    {
                        parsedState = (CarListingModel.State)idx;
                    }
                    else
                    {
                        // small mapping to accept common misspellings / alternative labels from the UI
                        var sNorm = vm.Status.Trim().ToLowerInvariant();
                        if (sNorm == "unavalible" || sNorm == "unavailable") // accept both spellings
                        {
                            // your enum has "Unavaliable" (typo) — map to that member
                            parsedState = CarListingModel.State.Unavaliable;
                        }
                        else if (sNorm == "sold")
                        {
                            parsedState = CarListingModel.State.Sold;
                        }
                        else if (sNorm == "rented")
                        {
                            parsedState = CarListingModel.State.Rented;
                        }
                        else if (sNorm == "available")
                        {
                            parsedState = CarListingModel.State.Available;
                        }
                        else
                        {
                            // give up: don't apply a status filter
                            parsedState = default;
                            // mark as invalid by using a sentinel flag — below, only apply if validParsed==true
                            goto SkipStatusFilter;
                        }
                    }
                }

                q = q.Where(x => x.CurrentState == parsedState);
            }

        SkipStatusFilter:;


            // Type filter
            if (!string.IsNullOrWhiteSpace(vm.Type) && !vm.Type.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<CarListingModel.ListingType>(vm.Type, true, out var parsedType))
                {
                    q = q.Where(x => x.Type == parsedType);
                }
            }

            // Date range filters
            if (vm.DateFrom.HasValue)
            {
                var fromDate = vm.DateFrom.Value.Date;
                q = q.Where(x => x.CreatedAt >= fromDate);
            }

            if (vm.DateTo.HasValue)
            {
                var toDate = vm.DateTo.Value.Date.AddDays(1).AddTicks(-1);
                q = q.Where(x => x.CreatedAt <= toDate);
            }

            // Count for pagination
            vm.TotalCount = await q.CountAsync();

            // Projection: pick primary image if available, otherwise first by id, otherwise default placeholder.
            var rows = await q
                .OrderByDescending(x => x.CreatedAt)
                .Skip((vm.Page - 1) * vm.PageSize)
                .Take(vm.PageSize)
                .Select(x => new AdminPostsViewModel.PostRow
                {
                    ListingId = x.ListingId,

                    Title = x.Car != null
                        ? (x.Car.ModelYear.ToString() + " " + (x.Car.Manufacturer ?? "") + " " + (x.Car.Model ?? "")).Trim()
                        : $"Listing #{x.ListingId}",

                    // Prefer image marked IsPrimary, else fallback to lowest CarImageId, else null (we'll normalize later)
                    ThumbnailUrl = x.CarImages != null && x.CarImages.Any()
                        ? x.CarImages.Where(ci => ci.IsPrimary).Select(ci => ci.Path).FirstOrDefault()
                          ?? x.CarImages.OrderBy(ci => ci.CarImageId).Select(ci => ci.Path).FirstOrDefault()
                        : null,

                    SellerDisplay = x.User != null
                        ? GetUserDisplayName(x.User)
                        : "Unknown",

                    SellerType = string.Empty,
                    Price = x.Type == CarListingModel.ListingType.ForSelling ? x.NewPrice : x.RentPrice,
                    PriceLabel = x.Type == CarListingModel.ListingType.ForSelling ? "Sale" : "Rent",
                    Status = x.CurrentState.ToString(),
                    IsFeatured = x.IsFeatured,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            // Normalize thumbnail paths so <img src="..."> will work
            var webDefault = "/images/default-car.png";
            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.ThumbnailUrl))
                {
                    r.ThumbnailUrl = webDefault;
                    continue;
                }

                var p = r.ThumbnailUrl.Trim();

                // If it looks like a Windows absolute path (C:\...) treat as missing
                if (p.Length > 1 && char.IsLetter(p[0]) && p[1] == ':' && p.Contains('\\'))
                {
                    r.ThumbnailUrl = webDefault;
                    continue;
                }

                // Remove leading ~ or leading slashes and ensure leading '/'
                p = p.TrimStart('~').TrimStart('/');
                r.ThumbnailUrl = "/" + p;
            }

            vm.Posts = rows;

            return View(vm);
        }


        /// <summary>
        /// Toggle featured state for a listing.
        /// Expects a POST with validated antiforgery token. Returns JSON { success: bool, isFeatured: bool }.
        /// Also sets TempData for SwiftTail2 notifications.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeature([FromForm] int listingId)
        {
            if (listingId <= 0)
            {
                TempData["Notification.Message"] = "Invalid listing id.";
                TempData["Notification.Type"] = "error";
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid listing id",
                    notification = new { message = "Invalid listing id.", type = "error" }
                });
            }

            var entity = await _projectDbContext.CarListings.FindAsync(listingId);
            if (entity == null)
            {
                TempData["Notification.Message"] = "Listing not found.";
                TempData["Notification.Type"] = "error";
                return NotFound(new
                {
                    success = false,
                    message = "Listing not found",
                    notification = new { message = "Listing not found.", type = "error" }
                });
            }

            entity.IsFeatured = !entity.IsFeatured;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _projectDbContext.SaveChangesAsync();

                var msg = entity.IsFeatured ? "Listing marked as featured." : "Listing removed from featured.";
                TempData["Notification.Message"] = msg;
                TempData["Notification.Type"] = "success";

                return Json(new
                {
                    success = true,
                    isFeatured = entity.IsFeatured,
                    notification = new { message = msg, type = "success" }
                });
            }
            catch (Exception ex)
            {
                TempData["Notification.Message"] = "Failed to toggle featured state.";
                TempData["Notification.Type"] = "error";
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to toggle featured state",
                    details = ex.Message,
                    notification = new { message = "Failed to toggle featured state.", type = "error" }
                });
            }
        }



        /// <summary>
        /// Delete a listing.
        /// Expects a POST with validated antiforgery token. Returns JSON { success: bool }.
        /// Also sets TempData for SwiftTail2 notifications.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int listingId)
        {
            if (listingId <= 0)
            {
                TempData["Notification.Message"] = "Invalid listing id.";
                TempData["Notification.Type"] = "error";
                return BadRequest(new { success = false, message = "Invalid listing id" });
            }

            // load the listing with all children we care about
            var entity = await _projectDbContext.CarListings
                .Include(x => x.CarImages)
                .Include(x => x.Reviews)
                .Include(x => x.Favourites)
                .FirstOrDefaultAsync(x => x.ListingId == listingId);

            if (entity == null)
            {
                TempData["Notification.Message"] = "Listing not found.";
                TempData["Notification.Type"] = "error";
                return NotFound(new { success = false, message = "Listing not found" });
            }

            using var t = await _projectDbContext.Database.BeginTransactionAsync();
            try
            {
                // 1) Delete image files from disk (if Path is physical/relative to wwwroot).
                var webRoot = _webHostEnvironment?.WebRootPath; // ensure IWebHostEnvironment injected in ctor
                if (!string.IsNullOrEmpty(webRoot) && entity.CarImages != null)
                {
                    foreach (var img in entity.CarImages)
                    {
                        if (string.IsNullOrWhiteSpace(img.Path)) continue;
                        try
                        {
                            var relative = img.Path.TrimStart('~').TrimStart('/');
                            var fullPath = Path.Combine(webRoot, relative.Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                        }
                        catch
                        {
                            // log but don't fail the whole operation for file system problems
                        }
                    }
                }

                // 2) Remove child rows explicitly (if your DB doesn't cascade)
                if (entity.CarImages != null && entity.CarImages.Any())
                {
                    _projectDbContext.CarImages.RemoveRange(entity.CarImages);
                }
                if (entity.Reviews != null && entity.Reviews.Any())
                {
                    _projectDbContext.Reviews.RemoveRange(entity.Reviews);
                }
                if (entity.Favourites != null && entity.Favourites.Any())
                {
                    _projectDbContext.Favourites.RemoveRange(entity.Favourites);
                }

                // 3) Remove the listing
                _projectDbContext.CarListings.Remove(entity);

                await _projectDbContext.SaveChangesAsync();
                await t.CommitAsync();

                // TempData for SwiftTail2
                TempData["Notification.Message"] = "Listing deleted.";
                TempData["Notification.Type"] = "success";

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                // log the exception (ex)
                TempData["Notification.Message"] = "Failed to delete listing.";
                TempData["Notification.Type"] = "error";
                return StatusCode(500, new { success = false, message = "Failed to delete listing", details = ex.Message });
            }
        }



        /// <summary>
        /// Optional details endpoint that returns the full CarListingModel to a details view.
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var item = await _projectDbContext.CarListings
                .Include(x => x.Car)
                .Include(x => x.User)
                .Include(x => x.CarImages)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ListingId == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // ----------------- Helpers -----------------

        /// <summary>
        /// Resolve a user display name. Adjust to use the correct property names on your ApplicationUserModel.
        /// This helper uses reflection-ish style to avoid compile error if FullName doesn't exist; but
        /// if you know the property (e.g. FullName) change to x.User.FullName directly for clarity.
        /// </summary>
        private static string GetUserDisplayName(ApplicationUserModel user)
        {
            if (user == null) return "Unknown";

            // Prefer FullName (if present), then UserName, then Email
            try
            {
                // If your ApplicationUserModel contains FullName property, you can cast and use it:
                // return string.IsNullOrWhiteSpace(user.FullName) ? (user.UserName ?? user.Email ?? "Unknown") : user.FullName;
                // But in case FullName isn't available on the model used at compile time above, fallback:
                var fullNameProp = user.GetType().GetProperty("FullName");
                if (fullNameProp != null)
                {
                    var fullName = fullNameProp.GetValue(user) as string;
                    if (!string.IsNullOrWhiteSpace(fullName)) return fullName;
                }
            }
            catch
            {
                // ignore reflection errors
            }

            if (!string.IsNullOrWhiteSpace(user.UserName)) return user.UserName;
            if (!string.IsNullOrWhiteSpace(user.Email)) return user.Email;
            return "Unknown";
        }

        //======================== User Management ==========================

        [HttpGet]
        public async Task<IActionResult> UserListing(
            string search = "",
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var vm = new AdminUsersViewModel
            {
                Search = search ?? string.Empty,
                Page = page,
                PageSize = pageSize
            };

            // Base query (exclude admins)
            IQueryable<ApplicationUserModel> q = _projectDbContext.Users
                .AsNoTracking()
                .Where(u => u.Role != ApplicationUserModel.RoleEnum.Admin)   // <-- IMPORTANT
                .Include(u => u.CarListings)
                .Include(u => u.Reviews);

            // Search filter
            if (!string.IsNullOrWhiteSpace(vm.Search))
            {
                var s = vm.Search.Trim().ToLower();
                q = q.Where(u =>
                    (!string.IsNullOrEmpty(u.Name) && u.Name.ToLower().Contains(s)) ||
                    (!string.IsNullOrEmpty(u.UserName) && u.UserName.ToLower().Contains(s)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(s)) ||
                    (!string.IsNullOrEmpty(u.CompanyName) && u.CompanyName.ToLower().Contains(s))
                );
            }

            vm.TotalCount = await q.CountAsync();

            var rows = await q
                .OrderByDescending(u => u.CreatedAt)
                .Skip((vm.Page - 1) * vm.PageSize)
                .Take(vm.PageSize)
                .Select(u => new AdminUsersViewModel.UserRow
                {
                    Id = u.Id,
                    Name = string.IsNullOrEmpty(u.Name) ? u.UserName : u.Name,
                    UserName = u.UserName,
                    Email = u.Email,
                    CompanyName = u.CompanyName,
                    AvatarUrl = u.AvatarUrl,
                    IsBanned = u.IsBanned,
                    JoinedAt = u.CreatedAt
                })
                .ToListAsync();

            // Avatar normalization
            var webDefault = "/images/Default/default.jpg";
            foreach (var r in rows)
            {
                var avatar = r.AvatarUrl;
                if (string.IsNullOrWhiteSpace(avatar))
                {
                    r.AvatarUrl = webDefault;
                    continue;
                }

                var p = avatar.Trim();

                if (p.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    p.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    r.AvatarUrl = p;
                    continue;
                }

                if (p.Length > 1 && char.IsLetter(p[0]) && p[1] == ':' && p.Contains('\\'))
                {
                    r.AvatarUrl = webDefault;
                    continue;
                }

                p = p.Replace('\\', '/').TrimStart('~').TrimStart('/');
                r.AvatarUrl = "/" + p;
            }

            vm.Users = rows;

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser([FromForm] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { success = false, notification = new { message = "Invalid user id.", type = "error" } });
            }

            var user = await _projectDbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, notification = new { message = "User not found.", type = "error" } });
            }

            user.IsBanned = !user.IsBanned;
            user.BannedAt = user.IsBanned ? DateTime.UtcNow : (DateTime?)null;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                _projectDbContext.Users.Update(user);
                await _projectDbContext.SaveChangesAsync();

                var msg = user.IsBanned ? "User banned." : "User unbanned.";
                TempData["Notification.Message"] = msg;
                TempData["Notification.Type"] = "success";

                return Json(new
                {
                    success = true,
                    isBanned = user.IsBanned,
                    notification = new { message = msg, type = "success" }
                });
            }
            catch (Exception ex)
            {
                // log ex
                TempData["Notification.Message"] = "Failed to update user.";
                TempData["Notification.Type"] = "error";
                return StatusCode(500, new { success = false, notification = new { message = "Failed to update user.", type = "error" }, details = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser([FromForm] int userId)
        {
            if (userId <= 0)
            {
                TempData["Notification.Message"] = "Invalid user id.";
                TempData["Notification.Type"] = "error";
                return BadRequest(new { success = false, notification = new { message = "Invalid user id.", type = "error" } });
            }

            // Load user and related collections required for safe deletion
            var user = await _projectDbContext.Users
                // load listings and their children so we can remove them safely
                .Include(u => u.CarListings)
                    .ThenInclude(l => l.CarImages)
                .Include(u => u.CarListings)
                    .ThenInclude(l => l.Reviews)
                .Include(u => u.CarListings)
                    .ThenInclude(l => l.Favourites)
                // also load the user's own authored reviews & favourites
                .Include(u => u.Reviews)
                .Include(u => u.Favourites)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                TempData["Notification.Message"] = "User not found.";
                TempData["Notification.Type"] = "error";
                return NotFound(new { success = false, notification = new { message = "User not found.", type = "error" } });
            }

            // Prevent deleting admin accounts accidentally
            if (user.Role == ApplicationUserModel.RoleEnum.Admin)
            {
                TempData["Notification.Message"] = "Cannot delete admin account.";
                TempData["Notification.Type"] = "error";
                return BadRequest(new { success = false, notification = new { message = "Cannot delete admin account.", type = "error" } });
            }

            using var tx = await _projectDbContext.Database.BeginTransactionAsync();
            try
            {
                var webRoot = _webHostEnvironment?.WebRootPath ?? "";

                // 1) Delete user's ID image if exists
                if (!string.IsNullOrWhiteSpace(user.IdImagePath))
                {
                    try
                    {
                        var relative = user.IdImagePath.TrimStart('~').TrimStart('/');
                        var fullPath = Path.Combine(webRoot, relative.Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                    }
                    catch
                    {
                        // ignore file deletion errors
                    }
                }

                // 2) For each listing: delete images (files + DB), delete reviews & favourites attached to listing
                if (user.CarListings != null && user.CarListings.Any())
                {
                    foreach (var listing in user.CarListings)
                    {
                        // delete listing images from disk and DB
                        if (listing.CarImages != null && listing.CarImages.Any())
                        {
                            foreach (var img in listing.CarImages)
                            {
                                try
                                {
                                    var relative = (img.Path ?? "").TrimStart('~').TrimStart('/');
                                    var fullPath = Path.Combine(webRoot, relative.Replace('/', Path.DirectorySeparatorChar));
                                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                                }
                                catch
                                {
                                    // ignore file deletion errors
                                }
                            }

                            _projectDbContext.CarImages.RemoveRange(listing.CarImages);
                        }

                        // remove reviews attached to the listing (may belong to other users)
                        if (listing.Reviews != null && listing.Reviews.Any())
                        {
                            _projectDbContext.Reviews.RemoveRange(listing.Reviews);
                        }

                        // remove favourites attached to the listing (may belong to other users)
                        if (listing.Favourites != null && listing.Favourites.Any())
                        {
                            _projectDbContext.Favourites.RemoveRange(listing.Favourites);
                        }
                    }

                    // remove listings themselves
                    _projectDbContext.CarListings.RemoveRange(user.CarListings);
                }

                // 3) Remove reviews authored by the user (they might also be referenced elsewhere; we already removed listing-reviews above)
                if (user.Reviews != null && user.Reviews.Any())
                {
                    _projectDbContext.Reviews.RemoveRange(user.Reviews);
                }

                // 4) Remove favourites authored by the user
                if (user.Favourites != null && user.Favourites.Any())
                {
                    _projectDbContext.Favourites.RemoveRange(user.Favourites);
                }

                // 5) Optionally remove other related user data (subscriptions, etc.) - uncomment if needed
                // var userSubs = await _projectDbContext.UserSubscriptions.Where(us => us.UserId == userId).ToListAsync();
                // if (userSubs.Any()) _projectDbContext.UserSubscriptions.RemoveRange(userSubs);

                // 6) Finally remove the user
                _projectDbContext.Users.Remove(user);

                await _projectDbContext.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Notification.Message"] = "User deleted.";
                TempData["Notification.Type"] = "success";

                return Json(new { success = true, notification = new { message = "User deleted.", type = "success" } });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["Notification.Message"] = "Failed to delete user.";
                TempData["Notification.Type"] = "error";
                return StatusCode(500, new { success = false, notification = new { message = "Failed to delete user.", type = "error" }, details = ex.Message });
            }
        }



        /*  // ==================== USER MANAGEMENT ====================

          /// <summary>
          /// GET: /Admin/Users - Display all users
          /// </summary>
          [HttpGet]
          public async Task<IActionResult> Users(string searchTerm = "", string roleFilter = "", int page = 1)
          {
              if (!IsAdminUser())
                  return Forbid();

              try
              {
                  var users = _userManager.Users
                      .Where(u => u.Role != ApplicationUserModel.RoleEnum.Admin) // Don't show other admins
                      .ToList();

                  // Search filter
                  if (!string.IsNullOrWhiteSpace(searchTerm))
                  {
                      users = users.Where(u =>
                          u.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                          u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                          (u.Name ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                      ).ToList();
                  }

                  // Role filter
                  if (!string.IsNullOrWhiteSpace(roleFilter) &&
                      Enum.TryParse<ApplicationUserModel.RoleEnum>(roleFilter, out var role))
                  {
                      users = users.Where(u => u.Role == role).ToList();
                  }

                  // Sort by newest first
                  users = users.OrderByDescending(u => u.CreatedAt).ToList();

                  // Pagination
                  const int pageSize = 10;
                  var totalPages = (int)Math.Ceiling(users.Count() / (double)pageSize);
                  var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                  ViewBag.SearchTerm = searchTerm;
                  ViewBag.RoleFilter = roleFilter;
                  ViewBag.CurrentPage = page;
                  ViewBag.TotalPages = totalPages;
                  ViewBag.TotalCount = users.Count();

                  return View(pagedUsers);
              }
              catch (Exception ex)
              {
                  TempData["Notification.Message"] = $"Error loading users: {ex.Message}";
                  TempData["Notification.Type"] = "error";
                  return View(new List<ApplicationUserModel>());
              }
          }

          // ✅ EDIT USER - in Users table
          [HttpGet]
          public IActionResult EditUser(int id)
          {
              if (!IsAdminUser())
                  return Forbid();

              var user = _userManager.Users.FirstOrDefault(u => u.Id == id);
              if (user == null)
                  return NotFound();

              // Redirects to: /Account/Edit/{id}
              return RedirectToAction("Edit", "Account", new { id = user.Id });
          }

          // ✅ EDIT LISTING - in Listings table
          [HttpGet]
          public IActionResult EditListing(int id)
          {
              if (!IsAdminUser())
                  return Forbid();

              var listing = _carListingRepository.GetById(id);
              if (listing == null)
                  return NotFound();

              // Redirects to: /Car/Edit/{id}
              return RedirectToAction("Edit", "Car", new { id = listing.ListingId });
          }

          /// <summary>
          /// POST: /Admin/Users/Ban/{id} - Ban a user
          /// </summary>
          [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> BanUser(int id, string reason = "")
          {
              if (!IsAdminUser())
                  return Forbid();

              try
              {
                  var user = await _userManager.FindByIdAsync(id.ToString());
                  if (user == null)
                      return NotFound();

                  if (user.Role == ApplicationUserModel.RoleEnum.Admin)
                  {
                      TempData["Notification.Message"] = "❌ Cannot ban another admin user";
                      TempData["Notification.Type"] = "error";
                      return RedirectToAction(nameof(Users));
                  }

                  if (user.IsBanned)
                  {
                      TempData["Notification.Message"] = "⚠️ User is already banned";
                      TempData["Notification.Type"] = "warning";
                      return RedirectToAction(nameof(Users));
                  }

                  user.IsBanned = true;
                  user.BannedAt = DateTime.Now;
                  user.BanReason = reason ?? "Banned by admin";
                  user.UpdatedAt = DateTime.Now;

                  var result = await _userManager.UpdateAsync(user);

                  if (result.Succeeded)
                  {
                      TempData["Notification.Message"] = $"✅ User '{user.UserName}' has been banned";
                      TempData["Notification.Type"] = "success";
                  }
                  else
                  {
                      TempData["Notification.Message"] = "❌ Error banning user";
                      TempData["Notification.Type"] = "error";
                  }

                  return RedirectToAction(nameof(Users));
              }
              catch (Exception ex)
              {
                  TempData["Notification.Message"] = $"❌ Error banning user: {ex.Message}";
                  TempData["Notification.Type"] = "error";
                  return RedirectToAction(nameof(Users));
              }
          }

          /// <summary>
          /// POST: /Admin/Users/Unban/{id} - Unban a user
          /// </summary>
          [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> UnbanUser(int id)
          {
              if (!IsAdminUser())
                  return Forbid();

              try
              {
                  var user = await _userManager.FindByIdAsync(id.ToString());
                  if (user == null)
                      return NotFound();

                  if (!user.IsBanned)
                  {
                      TempData["Notification.Message"] = "⚠️ User is not banned";
                      TempData["Notification.Type"] = "warning";
                      return RedirectToAction(nameof(Users));
                  }

                  user.IsBanned = false;
                  user.BannedAt = null;
                  user.BanReason = null;
                  user.UpdatedAt = DateTime.Now;

                  var result = await _userManager.UpdateAsync(user);

                  if (result.Succeeded)
                  {
                      TempData["Notification.Message"] = $"✅ User '{user.UserName}' has been unbanned";
                      TempData["Notification.Type"] = "success";
                  }
                  else
                  {
                      TempData["Notification.Message"] = "❌ Error unbanning user";
                      TempData["Notification.Type"] = "error";
                  }

                  return RedirectToAction(nameof(Users));
              }
              catch (Exception ex)
              {
                  TempData["Notification.Message"] = $"❌ Error unbanning user: {ex.Message}";
                  TempData["Notification.Type"] = "error";
                  return RedirectToAction(nameof(Users));
              }
          }

          /// <summary>
          /// POST: /Admin/Users/Delete/{id} - Delete a user and their listings
          /// </summary>
          [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> DeleteUser(int id)
          {
              if (!IsAdminUser())
                  return Forbid();

              try
              {
                  var user = await _userManager.FindByIdAsync(id.ToString());
                  if (user == null)
                      return NotFound();

                  if (user.Role == ApplicationUserModel.RoleEnum.Admin)
                  {
                      TempData["Notification.Message"] = "❌ Cannot delete another admin user";
                      TempData["Notification.Type"] = "error";
                      return RedirectToAction(nameof(Users));
                  }

                  // Delete all listings first
                  //-------------------TO DO DELETE REVIEWS ON THAT LISTING FIRST

                  var listings = _carListingRepository.Get().Where(l => l.UserId == id).ToList();
                  foreach (var listing in listings)
                  {
                      _carListingRepository.Delete(listing.ListingId);
                  }

                  // Delete user
                  var result = await _userManager.DeleteAsync(user);

                  if (result.Succeeded)
                  {
                      TempData["Notification.Message"] = $"✅ User '{user.UserName}' and all their listings have been deleted";
                      TempData["Notification.Type"] = "success";
                  }
                  else
                  {
                      TempData["Notification.Message"] = "❌ Error deleting user";
                      TempData["Notification.Type"] = "error";
                  }

                  return RedirectToAction(nameof(Users));
              }
              catch (Exception ex)
              {
                  TempData["Notification.Message"] = $"❌ Error deleting user: {ex.Message}";
                  TempData["Notification.Type"] = "error";
                  return RedirectToAction(nameof(Users));
              }
          }

          // ==================== LISTING MANAGEMENT ====================

          /// <summary>
          /// GET: /Admin/Listings - Display all listings with status filter
          /// </summary>
          [HttpGet]
          public IActionResult Listings(string statusFilter = "all", string searchTerm = "", int page = 1)
          {
              if (!IsAdminUser())
                  return Forbid();

              try
              {
                  var listings = _carListingRepository.Get().ToList();

                  // Status filter
                  if (statusFilter != "all" && Enum.TryParse<CarListingModel.State>(statusFilter, ignoreCase: true, out var state))
                  {
                      listings = listings.Where(l => l.CurrentState == state).ToList();
                  }

                  // Search filter
                  if (!string.IsNullOrWhiteSpace(searchTerm))
                  {
                      listings = listings.Where(l =>
                          (l.Car?.Manufacturer ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                          (l.Car?.Model ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                          (l.User?.UserName ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                          (l.Description ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                      ).ToList();
                  }

                  // Sort by newest first
                  listings = listings.OrderByDescending(l => l.CreatedAt).ToList();

                  // Pagination
                  const int pageSize = 10;
                  var totalPages = (int)Math.Ceiling(listings.Count() / (double)pageSize);
                  var pagedListings = listings.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                  ViewBag.StatusFilter = statusFilter;
                  ViewBag.SearchTerm = searchTerm;
                  ViewBag.CurrentPage = page;
                  ViewBag.TotalPages = totalPages;
                  ViewBag.TotalCount = listings.Count();
                  ViewBag.States = Enum.GetValues(typeof(CarListingModel.State)).Cast<CarListingModel.State>().ToList();

                  return View(pagedListings);
              }
              catch (Exception ex)
              {
                  TempData["Notification.Message"] = $"Error loading listings: {ex.Message}";
                  TempData["Notification.Type"] = "error";
                  return View(new List<CarListingModel>());
              }
          }

          /// <summary>
          /// POST: /Admin/Listings/Delete/{id} - Delete a listing
          /// </summary>
          [HttpPost]
          [ValidateAntiForgeryToken]
          public IActionResult DeleteListing(int id)
          {
              if (!IsAdminUser())
                  return Forbid();

              try
              {
                  //-----------------TO DO DELETE ALL REVIEWS ON THE THAT LISTING FIRST
                  var listing = _carListingRepository.GetById(id);
                  if (listing == null)
                      return NotFound();

                  var carTitle = $"{listing.Car?.Manufacturer} {listing.Car?.Model}";
                  _carListingRepository.Delete(id);

                  TempData["Notification.Message"] = $"✅ Listing '{carTitle}' has been deleted";
                  TempData["Notification.Type"] = "success";
                  return RedirectToAction(nameof(Listings));
              }
              catch (Exception ex)
              {
                  TempData["Notification.Message"] = $"❌ Error deleting listing: {ex.Message}";
                  TempData["Notification.Type"] = "error";
                  return RedirectToAction(nameof(Listings));
              }
          }

          /// <summary>
          /// POST: /Admin/Listings/Feature/{id} - Toggle featured status
          /// </summary>
          [HttpPost]
          [ValidateAntiForgeryToken]
          public IActionResult FeatureListing(int id)
          {
              if (!IsAdminUser())
                  return Forbid();

              try
              {
                  var listing = _carListingRepository.GetById(id);
                  if (listing == null)
                      return NotFound();

                  listing.IsFeatured = !listing.IsFeatured;
                  listing.UpdatedAt = DateTime.Now;

                  // ✅ KEEP ALL EXISTING IMAGES - Extract their IDs
                  int[] existingImageIds = listing.CarImages
                      ?.Select(img => img.CarImageId)
                      .ToArray() ?? new int[0];

                  System.Diagnostics.Debug.WriteLine($"📋 Keeping {existingImageIds.Length} images when toggling featured");

                  // Update WITHOUT modifying images
                  _carListingRepository.Update(listing, existingImageIds, null);

                  var message = listing.IsFeatured ? "featured" : "unfeatured";
                  TempData["Notification.Message"] = $"✅ Listing has been {message}";
                  TempData["Notification.Type"] = "success";
                  return RedirectToAction(nameof(Listings));
              }
              catch (Exception ex)
              {
                  System.Diagnostics.Debug.WriteLine($"❌ FeatureListing Error: {ex}");
                  TempData["Notification.Message"] = $"❌ Error updating listing: {ex.Message}";
                  TempData["Notification.Type"] = "error";
                  return RedirectToAction(nameof(Listings));
              }
          }

          /// <summary>
          /// POST: /Admin/Listings/ChangeState/{id} - Change listing state
          /// </summary>
          [HttpPost]
          [ValidateAntiForgeryToken]
          public IActionResult ChangeListingState(int id, string newState)
          {
              if (!IsAdminUser())
                  return Forbid();

              try
              {
                  var listing = _carListingRepository.GetById(id);
                  if (listing == null)
                      return NotFound();

                  if (Enum.TryParse<CarListingModel.State>(newState, ignoreCase: true, out var state))
                  {
                      listing.CurrentState = state;
                      listing.UpdatedAt = DateTime.Now;
                      _carListingRepository.Update(listing, new int[0], null);

                      TempData["Notification.Message"] = $"✅ Listing state changed to {state}";
                      TempData["Notification.Type"] = "success";
                  }
                  else
                  {
                      TempData["Notification.Message"] = "❌ Invalid state";
                      TempData["Notification.Type"] = "error";
                  }

                  return RedirectToAction(nameof(Listings));
              }
              catch (Exception ex)
              {
                  TempData["Notification.Message"] = $"❌ Error changing listing state: {ex.Message}";
                  TempData["Notification.Type"] = "error";
                  return RedirectToAction(nameof(Listings));
              }
          }

      */
    }

}