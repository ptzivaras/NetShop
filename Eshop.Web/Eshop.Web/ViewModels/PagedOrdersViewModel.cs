using Eshop.Contracts.DTOs;

namespace Eshop.Web.ViewModels
{
    public class PagedOrdersViewModel
    {
        public int TotalOrders { get; set; }
        public List<OrderDto> Orders { get; set; } = new();
    }
}
