using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SignalRDrawingApp.Controllers
{
    public class AdminLoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // If already authenticated as admin, redirect to admin panel
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return Redirect("/Admin/Index");
            }

            // Redirect to main login page since we're using unified authentication
            return RedirectToAction("Login", "Account", new { returnUrl = "/Admin/Index" });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Logout()
        {
            return RedirectToAction("Logout", "Account");
        }

        public static bool IsAdminAuthenticated(HttpContext httpContext)
        {
            return httpContext.User.Identity?.IsAuthenticated == true && 
                   httpContext.User.IsInRole("Admin");
        }
    }
} 