using System;
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

        // ========================
        // STUDENT DASHBOARD (List books + show Applied status)
        // ========================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var membership = await _context.Memberships
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.StudentEmail == email && m.IsActive);

            // Friendly name from email
            var namePart = email.Split('@')[0];
            var friendlyName = char.ToUpper(namePart[0]) + namePart.Substring(1);

            // Applied BookIds by this student (disable Apply button)
            var appliedBookIds = await _context.BookApplications
                .AsNoTracking()
                .Where(a => a.StudentId == email)
                .Select(a => a.BookId)
                .Distinct()
                .ToListAsync();

            // All books list
            var books = await _context.Books
                .AsNoTracking()
                .OrderBy(b => b.Title)
                .ToListAsync();

            // Recent Applications (latest 5)
            var recentApps = await _context.BookApplications
                .AsNoTracking()
                .Include(a => a.Book)
                .Where(a => a.StudentId == email)
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

                // summary cards (simple)
                CurrentIssuedCount = 0, // application system doesn't issue directly
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

                // show applications as "RecentIssues" to avoid changing your VM
                RecentIssues = recentApps.Select(a => new IssueRecord
                {
                    Book = a.Book,
                    IssueDate = a.IssueDate,
                    Status = "Applied",
                    FineAmount = 0
                }).ToList()
            };

            return View(vm);
        }

        // ========================
        // APPLY FORM (GET) -> opens form with prefilled book + student + suggested dates
        // ========================
        [HttpGet]
        public async Task<IActionResult> Apply(int bookId)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            // Must have valid member id (your requirement)
            var membership = await _context.Memberships
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.StudentEmail == email && m.IsActive);

            if (membership == null || string.IsNullOrWhiteSpace(membership.MembershipBarcode))
            {
                TempData["Error"] = "You cannot apply for a book without a valid Member ID.";
                return RedirectToAction(nameof(Dashboard));
            }

            var book = await _context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            // prevent duplicate application
            var alreadyApplied = await _context.BookApplications
                .AsNoTracking()
                .AnyAsync(a => a.StudentId == email && a.BookId == bookId);

            if (alreadyApplied)
            {
                TempData["Error"] = "You already applied for this book.";
                return RedirectToAction(nameof(Dashboard));
            }

            var model = new BookApplication
            {
                BookId = book.Id,
                StudentId = email,                 // Normal ID
                IssueDate = DateTime.Today,        // Proposed Issue Date
                ReturnDate = DateTime.Today.AddDays(7), // Proposed Return Date
                Book = book
            };

            return View(model); // Views/Student/Apply.cshtml
        }

        // ========================
        // APPLY SUBMIT (POST) -> saves application to BookApplications table
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(BookApplication model)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            // force student id to current user (security)
            model.StudentId = email;

            // validate
            if (model.ReturnDate.Date < model.IssueDate.Date)
            {
                ModelState.AddModelError("", "Return Date cannot be before Issue Date.");
            }

            var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == model.BookId);
            if (book == null)
            {
                ModelState.AddModelError("", "Book not found.");
            }

            // prevent duplicate application
            var alreadyApplied = await _context.BookApplications
                .AsNoTracking()
                .AnyAsync(a => a.StudentId == email && a.BookId == model.BookId);

            if (alreadyApplied)
            {
                ModelState.AddModelError("", "You already applied for this book.");
            }

            if (!ModelState.IsValid)
            {
                model.Book = book;
                return View(model); // back to Apply form with errors
            }

            model.IssueDate = model.IssueDate.Date;
            model.ReturnDate = model.ReturnDate.Date;
            model.CreatedAt = DateTime.UtcNow;

            _context.BookApplications.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Application submitted for '{book!.Title}'.";
            return RedirectToAction(nameof(MyApplications));
        }

        // ========================
        // STUDENT APPLICATION HISTORY (only student sees own history here)
        // ========================
        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var apps = await _context.BookApplications
                .AsNoTracking()
                .Include(a => a.Book)
                .Where(a => a.StudentId == email)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(apps); // Views/Student/MyApplications.cshtml
        }

        // ========================
        // EDIT APPLICATION DATES (GET) - student only
        // ========================
        [HttpGet]
        public async Task<IActionResult> EditApplication(int id)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var app = await _context.BookApplications
                .Include(a => a.Book)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == email);

            if (app == null)
                return NotFound();

            return View(app); // Views/Student/EditApplication.cshtml
        }

        // ========================
        // EDIT APPLICATION DATES (POST) - student only
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditApplication(int id, DateTime issueDate, DateTime returnDate)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var app = await _context.BookApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == email);

            if (app == null)
                return NotFound();

            if (returnDate.Date < issueDate.Date)
            {
                TempData["Error"] = "Return Date cannot be before Issue Date.";
                return RedirectToAction(nameof(EditApplication), new { id });
            }

            app.IssueDate = issueDate.Date;
            app.ReturnDate = returnDate.Date;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Application dates updated.";
            return RedirectToAction(nameof(MyApplications));
        }
    }
}
