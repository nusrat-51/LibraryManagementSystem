using System;

namespace LibraryManagementSystem.Models
{
    public class Fine
    {
        public int Id { get; set; }

        // কোন বই/issue থেকে fine এসেছে (ইচ্ছা করলে null)
        public int? IssueRecordId { get; set; }
        public IssueRecord? IssueRecord { get; set; }

        public string StudentEmail { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public bool IsPaid { get; set; }
        public DateTime? PaidOn { get; set; }

        // Online payment reference (Bkash/Stripe mock)
        public string? PaymentReference { get; set; }

        public string? Notes { get; set; }
    }
}
