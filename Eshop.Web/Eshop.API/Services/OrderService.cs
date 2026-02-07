using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using Eshop.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Eshop.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShoppingCartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IShoppingCartRepository cartRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToDto).ToList();
        }

        public async Task<(IEnumerable<OrderDto> Orders, int TotalOrders)> GetOrdersByUserIdAsync(string userId, int page, int pageSize)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId, page, pageSize);
            var totalOrders = await _orderRepository.CountByUserIdAsync(userId);
            
            var orderDtos = orders.Select(MapToDto).ToList();
            return (orderDtos, totalOrders);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(id);
            if (order == null)
                return null;

            return MapToDto(order);
        }

        public async Task<(bool Success, string Message, int? OrderId)> CreateOrderAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "Invalid user ID.", null);

            var txOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.DefaultTimeout
            };

            using var scope = new TransactionScope(
                TransactionScopeOption.Required,
                txOptions,
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                // Get cart with items
                var cart = await _cartRepository.GetCartWithItemsAsync(userId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return (false, "Cart is empty or does not exist.", null);

                // Validate cart items
                foreach (var item in cart.CartItems)
                {
                    if (item.Product == null)
                        return (false, $"Product {item.ProductId} not found.", null);
                    
                    if (item.Quantity <= 0)
                        return (false, $"Invalid quantity for product {item.Product.Name}.", null);
                    
                    if (item.Quantity > item.Product.StockQuantity)
                        return (false, $"Not enough stock for '{item.Product.Name}'.", null);
                }

                // Update product stock
                foreach (var item in cart.CartItems)
                {
                    item.Product!.StockQuantity -= item.Quantity;
                    try
                    {
                        await _productRepository.UpdateAsync(item.Product);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return (false, $"Product '{item.Product.Name}' was updated by another user. Please try again.", null);
                    }
                }

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = cart.CartItems.Select(ci => new OrderItem
                    {
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.Product.Price
                    }).ToList()
                };

                order.TotalPrice = order.OrderItems.Sum(item => item.UnitPrice * item.Quantity);

                var createdOrder = await _orderRepository.AddAsync(order);

                // Clear cart items
                foreach (var item in cart.CartItems.ToList())
                {
                    cart.CartItems.Remove(item);
                }
                await _cartRepository.UpdateAsync(cart);

                scope.Complete();

                return (true, "Order placed successfully", createdOrder.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Error placing order: {ex.Message}", null);
            }
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Items = order.OrderItems?.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList() ?? new List<OrderItemDto>()
            };
        }
    }
}
