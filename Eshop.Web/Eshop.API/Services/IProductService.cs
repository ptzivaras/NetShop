using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using System.Threading.Tasks;

namespace Eshop.API.Services
{
    public interface IProductService
    {
        Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize, string? searchTerm, int? categoryId);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(ProductDto productDto);
        Task<bool> UpdateProductAsync(int id, ProductDto productDto);
        Task<bool> DeleteProductAsync(int id);
        Task<(byte[] ImageBytes, string ContentType)?> GetProductImageAsync(int id);
        Task<bool> UpdateProductImageAsync(int id, byte[] imageBytes, string contentType, string fileName, long fileSize);
    }
}
