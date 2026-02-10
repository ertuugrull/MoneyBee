using MoneyBee.Customer.Services.Interfaces;
using MoneyBee.Shared.Models;

namespace MoneyBee.Customer.Services;

public class TransferNotificationService : ITransferNotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TransferNotificationService> _logger;
    private const string ClientName = "TransferService";

    public TransferNotificationService(IHttpClientFactory httpClientFactory, ILogger<TransferNotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(ClientName);

    public async Task NotifyCustomerStatusChangedAsync(Guid customerId, CustomerStatus newStatus)
    {
        try
        {
            if (newStatus == CustomerStatus.Blocked)
            {
                var client = GetClient();
                var request = new { customerId };
                var response = await client.PostAsJsonAsync("/api/transfers/customer-blocked", request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Transfer service returned status {StatusCode} for blocked customer notification", response.StatusCode);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying transfer service about customer status change");
        }
    }
}
