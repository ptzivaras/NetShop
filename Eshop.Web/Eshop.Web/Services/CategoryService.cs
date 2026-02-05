using Eshop.Web.Services.Interfaces;
using RestSharp;
using Eshop.Contracts.DTOs;
using Eshop.Web.ViewModels;

namespace Eshop.Web.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly RestClient _client;

        public CategoryService(IConfiguration configuration)
        {
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] 
                ?? throw new InvalidOperationException("API BaseUrl not configured in appsettings.json");
            _client = new RestClient(apiBaseUrl);
        }

        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            var request = new RestRequest("categories", Method.Get);
            var response = await _client.ExecuteAsync<List<CategoryDto>>(request);

            var dtoList = response.Data ?? new List<CategoryDto>();
            return dtoList.Select(dto => new CategoryViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description
            }).ToList();
        }

        public async Task<CategoryViewModel?> GetByIdAsync(int id)
        {
            var request = new RestRequest($"categories/{id}", Method.Get);
            var response = await _client.ExecuteAsync<CategoryDto>(request);
            var dto = response.Data;

            return dto == null ? null : new CategoryViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description
            };
        }

        public async Task CreateAsync(CategoryViewModel category)
        {
            var dto = new CategoryDto
            {
                Name = category.Name,
                Description = category.Description
            };
            var request = new RestRequest("categories", Method.Post).AddJsonBody(dto);
            await _client.ExecuteAsync(request);
        }

        public async Task UpdateAsync(CategoryViewModel category)
        {
            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
            var request = new RestRequest($"categories/{category.Id}", Method.Put).AddJsonBody(dto);
            await _client.ExecuteAsync(request);
        }

        public async Task DeleteAsync(int id)
        {
            var request = new RestRequest($"categories/{id}", Method.Delete);
            await _client.ExecuteAsync(request);
        }
    }
}
