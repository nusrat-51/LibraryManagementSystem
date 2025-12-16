using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class BookApplicationsController : Controller
    {
        private readonly LibraryContext _context;

        public BookApplicationsController(LibraryContext context)
        {
            _context = context;
        }

        // ==========================
        // APPLY FORM (STUDENT) - GET
        // ==========================
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Apply(int bookId)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email)) return Unauthorized();

            var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null) return NotFound();

            // build "Normal ID" from email (same style you used)
            var namePart = email.Split('@')[0];
            var studentId = $"STU-{namePart.ToUpper()}";

            // already applied?
            var alreadyApplied = await _context.BookApplications
                .AsNoTracking()
                .AnyAsync(a => a.BookId == bookId && a.StudentEmail == email);

            if (alreadyApplied)
            {
                TempData["Error"] = "You already applied for this book.";
                return RedirectToAction("Dashboard", "Student");
            }

            if (book.AvailableCopies <= 0)
            {
                TempData["Error"] = "This book is out of stock.";
                return RedirectToAction("Dashboard", "Student");
            }

            // prefill dates (you can change days)
            var vm = new BookApplication
            {
                BookId = book.Id,
                StudentEmail = email,
                StudentId = studentId,
                IssueDate = DateTime.Today,
                ReturnDate = DateTime.Today.AddDays(7),
                Book = book
            };

            return View(vm);
        }

        // ==========================
        // APPLY FORM (STUDENT) - POST (SUBMIT)
        // ==========================
        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(BookApplication form)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email)) return Unauthorized();

            // force ownership
            form.StudentEmail = email;

            // load book
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == form.BookId);
            if (book == null) return NotFound();

            if (book.AvailableCopies <= 0)
            {
                TempData["Error"] = "This book is out of stock.";
                return RedirectToAction("Dashboard", "Student");
            }

            // prevent duplicates
            var alreadyApplied = await _context.BookApplications
                .AnyAsync(a => a.BookId == form.BookId && a.StudentEmail == email);

            if (alreadyApplied)
            {
                TempData["Error"] = "You already applied for this book.";
                return RedirectToAction("Dashboard", "Student");
            }

            // basic validation
            if (form.ReturnDate < form.IssueDate)
            {
                ModelState.AddModelError("", "Return Date cannot be before Issue Date.");
            }

            if (!ModelState.IsValid)
            {
                form.Book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == form.BookId);
                return View(form);
            }

            // save + decrease stock (same behavior you wanted)
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // re-check stock inside transaction
                book = await _context.Books.FirstAsync(b => b.Id == form.BookId);
                if (book.AvailableCopies <= 0)
                {
                    TempData["Error"] = "This book just went out of stock.";
                    return RedirectToAction("Dashboard", "Student");
                }

                book.AvailableCopies -= 1;

                form.CreatedAt = DateTime.UtcNow;
                _context.BookApplications.Add(form);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = $"Application submitted for '{book.Title}'.";
                return RedirectToAction("Dashboard", "Student");
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Could not submit application right now.";
                return RedirectToAction("Dashboard", "Student");
            }
        }

        // ==========================
        // HISTORY (VISIBLE TO ALL)
        // ==========================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var data = await _context.BookApplications
                .AsNoTracking()
                .Include(a => a.Book)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(data);
        }

        // ==========================
        // EDIT (ONLY STUDENT WHO APPLIED) - GET
        // Only IssueDate + ReturnDate editable
        // ==========================
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email)) return Unauthorized();

            var app = await _context.BookApplications
                .Include(a => a.Book)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound();

            if (!string.Equals(app.StudentEmail, email, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            return View(app);
        }

        // ==========================
        // EDIT (ONLY STUDENT WHO APPLIED) - POST
        // ==========================
        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DateTime issueDate, DateTime returnDate)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email)) return Unauthorized();

            var app = await _context.BookApplications.FirstOrDefaultAsync(a => a.Id == id);
            if (app == null) return NotFound();

            if (!string.Equals(app.StudentEmail, email, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            if (returnDate < issueDate)
            {
                TempData["Error"] = "Return Date cannot be before Issue Date.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            app.IssueDate = issueDate;
            app.ReturnDate = returnDate;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Application dates updated.";
            return RedirectToAction(nameof(History));
        }

        // ==========================
        // DELETE (ADMIN ONLY)
        // ==========================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var app = await _context.BookApplications.FirstOrDefaultAsync(a => a.Id == id);
            if (app == null) return NotFound();

            // return stock (optional but correct)
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == app.BookId);
            if (book != null) book.AvailableCopies += 1;

            _context.BookApplications.Remove(app);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Application deleted.";
            return RedirectToAction(nameof(History));
        }
    }
}
