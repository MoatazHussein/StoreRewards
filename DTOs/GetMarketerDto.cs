
using Microsoft.EntityFrameworkCore;

namespace StoreRewards.DTOs
{
    public class GetMarketerDto
    {
        public int UserId { get; set; }
        public string? BankName { get; set; }
        public string? IBAN { get; set; } // Example: "GB33BUKB20201555555555"
        public string? SWIFTCode { get; set; } // Example: "BUKBGB22"
        public string? ProductQuery { get; set; }
        public GetUserDto? User { get; set; }

    }

}
