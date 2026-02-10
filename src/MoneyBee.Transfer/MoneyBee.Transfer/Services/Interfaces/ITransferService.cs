using MoneyBee.Transfer.Models;

namespace MoneyBee.Transfer.Services.Interfaces;

public interface ITransferService
{
    Task<TransferResponse> CreateTransferAsync(CreateTransferRequest request);
    TransferResponse? GetById(Guid id);
    TransferResponse? GetByCode(string code);
    Task<TransferResponse> CompleteTransferAsync(Guid id);
    Task<TransferResponse> CancelTransferAsync(Guid id);
    Task<decimal> GetDailyTotalAsync(Guid customerId);
    Task CancelPendingTransfersForBlockedCustomerAsync(Guid customerId);
    Task ProcessAwaitingApprovalTransfersAsync();
}
