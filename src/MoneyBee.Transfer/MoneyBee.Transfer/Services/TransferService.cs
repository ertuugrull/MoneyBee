using AutoMapper;
using Microsoft.Extensions.Logging;
using MoneyBee.Shared.Exceptions;
using MoneyBee.Shared.Models;
using MoneyBee.Shared.Utilities;
using MoneyBee.Transfer.Data;
using MoneyBee.Transfer.Models;
using MoneyBee.Transfer.Services.Interfaces;
using MoneyBee.Transfer.Services.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MoneyBee.Transfer.Services;

public class TransferService : ITransferService
{
    private readonly TransferStore _transferDb;
    private readonly IFraudDetectionService _fraudService;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ICustomerVerificationService _customerVerificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<TransferService> _logger;

    // redlock yerine semaphore işimizi görücektir.
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _customerLocks = new();

    private const decimal DailyLimitTry = 50000m;
    private const decimal HighValueThresholdTry = 10000m;
    private const decimal FeePercentage = 0.01m;

    public TransferService(
        TransferStore transferDb,
        IFraudDetectionService fraudService,
        IExchangeRateService exchangeRateService,
        ICustomerVerificationService customerVerificationService,
        IMapper mapper,
        ILogger<TransferService> logger)
    {
        _transferDb = transferDb;
        _fraudService = fraudService;
        _exchangeRateService = exchangeRateService;
        _customerVerificationService = customerVerificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransferResponse> CreateTransferAsync(CreateTransferRequest request)
    {
        var senderResult = await _customerVerificationService.VerifyCustomerAsync(request.SenderCustomerId);
        if (!senderResult.Success || senderResult.Customer == null)
        {
            throw new ValidationException(senderResult.ErrorMessage ?? "Sender customer not found or inactive");
        }
        if (!senderResult.Customer.IsActive)
            throw new ValidationException("Sender customer is not active");

        var receiverResult = await _customerVerificationService.VerifyCustomerAsync(request.ReceiverCustomerId);
        if (!receiverResult.Success || receiverResult.Customer == null)
        {
            throw new ValidationException(receiverResult.ErrorMessage ?? "Receiver customer not found or inactive");
        }
        if (!receiverResult.Customer.IsActive)
            throw new ValidationException("Receiver customer is not active");

        decimal exchangeRate = 1m;
        decimal amountInTry = request.Amount;

        if (!request.Currency.Equals("TRY", StringComparison.OrdinalIgnoreCase))
        {
            var rateResult = await _exchangeRateService.GetRateAsync("TRY", request.Currency);
            if (!rateResult.Success)
                throw new ValidationException(rateResult.ErrorMessage ?? $"Unable to get exchange rate for {request.Currency} to TRY");

            exchangeRate = rateResult.Rate;
            amountInTry = request.Amount * exchangeRate;
        }

        // race conditionun önüne geçmek amacıyla basit bir lock mekanizması kuralım, performans için yukardan ayrı tuttum.
        var semaphore = _customerLocks.GetOrAdd(request.SenderCustomerId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        try
        {
            var dailyTotal = CalculateDailyTotal(request.SenderCustomerId);
            if (dailyTotal + amountInTry > DailyLimitTry)
                throw new ValidationException($"Daily transfer limit exceeded. Current: {dailyTotal:N2} TRY, Limit: {DailyLimitTry:N2} TRY");

            var transferId = Guid.NewGuid();
            var fraudResult = await _fraudService.CheckTransferAsync(transferId, request.SenderCustomerId, request.ReceiverCustomerId, amountInTry, "TRY");

            if (fraudResult.RiskLevel == RiskLevel.High)
            {
                var failedTransfer = CreateTransferEntity(transferId, request, amountInTry, exchangeRate, TransferStatus.Failed);
                failedTransfer.RiskLevel = RiskLevel.High;
                failedTransfer.FailureReason = $"High risk transfer rejected: {fraudResult.Reason}";
                _transferDb.Add(failedTransfer);
                throw new ValidationException($"Transfer rejected due to high risk: {fraudResult.Reason}");
            }

            var status = TransferStatus.Pending;
            DateTime? approvalDueAt = null;

            if (amountInTry > HighValueThresholdTry)
            {
                status = TransferStatus.AwaitingApproval;
                approvalDueAt = DateTime.UtcNow.AddMinutes(5);
            }

            var transfer = CreateTransferEntity(transferId, request, amountInTry, exchangeRate, status);
            transfer.RiskLevel = fraudResult.RiskLevel;
            transfer.ApprovalDueAt = approvalDueAt;

            var saved = _transferDb.Add(transfer);
            return _mapper.Map<TransferResponse>(saved);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public TransferResponse? GetById(Guid id)
    {
        var transfer = _transferDb.GetById(id);
        return transfer == null ? null : _mapper.Map<TransferResponse>(transfer);
    }

    public TransferResponse? GetByCode(string code)
    {
        var transfer = _transferDb.GetByCode(code);
        return transfer == null ? null : _mapper.Map<TransferResponse>(transfer);
    }

    public async Task<TransferResponse> CompleteTransferAsync(Guid id)
    {
        var transfer = _transferDb.GetById(id);
        if (transfer == null)
            throw new NotFoundException("Transfer", id);

        if (transfer.Status != TransferStatus.AwaitingApproval && transfer.Status != TransferStatus.Pending)
            throw new ValidationException($"Transfer cannot be completed. Current status: {transfer.Status}");

        transfer.Status = TransferStatus.Completed;
        transfer.CompletedAt = DateTime.UtcNow;
        _transferDb.Update(transfer);

        _logger.LogInformation("Transfer {TransferId} completed", transfer.Id);
        return _mapper.Map<TransferResponse>(transfer);
    }

    public async Task<TransferResponse> CancelTransferAsync(Guid id)
    {
        var transfer = _transferDb.GetById(id);
        if (transfer == null)
            throw new NotFoundException("Transfer", id);

        if (transfer.Status == TransferStatus.Completed || transfer.Status == TransferStatus.Cancelled || transfer.Status == TransferStatus.Failed)
            throw new ValidationException($"Transfer cannot be cancelled. Current status: {transfer.Status}");

        transfer.Status = TransferStatus.Cancelled;
        transfer.FailureReason = "Cancelled by user";
        _transferDb.Update(transfer);

        _logger.LogInformation("Transfer {TransferId} cancelled", transfer.Id);
        return _mapper.Map<TransferResponse>(transfer);
    }

    public async Task<decimal> GetDailyTotalAsync(Guid customerId)
    {
        var verification = await _customerVerificationService.VerifyCustomerAsync(customerId);
        if (!verification.Success || verification.Customer == null)
            throw new NotFoundException("Customer", customerId);

        return CalculateDailyTotal(customerId);
    }

    private decimal CalculateDailyTotal(Guid customerId)
    {
        var today = DateTime.UtcNow.Date;
        return _transferDb.GetAll()
            .Where(t => t.SenderCustomerId == customerId &&
                       t.CreatedAt.Date == today &&
                       t.Status != TransferStatus.Failed &&
                       t.Status != TransferStatus.Cancelled)
            .Sum(t => t.AmountInTry ?? t.Amount);
    }

    public Task CancelPendingTransfersForBlockedCustomerAsync(Guid customerId)
    {
        var pendingTransfers = _transferDb.GetAll()
            .Where(t => (t.SenderCustomerId == customerId || t.ReceiverCustomerId == customerId) &&
                       (t.Status == TransferStatus.Pending || t.Status == TransferStatus.AwaitingApproval))
            .ToList();

        foreach (var transfer in pendingTransfers)
        {
            transfer.Status = TransferStatus.Cancelled;
            transfer.FailureReason = "Customer blocked";
            _transferDb.Update(transfer);
            _logger.LogInformation("Transfer {TransferId} cancelled due to blocked customer", transfer.Id);
        }

        return Task.CompletedTask;
    }

    public Task ProcessAwaitingApprovalTransfersAsync()
    {
        var now = DateTime.UtcNow;
        var expiredTransfers = _transferDb.GetAll()
            .Where(t => t.Status == TransferStatus.AwaitingApproval && t.ApprovalDueAt.HasValue && t.ApprovalDueAt.Value < now)
            .ToList();

        foreach (var transfer in expiredTransfers)
        {
            transfer.Status = TransferStatus.Pending;
            transfer.ApprovalDueAt = null;
            _transferDb.Update(transfer);
            _logger.LogInformation("Transfer {TransferId} moved from AwaitingApproval to Pending", transfer.Id);
        }
        return Task.CompletedTask;
    }

    private Entities.Transfer CreateTransferEntity(Guid transferId, CreateTransferRequest request, decimal amountInTry, decimal? exchangeRate, TransferStatus status)
    {
        return new Entities.Transfer
        {
            Id = transferId,
            SenderCustomerId = request.SenderCustomerId,
            ReceiverCustomerId = request.ReceiverCustomerId,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            ExchangeRate = exchangeRate,
            AmountInTry = request.Currency.Equals("TRY", StringComparison.OrdinalIgnoreCase) ? null : amountInTry,
            Fee = amountInTry * FeePercentage,
            TransactionCode = TransactionCodeGenerator.Generate(),
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }
}
