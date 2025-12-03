using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Librarian,Admin")]
    public class LibrarianController : Controller
    {
        private readonly LibraryContext _context;

        public LibrarianController(LibraryContext context)
        {
            _context = context;
        }

        // ===========================
        //  DASHBOARD (VIEW ONLY)
        // ===========================
        public IActionResult Dashboard()
        {
            // Right now just returns the Librarian/Dashboard.cshtml view
            // Later we can add stats (total books, overdue, etc.) from _context
            return View();
        }

        // ===========================
        //  BOOK LIST PAGE
        // ===========================
        public async Task<IActionResult> ManageBooks()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        // ===========================
        //  CREATE BOOK (GET)
        // ===========================
        public IActionResult CreateBook()
        {
            return View();
        }

        // ===========================
        //  CREATE BOOK (POST)
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(Book model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // AvailableCopies = TotalCopies initially
            model.AvailableCopies = model.TotalCopies;

            _context.Books.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Book added successfully!";
            return RedirectToAction(nameof(ManageBooks));
        }

        // ===========================
        //  EDIT BOOK (GET)
        // ===========================
        public async Task<IActionResult> EditBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        // ===========================
        //  EDIT BOOK (POST)
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(Book model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var book = await _context.Books.FindAsync(model.Id);
            if (book == null) return NotFound();

            // Update fields
            book.Title = model.Title;
            book.Author = model.Author;
            book.Category = model.Category;
            book.TotalCopies = model.TotalCopies;

            // For now, reset AvailableCopies = TotalCopies
            book.AvailableCopies = model.TotalCopies;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Book updated successfully!";
            return RedirectToAction(nameof(ManageBooks));
        }

        // ===========================
        //  DELETE BOOK (GET)
        // ===========================
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        // ===========================
        //  DELETE BOOK (POST)
        // ===========================
        [HttpPost, ActionName("DeleteBook")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Book deleted successfully!";
            return RedirectToAction(nameof(ManageBooks));
        }

        // ===========================
        //  BOOK DETAILS
        // ===========================
        public async Task<IActionResult> BookDetails(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }
    }
}
