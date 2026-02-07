using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using System.Threading.Tasks;

namespace Eshop.API.Services
{
    public interface IProductService
    {
        Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize, string? searchTerm, int? categoryId);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(int id, Product product);
        Task<bool> DeleteProductAsync(int id);
    }
}
