using Eshop.Contracts.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eshop.API.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<(IEnumerable<OrderDto> Orders, int TotalOrders)> GetOrdersByUserIdAsync(string userId, int page, int pageSize);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<(bool Success, string Message, int? OrderId)> CreateOrderAsync(string userId);
    }
}
