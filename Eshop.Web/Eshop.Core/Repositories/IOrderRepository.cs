using Eshop.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eshop.Core.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId, int page, int pageSize);
        Task<int> CountByUserIdAsync(string userId);
        Task<Order?> GetOrderWithItemsAsync(int orderId);
    }
}
