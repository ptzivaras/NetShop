using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eshop.Contracts.DTOs
{
    public class ShoppingCartDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; 
        public List<CartItemDto> Items { get; set; } = new();
    }
}
