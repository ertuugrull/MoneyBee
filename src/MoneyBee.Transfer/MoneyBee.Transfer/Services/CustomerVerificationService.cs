using System.Text.Json;
using MoneyBee.Shared.Models;
using MoneyBee.Transfer.Services.Interfaces;
using MoneyBee.Transfer.Services.Models;

namespace MoneyBee.Transfer.Services;

public class CustomerVerificationService : ICustomerVerificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CustomerVerificationService> _logger;
    private const string ClientName = "CustomerService";

    public CustomerVerificationService(IHttpClientFactory httpClientFactory, ILogger<CustomerVerificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(ClientName);

    public async Task<CustomerVerificationResult> VerifyCustomerAsync(Guid customerId)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/customers/{customerId}/verify");
            var responseContent = await response.Content.ReadAsStringAsync();
            
            var apiResponse = JsonSerializer.Deserialize<CustomerApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (response.IsSuccessStatusCode && apiResponse?.Success == true && apiResponse.Data != null)
            {
                return new CustomerVerificationResult
                {
                    Success = true,
                    Customer = new CustomerVerification
                    {
                        Id = apiResponse.Data.Id,
                        FullName = apiResponse.Data.FullName,
                        IsActive = apiResponse.Data.IsActive,
                        Status = apiResponse.Data.Status
                    }
                };
            }

            var errorMessage = apiResponse?.Message ?? apiResponse?.Error ?? $"Customer verification failed ({response.StatusCode})";
            
            _logger.LogWarning("Customer verification failed for {CustomerId}: {ErrorMessage}", customerId, errorMessage);
            
            return new CustomerVerificationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error verifying customer {CustomerId}", customerId);
            return new CustomerVerificationResult
            {
                Success = false,
                ErrorMessage = $"Customer service network error: {ex.Message}"
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout verifying customer {CustomerId}", customerId);
            return new CustomerVerificationResult
            {
                Success = false,
                ErrorMessage = "Customer service timeout"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying customer {CustomerId}", customerId);
            return new CustomerVerificationResult
            {
                Success = false,
                ErrorMessage = $"Customer service error: {ex.Message}"
            };
        }
    }
}
