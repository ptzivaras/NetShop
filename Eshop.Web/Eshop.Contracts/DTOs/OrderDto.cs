namespace Eshop.Contracts.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public string Message { get; set; } = string.Empty;// for confirmation order view
        public int OrderId { get; set; }

    }
}
