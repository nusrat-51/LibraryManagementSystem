using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly LibraryContext _context;

        public StudentController(LibraryContext context)
        {
            _context = context;
        }

        // Student Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // ALL available books list for student
        public IActionResult Books()
        {
            var books = _context.Books.ToList();
            return View(books);
        }

        // Student's own issued books list
        public IActionResult MyIssues()
        {
            var user = User.Identity.Name;

            var issues = _context.IssueRecords
                .Where(i => i.StudentEmail == user)
                .ToList();

            return View(issues);
        }
    }
}
