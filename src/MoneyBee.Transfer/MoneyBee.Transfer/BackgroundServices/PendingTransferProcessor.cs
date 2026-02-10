using MoneyBee.Transfer.Services;
using MoneyBee.Transfer.Services.Interfaces;

namespace MoneyBee.Transfer.BackgroundServices;

public class PendingTransferProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingTransferProcessor> _logger;

    public PendingTransferProcessor(IServiceProvider serviceProvider, ILogger<PendingTransferProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var transferService = scope.ServiceProvider.GetRequiredService<ITransferService>();
                await transferService.ProcessAwaitingApprovalTransfersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing awaiting approval transfers");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
