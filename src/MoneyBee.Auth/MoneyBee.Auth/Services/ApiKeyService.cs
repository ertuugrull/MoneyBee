using MoneyBee.Auth.Data;
using MoneyBee.Auth.Models;

namespace MoneyBee.Auth.Services;

public interface IApiKeyService
{
    ApiKey? ValidateKey(string key);
    ApiKey CreateKey(string name, int? expiresInDays);
    bool RevokeKey(Guid id);
    IEnumerable<ApiKey> GetAllKeys();
}

public class ApiKeyService : IApiKeyService
{
    private readonly ApiKeyStore _store;

    public ApiKeyService(ApiKeyStore store)
    {
        _store = store;
    }

    public ApiKey? ValidateKey(string key)
    {
        var apiKey = _store.GetByKey(key);
        if (apiKey == null || !apiKey.IsActive)
            return null;

        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
            return null;

        return apiKey;
    }

    public ApiKey CreateKey(string name, int? expiresInDays)
    {
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            Key = GenerateApiKey(),
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresInDays.HasValue 
                ? DateTime.UtcNow.AddDays(expiresInDays.Value) 
                : null
        };

        return _store.Add(apiKey);
    }

    public bool RevokeKey(Guid id)
    {
        return _store.Remove(id);
    }

    public IEnumerable<ApiKey> GetAllKeys()
    {
        return _store.GetAll();
    }

    private static string GenerateApiKey()
    {
        return $"mb_{Guid.NewGuid():N}";
    }
}
