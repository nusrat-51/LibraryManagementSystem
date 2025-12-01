using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models   // 👈 same namespace as other models
{
    public class Reservation
    {
        // Primary key (unique id)
        public int Id { get; set; }

        // Foreign key to Book (kon boi)
        [Required]
        public int BookId { get; set; }

        // Navigation property: pore Include() diye Book data pete parbo
        public Book Book { get; set; } = default!;

        // Student er email (Identity user er email use korbo)
        [Required]
        [EmailAddress]
        public string StudentEmail { get; set; } = default!;

        // Kokkhn request korse
        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Line e kon position: 1, 2, 3 ...
        public int QueuePosition { get; set; }

        // Upore banano enum
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    }
}
