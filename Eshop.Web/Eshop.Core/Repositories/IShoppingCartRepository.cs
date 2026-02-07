using Eshop.Core.Models;
using System.Threading.Tasks;

namespace Eshop.Core.Repositories
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        Task<ShoppingCart?> GetCartByUserIdAsync(string userId);
        Task<ShoppingCart?> GetCartWithItemsAsync(string userId);
    }
}
