namespace MoneyBee.Auth.Models;

public class ApiKey
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class CreateApiKeyRequest
{
    public string Name { get; set; } = string.Empty;
    public int? ExpiresInDays { get; set; }
}

public class ApiKeyResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
