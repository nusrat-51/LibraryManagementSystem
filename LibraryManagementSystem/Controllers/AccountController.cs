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
        private readonly RoleManager<IdentityRole> _roleManager;   // 👈 NEW
        private readonly LibraryContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,                 // 👈 NEW
            LibraryContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;                            // 👈 NEW
            _context = context;
        }


        // =========================
        //  LOGIN (GET)
        // =========================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // =========================
        //  LOGIN (POST)
        // =========================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // find user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // password check
            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // redirect based on real role from DB
            if (await _userManager.IsInRoleAsync(user, "Librarian"))
            {
                return RedirectToAction("Dashboard", "Librarian");
            }
            else if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                // default: student
                return RedirectToAction("Dashboard", "Student");
            }
        }

        // =========================
        //  REGISTER STUDENT (GET)
        // =========================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // =========================
        //  REGISTER STUDENT (POST)
        // =========================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // ✅ 1) Ensure Student role exists
            const string studentRole = "Student";
            if (!await _roleManager.RoleExistsAsync(studentRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(studentRole));
            }

            // ✅ 2) Put this user into Student role
            var roleResult = await _userManager.AddToRoleAsync(user, studentRole);
            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // optional: delete user if role assign failed
                return View(model);
            }

            // (optional) membership creation here...

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Dashboard", "Student");
        }

        // =========================
        //  LOGOUT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // optional AccessDenied action
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
    }
}
