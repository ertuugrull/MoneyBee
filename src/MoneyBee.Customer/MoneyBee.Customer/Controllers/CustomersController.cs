using Microsoft.AspNetCore.Mvc;
using MoneyBee.Customer.Models;
using MoneyBee.Customer.Services.Interfaces;
using MoneyBee.Shared.Exceptions;
using MoneyBee.Shared.Models;

namespace MoneyBee.Customer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResult<CustomerResponse>>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var response = await _customerService.CreateCustomerAsync(request);
        return Ok(ServiceResult<CustomerResponse>.Ok(response, "Customer created successfully"));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ServiceResult<CustomerResponse>> GetById(Guid id)
    {
        var response = _customerService.GetById(id);
        if (response == null)
        {
            throw new NotFoundException("Customer", id);
        }

        return Ok(ServiceResult<CustomerResponse>.Ok(response));
    }

    [HttpGet("by-national-id/{nationalId}")] // burasý aslýnda güvenlik zaafý yaratabilir , saldýrganlarýn en sevdiði yerlerden biri.
    public ActionResult<ServiceResult<CustomerResponse>> GetByNationalId(string nationalId)
    {
        var response = _customerService.GetByNationalId(nationalId);
        if (response == null)
        {
            throw new NotFoundException("Customer", nationalId);
        }

        return Ok(ServiceResult<CustomerResponse>.Ok(response));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ServiceResult<CustomerResponse>>> UpdateStatus(Guid id, [FromBody] UpdateCustomerStatusRequest request)
    {
        var response = await _customerService.UpdateStatusAsync(id, request.Status);
        return Ok(ServiceResult<CustomerResponse>.Ok(response, "Status updated successfully"));
    }

    [HttpGet("{id:guid}/verify")] 
    public ActionResult<ServiceResult<CustomerVerificationResponse>> VerifyCustomer(Guid id)
    {
        var verification = _customerService.VerifyCustomer(id);
        if (verification == null)
        {
            throw new NotFoundException("Customer", id);
        }

        return Ok(ServiceResult<CustomerVerificationResponse>.Ok(verification));
    }
}
