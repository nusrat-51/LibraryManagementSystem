using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.ViewModels.Admin;
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

        // =========================
        // DASHBOARD
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var vm = new ViewModels.Admin.AdminDashboardVM
            {
                TotalBooks = await _context.Books.CountAsync(),
                TotalIssues = await _context.IssueRecords.CountAsync(),
                TotalMembers = await _context.Memberships.CountAsync(m => m.IsActive),
                TotalUnpaidFine = await _context.Fines
                    .Where(f => !f.IsPaid)
                    .Select(f => (decimal?)f.Amount)
                    .SumAsync() ?? 0m
            };

            return View(vm);
        }


        // =========================
        // BOOK LIST (Admin can view + create + edit + delete)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Books()
        {
            var books = await _context.Books.AsNoTracking().ToListAsync();
            return View(books);
        }

        // CREATE (GET)
        [HttpGet]
        public IActionResult CreateBook()
        {
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(Book book)
        {
            if (!ModelState.IsValid) return View(book);

            if (book.TotalCopies <= 0) book.TotalCopies = book.AvailableCopies;
            if (book.AvailableCopies <= 0) book.AvailableCopies = book.TotalCopies;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Book created successfully!";
            return RedirectToAction(nameof(Books));
        }

        // EDIT (GET)
        [HttpGet]
        public async Task<IActionResult> EditBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(int id, Book book)
        {
            if (id != book.Id) return BadRequest();
            if (!ModelState.IsValid) return View(book);

            var dbBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (dbBook == null) return NotFound();

            dbBook.Title = book.Title;
            dbBook.Author = book.Author;
            dbBook.Category = book.Category;
            dbBook.TotalCopies = book.TotalCopies;

            dbBook.AvailableCopies = book.AvailableCopies;
            if (dbBook.AvailableCopies > dbBook.TotalCopies)
                dbBook.AvailableCopies = dbBook.TotalCopies;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Book updated successfully!";
            return RedirectToAction(nameof(Books));
        }

        // DELETE (GET confirm)
        [HttpGet]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();
            return View(book);
        }

        // DELETE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBookConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Book deleted successfully!";
            return RedirectToAction(nameof(Books));
        }

        // =========================
        // ISSUE LIST (Admin sees all current/past issues)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Issues()
        {
            var issues = await _context.IssueRecords
                .AsNoTracking()
                .Include(i => i.Book)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return View(issues);
        }

        // =========================
        // APPLICATION LIST (Admin sees all applications)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Applications()
        {
            var apps = await _context.BookApplications
                .AsNoTracking()
                .Include(a => a.Book)
                .OrderByDescending(a => a.IssueDate)
                .ToListAsync();

            return View(apps);
        }

        // DELETE APPLICATION (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var app = await _context.BookApplications.FindAsync(id);
            if (app == null) return NotFound();

            _context.BookApplications.Remove(app);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Application deleted successfully!";
            return RedirectToAction(nameof(Applications));
        }
        //reservation
        [HttpGet]
        public async Task<IActionResult> Reservations()
        {
            var list = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Book)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

           
            return View(list);
        }

    }
}
