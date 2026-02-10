using MoneyBee.Shared.Models;

namespace MoneyBee.Transfer.Services.Models;

public class FraudCheckResult
{
    public RiskLevel RiskLevel { get; set; }
    public string? Reason { get; set; }
    public bool ServiceError { get; set; }
    public string? TransactionId { get; set; }
    public string[]? RiskFactors { get; set; }
    public string? Recommendation { get; set; }
}

internal class FraudCheckRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
}

internal class FraudApiResponse
{
    public bool Success { get; set; }
    public FraudDataResponse? Data { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public FraudApiError[]? Details { get; set; }
}

internal class FraudDataResponse
{
    public string? TransactionId { get; set; }
    public string? RiskLevel { get; set; }
    public string? Reason { get; set; }
    public string[]? RiskFactors { get; set; }
    public string? Recommendation { get; set; }
}

internal class FraudApiError
{
    public string? Field { get; set; }
    public string? Message { get; set; }
}
