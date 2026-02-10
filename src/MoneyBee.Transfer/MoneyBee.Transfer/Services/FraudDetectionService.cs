using System.Text.Json;
using MoneyBee.Shared.Models;
using MoneyBee.Transfer.Services.Interfaces;
using MoneyBee.Transfer.Services.Models;

namespace MoneyBee.Transfer.Services;

public class FraudDetectionService : IFraudDetectionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FraudDetectionService> _logger;
    private const string ClientName = "FraudService";

    public FraudDetectionService(IHttpClientFactory httpClientFactory, ILogger<FraudDetectionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(ClientName);

    public async Task<FraudCheckResult> CheckTransferAsync(Guid transactionId, Guid senderId, Guid receiverId, decimal amount, string currency)
    {
        try
        {
            var client = GetClient();
            var request = new FraudCheckRequest
            {
                TransactionId = transactionId.ToString(),
                UserId = senderId.ToString(),
                ToUserId = receiverId.ToString(),
                Amount = amount,
                Currency = currency.ToUpperInvariant()
            };

            var response = await client.PostAsJsonAsync("/api/fraud/check", request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            var result = JsonSerializer.Deserialize<FraudApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (response.IsSuccessStatusCode && result?.Success == true && result.Data != null)
            {
                return new FraudCheckResult
                {
                    RiskLevel = MapRiskLevel(result.Data.RiskLevel),
                    Reason = result.Data.Reason,
                    TransactionId = result.Data.TransactionId,
                    RiskFactors = result.Data.RiskFactors,
                    Recommendation = result.Data.Recommendation
                };
            }

            var errorMessage = result?.Error ?? result?.Message ?? "Fraud check failed";
            if (result?.Details?.Length > 0)
            {
                var messages = result.Details.Select(d => d.Message).Where(m => !string.IsNullOrEmpty(m));
                errorMessage = string.Join("; ", messages);
            }
            
            _logger.LogWarning("Fraud service returned: {ErrorMessage}", errorMessage);
            
            return new FraudCheckResult 
            { 
                RiskLevel = RiskLevel.Medium, 
                Reason = errorMessage,
                ServiceError = true
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling Fraud Detection service");
            return new FraudCheckResult 
            { 
                RiskLevel = RiskLevel.Medium, 
                Reason = $"Fraud service network error: {ex.Message}",
                ServiceError = true
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout calling Fraud Detection service");
            return new FraudCheckResult 
            { 
                RiskLevel = RiskLevel.Medium, 
                Reason = "Fraud service timeout",
                ServiceError = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Fraud Detection service");
            return new FraudCheckResult 
            { 
                RiskLevel = RiskLevel.Medium, 
                Reason = $"Fraud service error: {ex.Message}",
                ServiceError = true
            };
        }
    }

    private static RiskLevel MapRiskLevel(string? level)
    {
        return level?.ToUpperInvariant() switch
        {
            "LOW" => RiskLevel.Low,
            "HIGH" => RiskLevel.High,
            _ => RiskLevel.Medium
        };
    }
}
