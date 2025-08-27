using Eshop.Web.ViewModels;
using Microsoft.AspNetCore.Http; // add this

namespace Eshop.Web.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductsListViewModel> GetProductsPagedAsync(int page, int pageSize, string? q = null, int? categoryId = null);
        Task<ProductViewModel?> GetByIdAsync(int id);
        Task CreateAsync(ProductViewModel product);
        Task UpdateAsync(ProductViewModel product);
        Task DeleteAsync(int id);
        Task UploadImageAsync(int productId, IFormFile file);
    }
}
