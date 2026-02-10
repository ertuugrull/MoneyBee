using MoneyBee.Shared.Models;

namespace MoneyBee.Transfer.Entities;

public class Transfer
{
    public Guid Id { get; set; }
    public Guid SenderCustomerId { get; set; }
    public Guid ReceiverCustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal? ExchangeRate { get; set; }
    public decimal? AmountInTry { get; set; }
    public decimal Fee { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public TransferStatus Status { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? ApprovalDueAt { get; set; }
    public bool FeeRefunded { get; set; }
    public string? FailureReason { get; set; }
}
