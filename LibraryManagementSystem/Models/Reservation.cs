namespace LibraryManagementSystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string StudentEmail { get; set; } = "";
        public DateTime RequestedAt { get; set; }
        public int QueuePosition { get; set; }
        public ReservationStatus Status { get; set; }

        public Book? Book { get; set; }
    }

    public enum ReservationStatus
    {
        Pending,
        Active,
        Fulfilled,
        Cancelled,
        Expired
    }
}
