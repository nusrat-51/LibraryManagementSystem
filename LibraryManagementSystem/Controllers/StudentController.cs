using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly LibraryContext _context;

        public StudentController(LibraryContext context)
        {
            _context = context;
        }

        // ========================
        //  STUDENT DASHBOARD
        // ========================
        public IActionResult Dashboard()
        {
            var email = User.Identity?.Name ?? string.Empty;

            // Student-er active issues (not returned)
            var currentIssues = _context.IssueRecords
                .Include(i => i.Book)
                .Where(i => i.StudentEmail == email && i.Status == "Issued")
                .ToList();

            // overdue: 14 din er beshi (example)
            var overdueCount = currentIssues.Count(i =>
                !i.ReturnDate.HasValue &&
                i.IssueDate.AddDays(14) < DateTime.Today);

            // premium collection preview (just sample)
            var premiumSample = _context.Books
                .Where(b => b.Category == "Premium")
                .Take(3)
                .ToList();

            // total unpaid fine (Fine table theke)
            var totalUnpaidFine = _context.Fines
                .Where(f => f.StudentEmail == email && !f.IsPaid)
                .Sum(f => (decimal?)f.Amount) ?? 0m;

            var vm = new StudentDashboardViewModel
            {
                StudentName = email,
                Email = email,

                // membership info (later DB theke asbe)
                MembershipType = "Normal",
                IsMembershipActive = true,
                HasPremiumAccess = false,

                // summary cards
                CurrentIssuedCount = currentIssues.Count,
                OverdueCount = overdueCount,
                TotalBooksCount = _context.Books.Count(),
                TotalUnpaidFine = totalUnpaidFine,

                // tables
                RecentIssues = currentIssues
                    .OrderByDescending(i => i.IssueDate)
                    .Take(5)
                    .ToList(),

                PremiumBooksSample = premiumSample
            };

            return View(vm);
        }

        // ========================
        //  MY ISSUED BOOKS
        // ========================
        // GET: /Student/MyIssues
        public IActionResult MyIssues()
        {
            var email = User.Identity?.Name ?? string.Empty;

            var issues = _context.IssueRecords
                .Include(i => i.Book)
                .Where(i => i.StudentEmail == email)
                .OrderByDescending(i => i.IssueDate)
                .ToList();

            return View(issues);
        }

        // ========================
        //  VIEW BOOKS (READ-ONLY)
        // ========================
        // GET: /Student/ViewBooks
        public IActionResult ViewBooks()
        {
            var books = _context.Books
                .OrderBy(b => b.Title)
                .ToList();

            return View(books);
        }

        // =========================
        //  PAY FINE PAGE (GET)
        // =========================
        public IActionResult PayFine()
        {
            var email = User.Identity?.Name ?? string.Empty;

            var unpaid = _context.Fines
                .Include(f => f.IssueRecord)
                    .ThenInclude(ir => ir.Book)
                .Where(f => f.StudentEmail == email && !f.IsPaid)
                .ToList();

            var paid = _context.Fines
                .Where(f => f.StudentEmail == email && f.IsPaid)
                .OrderByDescending(f => f.PaidOn)
                .Take(10)
                .ToList();

            var vm = new PayFineViewModel
            {
                UnpaidFines = unpaid,
                PaidHistory = paid,
                TotalUnpaid = unpaid.Sum(f => f.Amount)
            };

            return View(vm);
        }

        // =========================
        //  PAY FINE ONLINE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PayFineOnline(int id)
        {
            var email = User.Identity?.Name ?? string.Empty;

            var fine = _context.Fines
                .FirstOrDefault(f => f.Id == id && f.StudentEmail == email && !f.IsPaid);

            if (fine == null)
            {
                return NotFound();
            }

            // ---- Mock online payment gateway ----
            fine.IsPaid = true;
            fine.PaidOn = DateTime.Now;
            fine.PaymentReference = "PAY-" + Guid.NewGuid().ToString("N").Substring(0, 8);

            _context.SaveChanges();

            TempData["FinePaidMessage"] =
                $"Fine #{fine.Id} paid successfully (Ref: {fine.PaymentReference}).";

            return RedirectToAction(nameof(PayFine));
        }

        // =========================
        //  PREMIUM COLLECTION PAGE
        // =========================
        public IActionResult PremiumCollection()
        {
            var premiumBooks = _context.Books
                .Where(b => b.Category == "Premium")
                .ToList();

            return View(premiumBooks);
        }
    }
}
