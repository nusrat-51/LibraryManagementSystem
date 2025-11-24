namespace LibraryManagementSystem.Models
{
    public class IssueRecord
    {
        public int Id { get; set; }

        public string StudentEmail { get; set; } = string.Empty;   // who borrowed
        public string BookTitle { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;

        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }

        // calculated property
        public bool IsOverdue => DateTime.Today > DueDate;
    }
}
