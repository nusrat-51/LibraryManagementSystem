using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    // Identity + Library data ek sathe
    public class LibraryContext : IdentityDbContext<ApplicationUser>
    {
        public LibraryContext(DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        // ====== Library tables ======
        public DbSet<Book> Books { get; set; } = default!;
        public DbSet<IssueRecord> IssueRecords { get; set; } = default!;
        public DbSet<Fine> Fines { get; set; } = default!;
        public DbSet<Membership> Memberships { get; set; } = default!;

        // 👉 Ei line ta chhilo na – tai shob error
        public DbSet<Reservation> Reservations { get; set; } = default!;

        // jodi aro model thake, ekhane add korbi
        // public DbSet<ReservationStatus> ReservationStatuses { get; set; } = default!;
        // public DbSet<MembershipStatus> MembershipStatuses { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Identity-r built in config
            base.OnModelCreating(builder);

            // ekhane chaile custom config/relations dibi
        }
    }
}
