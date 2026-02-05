namespace Eshop.Contracts.DTOs
{
    public class StockAlertDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int QuantityAtTrigger { get; set; }
        public DateTime TriggeredAt { get; set; }
        public bool IsAcknowledged { get; set; }
    }
}
