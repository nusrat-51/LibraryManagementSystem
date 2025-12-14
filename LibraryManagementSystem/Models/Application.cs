using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Required by your new requirements (Student Dashboard)
        public string? StudentId { get; set; }   // ex: STU-000123
        public string? MemberId { get; set; }    // Member ID (required to Apply)
        public string? Name { get; set; }
        public string? Address { get; set; }
    }
}
