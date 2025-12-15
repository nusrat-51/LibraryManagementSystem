using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.ViewModels.Student
{
    public class BookApplyVM
    {
        public int BookId { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";

        public string StudentId { get; set; } = ""; // Normal ID

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }
    }
}
