using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public class PayFineViewModel
    {
        public decimal TotalUnpaid { get; set; }

        public List<Fine> UnpaidFines { get; set; } = new();
        public List<Fine> PaidHistory { get; set; } = new();
    }
}
