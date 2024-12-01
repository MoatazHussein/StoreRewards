using System.ComponentModel.DataAnnotations;

namespace StoreRewards.DTOs
{
    public class UserLoginRequest
    {
        //[Required, EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
