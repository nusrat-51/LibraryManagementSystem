using System;
using System.Collections.Generic;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.ViewModels.Student
{
    public class StudentDashboardViewModel
    {
        // Identity info
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Membership info
        public string MembershipType { get; set; } = "Standard";
        public bool IsMembershipActive { get; set; }
        public bool HasPremiumAccess { get; set; }
        public string MembershipBarcode { get; set; } = "Not assigned";
        public DateTime? MembershipExpiry { get; set; }

        // Summary cards
        public int CurrentIssuedCount { get; set; }
        public int OverdueCount { get; set; }
        public int TotalBooksCount { get; set; }
        public decimal TotalUnpaidFine { get; set; }

        // Tables
        public IList<IssueRecord> RecentIssues { get; set; } = new List<IssueRecord>();
        public IList<Book> PremiumBooksSample { get; set; } = new List<Book>();
    }
}
