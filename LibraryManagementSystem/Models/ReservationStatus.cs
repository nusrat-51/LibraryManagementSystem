namespace LibraryManagementSystem.Models   // 👈 jodi tomader namespace onno hoy, ota use korba
{
    public enum ReservationStatus
    {
        Pending = 0,    // Student request korse, book ekhono free ba assign hoyni
        Active = 1,     // Book ready student er jonno, hold kore rakha
        Fulfilled = 2,  // Reservation theke book issue hoye geche
        Cancelled = 3,  // Student ba librarian cancel koreche
        Expired = 4     // Shomoy sesh, student aasheni, next ke chance
    }
}
