using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public class StudentDashboardViewModel
    {
        // Identity info
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Membership info
        public string MembershipType { get; set; } = "Normal";   // Normal / Premium
        public bool IsMembershipActive { get; set; } = true;
        public bool HasPremiumAccess { get; set; } = false;

        // NEW: these are used in Dashboard.cshtml
        public string? MembershipBarcode { get; set; }
        public DateTime? MembershipExpiry { get; set; }

        // Summary cards
        public int CurrentIssuedCount { get; set; }
        public int OverdueCount { get; set; }
        public int TotalBooksCount { get; set; }
        public decimal TotalUnpaidFine { get; set; }

        // Recent issued books table
        public List<IssueRecord> RecentIssues { get; set; } = new();

        // Premium collection preview
        public List<Book> PremiumBooksSample { get; set; } = new();
    }
}
