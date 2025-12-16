using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.ViewModels.Librarian;
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
        //  DASHBOARD
        // ===========================
        public async Task<IActionResult> Dashboard()
        {
            var model = new LibrarianDashboardViewModel
            {
                TotalBooks = await _context.Books.CountAsync(),

                IssuedToday = 0,
                OverdueCount = 0,
                ActiveReservations = 0,

                StudentsWithUnpaidFines = 0,
                TotalUnpaidFine = 0m,
                LastFineCollected = 0m
            };

            return View(model);
        }

        // ===========================
        //  ISSUES MENU (SIDEBAR)
        //  /Librarian/Issues  ->  /Issue/Index
        // ===========================
        // ===========================
        //  ISSUES & RETURNS MENU LINKS
        // ===========================

        // /Librarian/Issues  ->  /Issue/Index
        public IActionResult Issues()
        {
            return RedirectToAction("Index", "Issue");
        }

        // /Librarian/Returns ->  /Returns/Index
        public IActionResult Returns()
        {
            return RedirectToAction("Index", "Returns");
        }
        // /Librarian/Reservations  ->  /Reservations/Index
        public IActionResult Reservations()
        {
            return RedirectToAction("Index", "Reservations");
        }


        // ===========================
        //  MANAGE BOOKS (LIST)
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

            model.AvailableCopies = model.TotalCopies;

            _context.Books.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Book added successfully!";
            return RedirectToAction(nameof(ManageBooks));
        }
        [HttpGet]
        public IActionResult Create()
        {
            return RedirectToAction(nameof(CreateBook));
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

            book.Title = model.Title;
            book.Author = model.Author;
            book.Category = model.Category;
            book.TotalCopies = model.TotalCopies;
            book.AvailableCopies = model.AvailableCopies;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Book updated successfully!";
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
        public async Task<IActionResult> DeleteBookConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Book deleted successfully!";
            return RedirectToAction(nameof(ManageBooks));
        }
    }
}
