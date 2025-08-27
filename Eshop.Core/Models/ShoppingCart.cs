using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eshop.Core.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }

        public ICollection<CartItem>? CartItems { get; set; }
    }
}
