using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public class StudentDashboardViewModel
    {
        // Basic info
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Membership info
        public string MembershipType { get; set; } = "None"; // None / Normal / Premium
        public string? MembershipBarcode { get; set; }
        public DateTime? MembershipExpiry { get; set; }
        public bool IsMembershipActive { get; set; }

        // Summary cards
        public int CurrentIssuedCount { get; set; }
        public int OverdueCount { get; set; }
        public int TotalBooksCount { get; set; }
        public decimal TotalUnpaidFine { get; set; }

        // Premium access
        public bool HasPremiumAccess { get; set; }
        public List<Book> PremiumBooksSample { get; set; } = new();
        public List<IssueRecord> RecentIssues { get; set; } = new();
    }
}
