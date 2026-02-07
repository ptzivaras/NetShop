using Eshop.Core.Data;
using Eshop.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eshop.Core.Repositories
{
    public class StockAlertRepository : Repository<StockAlert>, IStockAlertRepository
    {
        public StockAlertRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<int> GetUnacknowledgedCountAsync()
        {
            return await _dbSet.CountAsync(sa => !sa.IsAcknowledged);
        }

        public async Task<IEnumerable<StockAlert>> GetAllAlertsAsync()
        {
            return await _dbSet
                .OrderByDescending(sa => sa.TriggeredAt)
                .ToListAsync();
        }
    }
}
