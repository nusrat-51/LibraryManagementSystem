// ViewModels/Librarian/LibrarianIssuesViewModel.cs
using System;
using System.Collections.Generic;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.ViewModels.Librarian
{
    public class LibrarianIssuesViewModel
    {
        public string Scope { get; set; } = "all";

        public int TotalIssues { get; set; }
        public int TodayIssues { get; set; }
        public int OverdueCount { get; set; }
        public int ReturnedToday { get; set; }

        // List shown in the table
        public List<IssueRecord> Issues { get; set; } = new();
    }
}
