using System.ComponentModel.DataAnnotations;

namespace Eshop.Core.Models
{
    public class Product
    {
        [Key] public int Id { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
        public ICollection<Review>? Reviews { get; set; }

        public int LowStockThreshold { get; set; } = 5;

        public string? ImagePath { get; set; }
        public byte[]? ImageBytes { get; set; }
        [MaxLength(128)] public string? ImageContentType { get; set; }
        [MaxLength(260)] public string? ImageFileName { get; set; }
        public long? ImageSize { get; set; }
    }
}



