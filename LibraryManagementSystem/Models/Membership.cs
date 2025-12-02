using System;

namespace LibraryManagementSystem.Models
{
    public class Membership
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;  // Identity user id
        public string StudentEmail { get; set; } = string.Empty;

        public string MembershipType { get; set; } = "Standard";  // Standard / Premium
        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; } = DateTime.UtcNow.AddYears(1);

        public string? MembershipBarcode { get; set; }
    }
}
