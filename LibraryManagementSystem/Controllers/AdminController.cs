using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly LibraryContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // ✅ ONLY ONE constructor
        public AdminController(
            LibraryContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // =========================
        // DASHBOARD
        // =========================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var vm = new AdminDashboardVM
            {
                TotalBooks = await _context.Books.CountAsync(),
                TotalIssues = await _context.IssueRecords.CountAsync(),
                TotalMembers = await _context.Memberships.CountAsync(m => m.IsActive),
                TotalUnpaidFine = await _context.Fines
                    .Where(f => !f.IsPaid)
                    .Select(f => (decimal?)f.Amount)
                    .SumAsync() ?? 0m
            };

            return View(vm);
        }

        // =========================
        // USERS
        // =========================
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var vm = new List<AdminUserVM>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                vm.Add(new AdminUserVM
                {
                    UserId = u.Id,
                    Email = u.Email ?? "",
                    Roles = roles.ToList(),
                    IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
                });
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRole(string id, string role)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(role))
                return RedirectToAction(nameof(Users));

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await _userManager.AddToRoleAsync(user, role);

            TempData["Success"] = $"Role updated to {role} for {user.Email}";
            return RedirectToAction(nameof(Users));
        }

        // =========================
        // BOOKS
        // =========================
        [HttpGet]
        public async Task<IActionResult> Books()
        {
            return View(await _context.Books.AsNoTracking().ToListAsync());
        }

        // =========================
        // ISSUES
        // =========================
        [HttpGet]
        public async Task<IActionResult> Issues()
        {
            var issues = await _context.IssueRecords
                .AsNoTracking()
                .Include(i => i.Book)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return View(issues);
        }

        // =========================
        // APPLICATIONS
        // =========================
        [HttpGet]
        public async Task<IActionResult> Applications()
        {
            var apps = await _context.BookApplications
                .AsNoTracking()
                .Include(a => a.Book)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(apps);
        }

        // ✅ Payment verify Admin করবে না (তুমি Librarian verify চেয়েছো)
        // Admin শুধু audit/read-only page চাইলে পরে add করবো।
    }
}
