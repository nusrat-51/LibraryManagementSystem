using System;

namespace LibraryManagementSystem.Models
{
    public class BookReturn
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        // 🔧 REQUIRED (IssueController uses this)
        public string MemberId { get; set; } = "";

        public string StudentEmail { get; set; } = "";

        public DateTime ReturnedAt { get; set; }

        public Book? Book { get; set; }
    }
}
