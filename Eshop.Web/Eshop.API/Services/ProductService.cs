using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using Eshop.Core.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace Eshop.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize, string? searchTerm, int? categoryId)
        {
            // Validation
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 11;

            var (products, totalCount) = await _productRepository.GetPagedAsync(page, pageSize, searchTerm, categoryId);

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId
            }).ToList();

            return new PagedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return null;

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId
            };
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                CategoryId = productDto.CategoryId
            };

            var created = await _productRepository.AddAsync(product);
            productDto.Id = created.Id;
            return productDto;
        }

        public async Task<bool> UpdateProductAsync(int id, ProductDto productDto)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
                return false;

            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;
            existingProduct.StockQuantity = productDto.StockQuantity;
            existingProduct.CategoryId = productDto.CategoryId;

            await _productRepository.UpdateAsync(existingProduct);
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            await _productRepository.DeleteAsync(product);
            return true;
        }

        public async Task<(byte[] ImageBytes, string ContentType)?> GetProductImageAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || product.ImageBytes == null || string.IsNullOrEmpty(product.ImageContentType))
                return null;

            return (product.ImageBytes, product.ImageContentType);
        }

        public async Task<bool> UpdateProductImageAsync(int id, byte[] imageBytes, string contentType, string fileName, long fileSize)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            product.ImageBytes = imageBytes;
            product.ImageContentType = contentType;
            product.ImageFileName = fileName;
            product.ImageSize = fileSize;

            await _productRepository.UpdateAsync(product);
            return true;
        }
    }
}
