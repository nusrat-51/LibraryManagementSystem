using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Librarian")]
    public class BooksController : Controller
    {
        public IActionResult Index()
        {
            return View("/Views/Books/Index.cshtml");
        }
    }
}
