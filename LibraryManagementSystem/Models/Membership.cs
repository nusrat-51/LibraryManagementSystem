using System;
using System.ComponentModel.DataAnnotations;

namespace YourNamespace.Models
{
    public class Membership
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string StudentEmail { get; set; } = default!;

        // E.g. "Premium", "Standard" – for now you mainly care about Premium
        [Required]
        [MaxLength(50)]
        public string PlanName { get; set; } = "Premium";

        // Validity
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public MembershipStatus Status { get; set; } = MembershipStatus.Pending;

        // Payment tracking (can be extended later with real gateways)
        [Range(0, double.MaxValue)]
        public decimal AmountPaid { get; set; }

        [MaxLength(100)]
        public string? PaymentReference { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
    }
}
