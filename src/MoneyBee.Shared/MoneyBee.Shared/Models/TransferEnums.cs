namespace MoneyBee.Shared.Models;

public enum TransferStatus
{
    Pending,
    AwaitingApproval,
    Completed,
    Cancelled,
    Failed
}

public enum RiskLevel
{
    Low,
    Medium,
    High
}
