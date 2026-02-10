using MoneyBee.Transfer.Services.Models;

namespace MoneyBee.Transfer.Services.Interfaces;

public interface IFraudDetectionService
{
    Task<FraudCheckResult> CheckTransferAsync(Guid transactionId, Guid senderId, Guid receiverId, decimal amount, string currency);
}
