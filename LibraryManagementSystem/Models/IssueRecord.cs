using System;

namespace LibraryManagementSystem.Models
{
    public class IssueRecord
    {
        public int IssueRecordId { get; set; }

        // Book er primary key
        public int BookId { get; set; }

        // Je student issue koreche tar email
        public string StudentEmail { get; set; } = string.Empty;

        // Issue date
        public DateTime IssueDate { get; set; }

        // Return date (nullable – jodi ekhono return na kore)
        public DateTime? ReturnDate { get; set; }

        // "Issued" / "Returned"
        public string Status { get; set; } = string.Empty;

        // Navigation property (optional)
        public Book? Book { get; set; }
    }
}
