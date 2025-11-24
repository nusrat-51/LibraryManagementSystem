using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    // Entire controller requires login
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Redirect root ? Dashboard
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // Only Admin + Librarian can see dashboard
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Default error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
