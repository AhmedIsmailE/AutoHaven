using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoHaven.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUserModel> _userManager;
        private readonly ICarListingModelRepository _carListingRepository;

        public AdminController(
            UserManager<ApplicationUserModel> userManager,
            ICarListingModelRepository carListingRepository)
        {
            _userManager = userManager;
            _carListingRepository = carListingRepository;
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

        // ==================== USER MANAGEMENT ====================

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
    }
}