using AutoHaven.Models;
using AutoHaven.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoHaven.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUserModel> _userManager;
        private readonly SignInManager<ApplicationUserModel> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly ProjectDbContext _projectDbContext;
        private const long MaxFileBytes = 2 * 1024 * 1024;
        private const int MaxWidth = 1024;
        private const int MaxHeight = 1024;

        public AccountController(
            UserManager<ApplicationUserModel> userManager,
            SignInManager<ApplicationUserModel> signInManager,
            ProjectDbContext projectDbContext,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _projectDbContext = projectDbContext;
            _env = env;
        }

        // ==================== GET: Register ====================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ==================== POST: Register ====================
        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserViewModel userViewModel)
        {
            // ✅ PROVIDER-SPECIFIC VALIDATION
            if (userViewModel.Role == ApplicationUserModel.RoleEnum.Provider)
            {
                if (string.IsNullOrEmpty(userViewModel.CompanyName))
                    ModelState.AddModelError("CompanyName", "Company Name is required.");

                if (string.IsNullOrEmpty(userViewModel.State))
                    ModelState.AddModelError("State", "State is required.");

                if (string.IsNullOrEmpty(userViewModel.City))
                    ModelState.AddModelError("City", "City is required.");

                if (string.IsNullOrEmpty(userViewModel.Street))
                    ModelState.AddModelError("Street", "Street is required.");

                if (string.IsNullOrEmpty(userViewModel.NationalId) || userViewModel.NationalId.Length != 14)
                    ModelState.AddModelError("NationalId", "National ID must be 14 digits.");

                if (userViewModel.IdImage == null || userViewModel.IdImage.Length == 0)
                    ModelState.AddModelError("IdImage", "ID Image is required.");

                // ✅ CHECK NATIONAL ID UNIQUENESS
                if (!string.IsNullOrWhiteSpace(userViewModel.NationalId))
                {
                    var existsNi = await _userManager.Users
                        .AnyAsync(u => u.NationalId == userViewModel.NationalId);
                    if (existsNi)
                        ModelState.AddModelError(nameof(userViewModel.NationalId),
                            "This National ID is already used.");
                }
            }

            if (ModelState.IsValid)
            {
                // ✅ CHECK EMAIL ALREADY EXISTS
                var existingEmail = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == userViewModel.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email is already in use.");
                    return View(userViewModel);
                }

                // ✅ CHECK USERNAME ALREADY EXISTS
                var existingUsername = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.UserName == userViewModel.UserName);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("UserName", "Username is already taken.");
                    return View(userViewModel);
                }

                // ==================== CREATE USER ====================
                ApplicationUserModel applicationUser = new ApplicationUserModel
                {
                    UserName = userViewModel.UserName,
                    Email = userViewModel.Email,
                    PhoneNumber = userViewModel.PhoneNumber,
                    Role = userViewModel.Role,
                    Name = userViewModel.Name,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // ✅ MAP PROVIDER FIELDS
                if (userViewModel.Role == ApplicationUserModel.RoleEnum.Provider)
                {
                    applicationUser.State = userViewModel.State;
                    applicationUser.Street = userViewModel.Street;
                    applicationUser.CompanyName = userViewModel.CompanyName;
                    applicationUser.NationalId = userViewModel.NationalId;
                    applicationUser.City = userViewModel.City;
                }

                // ✅ HANDLE ID IMAGE UPLOAD
                if (userViewModel.IdImage != null && userViewModel.IdImage.Length > 0)
                {
                    var allowed = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif" };
                    if (!allowed.Contains(userViewModel.IdImage.ContentType.ToLower()))
                    {
                        ModelState.AddModelError("IdImage", "Only PNG/JPG/GIF images are allowed.");
                        return View(userViewModel);
                    }

                    if (userViewModel.IdImage.Length > MaxFileBytes)
                    {
                        ModelState.AddModelError("IdImage", "Maximum file size is 2 MB.");
                        return View(userViewModel);
                    }

                    try
                    {
                        var webRoot = _env.WebRootPath;
                        var usernameSafe = string.Concat(userViewModel.UserName
                            .Split(Path.GetInvalidFileNameChars()));
                        var uploadDir = Path.Combine(webRoot, "images", "Providers", usernameSafe, "ID");
                        Directory.CreateDirectory(uploadDir);

                        var ext = Path.GetExtension(userViewModel.IdImage.FileName);
                        if (string.IsNullOrWhiteSpace(ext)) ext = ".png";

                        var fileName = $"id_{Guid.NewGuid():N}{ext}";
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await userViewModel.IdImage.CopyToAsync(fs);
                        }

                        applicationUser.IdImagePath = $"/images/Providers/{usernameSafe}/ID/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("IdImage", $"Error uploading image: {ex.Message}");
                        return View(userViewModel);
                    }
                }

                // ✅ CREATE USER
                IdentityResult result = await _userManager.CreateAsync(applicationUser, userViewModel.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(userViewModel);
                }
            }
            return View(userViewModel);
        }

        // ==================== GET: Login ====================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ==================== POST: Login ====================
        [HttpPost]
        public async Task<IActionResult> Login(LoginUserViewModel loginUserViewModel)
        {
            if (ModelState.IsValid)
            {
                ApplicationUserModel user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == loginUserViewModel.EmailOrPhone
                                           || u.PhoneNumber == loginUserViewModel.EmailOrPhone);

                if (user != null)
                {
                    bool result = await _userManager.CheckPasswordAsync(user, loginUserViewModel.Password);

                    if (result)
                    {
                        user.UpdatedAt = DateTime.Now;
                        await _userManager.UpdateAsync(user);

                        List<Claim> claims = new List<Claim>
                        {
                            new Claim("Role", user.Role.ToString())
                        };

                        await _signInManager.SignInWithClaimsAsync(user,
                            isPersistent: loginUserViewModel.RememberMe, claims);

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid Credentials");
                return View(loginUserViewModel);
            }

            return View(loginUserViewModel);
        }

        // ==================== Logout ====================
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ==================== GET: Profile ====================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile(string? edit)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = ProfileViewModel.MapToModel(user);

            if (edit == "1")
                ViewData["EditMode"] = true;

            return View("Profile", model);
        }

        // ==================== GET: Edit ====================
        [HttpGet]
        public IActionResult Edit()
        {
            return RedirectToAction(nameof(Profile), new { edit = 1 });
        }

        // ==================== POST: Edit ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model, IFormFile? avatar)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewData["EditMode"] = true;

            if (!ModelState.IsValid)
                return View("Profile", model);

            user.Name = model.Name;
            user.CompanyName = model.CompanyName;
            user.Street = model.Street;
            user.City = model.City;
            user.State = model.State;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmail = await _userManager.SetEmailAsync(user, model.Email ?? string.Empty);
                if (!setEmail.Succeeded)
                {
                    foreach (var e in setEmail.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);
                    return View("Profile", model);
                }
            }

            if (!string.Equals(user.PhoneNumber, model.PhoneNumber, StringComparison.OrdinalIgnoreCase))
            {
                var setPhone = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhone.Succeeded)
                {
                    foreach (var e in setPhone.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);
                    return View("Profile", model);
                }
            }

            // ✅ HANDLE AVATAR UPLOAD
            if (avatar != null && avatar.Length > 0)
            {
                var allowed = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif" };
                if (!allowed.Contains(avatar.ContentType.ToLower()))
                {
                    ModelState.AddModelError("Avatar", "Only PNG/JPG/GIF images allowed.");
                    return View("Profile", model);
                }

                if (avatar.Length > MaxFileBytes)
                {
                    ModelState.AddModelError("Avatar", $"Max file size is {MaxFileBytes / 1024 / 1024} MB.");
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
                            ModelState.AddModelError("Avatar",
                                $"Image too large. Max {MaxWidth}x{MaxHeight}px.");
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
                    ModelState.AddModelError("Avatar", "Failed to upload image.");
                    return View("Profile", model);
                }
            }

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                foreach (var err in update.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View("Profile", model);
            }

            TempData["Notification.Message"] = "Profile updated successfully!";
            TempData["Notification.Type"] = "success";
            return RedirectToAction(nameof(Profile));
        }

        // ==================== POST: ResetAvatar ====================
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

        // ==================== Navigation ====================
        [Authorize]
        public IActionResult About()
        {
            return View("About");
        }

        public IActionResult Home()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Admin()
        {
            return View("AdminDashboard");
        }
    }
}