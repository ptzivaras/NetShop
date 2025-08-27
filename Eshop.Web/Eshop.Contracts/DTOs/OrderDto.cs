namespace Eshop.Contracts.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public ICollection<OrderItemDto>? Items { get; set; }
    }
}
