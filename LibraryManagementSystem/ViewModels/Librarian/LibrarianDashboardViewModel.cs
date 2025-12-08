namespace LibraryManagementSystem.ViewModels.Librarian
{
    public class LibrarianDashboardViewModel
    {
        public int TotalBooks { get; set; }

        public int IssuedToday { get; set; }
        public int ReturnedToday { get; set; }

        public int OverdueCount { get; set; }

        public int ActiveReservations { get; set; }

        public int StudentsWithUnpaidFines { get; set; }
        public decimal TotalUnpaidFine { get; set; }
        public decimal LastFineCollected { get; set; }
    }
}
