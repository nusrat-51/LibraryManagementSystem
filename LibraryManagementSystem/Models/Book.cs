namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int Id { get; set; }              // INT kore dilam

        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }

        // later jodi navigation chai:
        // public ICollection<IssueRecord>? IssueRecords { get; set; }
    }
}
