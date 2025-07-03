using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignalRDrawingApp.Models;
using System.Diagnostics;

namespace SignalRDrawingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.UserName = user?.Nickname ?? "Anonymous";
                ViewBag.IsAuthenticated = true;
            }
            else
            {
                ViewBag.UserName = null;
                ViewBag.IsAuthenticated = false;
            }
            
            return View();
        }

        [HttpPost]
        public IActionResult SetUserName(string userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                HttpContext.Session.SetString("UserName", userName);
                // Also set a cookie for persistence
                Response.Cookies.Append("UserName", userName, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30),
                    HttpOnly = false, // Allow JavaScript access for SignalR
                    SameSite = SameSiteMode.Lax
                });
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserName");
            Response.Cookies.Delete("UserName");
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
