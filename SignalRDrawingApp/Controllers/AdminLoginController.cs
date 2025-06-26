using Microsoft.AspNetCore.Mvc;

namespace SignalRDrawingApp.Controllers
{
    public class AdminLoginController : Controller
    {
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";
        private const string AdminSessionKey = "IsAdminAuthenticated";

        [HttpGet]
        public IActionResult Index()
        {
            // If already authenticated, redirect to admin
            if (IsAdminAuthenticated())
            {
                return Redirect("/Admin/Index");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == AdminUsername && password == AdminPassword)
            {
                // Set admin session
                HttpContext.Session.SetString(AdminSessionKey, "true");
                
                // Direct redirect to admin dashboard
                return Redirect("/Admin/Index");
            }

            ViewBag.Error = "Invalid username or password";
            return View("Index");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(AdminSessionKey);
            return RedirectToAction("Index", "Home");
        }

        public static bool IsAdminAuthenticated(HttpContext httpContext)
        {
            return httpContext.Session.GetString(AdminSessionKey) == "true";
        }

        private bool IsAdminAuthenticated()
        {
            return IsAdminAuthenticated(HttpContext);
        }
    }
} 