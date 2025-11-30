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

        // GET: /Student/Dashboard
        public IActionResult Dashboard()
        {
            // 1) currently logged in user-er email
            var email = User.Identity?.Name ?? "";

            // 2) sob issue record dekhi -- filter pore korbo
            var currentIssues = _context.IssueRecords
                .Include(i => i.Book)
                .Where(i => i.StudentEmail == email && i.Status == "Issued")
                .ToList();

            var overdueCount = currentIssues.Count(i =>
                !i.ReturnDate.HasValue &&
                i.IssueDate.AddDays(14) < DateTime.Today);

            var premiumSample = _context.Books
                .Where(b => b.Category == "Premium")
                .Take(3)
                .ToList();

            var vm = new StudentDashboardViewModel
            {
                StudentName = email,
                Email = email,
                MembershipType = "Normal",
                IsMembershipActive = true,
                CurrentIssuedCount = currentIssues.Count,
                OverdueCount = overdueCount,
                TotalBooksCount = _context.Books.Count(),
                TotalUnpaidFine = 0m,
                HasPremiumAccess = false,
                RecentIssues = currentIssues
                    .OrderByDescending(i => i.IssueDate)
                    .Take(5)
                    .ToList(),
                PremiumBooksSample = premiumSample
            };

            return View(vm);
        }

        // GET: /Student/MyIssues
        public IActionResult MyIssues()
        {
            var email = User.Identity?.Name ?? "";

            // Debug er jonno first step: KONO filter chara sob data niye ashi
            // pore again StudentEmail filter use korbo
            var issues = _context.IssueRecords
                .Include(i => i.Book)
                //.Where(i => i.StudentEmail == email)  // later enable
                .ToList();

            return View(issues);
        }

        // GET: /Student/ViewBooks
        public IActionResult ViewBooks()
        {
            var books = _context.Books.ToList();
            return View(books);
        }
    }
}
