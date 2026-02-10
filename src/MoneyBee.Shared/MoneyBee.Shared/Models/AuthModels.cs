namespace MoneyBee.Shared.Models;

public class AuthValidationResponse
{
    public bool IsValid { get; set; }
    public string? KeyName { get; set; }
    public Guid? KeyId { get; set; }
}

public class RateLimitResponse
{
    public bool IsAllowed { get; set; }
    public int Remaining { get; set; }
    public DateTime ResetTime { get; set; }
}
