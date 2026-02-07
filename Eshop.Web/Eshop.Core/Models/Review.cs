using System.ComponentModel.DataAnnotations;

namespace Eshop.Core.Models
{
    public class Review
    {
        [Key] public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        [Range(1, 5)] public int Rating { get; set; }
        [MaxLength(1000)] public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Product? Product { get; set; }
    }
}
