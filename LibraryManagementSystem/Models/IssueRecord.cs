using System;

namespace LibraryManagementSystem.Models
{
    public class IssueRecord
    {
        public int Id { get; set; }          // PK
        public int BookId { get; set; }      // FK -> Book (INT, same type)

        public string StudentEmail { get; set; } = string.Empty;

        public DateTime IssueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public Book? Book { get; set; }
    }
}
