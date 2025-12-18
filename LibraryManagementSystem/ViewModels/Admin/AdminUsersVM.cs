using System.Collections.Generic;

namespace LibraryManagementSystem.ViewModels.Admin
{
    public class AdminUserVM
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Roles { get; set; } = new();
        public bool IsLocked { get; set; }
    }
}
