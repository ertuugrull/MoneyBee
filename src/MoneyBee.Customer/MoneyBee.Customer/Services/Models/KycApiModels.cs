namespace MoneyBee.Customer.Services.Models;

internal class KycVerifyRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Tcno { get; set; } = string.Empty;
    public int BirthYear { get; set; }
}

internal class KycApiResponse
{
    public bool Success { get; set; }
    public string? VerificationId { get; set; }
    public string? UserId { get; set; }
    public bool Verified { get; set; }
    public string? Reason { get; set; }
    public int? VerificationScore { get; set; }
    public string? Level { get; set; }
    public long? Timestamp { get; set; }
    public int? ProcessingTimeMs { get; set; }
    public string? Message { get; set; }
    public KycApiError[]? Errors { get; set; }
}

internal class KycApiError
{
    public string? Field { get; set; }
    public string? Message { get; set; }
}
