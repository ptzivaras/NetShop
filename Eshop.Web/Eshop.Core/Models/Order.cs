using System.ComponentModel.DataAnnotations;

namespace Eshop.Core.Models
{
    public class Order
    {
        [Key] public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string CustomerName { get; set; } = string.Empty;

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
