namespace YourNamespace.Models
{
    public enum MembershipStatus
    {
        Pending = 0,   // Student requested upgrade, waiting for librarian approval
        Active = 1,    // Premium currently valid
        Expired = 2,   // Validity period over
        Rejected = 3,  // Librarian rejected request
        Cancelled = 4  // Cancelled before expiry
    }
}
