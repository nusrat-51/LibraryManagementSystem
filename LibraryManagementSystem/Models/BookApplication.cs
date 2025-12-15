using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class BookApplication
    {
        [Key]
        public int Id { get; set; }

        // Required fields (as per requirement)
        public int BookId { get; set; }

        [Required]
        public string StudentId { get; set; } = ""; // store User.Identity.Name (email)

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        // extra (safe)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // navigation (optional but useful)
        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }
    }
}
