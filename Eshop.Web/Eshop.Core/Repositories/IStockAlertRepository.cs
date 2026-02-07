using Eshop.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eshop.Core.Repositories
{
    public interface IStockAlertRepository : IRepository<StockAlert>
    {
        Task<int> GetUnacknowledgedCountAsync();
        Task<IEnumerable<StockAlert>> GetAllAlertsAsync();
    }
}
