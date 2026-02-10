using MoneyBee.Customer.Models;
using MoneyBee.Shared.Models;

namespace MoneyBee.Customer.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
    CustomerResponse? GetById(Guid id);
    CustomerResponse? GetByNationalId(string nationalId);
    Task<CustomerResponse> UpdateStatusAsync(Guid id, CustomerStatus status);
    CustomerVerificationResponse? VerifyCustomer(Guid id);
}
