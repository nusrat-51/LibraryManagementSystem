using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // This is bound to the "Login as" dropdown
        [Required]
        public string SelectedRole { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
