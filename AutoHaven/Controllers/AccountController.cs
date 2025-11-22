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

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
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
                return RedirectToAction("Login");
            }

            
            ModelState.AddModelError(string.Empty, "Invalid Credentials");
            return RedirectToAction("Login");
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
        public IActionResult About()
        {
           return View();
        }
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