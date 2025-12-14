using System;

namespace LibraryManagementSystem.Models
{
    public class ReturnRecord
    {
        public int Id { get; set; }

        public string MemberId { get; set; } = string.Empty;

        public int BookId { get; set; }

        public DateTime DateOfReturn { get; set; } = DateTime.UtcNow;

        // Optional (helps tracking which student returned)
        public string StudentEmail { get; set; } = string.Empty;

        // Navigation (optional)
        public Book? Book { get; set; }
    }
}
