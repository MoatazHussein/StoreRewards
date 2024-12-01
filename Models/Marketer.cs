using Microsoft.EntityFrameworkCore;

namespace StoreRewards.Models
{
    public class Marketer 
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? BankName { get; set; } 
        public string? IBAN { get; set; } // Example: "GB33BUKB20201555555555"
        public string? SWIFTCode { get; set; } // Example: "BUKBGB22"
        public string? ProductQuery { get; set; }
        public bool IsActive { get; set; }
        public int GainedWatchers { get; set; }
        public int GainedBuyers { get; set; }

        [Precision(18, 2)]
        public decimal AvailableCommissionBalance { get; set; }
        [Precision(18, 2)]
        public decimal CyclicCommissionBalance { get; set; }
        public User User { get; set; }
    }
}
