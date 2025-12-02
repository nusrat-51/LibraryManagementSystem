using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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

        // ========================
        //  STUDENT DASHBOARD
        // ========================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            // Friendly name from email
            var userNamePart = email.Split('@')[0];
            var friendlyName = char.ToUpper(userNamePart[0]) + userNamePart.Substring(1);

            // 1) Load membership from DB for this student
            var membership = await _context.Memberships
                .FirstOrDefaultAsync(m => m.StudentEmail == email && m.IsActive);

            bool isPremium = membership?.MembershipType == "Premium";

            var today = DateTime.UtcNow.Date;

            // All issue records for this student
            var studentIssuesQuery = _context.IssueRecords
                .Include(i => i.Book)
                .Where(i => i.StudentEmail == email);

            // Current issued (not returned)
            var currentIssues = await studentIssuesQuery
                .Where(i => i.Status == "Issued")
                .ToListAsync();

            // Overdue: more than 14 days and not returned
            var overdueCount = currentIssues.Count(i =>
                !i.ReturnDate.HasValue &&
                i.IssueDate.AddDays(14).Date < today);

            // Total books in library
            var totalBooksCount = await _context.Books.CountAsync();

            // Total unpaid fine for this student
            var totalUnpaidFine = await _context.Fines
                .Where(f => f.StudentEmail == email && !f.IsPaid)
                .SumAsync(f => (decimal?)f.Amount) ?? 0m;

            // Recent issues (latest 5)
            var recentIssues = await studentIssuesQuery
                .OrderByDescending(i => i.IssueDate)
                .Take(5)
                .ToListAsync();

            // Premium books sample (first 3 premium category)
            var premiumBooksSample = await _context.Books
                .Where(b => b.Category == "Premium")
                .OrderBy(b => b.Title)
                .Take(3)
                .ToListAsync();

            var vm = new StudentDashboardViewModel
            {
                // Identity info
                StudentName = friendlyName,
                Email = email,

                // Membership info from DB (with safe defaults)
                MembershipType = membership?.MembershipType ?? "Standard",
                IsMembershipActive = membership?.IsActive ?? false,
                HasPremiumAccess = isPremium,
                MembershipBarcode = membership?.MembershipBarcode ?? "Not assigned",
                MembershipExpiry = membership?.ExpiryDate,

                // Summary cards
                CurrentIssuedCount = currentIssues.Count,
                OverdueCount = overdueCount,
                TotalBooksCount = totalBooksCount,
                TotalUnpaidFine = totalUnpaidFine,

                // Tables
                RecentIssues = recentIssues,
                PremiumBooksSample = premiumBooksSample
            };

            return View(vm);
        }

        // ========================
        //  MY ISSUED BOOKS
        // ========================
        [HttpGet]
        public async Task<IActionResult> MyIssues()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var issues = await _context.IssueRecords
                .Include(i => i.Book)
                .Where(i => i.StudentEmail == email)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return View(issues);
        }

        // ========================
        //  VIEW BOOKS (READ-ONLY)
        // ========================
        [HttpGet]
        public async Task<IActionResult> ViewBooks()
        {
            var books = await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View(books);
        }

        // =========================
        //  PAY FINE PAGE (GET)
        // =========================
        [HttpGet]
        public async Task<IActionResult> PayFine()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var unpaid = await _context.Fines
                .Include(f => f.IssueRecord)
                    .ThenInclude(ir => ir.Book)
                .Where(f => f.StudentEmail == email && !f.IsPaid)
                .ToListAsync();

            var paid = await _context.Fines
                .Where(f => f.StudentEmail == email && f.IsPaid)
                .OrderByDescending(f => f.PaidOn)
                .Take(10)
                .ToListAsync();

            var vm = new PayFineViewModel
            {
                UnpaidFines = unpaid,
                PaidHistory = paid,
                TotalUnpaid = unpaid.Sum(f => f.Amount)
            };

            return View(vm);
        }

        // =========================
        //  PAY FINE ONLINE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayFineOnline(int id)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var fine = await _context.Fines
                .FirstOrDefaultAsync(f => f.Id == id &&
                                          f.StudentEmail == email &&
                                          !f.IsPaid);

            if (fine == null)
            {
                return NotFound();
            }

            // ---- Mock online payment gateway ----
            fine.IsPaid = true;
            fine.PaidOn = DateTime.UtcNow;
            fine.PaymentReference = "PAY-" + Guid.NewGuid().ToString("N").Substring(0, 8);

            await _context.SaveChangesAsync();

            TempData["FinePaidMessage"] =
                $"Fine #{fine.Id} paid successfully (Ref: {fine.PaymentReference}).";

            return RedirectToAction(nameof(PayFine));
        }

        // =========================
        //  PREMIUM COLLECTION PAGE
        // =========================
        [HttpGet]
        public async Task<IActionResult> PremiumCollection()
        {
            var premiumBooks = await _context.Books
                .Where(b => b.Category == "Premium")
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View(premiumBooks);
        }
        // =========================
        //  UPGRADE MEMBERSHIP (MOCK)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpgradeToPremium()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            // Find existing active membership (if any)
            var membership = await _context.Memberships
                .FirstOrDefaultAsync(m => m.StudentEmail == email);

            if (membership == null)
            {
                // Create a new membership row if it doesn't exist
                membership = new Membership
                {
                    StudentEmail = email,
                    MembershipType = "Premium",
                    IsActive = true,
                    MembershipBarcode = GenerateMembershipBarcode(),
                    ExpiryDate = DateTime.Today.AddYears(1),   // 1 year validity
                   
                };

                _context.Memberships.Add(membership);
            }
            else
            {
                // Upgrade existing membership
                membership.MembershipType = "Premium";
                membership.IsActive = true;
                membership.MembershipBarcode = GenerateMembershipBarcode();
                membership.ExpiryDate = DateTime.Today.AddYears(1);
            }

            await _context.SaveChangesAsync();

            TempData["MembershipMessage"] = "Your membership has been upgraded to Premium.";
            return RedirectToAction(nameof(Dashboard));
        }

        // Simple helper to generate a fake barcode/id
        private string GenerateMembershipBarcode()
        {
            // Example: LM-2025-123456
            var random = new Random();
            var number = random.Next(100000, 999999);
            return $"LM-{DateTime.Now:yyyy}-{number}";
        }

    }
}
