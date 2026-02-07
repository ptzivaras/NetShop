using Eshop.Contracts.DTOs;
using Eshop.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eshop.API.Services
{
    public class StockAlertService : IStockAlertService
    {
        private readonly IStockAlertRepository _stockAlertRepository;

        public StockAlertService(IStockAlertRepository stockAlertRepository)
        {
            _stockAlertRepository = stockAlertRepository;
        }

        public async Task<IEnumerable<StockAlertDto>> GetAllAlertsAsync()
        {
            var alerts = await _stockAlertRepository.GetAllAlertsAsync();
            return alerts.Select(sa => new StockAlertDto
            {
                Id = sa.Id,
                ProductId = sa.ProductId,
                ProductName = sa.ProductName,
                QuantityAtTrigger = sa.QuantityAtTrigger,
                TriggeredAt = sa.TriggeredAt,
                IsAcknowledged = sa.IsAcknowledged
            }).ToList();
        }

        public async Task<int> GetUnacknowledgedCountAsync()
        {
            return await _stockAlertRepository.GetUnacknowledgedCountAsync();
        }

        public async Task<StockAlertDto?> GetAlertByIdAsync(int id)
        {
            var alert = await _stockAlertRepository.GetByIdAsync(id);
            if (alert == null)
                return null;

            return new StockAlertDto
            {
                Id = alert.Id,
                ProductId = alert.ProductId,
                ProductName = alert.ProductName,
                QuantityAtTrigger = alert.QuantityAtTrigger,
                TriggeredAt = alert.TriggeredAt,
                IsAcknowledged = alert.IsAcknowledged
            };
        }

        public async Task<bool> AcknowledgeAlertAsync(int id)
        {
            var alert = await _stockAlertRepository.GetByIdAsync(id);
            if (alert == null)
                return false;

            alert.IsAcknowledged = true;
            await _stockAlertRepository.UpdateAsync(alert);
            return true;
        }

        public async Task<bool> DeleteAlertAsync(int id)
        {
            var alert = await _stockAlertRepository.GetByIdAsync(id);
            if (alert == null)
                return false;

            await _stockAlertRepository.DeleteAsync(alert);
            return true;
        }
    }
}
