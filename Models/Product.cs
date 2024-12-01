using Microsoft.EntityFrameworkCore;

namespace StoreRewards.Models
{
    public class Product
    {
        public int Id { get; set; } 
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; } 
        [Precision(18, 2)]
        public decimal Commission { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }

}
