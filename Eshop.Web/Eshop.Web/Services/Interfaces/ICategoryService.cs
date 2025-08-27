using Eshop.Web.ViewModels;

namespace Eshop.Web.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetAllCategoriesAsync();
        Task<CategoryViewModel?> GetByIdAsync(int id);
        Task CreateAsync(CategoryViewModel category);
        Task UpdateAsync(CategoryViewModel category);
        Task DeleteAsync(int id);
    }
}
