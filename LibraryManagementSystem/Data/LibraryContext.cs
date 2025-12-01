using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; } = default!;
        public DbSet<IssueRecord> IssueRecords { get; set; } = default!;
        public DbSet<Fine> Fines { get; set; } = default!;

        // 👉 ekhane new line:
        public DbSet<Reservation> Reservations { get; set; } = default!;
    }
}
