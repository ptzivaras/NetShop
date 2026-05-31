using System.ComponentModel.DataAnnotations;

namespace Eshop.Core.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
