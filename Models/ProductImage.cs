using System.Text.Json.Serialization;

namespace StoreRewards.Models
{
    public class ProductImage
    {
        public int Id { get; set; } 
        public int ProductId { get; set; } // Foreign Key
        public string? Url { get; set; } // Path or URL to the image

        [JsonIgnore] // Prevent serialization to avoid cycles
        public Product? Product { get; set; } // Navigation Property
    }

}
