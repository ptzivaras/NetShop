using Eshop.Contracts.DTOs;
using System.Threading.Tasks;

namespace Eshop.API.Services
{
    public interface IShoppingCartService
    {
        Task<ShoppingCartDto?> GetCartByUserIdAsync(string userId);
        Task<(bool Success, string Message)> AddItemToCartAsync(string userId, int productId, int quantity);
        Task<(bool Success, string Message)> DecreaseItemQuantityAsync(string userId, int productId, int quantityToDecrease);
        Task<bool> ClearCartAsync(string userId);
    }
}
