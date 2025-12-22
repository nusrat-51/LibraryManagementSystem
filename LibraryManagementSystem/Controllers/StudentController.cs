using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly LibraryContext _context;
        private readonly FineCalculator _fineCalculator;
        private readonly IReceiptPdfService _receiptPdf;

        // ✅ Only ONE constructor
        public StudentController(
            LibraryContext context,
            FineCalculator fineCalculator,
            IReceiptPdfService receiptPdf)
        {
            _context = context;
            _fineCalculator = fineCalculator;
            _receiptPdf = receiptPdf;
        }

        // =========================
        // DASHBOARD
        // =========================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var membership = await _context.Memberships
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.StudentEmail == email && m.IsActive);

            var namePart = email.Split('@')[0];
            var friendlyName = char.ToUpper(namePart[0]) + namePart.Substring(1);

            var appliedBookIds = await _context.BookApplications
                .AsNoTracking()
                .Where(a => a.StudentEmail == email)
                .Select(a => a.BookId)
                .ToListAsync();

            var books = await _context.Books
                .AsNoTracking()
                .OrderBy(b => b.Title)
                .ToListAsync();

            var recent = await _context.BookApplications
                .AsNoTracking()
                .Include(a => a.Book)
                .Where(a => a.StudentEmail == email)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var vm = new StudentApplyDashboardVM
            {
                StudentId = $"STU-{namePart.ToUpper()}",
                MemberId = membership?.MembershipBarcode,
                Name = friendlyName,
                Address = "Not set",
                HasValidMemberId = membership != null && !string.IsNullOrWhiteSpace(membership.MembershipBarcode),

                CurrentIssuedCount = appliedBookIds.Count,
                OverdueCount = 0,
                TotalBooksCount = books.Count,
                TotalUnpaidFine = 0,

                Books = books.Select(b => new StudentBookVM
                {
                    BookId = b.Id,
                    Title = b.Title,
                    AvailableCopies = b.AvailableCopies,
                    IsApplied = appliedBookIds.Contains(b.Id)
                }).ToList(),

                RecentIssues = recent.Select(x => new IssueRecord
                {
                    Book = x.Book!,
                    IssueDate = x.IssueDate,
                    Status = "Applied",
                    FineAmount = 0
                }).ToList()
            };

            return View(vm);
        }

        // =========================
        // FINES
        // =========================
        [HttpGet]
        public async Task<IActionResult> MyFines()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            await _fineCalculator.RecalculateStudentFinesAsync(email);

            var fines = await _context.Fines
                .AsNoTracking()
                .Include(f => f.IssueRecord)
                .ThenInclude(i => i.Book)
                .Where(f => f.StudentEmail == email)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(fines);
        }

        [HttpGet]
        public async Task<IActionResult> PayFine(int fineId)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var fine = await _context.Fines
                .Include(f => f.IssueRecord)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(f => f.Id == fineId && f.StudentEmail == email);

            if (fine == null) return NotFound();

            if (fine.IsPaid)
            {
                TempData["Success"] = "Fine already paid.";
                return RedirectToAction(nameof(MyFines));
            }

            return View(fine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayFineCOD(int fineId)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var fine = await _context.Fines.FirstOrDefaultAsync(f => f.Id == fineId && f.StudentEmail == email);
            if (fine == null) return NotFound();
            if (fine.IsPaid) return RedirectToAction(nameof(MyFines));

            _context.Payments.Add(new Payment
            {
                FineId = fine.Id,
                Method = PaymentMethod.COD,
                Status = PaymentStatus.Pending,
                Amount = fine.Amount
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "COD request submitted. Please pay at counter and wait for confirmation.";
            return RedirectToAction(nameof(MyFines));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayFineBkash(int fineId, string transactionRef)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var fine = await _context.Fines.FirstOrDefaultAsync(f => f.Id == fineId && f.StudentEmail == email);
            if (fine == null) return NotFound();
            if (fine.IsPaid) return RedirectToAction(nameof(MyFines));

            if (string.IsNullOrWhiteSpace(transactionRef))
            {
                TempData["Error"] = "bKash Transaction ID is required.";
                return RedirectToAction(nameof(PayFine), new { fineId });
            }

            _context.Payments.Add(new Payment
            {
                FineId = fine.Id,
                Method = PaymentMethod.BKASH,
                Status = PaymentStatus.PendingVerification,
                Amount = fine.Amount,
                TransactionRef = transactionRef.Trim()
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "bKash payment submitted for verification.";
            return RedirectToAction(nameof(MyFines));
        }

        // =========================
        // DOWNLOAD RECEIPT PDF
        // =========================
        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int paymentId)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var ok = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Fine)
                .AnyAsync(p =>
                    p.Id == paymentId &&
                    p.Fine.StudentEmail == email &&
                    p.Status == PaymentStatus.Paid);

            if (!ok) return NotFound();

            var bytes = await _receiptPdf.BuildPaymentReceiptAsync(paymentId);
            return File(bytes, "application/pdf", $"Receipt-PAY-{paymentId}.pdf");
        }

        // =========================
        // MY ISSUES
        // =========================
        [HttpGet]
        public async Task<IActionResult> MyIssues()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var issues = await _context.IssueRecords
                .AsNoTracking()
                .Include(i => i.Book)
                .Where(i => i.StudentEmail == email)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return View(issues);
        }

        // =========================
        // MY APPLICATIONS
        // =========================
        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var apps = await _context.BookApplications
                .AsNoTracking()
                .Include(a => a.Book)
                .Where(a => a.StudentEmail == email)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(apps);
        }

        // =========================
        // VIEW BOOKS
        // =========================
        [HttpGet]
        public async Task<IActionResult> ViewBooks()
        {
            var books = await _context.Books
                .AsNoTracking()
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View(books);
        }
    }
}
