using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;

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
        // GET: /Student/Books
        public IActionResult Books(string? search, string? category)
        {
            // base list
            var books = FakeLibraryData.GetBooks();

            // distinct category list for dropdown
            ViewBag.Categories = FakeLibraryData.GetBooks()
                .Select(b => b.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // remember current filters
            ViewBag.Search = search;
            ViewBag.SelectedCategory = category;

            // filter by category
            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                books = books
                    .Where(b => b.Category == category)
                    .ToList();
            }

            // search in title or author
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                books = books
                    .Where(b =>
                        (!string.IsNullOrEmpty(b.Title) && b.Title.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(b.Author) && b.Author.ToLower().Contains(term)))
                    .ToList();
            }

            return View(books);   // Views/Student/Books.cshtml
        }


        // GET: /Student/MyIssues
        public IActionResult MyIssues()
        {
            var email = User.Identity?.Name ?? "student@example.com";
            var issues = FakeLibraryData.GetIssuesForStudent(email);
            return View(issues);         // Views/Student/MyIssues.cshtml
        }
        // GET: /Student/Profile
        public IActionResult Profile()
        {
            // Example fake student data
            var model = new StudentProfileViewModel
            {
                Name = "Test Student",
                Email = "student@example.com",
                Role = "Student"
            };

            return View(model); // will look for Views/Student/Profile.cshtml
        }


    }
}
