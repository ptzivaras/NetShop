using Eshop.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eshop.Core.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
            int page, 
            int pageSize, 
            string? searchTerm = null, 
            int? categoryId = null);
        Task<bool> ExistsAsync(int id);
    }
}
