using System.ComponentModel.DataAnnotations;

namespace Eshop.Core.Models
{
    public class ShoppingCart
    {
        [Key] public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        public ICollection<CartItem>? CartItems { get; set; }
    }
}
