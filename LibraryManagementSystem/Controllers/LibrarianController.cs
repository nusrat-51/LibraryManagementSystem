using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.ViewModels.Librarian;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class LibrarianController : Controller
    {
        private readonly LibraryContext _context;

        public LibrarianController(LibraryContext context)
        {
            _context = context;
        }

        // =========================
        // DASHBOARD
        // =========================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var vm = new LibrarianDashboardViewModel
            {
                TotalBooks = await _context.Books.CountAsync(),

                IssuedToday = await _context.IssueRecords
                    .CountAsync(i => i.IssueDate.Date == DateTime.UtcNow.Date),

                OverdueCount = await _context.IssueRecords
                    .CountAsync(i =>
                        i.ReturnDate.HasValue &&
                        i.ReturnDate.Value.Date < DateTime.UtcNow.Date &&
                        i.Status != "Returned"),

                // ✅ ReservationStatus is ENUM, not string
                ActiveReservations = await _context.Reservations
    .CountAsync(r => r.Status == ReservationStatus.Pending),

                StudentsWithUnpaidFines = await _context.Fines
                    .Where(f => !f.IsPaid)
                    .Select(f => f.StudentEmail)
                    .Distinct()
                    .CountAsync(),

                TotalUnpaidFine = await _context.Fines
                    .Where(f => !f.IsPaid)
                    .Select(f => (decimal?)f.Amount)
                    .SumAsync() ?? 0m,

                LastFineCollected = await _context.Fines
                    .Where(f => f.IsPaid && f.PaidAt.HasValue)
                    .OrderByDescending(f => f.PaidAt)
                    .Select(f => (decimal?)f.Amount)
                    .FirstOrDefaultAsync() ?? 0m
            };

            return View(vm); // Views/Librarian/Dashboard.cshtml
        }

        // =========================
        // BOOKS LIST (for ManageBooks page)
        // =========================
        [HttpGet]
        public async Task<IActionResult> ManageBooks()
        {
            var books = await _context.Books
                .AsNoTracking()
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View(books); // Views/Librarian/ManageBooks.cshtml
        }

        // =========================
        // RESERVATIONS
        // =========================
        [HttpGet]
        public async Task<IActionResult> Reservations()
        {
            var list = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Book)
                // ✅ Your model doesn't have CreatedAt, so order by Id (safe)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(list); // Views/Librarian/Reservations.cshtml
        }

        // =========================
        // PAYMENTS (VERIFY)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Payments()
        {
            var list = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Fine)
                .ThenInclude(f => f.IssueRecord)
                .ThenInclude(i => i.Book)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(list); // Views/Librarian/Payments.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Fine)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();

            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;

            payment.Fine.IsPaid = true;
            payment.Fine.PaidAt = DateTime.UtcNow;
            payment.Fine.PaymentMethod = payment.Method;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment confirmed.";
            return RedirectToAction(nameof(Payments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPayment(int id)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();

            payment.Status = PaymentStatus.Rejected;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment rejected.";
            return RedirectToAction(nameof(Payments));
        }
    }
}
