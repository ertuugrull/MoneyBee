using MoneyBee.Shared.Models;

namespace MoneyBee.Transfer.Models;

public class CreateTransferRequest
{
    public Guid SenderCustomerId { get; set; }
    public Guid ReceiverCustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
}

public class TransferResponse
{
    public Guid Id { get; set; }
    public Guid SenderCustomerId { get; set; }
    public Guid ReceiverCustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal? ExchangeRate { get; set; }
    public decimal? AmountInTry { get; set; }
    public decimal Fee { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public TransferStatus Status { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ApprovalDueAt { get; set; }
}

public class CustomerBlockedRequest
{
    public Guid CustomerId { get; set; }
}
