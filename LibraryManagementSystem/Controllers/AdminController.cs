using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly LibraryContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            LibraryContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // /Admin -> /Admin/Dashboard
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        // ========================
        //  ADMIN DASHBOARD
        // ========================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var totalBooks = await _context.Books.CountAsync();
            var totalIssues = await _context.IssueRecords.CountAsync();
            var totalMembers = await _context.Memberships.CountAsync();
            var totalUnpaidFine = await _context.Fines
                .Where(f => !f.IsPaid)
                .SumAsync(f => (decimal?)f.Amount) ?? 0m;

            var vm = new AdminDashboardVM
            {
                TotalBooks = totalBooks,
                TotalIssues = totalIssues,
                TotalMembers = totalMembers,
                TotalUnpaidFine = totalUnpaidFine
            };

            return View(vm);   // Views/Admin/Dashboard.cshtml
        }

        // ========================
        //  MANAGE USERS (LIST)
        // ========================
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var list = new List<AdminUserVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                bool isLocked =
                    user.LockoutEnd.HasValue &&
                    user.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow;

                list.Add(new AdminUserVM
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList(),
                    IsLocked = isLocked
                });
            }

            return View(list);   // Views/Admin/Users.cshtml
        }

        // ========================
        //  CHANGE USER ROLE
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRole(string id, string role)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(role))
            {
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // prevent admin from removing their own admin role
            if (User.Identity?.Name == user.Email && role != "Admin")
            {
                TempData["AdminMessage"] = "You cannot remove your own Admin role.";
                return RedirectToAction(nameof(Users));
            }

            // ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            TempData["AdminMessage"] = $"{user.Email} is now in role: {role}.";
            return RedirectToAction(nameof(Users));
        }

        // ========================
        //  LOCK / UNLOCK USER
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToAction(nameof(Users));

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // don't allow locking yourself
            if (User.Identity?.Name == user.Email)
            {
                TempData["AdminMessage"] = "You cannot lock your own account.";
                return RedirectToAction(nameof(Users));
            }

            bool isLocked =
                user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow;

            if (isLocked)
            {
                user.LockoutEnd = null; // unlock
                TempData["AdminMessage"] = $"{user.Email} has been unlocked.";
            }
            else
            {
                // lock for a long time (effectively deactivated)
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(50);
                TempData["AdminMessage"] = $"{user.Email} has been locked.";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Users));
        }

        // ========================
        //  MANAGE BOOKS (ADMIN -> reuse Books/Index)
        // ========================
        [HttpGet]
        public async Task<IActionResult> ManageBooks()
        {
            // Reuse your existing Books index UI
            var books = await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();

            // This assumes your old page is Views/Books/Index.cshtml
            return View("~/Views/Books/Index.cshtml", books);
        }
    }

    // ========================
    //  VIEW MODELS
    // ========================

    public class AdminDashboardVM
    {
        public int TotalBooks { get; set; }
        public int TotalIssues { get; set; }
        public int TotalMembers { get; set; }
        public decimal TotalUnpaidFine { get; set; }
    }

    public class AdminUserVM
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsLocked { get; set; }
    }
}
