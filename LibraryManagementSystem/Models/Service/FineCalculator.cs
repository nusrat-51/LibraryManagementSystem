using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    public class FineCalculator
    {
        private readonly LibraryContext _context;
        private const decimal FinePerDay = 10m;

        public FineCalculator(LibraryContext context)
        {
            _context = context;
        }

        public async Task RecalculateStudentFinesAsync(string studentEmail)
        {
            var today = DateTime.UtcNow.Date;

            var issues = await _context.IssueRecords
                .Include(i => i.Book)
                .Where(i =>
                    i.StudentEmail == studentEmail &&
                    i.ReturnDate != null &&
                    today > i.ReturnDate.Value.Date)
                .ToListAsync();

            foreach (var issue in issues)
            {
                var overdueDays = (today - issue.ReturnDate!.Value.Date).Days;
                if (overdueDays <= 0) continue;

                var fineAmount = overdueDays * FinePerDay;

                var fine = await _context.Fines
                    .FirstOrDefaultAsync(f => f.IssueRecordId == issue.Id);

                if (fine == null)
                {
                    _context.Fines.Add(new Fine
                    {
                        IssueRecordId = issue.Id,
                        StudentEmail = studentEmail,
                        Amount = fineAmount,
                        IsPaid = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else if (!fine.IsPaid)
                {
                    fine.Amount = fineAmount;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
