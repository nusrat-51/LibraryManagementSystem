using System.Collections.Generic;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.ViewModels.Librarian
{
    public class IssuePageVM
    {
        public List<Book> AvailableBooks { get; set; } = new();
        public List<IssueRecord> ActiveIssues { get; set; } = new();
    }
}
