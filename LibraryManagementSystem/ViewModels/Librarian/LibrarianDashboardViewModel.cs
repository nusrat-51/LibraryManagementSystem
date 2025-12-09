namespace LibraryManagementSystem.ViewModels.Librarian
{
    public class LibrarianDashboardViewModel
    {
        // Top stats
        public int TotalBooks { get; set; }
        public int IssuedToday { get; set; }
        public int OverdueCount { get; set; }
        public int ActiveReservations { get; set; }

        // Fines block
        public int StudentsWithUnpaidFines { get; set; }
        public decimal TotalUnpaidFine { get; set; }
        public decimal LastFineCollected { get; set; }
    }
}
