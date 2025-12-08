using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.ViewModels.Librarian;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class IssueController : Controller
    {
        private readonly LibraryContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IssueController(LibraryContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ========================
        //  ISSUE DASHBOARD
        // ========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new IssuePageVM
            {
                AvailableBooks = await _context.Books
                    .Where(b => b.AvailableCopies > 0)
                    .OrderBy(b => b.Title)
                    .ToListAsync(),

                ActiveIssues = await _context.IssueRecords
                    .Include(i => i.Book)
                    .Where(i => i.Status == "Issued")
                    .OrderByDescending(i => i.IssueDate)
                    .ToListAsync()
            };

            return View(vm); // Views/Issue/Index.cshtml
        }

        // ========================
        //  ISSUE A BOOK
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueBook(int bookId, string studentEmail)
        {
            studentEmail = studentEmail?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(studentEmail))
            {
                TempData["IssueError"] = "Student email is required.";
                return RedirectToAction(nameof(Index));
            }

            // 1) Check book exists and is available
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["IssueError"] = "Selected book does not exist.";
                return RedirectToAction(nameof(Index));
            }

            if (book.AvailableCopies <= 0)
            {
                TempData["IssueError"] = "No available copies for this book.";
                return RedirectToAction(nameof(Index));
            }

            // 2) Check that user exists and is in Student role
            var student = await _userManager.FindByEmailAsync(studentEmail);
            if (student == null || !(await _userManager.IsInRoleAsync(student, "Student")))
            {
                TempData["IssueError"] = "No registered student with that email.";
                return RedirectToAction(nameof(Index));
            }

            // 3) Create IssueRecord
            var issue = new IssueRecord
            {
                BookId = book.Id,
                StudentEmail = studentEmail,
                IssueDate = DateTime.UtcNow,
                ReturnDate = null,
                Status = "Issued",
                FineAmount = 0m
            };

            book.AvailableCopies -= 1;

            _context.IssueRecords.Add(issue);
            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            TempData["IssueMessage"] = $"Book '{book.Title}' issued to {studentEmail}.";
            return RedirectToAction(nameof(Index));
        }

        // ========================
        //  RETURN A BOOK
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int issueId)
        {
            var issue = await _context.IssueRecords
                .Include(i => i.Book)
                .FirstOrDefaultAsync(i => i.Id == issueId);

            if (issue == null)
            {
                TempData["IssueError"] = "Issue record not found.";
                return RedirectToAction(nameof(Index));
            }

            if (issue.Status == "Returned")
            {
                TempData["IssueError"] = "This book is already returned.";
                return RedirectToAction(nameof(Index));
            }

            // Mark as returned
            issue.ReturnDate = DateTime.UtcNow;
            issue.Status = "Returned";

            // Increase available copies
            if (issue.Book != null)
            {
                issue.Book.AvailableCopies += 1;
                _context.Books.Update(issue.Book);
            }

            // (Fine calculation will be added later in Fine module)
            await _context.SaveChangesAsync();

            TempData["IssueMessage"] = $"Book '{issue.Book?.Title}' returned by {issue.StudentEmail}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
