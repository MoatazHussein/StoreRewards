
namespace StoreRewards.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; } 
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Commission { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }

}
