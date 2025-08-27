using Eshop.Contracts.DTOs;
using Eshop.Web.ViewModels;

namespace Eshop.Web.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto?> GetOrderByIdAsync(int orderId);
        Task<PagedOrdersViewModel> GetOrdersByUserIdAsync(string userId, int page, int pageSize);
    }
}
