
namespace StoreRewards.DTOs
{
    public class GetUserDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string? WhatsAppNo { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? ProfileImagePath { get; set; }

        public DateTime Birthday { get; set; }

    }
}
