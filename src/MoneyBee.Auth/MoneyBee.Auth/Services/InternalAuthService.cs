using MoneyBee.Auth.Data;
using MoneyBee.Auth.Services;
using MoneyBee.Shared.Models;
using MoneyBee.Shared.Services;

namespace MoneyBee.Auth.Services;

public class InternalAuthService : IInternalAuthService
{
    private readonly IApiKeyService _apiKeyService;
    private readonly RateLimitStore _rateLimitStore;

    public InternalAuthService(IApiKeyService apiKeyService, RateLimitStore rateLimitStore)
    {
        _apiKeyService = apiKeyService;
        _rateLimitStore = rateLimitStore;
    }

    public Task<AuthValidationResponse> ValidateApiKeyAsync(string apiKey)
    {
        var result = _apiKeyService.ValidateKey(apiKey);
        return Task.FromResult(new AuthValidationResponse
        {
            IsValid = result != null,
            KeyName = result?.Name,
            KeyId = result?.Id
        });
    }

    public Task<RateLimitResponse> CheckRateLimitAsync(string identifier)
    {
        var isAllowed = _rateLimitStore.IsAllowed(identifier, out var remaining, out var resetTime);
        return Task.FromResult(new RateLimitResponse
        {
            IsAllowed = isAllowed,
            Remaining = remaining,
            ResetTime = resetTime
        });
    }
}
