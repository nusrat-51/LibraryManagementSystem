using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class BookApplication
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        // your project uses email for identity
        [Required]
        public string StudentEmail { get; set; } = string.Empty;

        // requirement: Student ID / Normal ID
        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }
    }
}
