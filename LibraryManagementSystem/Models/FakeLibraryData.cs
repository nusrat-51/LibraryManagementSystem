using LibraryManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Data
{
    public static class FakeLibraryData
    {
        // In-memory book list
        private static readonly List<Book> _books = new()
        {
            new Book
            {
                Id = "B001",              // string, not int
                Title = "Algorithms",
                Author = "Cormen",
                Category = "CS",
                TotalCopies = 10,
                AvailableCopies = 4
            }
        };

        // In-memory issue list
        private static readonly List<IssueRecord> _issues = new()
        {
            new IssueRecord
            {
                Id = 1,
                StudentEmail = "student@example.com",
                BookTitle = "Algorithms",
                Author = "Cormen",
                IssueDate = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(5)
            }
        };

        // --- Public helper methods used by controllers ---

        public static List<Book> GetBooks()
        {
            return _books;
        }

        public static List<IssueRecord> GetIssuesForStudent(string email)
        {
            return _issues
                   .Where(i => i.StudentEmail == email)
                   .ToList();
        }
    }
}
