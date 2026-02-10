using Microsoft.AspNetCore.Mvc;
using MoneyBee.Shared.Exceptions;
using MoneyBee.Shared.Models;
using MoneyBee.Transfer.Models;
using MoneyBee.Transfer.Services.Interfaces;

namespace MoneyBee.Transfer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResult<TransferResponse>>> CreateTransfer([FromBody] CreateTransferRequest request)
    {
        var response = await _transferService.CreateTransferAsync(request);
        return Ok(ServiceResult<TransferResponse>.Ok(response, "Transfer created successfully"));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ServiceResult<TransferResponse>> GetById(Guid id)
    {
        var response = _transferService.GetById(id);
        if (response == null)
            throw new NotFoundException("Transfer", id);

        return Ok(ServiceResult<TransferResponse>.Ok(response));
    }

    [HttpGet("by-code/{code}")]
    public ActionResult<ServiceResult<TransferResponse>> GetByCode(string code)
    {
        var response = _transferService.GetByCode(code);
        if (response == null)
            throw new NotFoundException("Transfer", code);

        return Ok(ServiceResult<TransferResponse>.Ok(response));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ServiceResult<TransferResponse>>> CompleteTransfer(Guid id)
    {
        var response = await _transferService.CompleteTransferAsync(id);
        return Ok(ServiceResult<TransferResponse>.Ok(response, "Transfer completed successfully"));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ServiceResult<TransferResponse>>> CancelTransfer(Guid id)
    {
        var response = await _transferService.CancelTransferAsync(id);
        return Ok(ServiceResult<TransferResponse>.Ok(response, "Transfer cancelled successfully. Fee has been refunded."));
    }

    [HttpGet("customer/{customerId:guid}/daily-total")]
    public async Task<ActionResult<ServiceResult<decimal>>> GetDailyTotal(Guid customerId)
    {
        var total = await _transferService.GetDailyTotalAsync(customerId);
        return Ok(ServiceResult<decimal>.Ok(total));
    }

    [HttpPost("customer-blocked")]
    public async Task<ActionResult<ApiResponse>> HandleCustomerBlocked([FromBody] CustomerBlockedRequest request)
    {
        await _transferService.CancelPendingTransfersForBlockedCustomerAsync(request.CustomerId);
        return Ok(ApiResponse.Ok("Pending transfers cancelled for blocked customer"));
    }
}
