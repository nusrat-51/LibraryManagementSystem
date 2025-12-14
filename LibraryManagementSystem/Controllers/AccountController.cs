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
        // Use ApplicationUser everywhere
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

            // 1) Find user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // 2) Check password first (without signing in yet)
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // 3) Check that selected role matches user's roles
            //    (SelectedRole comes from the "Login as" dropdown)
            if (!string.IsNullOrWhiteSpace(model.SelectedRole))
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains(model.SelectedRole))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"You are not registered as {model.SelectedRole}.");

                    return View(model);
                }
            }

            // 4) Actually sign in
            await _signInManager.SignOutAsync();
            var signInResult = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // 5) Redirect based on selected (now validated) role
            switch (model.SelectedRole)
            {
                case "Admin":
                    return RedirectToAction("Dashboard", "Admin");

                case "Librarian":
                    return RedirectToAction("Dashboard", "Librarian");

                case "Student":
                default:
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
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ✅ Generate StudentId + MemberId
            var studentId = "STU-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var memberId = "MID-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,

                // ✅ new fields
                StudentId = studentId,
                MemberId = memberId,
                Name = model.FullName,
                Address = model.Address
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            // Ensure Student role exists
            const string studentRole = "Student";
            if (!await _roleManager.RoleExistsAsync(studentRole))
                await _roleManager.CreateAsync(new IdentityRole(studentRole));

            await _userManager.AddToRoleAsync(user, studentRole);

            // ✅ Create membership record too (so MemberId exists)
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

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
