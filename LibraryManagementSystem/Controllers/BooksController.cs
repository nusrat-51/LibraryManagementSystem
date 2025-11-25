using System.Linq;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Controllers
{
    // Role na lagate chaile [Authorize] line ta delete korte paro
    [Authorize(Roles = "Librarian,Admin")]
    public class BooksController : Controller
    {
        // ==== STEP 2: Dependency Injection of LibraryContext ====
        private readonly LibraryContext _context;

        // Framework ekhane LibraryContext automatic pathabe
        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        // GET: /Books
        public IActionResult Index()
        {
            // Database theke sob book ana
            var books = _context.Books.ToList();
            return View(books);
        }

        // GET: /Books/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book book)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }

            _context.Books.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Books/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: /Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Book book)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }

            _context.Books.Update(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Books/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: /Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
