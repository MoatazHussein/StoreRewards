namespace StoreRewards.Models
{
    public class User //: IdentityUser   
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Gender Gender { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string? WhatsAppNo { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public DateTime Birthday { get; set; }
        public string? ProfileImagePath { get; set; }
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];
        public string? VerificationToken { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }

    }
}
