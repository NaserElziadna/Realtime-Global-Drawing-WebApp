using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignalRDrawingApp.Models;
using SignalRDrawingApp.ViewModels;

namespace SignalRDrawingApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                // Debug: Check if user exists
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found with email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }
                
                // Debug: Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("User account is inactive for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Account is inactive.");
                    return View(model);
                }
                
                // Debug: Log sign-in attempt
                _logger.LogInformation("Attempting sign-in for user: {Email}, UserName: {UserName}", model.Email, user.UserName);
                
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                
                if (result.Succeeded)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    
                    _logger.LogInformation("User logged in with email: {Email}", model.Email);
                    return RedirectToLocal(returnUrl);
                }
                
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Account locked out.");
                }
                else if (result.IsNotAllowed)
                {
                    _logger.LogWarning("User sign-in not allowed for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Sign-in not allowed.");
                }
                else if (result.RequiresTwoFactor)
                {
                    _logger.LogWarning("User requires two-factor authentication for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Two-factor authentication required.");
                }
                else
                {
                    _logger.LogWarning("Invalid password for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                }
            }
            else
            {
                _logger.LogWarning("Model state invalid for login attempt with email: {Email}", model.Email);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nickname = model.Nickname
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with email: {Email}", model.Email);
                    
                    // Add user to "User" role by default
                    await _userManager.AddToRoleAsync(user, "User");
                    
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl);
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Email = user.Email!,
                Nickname = user.Nickname,
                PreferredColor = user.PreferredColor ?? "#007bff"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.Nickname = model.Nickname;
            user.PreferredColor = model.PreferredColor;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                ViewData["StatusMessage"] = "Your profile has been updated";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
} 