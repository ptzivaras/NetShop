using System.ComponentModel.DataAnnotations;

namespace Eshop.Core.Models
{
    public class StockAlert
    {
        [Key] public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = string.Empty;
    }
}
