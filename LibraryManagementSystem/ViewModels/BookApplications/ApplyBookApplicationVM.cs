using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.ViewModels.BookApplications
{
    public class ApplyBookApplicationVM
    {
        // Pre-filled
        public int BookId { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Category { get; set; } = "";

        // Normal ID
        public string StudentId { get; set; } = "";

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }
    }
}
