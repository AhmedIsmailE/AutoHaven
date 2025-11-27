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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly ProjectDbContext _projectDbContext;
        private const long MaxFileBytes = 2 * 1024 * 1024;
        private const int MaxWidth = 1024;
        private const int MaxHeight = 1024;
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
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

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                // Create new ApplicationUser
                ApplicationUser applicationUser = new ApplicationUser();
                applicationUser.UserName = userViewModel.UserName;
                applicationUser.Email = userViewModel.Email;
                applicationUser.PhoneNumber = userViewModel.PhoneNumber;
                applicationUser.Role = userViewModel.Role;
                applicationUser.CreatedAt = DateTime.Now;
                applicationUser.UpdatedAt = DateTime.Now;
                //  applicationUser.PasswordHash=userViewModel.Password;  
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
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginUserViewModel loginUserViewModel)
        {
            if (ModelState.IsValid)
            {
                // Search user by Email or Phone
                ApplicationUser user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == loginUserViewModel.EmailOrPhone
                                           || u.PhoneNumber == loginUserViewModel.EmailOrPhone);

                if (user != null)
                {
                    // التحقق من كلمة المرور
                    bool result = await _userManager.CheckPasswordAsync(user, loginUserViewModel.Password);

                    if (result)
                    {
                        user.UpdatedAt = DateTime.Now;
                        await _userManager.UpdateAsync(user);
                        List<Claim> claims = new List<Claim>
                            {
                                new Claim("Role", user.Role.ToString())

                            };

                        await _signInManager.SignInWithClaimsAsync(user, isPersistent: loginUserViewModel.RememberMe, claims); // Create Cookies


                        return RedirectToAction("Index");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid Credentials");
                return View(loginUserViewModel);
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
        public IActionResult Index()
        {
            return View();
        }
      
        [Authorize]
        [HttpGet]
        // ==================== GET: Profile ====================
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
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(ProfileViewModel model, IFormFile? avatar)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null) return RedirectToAction("Login", "Account");

        //    ViewData["EditMode"] = true;

        //    if (!ModelState.IsValid)
        //        return View("Profile", model);

        //    user.Name = model.Name;
        //    user.CompanyName = model.CompanyName;
        //    user.Street = model.Street;
        //    user.City = model.City;
        //    user.State = model.State;
        //    user.UpdatedAt = DateTime.Now;

        //    if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        //    {
        //        var setEmail = await _userManager.SetEmailAsync(user, model.Email ?? string.Empty);
        //        if (!setEmail.Succeeded)
        //        {
        //            foreach (var e in setEmail.Errors) ModelState.AddModelError(string.Empty, e.Description);
        //            return View("Profile", model);
        //        }
        //    }

        //    if (!string.Equals(user.PhoneNumber, model.PhoneNumber, StringComparison.OrdinalIgnoreCase))
        //    {
        //        var setPhone = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
        //        if (!setPhone.Succeeded)
        //        {
        //            foreach (var e in setPhone.Errors) ModelState.AddModelError(string.Empty, e.Description);
        //            return View("Profile", model);
        //        }
        //    }

        //    if (avatar != null && avatar.Length > 0)
        //    {
        //        var allowed = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif" };
        //        if (!allowed.Contains(avatar.ContentType.ToLower()))
        //        {
        //            ModelState.AddModelError("Avatar", "Only PNG/JPG/GIF images allowed.");
        //            return View("Profile", model);
        //        }

        //        if (avatar.Length > MaxFileBytes)
        //        {
        //            ModelState.AddModelError("Avatar", $"Max file size is {MaxFileBytes / 1024 / 1024} MB.");
        //            return View("Profile", model);
        //        }

        //        try
        //        {
        //            using var mem = new MemoryStream();
        //            await avatar.CopyToAsync(mem);
        //            mem.Position = 0;

        //            try
        //            {
        //                using var img = System.Drawing.Image.FromStream(mem);
        //                if (img.Width > MaxWidth || img.Height > MaxHeight)
        //                {
        //                    ModelState.AddModelError("Avatar", $"Image too large. Max {MaxWidth}x{MaxHeight}px.");
        //                    return View("Profile", model);
        //                }
        //            }
        //            catch { }

        //            mem.Position = 0;

        //            var usernameSafe = string.IsNullOrWhiteSpace(user.UserName)
        //                ? $"user_{user.Id}"
        //                : string.Concat(user.UserName.Split(Path.GetInvalidFileNameChars()));

        //            var webRoot = _env.WebRootPath;
        //            var userPfpDir = Path.Combine(webRoot, "images", usernameSafe, "PFP");
        //            Directory.CreateDirectory(userPfpDir);

        //            var ext = Path.GetExtension(avatar.FileName);
        //            if (string.IsNullOrWhiteSpace(ext)) ext = ".png";

        //            var newFileName = $"p{Guid.NewGuid():N}{ext}";
        //            var filePath = Path.Combine(userPfpDir, newFileName);

        //            using var fs = new FileStream(filePath, FileMode.Create);
        //            mem.CopyTo(fs);

        //            var prev = user.AvatarUrl;
        //            if (!string.IsNullOrWhiteSpace(prev) && prev.StartsWith($"/images/{usernameSafe}/PFP/"))
        //            {
        //                var prevPath = Path.Combine(webRoot, prev.TrimStart('/'));
        //                if (System.IO.File.Exists(prevPath)) System.IO.File.Delete(prevPath);
        //            }

        //            user.AvatarUrl = $"/images/{usernameSafe}/PFP/{newFileName}";
        //        }
        //        catch
        //        {
        //            ModelState.AddModelError("Avatar", "Failed to upload image.");
        //            return View("Profile", model);
        //        }
        //    }

        //    var update = await _userManager.UpdateAsync(user);
        //    if (!update.Succeeded)
        //    {
        //        foreach (var err in update.Errors) ModelState.AddModelError(string.Empty, err.Description);
        //        return View("Profile", model);
        //    }

        //    TempData["Success"] = "Profile updated.";
        //    return RedirectToAction(nameof(Profile));
        //}
        // ==================== POST: ResetAvatar ====================
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ResetAvatar()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null) return RedirectToAction("Login", "Account");

        //    var webRoot = _env.WebRootPath;

        //    try
        //    {
        //        var usernameSafe = string.Concat(user.UserName.Split(Path.GetInvalidFileNameChars()));

        //        // Delete previous custom avatar IF it's inside user PFP folder
        //        if (!string.IsNullOrWhiteSpace(user.AvatarUrl) &&
        //            user.AvatarUrl.StartsWith($"/images/{usernameSafe}/PFP/"))
        //        {
        //            var prevPath = Path.Combine(webRoot, user.AvatarUrl.TrimStart('/'));
        //            if (System.IO.File.Exists(prevPath)) System.IO.File.Delete(prevPath);
        //        }

        //        // Set the default avatar instead of null
        //        user.AvatarUrl = ProfileViewModel.DevFallbackLocalPath;

        //        user.UpdatedAt = DateTime.Now;

        //        await _userManager.UpdateAsync(user);
        //    }
        //    catch { }

        //    TempData["Success"] = "Avatar reset.";
        //    return RedirectToAction(nameof(Profile));
        //}






        //[Authorize]
        //public IActionResult Index()
        //{
        //    //List<Product> products = productData.Products;
        //    //return View(products);
        //    if (User.Identity.IsAuthenticated)
        //    {

        //        //return View(specific_model);

        //    }
        //    else
        //    {
        //        return View("Login");
        //    }

        //}

    }
}







