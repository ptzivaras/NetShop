namespace Eshop.Contracts.DTOs
{
    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class AddToWishlistDto
    {
        public string UserId { get; set; } = "";
        public int ProductId { get; set; }
    }
}
