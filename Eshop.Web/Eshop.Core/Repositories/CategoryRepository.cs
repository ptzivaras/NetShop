using Eshop.Core.Data;
using Eshop.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Eshop.Core.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(c => c.Id == id);
        }
    }
}
