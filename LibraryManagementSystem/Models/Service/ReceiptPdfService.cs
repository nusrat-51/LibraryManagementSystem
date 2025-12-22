using System;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LibraryManagementSystem.Services
{
    public class ReceiptPdfService : IReceiptPdfService
    {
        private readonly LibraryContext _context;

        public ReceiptPdfService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<byte[]> BuildPaymentReceiptAsync(int paymentId)
        {
            var payment = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Fine)
                    .ThenInclude(f => f.IssueRecord)
                        .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            var fine = payment.Fine;
            var issue = fine.IssueRecord;

            var bookTitle = issue?.Book?.Title ?? "N/A";
            var studentEmail = fine.StudentEmail ?? "N/A";

            var method = payment.Method.ToString();
            var status = payment.Status.ToString();

            var txn = string.IsNullOrWhiteSpace(payment.TransactionRef) ? "N/A" : payment.TransactionRef;
            var createdAt = payment.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd hh:mm tt");
            var paidAt = payment.PaidAt?.ToLocalTime().ToString("yyyy-MM-dd hh:mm tt") ?? "Not Paid Yet";

            var amount = payment.Amount;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Library Management System").FontSize(18).SemiBold();
                        col.Item().Text("Payment Receipt").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().LineHorizontal(1);
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Receipt No: PAY-{payment.Id}").SemiBold();
                        col.Item().Text($"Created: {createdAt}");
                        col.Item().Text($"Paid At: {paidAt}");
                        col.Item().Text($"Status: {status}");
                        col.Item().Text($"Method: {method}");
                        col.Item().Text($"Transaction Ref: {txn}");

                        col.Item().LineHorizontal(1);

                        col.Item().Text("Student & Issue Details").SemiBold().FontSize(12);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(4);
                            });

                            void Row(string k, string v)
                            {
                                table.Cell().Element(CellStyle).Text(k).SemiBold();
                                table.Cell().Element(CellStyle).Text(v);
                            }

                            Row("Student Email", studentEmail);
                            Row("Book", bookTitle);
                            Row("Issue Date", issue?.IssueDate.ToLocalTime().ToString("yyyy-MM-dd") ?? "N/A");
                            Row("Return Date", issue?.ReturnDate?.ToLocalTime().ToString("yyyy-MM-dd") ?? "N/A");
                            Row("Fine Amount", amount.ToString("0.00"));
                        });

                        col.Item().PaddingTop(10).LineHorizontal(1);
                        col.Item().Text("Thank you for your payment.").Italic().FontColor(Colors.Grey.Darken2);
                    });

                    page.Footer().AlignCenter().Text($"© 2025 LMS • Generated {DateTime.Now:yyyy-MM-dd}");
                });
            }).GeneratePdf();

            return pdf;

            static IContainer CellStyle(IContainer container)
            {
                return container
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(6);
            }
        }
    }
}
