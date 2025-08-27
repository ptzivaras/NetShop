using Eshop.Web.ViewModels;

namespace Eshop.Web.Services.Interfaces
{
    public interface ICartService
    {
        Task<ShoppingCartViewModel> GetCartByUserIdAsync(string userId);
        Task AddItemToCartAsync(int productId, string userId);
        Task<int> PlaceOrderAsync(string userId);
        Task<int> AddItemToCartAndReturnQuantityAsync(int productId, string userId);
        Task DecreaseItemAsync(int productId, string userId);
    }
}
