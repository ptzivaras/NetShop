using Eshop.Web.Services.Interfaces;
using RestSharp;
using Eshop.Web.ViewModels;
using Eshop.Contracts.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http; // add this as well

namespace Eshop.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly RestClient _client;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IMemoryCache cache, ILogger<ProductService> logger)
        {
            _cache = cache;
            _logger = logger;
            _client = new RestClient("https://localhost:7068/api");
        }

        public async Task<ProductsListViewModel> GetProductsPagedAsync(
            int page, int pageSize, string? q = null, int? categoryId = null)
        {
            var req = new RestRequest("products", Method.Get)
                .AddQueryParameter("page", page)
                .AddQueryParameter("pageSize", pageSize);

            if (!string.IsNullOrWhiteSpace(q)) req.AddQueryParameter("q", q);
            if (categoryId.HasValue) req.AddQueryParameter("categoryId", categoryId.Value);

            var resp = await _client.ExecuteAsync<PagedResult<ProductDto>>(req);
            if (!resp.IsSuccessful || resp.Data is null)
                throw new Exception($"API Error: {resp.StatusCode} - {resp.ErrorMessage}");

            var items = resp.Data.Items.Select(o => new ProductViewModel
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
                Price = o.Price,
                StockQuantity = o.StockQuantity,
                CategoryId = o.CategoryId,
                CategoryName = o.CategoryName ?? "Unknown"
            }).ToList();

            return new ProductsListViewModel
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalPages = resp.Data.TotalPages,
                TotalCount = resp.Data.TotalCount,
                Query = q,
                CategoryId = categoryId
            };
        }

        public async Task<ProductViewModel?> GetByIdAsync(int id)
        {
            var productRequest = new RestRequest($"products/{id}", Method.Get);
            var productResponse = await _client.ExecuteAsync<ProductDto>(productRequest);
            var dto = productResponse.Data;
            if (dto == null) return null;

            var categoryRequest = new RestRequest("categories", Method.Get);
            var categoryResponse = await _client.ExecuteAsync<List<CategoryDto>>(categoryRequest);
            var categories = categoryResponse.Data ?? new List<CategoryDto>();
            var categoryName = categories.FirstOrDefault(c => c.Id == dto.CategoryId)?.Name ?? "Unknown";

            return new ProductViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                CategoryName = categoryName,
                Categories = categories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
            };
        }

        public async Task CreateAsync(ProductViewModel product)
        {
            var dto = new ProductDto
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
            };
            var request = new RestRequest("products", Method.Post).AddJsonBody(dto);
            var response = await _client.ExecuteAsync(request);
            _cache.Remove("all_products_with_categories");
            if (!response.IsSuccessful)
                throw new Exception($"Failed to create product: {response.StatusCode} - {response.ErrorMessage}");
        }

        public async Task UpdateAsync(ProductViewModel product)
        {
            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId
            };
            var request = new RestRequest($"products/{product.Id}", Method.Put).AddJsonBody(dto);
            var response = await _client.ExecuteAsync(request);
            _cache.Remove("all_products_with_categories");

            if (!response.IsSuccessful)
                throw new Exception($"API Error: {response.StatusCode} - {response.ErrorMessage}");
        }

        public async Task DeleteAsync(int id)
        {
            var request = new RestRequest($"products/{id}", Method.Delete);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"API Error: {response.StatusCode} - {response.ErrorMessage}");
        }

        public async Task UploadImageAsync(int productId, IFormFile file)
        {
            var request = new RestRequest($"products/{productId}/image", Method.Post);

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            request.AddFile("file", ms.ToArray(), file.FileName, file.ContentType);

            var response = await _client.ExecuteAsync(request);
            _cache.Remove("all_products_with_categories");
            if (!response.IsSuccessful)
                throw new Exception($"Image upload failed: {response.StatusCode} - {response.ErrorMessage}");
        }
    }
}
