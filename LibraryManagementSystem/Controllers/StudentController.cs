using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;   // <-- IMPORTANT

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        // GET: /Student/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: /Student/Books
        public IActionResult Books()
        {
            var books = FakeLibraryData.GetBooks();
            return View(books);
        }

        // GET: /Student/MyIssues
        public IActionResult MyIssues()
        {
            var email = User.Identity?.Name ?? "student@example.com"; // fallback for now
            var issues = FakeLibraryData.GetIssuesForStudent(email);
            return View(issues);
        }
    }
}
