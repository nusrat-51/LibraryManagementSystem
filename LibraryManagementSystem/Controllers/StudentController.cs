using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // =========================
        // DASHBOARD
        // =========================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var membership = await _context.Memberships
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.StudentEmail == email && m.IsActive);

            var namePart = email.Split('@')[0];
            var friendlyName = char.ToUpper(namePart[0]) + namePart.Substring(1);

            // applied book ids (disable Apply)
            var appliedBookIds = await _context.BookApplications
                .AsNoTracking()
                .Where(a => a.StudentEmail == email)
                .Select(a => a.BookId)
                .ToListAsync();

            var books = await _context.Books
                .AsNoTracking()
                .OrderBy(b => b.Title)
                .ToListAsync();

            // last 5 applications as "recent"
            var recent = await _context.BookApplications
                .AsNoTracking()
                .Include(a => a.Book)
                .Where(a => a.StudentEmail == email)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var vm = new StudentApplyDashboardVM
            {
                StudentId = $"STU-{namePart.ToUpper()}",
                MemberId = membership?.MembershipBarcode,
                Name = friendlyName,
                Address = "Not set",
                HasValidMemberId = membership != null && !string.IsNullOrWhiteSpace(membership.MembershipBarcode),

                CurrentIssuedCount = appliedBookIds.Count,
                OverdueCount = 0,
                TotalBooksCount = books.Count,
                TotalUnpaidFine = 0,

                Books = books.Select(b => new StudentBookVM
                {
                    BookId = b.Id,
                    Title = b.Title,
                    AvailableCopies = b.AvailableCopies,
                    IsApplied = appliedBookIds.Contains(b.Id)
                }).ToList(),

                RecentIssues = recent.Select(x => new IssueRecord
                {
                    Book = x.Book,
                    IssueDate = x.IssueDate,
                    Status = "Applied",
                    FineAmount = 0
                }).ToList()
            };

            return View(vm);
        }

        // =========================
        // MY ISSUES  -> Views/Student/MyIssues.cshtml ✅ already exists
        // =========================
        [HttpGet]
        public async Task<IActionResult> MyIssues()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var issues = await _context.IssueRecords
                .AsNoTracking()
                .Include(i => i.Book)
                .Where(i => i.StudentEmail == email)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return View(issues); // Views/Student/MyIssues.cshtml
        }

        // =========================
        // MY APPLICATIONS -> Views/Student/MyApplications.cshtml ✅ already exists
        // =========================
        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var apps = await _context.BookApplications
                .AsNoTracking()
                .Include(a => a.Book)
                .Where(a => a.StudentEmail == email)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(apps); // Views/Student/MyApplications.cshtml
        }
    }
}
