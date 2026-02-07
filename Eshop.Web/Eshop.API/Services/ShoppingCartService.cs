using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using Eshop.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eshop.API.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public ShoppingCartService(
            IShoppingCartRepository cartRepository,
            IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<ShoppingCartDto?> GetCartByUserIdAsync(string userId)
        {
            var cart = await _cartRepository.GetCartWithItemsAsync(userId);
            if (cart == null)
                return null;

            return new ShoppingCartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems?.Select(ci => new CartItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Name ?? "",
                    Price = ci.Product?.Price ?? 0,
                    Quantity = ci.Quantity
                }).ToList() ?? new List<CartItemDto>()
            };
        }

        public async Task<(bool Success, string Message)> AddItemToCartAsync(string userId, int productId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "User ID is required.");

            if (productId <= 0)
                return (false, "Valid Product ID is required.");

            // Get product
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return (false, "Product not found.");

            // Get or create cart
            var cart = await _cartRepository.GetCartWithItemsAsync(userId);
            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                cart = await _cartRepository.AddAsync(cart);
                cart = await _cartRepository.GetCartWithItemsAsync(userId); // Reload with items
            }

            // Check if product already in cart
            var existingItem = cart.CartItems?.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.CartItems ??= new List<CartItem>();
                cart.CartItems.Add(new CartItem
                {
                    ShoppingCartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _cartRepository.UpdateAsync(cart);
            return (true, "Item added to cart successfully.");
        }

        public async Task<(bool Success, string Message)> DecreaseItemQuantityAsync(string userId, int productId, int quantityToDecrease)
        {
            var cart = await _cartRepository.GetCartWithItemsAsync(userId);
            if (cart == null || cart.CartItems == null)
                return (false, "Cart not found.");

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item == null)
                return (false, "Item not found in cart.");

            item.Quantity -= quantityToDecrease;

            if (item.Quantity <= 0)
            {
                cart.CartItems.Remove(item);
            }

            await _cartRepository.UpdateAsync(cart);
            return (true, "Item quantity updated successfully.");
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var cart = await _cartRepository.GetCartWithItemsAsync(userId);
            if (cart == null)
                return false;

            if (cart.CartItems != null)
            {
                cart.CartItems.Clear();
                await _cartRepository.UpdateAsync(cart);
            }

            return true;
        }
    }
}
