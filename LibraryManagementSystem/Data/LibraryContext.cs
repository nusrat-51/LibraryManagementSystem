using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class LibraryContext : DbContext
    {
        // Runtime constructor (application uses this)
        public LibraryContext(DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        // Tables
        public DbSet<Book> Books { get; set; }
        public DbSet<IssueRecord> IssueRecords { get; set; }
    }
}
