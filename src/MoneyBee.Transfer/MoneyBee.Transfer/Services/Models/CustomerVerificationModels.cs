using MoneyBee.Shared.Models;

namespace MoneyBee.Transfer.Services.Models;

public class CustomerVerification
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public CustomerStatus Status { get; set; }
}

public class CustomerVerificationResult
{
    public bool Success { get; set; }
    public CustomerVerification? Customer { get; set; }
    public string? ErrorMessage { get; set; }
}

internal class CustomerApiResponse
{
    public bool Success { get; set; }
    public CustomerData? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}

internal class CustomerData
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public CustomerStatus Status { get; set; }
}
