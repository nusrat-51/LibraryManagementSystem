using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Librarian,Admin")]
public class LibrarianController : Controller
{
    private readonly LibraryContext _context;

    public LibrarianController(LibraryContext context)
    {
        _context = context;
    }

    // ========== BOOK LIST (MANAGE) ==========
    // GET: /Librarian/ManageBooks
    public async Task<IActionResult> ManageBooks()
    {
        var books = await _context.Books
            .OrderBy(b => b.Title)
            .ToListAsync();

        return View(books);
    }

    // ========== CREATE BOOK ==========

    // GET: /Librarian/CreateBook
    [HttpGet]
    public IActionResult CreateBook()
    {
        return View();
    }

    // POST: /Librarian/CreateBook
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBook(Book model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // if AvailableCopies field exists, default it to TotalCopies
        try
        {
            var prop = typeof(Book).GetProperty("AvailableCopies");
            if (prop != null)
            {
                var value = prop.GetValue(model);
                if (value is int intVal && intVal == 0)
                {
                    prop.SetValue(model, model.TotalCopies);
                }
            }
        }
        catch { }

        _context.Books.Add(model);
        await _context.SaveChangesAsync();

        TempData["BookMessage"] = "Book added successfully.";
        return RedirectToAction(nameof(ManageBooks));
    }

    // ========== EDIT BOOK ==========

    // GET: /Librarian/EditBook/5
    [HttpGet]
    public async Task<IActionResult> EditBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound();

        return View(book);
    }

    // POST: /Librarian/EditBook/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBook(int id, Book model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        _context.Entry(model).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["BookMessage"] = "Book updated successfully.";
        return RedirectToAction(nameof(ManageBooks));
    }

    // ========== DELETE BOOK ==========

    // GET: /Librarian/DeleteBook/5  (confirmation)
    [HttpGet]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound();

        return View(book);
    }

    // POST: /Librarian/DeleteBookConfirmed/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBookConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        TempData["BookMessage"] = "Book deleted successfully.";
        return RedirectToAction(nameof(ManageBooks));
    }
}
