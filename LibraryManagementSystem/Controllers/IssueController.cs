using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class IssueController : Controller
    {
        public IActionResult Index()
        {
            return View("/Views/Issue/Index.cshtml");
        }
    }
}
