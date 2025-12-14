using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class BookApplication
    {
        public int Id { get; set; }

        // ✅ because your project uses email everywhere
        [Required]
        public string StudentEmail { get; set; } = string.Empty;

        // ✅ Member ID required (MembershipBarcode)
        [Required]
        public string MemberId { get; set; } = string.Empty;

        [Required]
        public int BookId { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        // optional navigation
        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }
    }
}
