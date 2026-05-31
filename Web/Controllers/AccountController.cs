using Application.Common.Interface;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
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
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.LoginAsync(new LoginDto
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = model.RememberMe
            });

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Login failed.");
                return View(model);
            }

            HttpContext.Session.SetString("UserFullName", $"{result.User!.FirstName} {result.User.LastName}");
            HttpContext.Session.SetString("UserEmail", result.User.Email ?? string.Empty);
            HttpContext.Session.SetString("UserRole", result.Role ?? "User");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Student");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.RegisterAsync(new RegisterDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role
            });

            if (!result.Succeeded)
            {
                var field = result.ErrorField ?? string.Empty;
                ModelState.AddModelError(field, result.ErrorMessage ?? "Registration failed.");
                return View(model);
            }

            HttpContext.Session.SetString("UserFullName", $"{result.User!.FirstName} {result.User.LastName}");
            HttpContext.Session.SetString("UserEmail", result.User.Email ?? string.Empty);
            HttpContext.Session.SetString("UserRole", result.Role ?? "User");

            return RedirectToAction("Index", "Student");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
