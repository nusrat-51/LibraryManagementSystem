using System;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly LibraryContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            LibraryContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // =========================
        // LOGIN (GET)
        // =========================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // =========================
        // LOGIN (POST)
        // =========================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null ||
                !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.SelectedRole))
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains(model.SelectedRole))
                {
                    ModelState.AddModelError(
                        "", $"You are not registered as {model.SelectedRole}.");
                    return View(model);
                }
            }

            await _signInManager.SignOutAsync();
            await _signInManager.PasswordSignInAsync(
                user, model.Password, false, false);

            return model.SelectedRole switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Librarian" => RedirectToAction("Dashboard", "Librarian"),
                _ => RedirectToAction("Dashboard", "Student")
            };
        }

        // =========================
        // REGISTER (GET)
        // =========================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // =========================
        // REGISTER (POST) ✅ FIXED
        // =========================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the errors and try again.";
                return View(model);
            }

            // prevent duplicate email
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("", "Email is already registered.");
                return View(model);
            }

            var studentId = "STU-" + Guid.NewGuid().ToString("N")[..6].ToUpper();
            var memberId = "MID-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                StudentId = studentId,
                MemberId = memberId,
                Name = model.FullName,
                Address = model.Address
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);

                TempData["Error"] = "Registration failed.";
                return View(model);
            }

            // Ensure Student role
            if (!await _roleManager.RoleExistsAsync("Student"))
                await _roleManager.CreateAsync(new IdentityRole("Student"));

            await _userManager.AddToRoleAsync(user, "Student");

            // Create membership
            _context.Memberships.Add(new Membership
            {
                UserId = user.Id,
                StudentEmail = user.Email!,
                MembershipType = model.MembershipType,
                IsActive = true,
                MembershipBarcode = memberId,
                StartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1)
            });

            await _context.SaveChangesAsync();

            // ✅ SUCCESS MESSAGE + redirect
            TempData["Success"] =
                "Registration successful! Please login to continue.";

            return RedirectToAction("Login", "Account");
        }

        // =========================
        // LOGOUT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
