using System;

namespace LibraryManagementSystem.Models
{
    public class Fine
    {
        public int Id { get; set; }

        public int IssueRecordId { get; set; }
        public IssueRecord IssueRecord { get; set; } = null!;

        public string StudentEmail { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public int? LastPaymentId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        

    }
}
