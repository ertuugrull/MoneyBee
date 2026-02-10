using System.Collections.Concurrent;
using MoneyBee.Shared.Models;
using MoneyBee.Transfer.Entities;

namespace MoneyBee.Transfer.Data;

public class TransferStore
{
    private readonly ConcurrentDictionary<Guid, Entities.Transfer> _transfers = new();
    private readonly ConcurrentDictionary<string, Guid> _codeIndex = new();

    public Entities.Transfer? GetById(Guid id)
    {
        _transfers.TryGetValue(id, out var transfer);
        return transfer;
    }

    public Entities.Transfer? GetByCode(string code)
    {
        if (_codeIndex.TryGetValue(code.ToUpperInvariant(), out var id))
        {
            return GetById(id);
        }
        return null;
    }

    public Entities.Transfer Add(Entities.Transfer transfer)
    {
        _transfers[transfer.Id] = transfer;
        _codeIndex[transfer.TransactionCode.ToUpperInvariant()] = transfer.Id;
        return transfer;
    }

    public Entities.Transfer Update(Entities.Transfer transfer)
    {
        _transfers[transfer.Id] = transfer;
        return transfer;
    }

    public IEnumerable<Entities.Transfer> GetByCustomerId(Guid customerId)
    {
        return _transfers.Values.Where(t => 
            t.SenderCustomerId == customerId || t.ReceiverCustomerId == customerId);
    }

    public IEnumerable<Entities.Transfer> GetPendingTransfersBySender(Guid senderId)
    {
        return _transfers.Values.Where(t => 
            t.SenderCustomerId == senderId && 
            (t.Status == TransferStatus.Pending || t.Status == TransferStatus.AwaitingApproval));
    }

    public decimal GetDailyTotalBySender(Guid senderId, DateTime date)
    {
        return _transfers.Values
            .Where(t => t.SenderCustomerId == senderId && 
                        t.CreatedAt.Date == date.Date &&
                        t.Status != TransferStatus.Cancelled &&
                        t.Status != TransferStatus.Failed)
            .Sum(t => t.AmountInTry ?? t.Amount);
    }

    public IEnumerable<Entities.Transfer> GetAwaitingApprovalTransfers()
    {
        return _transfers.Values.Where(t => 
            t.Status == TransferStatus.AwaitingApproval &&
            t.ApprovalDueAt.HasValue &&
            t.ApprovalDueAt.Value <= DateTime.UtcNow);
    }

    public IEnumerable<Entities.Transfer> GetAll()
    {
        return _transfers.Values;
    }
}
