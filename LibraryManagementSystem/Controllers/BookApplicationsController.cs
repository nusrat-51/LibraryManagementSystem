using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize] // everyone logged-in can view (Student/Librarian/Admin)
    public class BookApplicationsController : Controller
    {
        private readonly LibraryContext _context;

        public BookApplicationsController(LibraryContext context)
        {
            _context = context;
        }

        // LIST for everyone
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var apps = await _context.BookApplications
                .AsNoTracking()
                .Include(a => a.Book)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(apps); // Views/BookApplications/Index.cshtml
        }

        // DELETE only Admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var app = await _context.BookApplications.FirstOrDefaultAsync(a => a.Id == id);
            if (app == null)
                return NotFound();

            _context.BookApplications.Remove(app);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Application deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
