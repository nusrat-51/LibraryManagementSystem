using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class LibraryContext : IdentityDbContext<ApplicationUser>
    {
        public LibraryContext(DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        // =====================
        // CORE TABLES
        // =====================
        public DbSet<Book> Books { get; set; }
        public DbSet<IssueRecord> IssueRecords { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Fine> Fines { get; set; }

        // =====================
        // REQUIRED FIXES
        // =====================
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<BookReturn> BookReturns { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
