using Eshop.Core.Models;
using Eshop.Core.Repositories;

namespace Eshop.API.BackgroundServices
{
    public class StockMonitorBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StockMonitorBackgroundService> _logger;
        private const int LowStockThreshold = 5;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

        public StockMonitorBackgroundService(IServiceScopeFactory scopeFactory, ILogger<StockMonitorBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stock monitor started. Threshold: {Threshold} units, Interval: {Interval}min",
                LowStockThreshold, Interval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckLowStockAsync();
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task CheckLowStockAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            var alertRepo = scope.ServiceProvider.GetRequiredService<IStockAlertRepository>();

            try
            {
                var lowStockProducts = await productRepo.GetLowStockAsync(LowStockThreshold);
                var created = 0;

                foreach (var product in lowStockProducts)
                {
                    if (await alertRepo.HasUnacknowledgedAlertAsync(product.Id))
                        continue;

                    await alertRepo.AddAsync(new StockAlert
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        QuantityAtTrigger = product.StockQuantity,
                        TriggeredAt = DateTime.UtcNow,
                        IsAcknowledged = false
                    });

                    created++;
                }

                if (created > 0)
                    _logger.LogWarning("Stock monitor: created {Count} new low-stock alert(s).", created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stock monitor failed during check.");
            }
        }
    }
}
