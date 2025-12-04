using System;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Models
{
    public class IssueRecord
    {
        public int Id { get; set; }

        // Book relationship
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        // Student (for now we are using email, because your existing code expects StudentEmail)
        public string StudentEmail { get; set; } = string.Empty;

        // Dates that your code & views already use
        public DateTime IssueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        // Optional extra fields
        public decimal FineAmount { get; set; }
        public string Status { get; set; } = "Issued";
    }
}
