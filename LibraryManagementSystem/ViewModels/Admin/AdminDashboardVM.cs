namespace LibraryManagementSystem.ViewModels.Admin
{
    public class AdminDashboardVM
    {
        public int TotalBooks { get; set; }
        public int TotalIssues { get; set; }
        public int TotalMembers { get; set; }
        public decimal TotalUnpaidFine { get; set; }
    }
}
