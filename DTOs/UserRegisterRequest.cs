using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using StoreRewards.Models;

namespace StoreRewards.DTOs
{
    public class UserRegisterRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Provide a valid FirstName")]
        public string? FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Provide a valid LastName")]
        public string? LastName { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]  // Ensures it works with API JSON
        public Gender Gender { get; set; }

        [Required]
        public string? MobileNo { get; set; }

        public string? WhatsAppNo { get; set; }

        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public DateTime Birthday { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=(.*[!@#$%^&*(),.?\':{}|<>]){2,}).+$", ErrorMessage = "Password must contain at least two special characters.")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; } = 3;
        public UserMarketerDto? MarketerData { get; set; }

    }
}
