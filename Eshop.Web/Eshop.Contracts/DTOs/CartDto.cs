namespace Eshop.Contracts.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ICollection<CartItemDto>? Items { get; set; }
    }
}
