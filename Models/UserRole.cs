
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace StoreRewards.Models
{
    public class UserRole
    {
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public User User { get; set; }

        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
