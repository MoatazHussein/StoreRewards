namespace StoreRewards.Models
{
    public class Transactions
    {
        public int Id { get; set; }
        public int MarketerUserId { get; set; }
        public int BuyerUserId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    }
}
