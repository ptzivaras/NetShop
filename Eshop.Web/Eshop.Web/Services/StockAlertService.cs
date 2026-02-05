using Eshop.Contracts.DTOs;
using RestSharp;

namespace Eshop.Web.Services
{
    public interface IStockAlertService
    {
        Task<int> GetUnacknowledgedCountAsync();
        Task<List<StockAlertDto>> GetAllAlertsAsync();
        Task<bool> AcknowledgeAlertAsync(int id);
        Task<bool> DeleteAlertAsync(int id);
    }

    public class StockAlertService : IStockAlertService
    {
        private readonly RestClient _client;
        private readonly ILogger<StockAlertService> _logger;

        public StockAlertService(IConfiguration configuration, ILogger<StockAlertService> logger)
        {
            _logger = logger;
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] 
                ?? throw new InvalidOperationException("API BaseUrl not configured in appsettings.json");
            _client = new RestClient(apiBaseUrl);
        }

        public async Task<int> GetUnacknowledgedCountAsync()
        {
            try
            {
                var request = new RestRequest("stockalerts/unacknowledged/count", Method.Get);
                var response = await _client.ExecuteAsync<int>(request);

                if (!response.IsSuccessful)
                {
                    _logger.LogWarning("Failed to get unacknowledged alerts count: {StatusCode}", response.StatusCode);
                    return 0;
                }

                return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unacknowledged alerts count");
                return 0;
            }
        }

        public async Task<List<StockAlertDto>> GetAllAlertsAsync()
        {
            try
            {
                var request = new RestRequest("stockalerts", Method.Get);
                var response = await _client.ExecuteAsync<List<StockAlertDto>>(request);

                if (!response.IsSuccessful || response.Data == null)
                {
                    _logger.LogWarning("Failed to get stock alerts: {StatusCode}", response.StatusCode);
                    return new List<StockAlertDto>();
                }

                return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock alerts");
                return new List<StockAlertDto>();
            }
        }

        public async Task<bool> AcknowledgeAlertAsync(int id)
        {
            try
            {
                var request = new RestRequest($"stockalerts/{id}/acknowledge", Method.Put);
                var response = await _client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging alert {AlertId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteAlertAsync(int id)
        {
            try
            {
                var request = new RestRequest($"stockalerts/{id}", Method.Delete);
                var response = await _client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting alert {AlertId}", id);
                return false;
            }
        }
    }
}
