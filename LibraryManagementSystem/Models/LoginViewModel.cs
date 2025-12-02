using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // only for UI dropdown (Student / Librarian / Admin) if you want
        public string LoginAs { get; set; } = "Student";
    }
}
