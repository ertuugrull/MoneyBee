using AutoMapper;
using MoneyBee.Customer.Data;
using MoneyBee.Customer.Models;
using MoneyBee.Customer.Services.Interfaces;
using MoneyBee.Shared.Exceptions;
using MoneyBee.Shared.Models;

namespace MoneyBee.Customer.Services;

public class CustomerService : ICustomerService
{
    private readonly CustomerStore _customerDb;
    private readonly IKycService _kycService;
    private readonly ITransferNotificationService _transferNotificationService;
    private readonly IMapper _mapper;

    public CustomerService(
        CustomerStore store, 
        IKycService kycService,
        ITransferNotificationService transferNotificationService,
        IMapper mapper)
    {
        _customerDb = store;
        _kycService = kycService;
        _transferNotificationService = transferNotificationService;
        _mapper = mapper;
    }

    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var existingCustomer = _customerDb.GetByNationalId(request.NationalId);
        if (existingCustomer != null)
        {
            throw new ValidationException("Customer with this National ID already exists");
        }

        var customerId = Guid.NewGuid();
        
        var kycResult = await _kycService.VerifyCustomerAsync(
            customerId, 
            request.NationalId, 
            request.BirthDate);

        if (!kycResult.IsVerified)
        {
            throw new ValidationException($"KYC verification failed: {kycResult.RejectionReason}");
        }

        var customer = new Entities.Customer
        {
            Id = customerId,
            Name = request.Name,
            Surname = request.Surname,
            NationalId = request.NationalId,
            PhoneNumber = request.PhoneNumber,
            BirthDate = request.BirthDate,
            Type = request.Type,
            TaxNumber = request.TaxNumber,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var saved = _customerDb.Add(customer);
        return _mapper.Map<CustomerResponse>(saved);
    }

    public CustomerResponse? GetById(Guid id)
    {
        var customer = _customerDb.GetById(id);
        return customer == null ? null : _mapper.Map<CustomerResponse>(customer);
    }

    public CustomerResponse? GetByNationalId(string nationalId)
    {
        var customer = _customerDb.GetByNationalId(nationalId);
        return customer == null ? null : _mapper.Map<CustomerResponse>(customer);
    }

    public async Task<CustomerResponse> UpdateStatusAsync(Guid id, CustomerStatus status)
    {
        var customer = _customerDb.GetById(id);
        if (customer == null)
        {
            throw new NotFoundException("Customer", id);
        }

        var oldStatus = customer.Status;
        customer.Status = status;
        customer.UpdatedAt = DateTime.UtcNow;
        _customerDb.Update(customer);

        if (oldStatus != status)
        {
            await _transferNotificationService.NotifyCustomerStatusChangedAsync(id, status);
        }

        return _mapper.Map<CustomerResponse>(customer);
    }

    public CustomerVerificationResponse? VerifyCustomer(Guid id)
    {
        var customer = _customerDb.GetById(id);
        if (customer == null) return null;

        return _mapper.Map<CustomerVerificationResponse>(customer);
    }
}
