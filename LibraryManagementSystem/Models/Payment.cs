using System;

namespace LibraryManagementSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int FineId { get; set; }
        public Fine Fine { get; set; } = null!;

        // ✅ Now enum exists
        public PaymentMethod Method { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public decimal Amount { get; set; }

        public string? TransactionRef { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}
