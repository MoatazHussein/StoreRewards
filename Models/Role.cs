
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace StoreRewards.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
