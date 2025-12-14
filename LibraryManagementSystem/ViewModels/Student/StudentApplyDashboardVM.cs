using System;
using System.Collections.Generic;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.ViewModels.Student
{
    public class StudentApplyDashboardVM
    {
        // Student Info (Requirement #2)
        public string StudentId { get; set; } = "";
        public string? MemberId { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "Not set";

        // Validation (Requirement #1)
        public bool HasValidMemberId { get; set; }

        // Summary cards
        public int CurrentIssuedCount { get; set; }
        public int OverdueCount { get; set; }
        public int TotalBooksCount { get; set; }
        public decimal TotalUnpaidFine { get; set; }

        // Apply list
        public List<StudentBookVM> Books { get; set; } = new();

        // Recent issues table
        public List<IssueRecord> RecentIssues { get; set; } = new();
    }

    public class StudentBookVM
    {
        // IMPORTANT: This must be BookId (your errors are because it’s missing)
        public int BookId { get; set; }

        public string Title { get; set; } = "";
        public int AvailableCopies { get; set; }

        // For showing Applied / disabling button
        public bool IsApplied { get; set; }
    }
}
