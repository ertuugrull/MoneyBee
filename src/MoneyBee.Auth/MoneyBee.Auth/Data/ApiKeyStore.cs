using System.Collections.Concurrent;
using MoneyBee.Auth.Models;

namespace MoneyBee.Auth.Data;

public class ApiKeyStore
{
    private readonly ConcurrentDictionary<string, ApiKey> _keys = new();
    private readonly ConcurrentDictionary<Guid, ApiKey> _keysById = new();

    public ApiKeyStore()
    {
        var defaultKey = new ApiKey
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Key = "moneybee-default-api-key-2026",
            Name = "Default API Key",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _keys[defaultKey.Key] = defaultKey;
        _keysById[defaultKey.Id] = defaultKey;
    }

    public ApiKey? GetByKey(string key)
    {
        _keys.TryGetValue(key, out var apiKey);
        return apiKey;
    }

    public ApiKey? GetById(Guid id)
    {
        _keysById.TryGetValue(id, out var apiKey);
        return apiKey;
    }

    public ApiKey Add(ApiKey apiKey)
    {
        _keys[apiKey.Key] = apiKey;
        _keysById[apiKey.Id] = apiKey;
        return apiKey;
    }

    public bool Remove(Guid id)
    {
        if (_keysById.TryRemove(id, out var apiKey))
        {
            _keys.TryRemove(apiKey.Key, out _);
            return true;
        }
        return false;
    }

    public IEnumerable<ApiKey> GetAll() => _keysById.Values;
}
