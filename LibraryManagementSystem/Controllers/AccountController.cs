using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Claims;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Simple hard-coded users (for project/demo)
         
               var users = new[]
{
    new { Email = "admin@gmail.com",     Password = "admin123",   Role = "Admin" },
    new { Email = "librarian@gmail.com", Password = "lib123",     Role = "Librarian" },
    new { Email = "student@gmail.com",   Password = "student123", Role = "Student" }
};

            var user = users.FirstOrDefault(u =>
      u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase) &&
      u.Password == model.Password &&
      u.Role == model.Role);


            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username, password, or role.");
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.Email),   // <-- PUT HERE
    new Claim(ClaimTypes.Role, user.Role)
};


            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true       // remember me like behavior
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect by role
            return user.Role switch
            {
                "Admin" or "Librarian" => RedirectToAction("Dashboard", "Home"),
                "Student" => RedirectToAction("Dashboard", "Student"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        // Redirect based on ROLE
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


    }
}
