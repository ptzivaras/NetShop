using Eshop.Core.Models;
using System.Threading.Tasks;

namespace Eshop.Core.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<bool> ExistsAsync(int id);
    }
}
