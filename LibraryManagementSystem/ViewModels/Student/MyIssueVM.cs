namespace LibraryManagementSystem.ViewModels.Student
{
    public class MyIssueVM
    {
        public string BookTitle { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal FineAmount { get; set; }
    }
}
