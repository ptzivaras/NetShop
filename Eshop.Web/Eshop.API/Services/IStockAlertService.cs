using Eshop.Contracts.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eshop.API.Services
{
    public interface IStockAlertService
    {
        Task<IEnumerable<StockAlertDto>> GetAllAlertsAsync();
        Task<int> GetUnacknowledgedCountAsync();
        Task<StockAlertDto?> GetAlertByIdAsync(int id);
        Task<bool> AcknowledgeAlertAsync(int id);
        Task<bool> DeleteAlertAsync(int id);
    }
}
