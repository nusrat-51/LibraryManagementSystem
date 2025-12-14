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
        // STUDENT DASHBOARD (Apply + Stock auto update)
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

            // BookIds already issued by this student (disable Apply)
            var issuedBookIds = await _context.IssueRecords
                .AsNoTracking()
                .Where(i => i.StudentEmail == email && i.Status == "Issued")
                .Select(i => i.BookId)
                .ToListAsync();

            // All books for apply list
            var books = await _context.Books
                .AsNoTracking()
                .OrderBy(b => b.Title)
                .ToListAsync();

            // Recent issues table (latest 5)
            var recentIssues = await _context.IssueRecords
                .AsNoTracking()
                .Include(i => i.Book)
                .Where(i => i.StudentEmail == email)
                .OrderByDescending(i => i.IssueDate)
                .Take(5)
                .ToListAsync();

            var vm = new StudentApplyDashboardVM
            {
                StudentId = $"STU-{namePart.ToUpper()}",
                MemberId = membership?.MembershipBarcode,
                Name = friendlyName,
                Address = "Not set",
                HasValidMemberId = membership != null && !string.IsNullOrWhiteSpace(membership.MembershipBarcode),

                CurrentIssuedCount = issuedBookIds.Count,
                OverdueCount = 0,
                TotalBooksCount = books.Count,
                TotalUnpaidFine = 0,

                Books = books.Select(b => new StudentBookVM
                {
                    BookId = b.Id,
                    Title = b.Title,
                    AvailableCopies = b.AvailableCopies,
                    IsApplied = issuedBookIds.Contains(b.Id)
                }).ToList(),

                RecentIssues = recentIssues
            };

            return View(vm);
        }

        // ========================
        // APPLY (decrease stock by 1)
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int bookId)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var membership = await _context.Memberships
                .FirstOrDefaultAsync(m => m.StudentEmail == email && m.IsActive);

            // Requirement: block apply if no valid MemberId
            if (membership == null || string.IsNullOrWhiteSpace(membership.MembershipBarcode))
            {
                TempData["Error"] = "You cannot apply for a book without a valid Member ID.";
                return RedirectToAction(nameof(Dashboard));
            }

            // Prevent duplicate apply
            var alreadyIssued = await _context.IssueRecords.AnyAsync(i =>
                i.StudentEmail == email &&
                i.BookId == bookId &&
                i.Status == "Issued");

            if (alreadyIssued)
            {
                TempData["Error"] = "You already applied for this book.";
                return RedirectToAction(nameof(Dashboard));
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // Lock and re-check stock inside transaction
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
                if (book == null)
                {
                    TempData["Error"] = "Book not found.";
                    return RedirectToAction(nameof(Dashboard));
                }

                if (book.AvailableCopies <= 0)
                {
                    TempData["Error"] = "This book is out of stock.";
                    return RedirectToAction(nameof(Dashboard));
                }

                book.AvailableCopies -= 1;

                var issue = new IssueRecord
                {
                    BookId = bookId,
                    StudentEmail = email,
                    IssueDate = DateTime.UtcNow,
                    Status = "Issued",
                    FineAmount = 0
                };

                _context.IssueRecords.Add(issue);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                TempData["Success"] = $"Applied successfully for '{book.Title}'.";
                return RedirectToAction(nameof(Dashboard));
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Could not apply right now. Please try again.";
                return RedirectToAction(nameof(Dashboard));
            }
        }
    }
}
