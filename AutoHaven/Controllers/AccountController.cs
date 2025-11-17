using Microsoft.AspNetCore.Mvc;

namespace AutoHaven.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
