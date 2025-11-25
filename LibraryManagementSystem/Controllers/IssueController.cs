using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Student,Librarian")]
    public class IssueController : Controller
    {
        private readonly LibraryContext _context;

        public IssueController(LibraryContext context)
        {
            _context = context;
        }

        // Show all issues (mostly for librarian)
        public IActionResult Index()
        {
            var issues = _context.IssueRecords.ToList();
            return View(issues);
        }

        // Issue a book
        [HttpPost]
        public IActionResult IssueBook(int bookId)
        {
            var book = _context.Books.Find(bookId);
            var user = User.Identity.Name;

            if (book == null || book.AvailableCopies <= 0)
                return BadRequest("Book not available.");

            var issue = new IssueRecord
            {
                BookId = bookId,
                StudentEmail = user,
                IssueDate = DateTime.Now,
                Status = "Issued"
            };

            // Save to DB
            _context.IssueRecords.Add(issue);

            // Decrease available copies
            book.AvailableCopies--;

            _context.SaveChanges();

            return RedirectToAction("MyIssues", "Student");
        }

        // Return Book
        [HttpPost]
        public IActionResult ReturnBook(int issueId)
        {
            var issue = _context.IssueRecords.Find(issueId);

            if (issue == null)
                return NotFound();

            issue.Status = "Returned";
            issue.ReturnDate = DateTime.Now;

            var book = _context.Books.Find(issue.BookId);
            book.AvailableCopies++;

            _context.SaveChanges();

            return RedirectToAction("MyIssues", "Student");
        }
    }
}
