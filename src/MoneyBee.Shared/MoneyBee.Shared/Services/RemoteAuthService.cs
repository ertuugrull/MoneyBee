using System.Net.Http.Json;
using MoneyBee.Shared.Models;

namespace MoneyBee.Shared.Services;

public class RemoteAuthService : IInternalAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string ClientName = "AuthService";

    public RemoteAuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(ClientName);

    public async Task<AuthValidationResponse> ValidateApiKeyAsync(string apiKey)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"api/internal/auth/validate?apiKey={Uri.EscapeDataString(apiKey)}");
            if (!response.IsSuccessStatusCode)
                return new AuthValidationResponse { IsValid = false };

            return await response.Content.ReadFromJsonAsync<AuthValidationResponse>() 
                   ?? new AuthValidationResponse { IsValid = false };
        }
        catch
        {
            return new AuthValidationResponse { IsValid = false };
        }
    }

    public async Task<RateLimitResponse> CheckRateLimitAsync(string identifier)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"api/internal/auth/rate-limit?identifier={Uri.EscapeDataString(identifier)}");
            if (!response.IsSuccessStatusCode)
                return new RateLimitResponse { IsAllowed = true, Remaining = 100, ResetTime = DateTime.UtcNow.AddMinutes(1) }; 
            return await response.Content.ReadFromJsonAsync<RateLimitResponse>() 
                   ?? new RateLimitResponse { IsAllowed = true };
        }
        catch
        {
            return new RateLimitResponse { IsAllowed = true };
        }
    }
}
