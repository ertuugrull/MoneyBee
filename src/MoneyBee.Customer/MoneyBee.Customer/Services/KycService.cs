using System.Text.Json;
using MoneyBee.Customer.Services.Interfaces;
using MoneyBee.Customer.Services.Models;

namespace MoneyBee.Customer.Services;

public class KycService : IKycService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<KycService> _logger;
    private const string ClientName = "KycService";

    public KycService(IHttpClientFactory httpClientFactory, ILogger<KycService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(ClientName);

    public async Task<KycVerificationResult> VerifyCustomerAsync(Guid customerId, string nationalId, DateTime birthDate)
    {
        try
        {
            var client = GetClient();
            var request = new KycVerifyRequest
            {
                UserId = customerId.ToString(),
                Tcno = nationalId,
                BirthYear = birthDate.Year
            };

            var response = await client.PostAsJsonAsync("/api/kyc/verify", request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            var result = JsonSerializer.Deserialize<KycApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (response.IsSuccessStatusCode && result?.Success == true)
            {
                return new KycVerificationResult
                {
                    IsVerified = result.Verified,
                    RejectionReason = result.Verified == false ? result.Reason : null,
                    VerificationId = result.VerificationId,
                    Level = result.Level,
                    VerificationScore = result.VerificationScore
                };
            }

            var errorMessage = result?.Message ?? "KYC verification failed";
            if (result?.Errors?.Length > 0)
            {
                var messages = result.Errors.Select(e => e.Message).Where(m => !string.IsNullOrEmpty(m));
                errorMessage = string.Join("; ", messages);
            }
            
            _logger.LogWarning("KYC service returned: {ErrorMessage}", errorMessage);
            
            return new KycVerificationResult
            {
                IsVerified = false,
                RejectionReason = errorMessage
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling KYC service");
            return new KycVerificationResult
            {
                IsVerified = false,
                RejectionReason = $"KYC service network error: {ex.Message}"
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout calling KYC service");
            return new KycVerificationResult
            {
                IsVerified = false,
                RejectionReason = "KYC service timeout"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling KYC service");
            return new KycVerificationResult
            {
                IsVerified = false,
                RejectionReason = $"KYC service error: {ex.Message}"
            };
        }
    }
}
