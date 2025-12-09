using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class ReturnsController : Controller
    {
        private readonly LibraryContext _context;

        public ReturnsController(LibraryContext context)
        {
            _context = context;
        }

        // /Returns/Index  => Returns history
        public async Task<IActionResult> Index()
        {
            var returned = await _context.IssueRecords
                .Include(i => i.Book)
                .Where(i => i.ReturnDate != null)
                .OrderByDescending(i => i.ReturnDate)
                .ToListAsync();

            return View(returned);
        }
    }
}
