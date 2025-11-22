////using AutoHaven.Models;
////using AutoHaven.ViewModel;
////using Microsoft.AspNetCore.Authorization;
////using Microsoft.AspNetCore.Identity;
////using Microsoft.AspNetCore.Mvc;
////using Microsoft.EntityFrameworkCore;
////using System.Security.Claims;

////namespace AutoHaven.Controllers
////{
////    public class AccountController : Controller
////    {
////        private readonly UserManager<ApplicationUser> _userManager;
////        private readonly SignInManager<ApplicationUser> _signInManager;

////        public AccountController(
////            UserManager<ApplicationUser> userManager,
////            SignInManager<ApplicationUser> signInManager)
////        {
////            _userManager = userManager;
////            _signInManager = signInManager;
////        }
////        [HttpGet]
////        public IActionResult Register()
////        {
////            return View();
////        }
////        [HttpPost]
////        public async Task<IActionResult> Register(RegisterUserViewModel userViewModel)
////        {
////            if (ModelState.IsValid)
////            {
////                // Create new ApplicationUser
////                ApplicationUser applicationUser = new ApplicationUser();
////                applicationUser.UserName = userViewModel.UserName;
////                applicationUser.Email = userViewModel.Email;
////                applicationUser.PhoneNumber = userViewModel.PhoneNumber;
////                applicationUser.Role = userViewModel.Role;
////                applicationUser.CreatedAt = DateTime.Now;
////                applicationUser.UpdatedAt = DateTime.Now;
////              //  applicationUser.PasswordHash=userViewModel.Password;  
////                IdentityResult result = await _userManager.CreateAsync(applicationUser, userViewModel.Password);
////                if (result.Succeeded)
////                {
////                    return RedirectToAction("Login");
////                }
////                else
////                {
////                    foreach (IdentityError error in result.Errors)
////                    {
////                        ModelState.AddModelError("", error.Description);
////                    }
////                    return View(userViewModel);
////                }
////            }
////            return View(userViewModel);
////        }
////        [HttpGet]
////        public IActionResult Login()
////        {
////            return View();
////        }
////        [HttpPost]
////        public async Task<IActionResult> Login(LoginUserViewModel loginUserViewModel)
////        {
////            if (ModelState.IsValid)
////            {
////                // Search user by Email or Phone
////                ApplicationUser user = await _userManager.Users
////                    .FirstOrDefaultAsync(u => u.Email == loginUserViewModel.EmailOrPhone
////                                           || u.PhoneNumber == loginUserViewModel.EmailOrPhone);

////                if (user != null)
////                {
////                    // التحقق من كلمة المرور
////                    bool result = await _userManager.CheckPasswordAsync(user, loginUserViewModel.Password);

////                    if (result)
////                    {
////                        user.UpdatedAt = DateTime.Now;
////                        await _userManager.UpdateAsync(user);
////                        List<Claim> claims = new List<Claim>
////                            {
////                                new Claim("Role", user.Role.ToString())

////                            };

////                        await _signInManager.SignInWithClaimsAsync(user, isPersistent: loginUserViewModel.RememberMe, claims); // Create Cookies


////                        return RedirectToAction("Index"); 
////                    }
////                }

////                ModelState.AddModelError(string.Empty, "Invalid Credentials");
////                return RedirectToAction("Login");
////            }


////            ModelState.AddModelError(string.Empty, "Invalid Credentials");
////            return RedirectToAction("Login");
////        }
////        public async Task<IActionResult> Logout()
////        {
////            await _signInManager.SignOutAsync();
////            return RedirectToAction("Login");
////        }
////        [Authorize]
////        public IActionResult Index()
////        {
////            return View();
////        }
////        [Authorize] 
////        public IActionResult About()
////        {
////           return View();
////        }
////        //[Authorize]
////        //public IActionResult Index()
////        //{
////        //    //List<Product> products = productData.Products;
////        //    //return View(products);
////        //    if (User.Identity.IsAuthenticated)
////        //    {

////        //        //return View(specific_model);

////        //    }
////        //    else
////        //    {
////        //        return View("Login");
////        //    }

////        //}

////    }
////}

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
        private readonly ProjectDbContext _projectDbContext;  // ✅ ADD THIS

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ProjectDbContext projectDbContext)  // ✅ ADD THIS
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _projectDbContext = projectDbContext;  // ✅ ADD THIS
        }

        // ==================== GET: Register ====================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ==================== POST: Register ====================
        [HttpPost]
        [ValidateAntiForgeryToken]  // ✅ ADD THIS
        public async Task<IActionResult> Register(RegisterUserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists
                    var existingUser = await _userManager.FindByEmailAsync(userViewModel.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("", "Email already registered.");
                        return View(userViewModel);
                    }

                    // Check if username already exists
                    existingUser = await _userManager.FindByNameAsync(userViewModel.UserName);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("", "Username already taken.");
                        return View(userViewModel);
                    }

                    // Generate next UserId
                    int nextUserId = 1;
                    var lastUser = await _userManager.Users.OrderByDescending(u => u.UserId).FirstOrDefaultAsync();
                    if (lastUser != null)
                        nextUserId = lastUser.UserId + 1;

                    // Create new ApplicationUser
                    ApplicationUser applicationUser = new ApplicationUser
                    {
                        UserName = userViewModel.UserName,
                        Email = userViewModel.Email,
                        PhoneNumber = userViewModel.PhoneNumber,
                        Role = userViewModel.Role.ToString() ,
                        UserId = nextUserId,  // ✅ ADD THIS
                        Name = userViewModel.UserName ?? string.Empty,  // ✅ ADD THIS
                        CreatedAt = DateTime.UtcNow,  // ✅ CHANGED: DateTime.Now to UtcNow
                        UpdatedAt = DateTime.UtcNow   // ✅ CHANGED: DateTime.Now to UtcNow
                    };

                    IdentityResult result = await _userManager.CreateAsync(applicationUser, userViewModel.Password);

                    if (result.Succeeded)
                    {
                        TempData["Success"] = "Registration successful! Please login.";
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error during registration: {ex.Message}");
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
        [ValidateAntiForgeryToken]  // ✅ ADD THIS
        public async Task<IActionResult> Login(LoginUserViewModel loginUserViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Search user by Email or Phone
                    ApplicationUser user = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.Email == loginUserViewModel.EmailOrPhone
                                               || u.PhoneNumber == loginUserViewModel.EmailOrPhone);

                    if (user != null)
                    {
                        // Verify password
                        bool passwordValid = await _userManager.CheckPasswordAsync(user, loginUserViewModel.Password);

                        if (passwordValid)
                        {
                            // Update last login time
                            user.UpdatedAt = DateTime.UtcNow;  // ✅ CHANGED: DateTime.Now to UtcNow
                            await _userManager.UpdateAsync(user);

                            // ✅ ADD USERID CLAIM - THIS IS IMPORTANT!
                            List<Claim> claims = new List<Claim>
                            {
                                new Claim("UserId", user.UserId.ToString()),  // ✅ ADD THIS
                                new Claim("Role", user.Role),  // ✅ CHANGED: removed ToString()
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.Name, user.UserName)
                            };

                            await _signInManager.SignInWithClaimsAsync(user, isPersistent: loginUserViewModel.RememberMe, claims);

                            TempData["Success"] = $"Welcome back, {user.UserName}!";
                            return RedirectToAction("Index", "Home");  // ✅ CHANGED: Added "Home" controller
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Invalid email/phone or password.");
                    return View(loginUserViewModel);  // ✅ CHANGED: Return view instead of redirect
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error during login: {ex.Message}");
                    return View(loginUserViewModel);  // ✅ CHANGED: Return view instead of redirect
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(loginUserViewModel);  // ✅ CHANGED: Return view instead of redirect
        }

        // ==================== POST: Logout ====================
        [Authorize]
        [HttpPost]  // ✅ CHANGED: Added HttpPost
        [ValidateAntiForgeryToken]  // ✅ ADD THIS
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                TempData["Success"] = "You have been logged out successfully.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error during logout: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // ==================== GET: Dashboard ====================
        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                var userRole = User.FindFirst("Role")?.Value;

                ViewBag.UserId = userIdClaim;
                ViewBag.UserName = userName;
                ViewBag.UserRole = userRole;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading dashboard: {ex.Message}";
                return View();
            }
        }

        // ==================== GET: About ====================
        [Authorize]
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }
    }
}