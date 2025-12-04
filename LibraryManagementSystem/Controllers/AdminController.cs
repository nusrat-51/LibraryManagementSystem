using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly LibraryContext _context;

        public AdminController(LibraryContext context)
        {
            _context = context;
        }

        // Redirect /Admin and /Admin/Index to Dashboard
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

            // Uses Views/Admin/Dashboard.cshtml
            return View(vm);
        }

        // ========================
        //  USERS PAGE (simple stub)
        // ========================
        [HttpGet]
        public IActionResult Users()
        {
            // For now return an empty list so Views/Admin/Users.cshtml compiles
            var users = new List<AdminUserVM>();
            return View(users);
        }
    }

    // Simple view-model for admin dashboard
    public class AdminDashboardVM
    {
        public int TotalBooks { get; set; }
        public int TotalIssues { get; set; }
        public int TotalMembers { get; set; }
        public decimal TotalUnpaidFine { get; set; }
    }

    // View-model used by Views/Admin/Users.cshtml
    public class AdminUserVM
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
    }
}
