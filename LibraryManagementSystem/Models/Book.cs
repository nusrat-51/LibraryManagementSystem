namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public string Id { get; set; }          // e.g. B1023
        public string Title { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }

        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
    }
}
