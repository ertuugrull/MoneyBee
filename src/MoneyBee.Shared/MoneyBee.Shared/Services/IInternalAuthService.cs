using MoneyBee.Shared.Models;

namespace MoneyBee.Shared.Services;

public interface IInternalAuthService
{
    Task<AuthValidationResponse> ValidateApiKeyAsync(string apiKey);
    Task<RateLimitResponse> CheckRateLimitAsync(string identifier);
}
