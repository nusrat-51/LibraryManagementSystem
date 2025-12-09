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

        // ===========================
        //  ISSUE & RETURN PAGE
        // ===========================
        public async Task<IActionResult> Index()
        {
            var vm = new IssuePageVM
            {
                // Books that have at least 1 copy available
                AvailableBooks = await _context.Books
                    .Where(b => b.AvailableCopies > 0)
                    .OrderBy(b => b.Title)
                    .ToListAsync(),

                // Active issues = not yet returned
                ActiveIssues = await _context.IssueRecords
                    .Include(i => i.Book)
                    .Where(i => i.ReturnDate == null)
                    .OrderByDescending(i => i.IssueDate)
                    .ToListAsync()
            };

            return View(vm);
        }

        // ===========================
        //  ISSUE A BOOK
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueBook(int bookId, string studentEmail)
        {
            if (string.IsNullOrWhiteSpace(studentEmail))
            {
                TempData["IssueError"] = "Student email is required.";
                return RedirectToAction(nameof(Index));
            }

            // make sure user exists (optional but nice)
            var user = await _userManager.FindByEmailAsync(studentEmail);
            if (user == null)
            {
                TempData["IssueError"] = $"No student found with email '{studentEmail}'.";
                return RedirectToAction(nameof(Index));
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["IssueError"] = "Selected book does not exist.";
                return RedirectToAction(nameof(Index));
            }

            if (book.AvailableCopies <= 0)
            {
                TempData["IssueError"] = $"No available copies for '{book.Title}'.";
                return RedirectToAction(nameof(Index));
            }

            // Create issue record
            var issue = new IssueRecord
            {
                BookId = book.Id,
                StudentEmail = studentEmail,
                IssueDate = DateTime.Now,
                Status = "Issued"
            };

            book.AvailableCopies -= 1;

            _context.IssueRecords.Add(issue);
            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            TempData["IssueMessage"] = $"Book '{book.Title}' issued to {studentEmail}.";
            return RedirectToAction(nameof(Index));
        }

        // ===========================
        //  RETURN A BOOK
        // ===========================
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

            if (issue.ReturnDate != null)
            {
                TempData["IssueError"] = "This issue is already returned.";
                return RedirectToAction(nameof(Index));
            }

            issue.ReturnDate = DateTime.Now;
            issue.Status = "Returned";

            if (issue.Book != null)
            {
                issue.Book.AvailableCopies += 1;
                _context.Books.Update(issue.Book);
            }

            // Fine calculation will be added later in Fine module
            await _context.SaveChangesAsync();

            TempData["IssueMessage"] = $"Book '{issue.Book?.Title}' returned by {issue.StudentEmail}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
