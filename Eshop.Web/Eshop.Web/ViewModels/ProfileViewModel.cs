using Eshop.Contracts.DTOs;

namespace Eshop.Web.ViewModels
{
    public class ProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public List<OrderDto> Orders { get; set; } = new();

        public int TotalOrders { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
