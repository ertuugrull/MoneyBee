namespace MoneyBee.Transfer.Services.Models;

public class ExchangeRateResult
{
    public bool Success { get; set; }
    public decimal Rate { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FromCurrency { get; set; }
    public string? ToCurrency { get; set; }
}

internal class ExchangeRateApiResponse
{
    public string? From { get; set; }
    public string? To { get; set; }
    public decimal Rate { get; set; }
    public long? Timestamp { get; set; }
    public int? ValidForSeconds { get; set; }
}

internal class ExchangeRateErrorResponse
{
    public string? Error { get; set; }
    public string? Message { get; set; }
    public string[]? Supported { get; set; }
}
