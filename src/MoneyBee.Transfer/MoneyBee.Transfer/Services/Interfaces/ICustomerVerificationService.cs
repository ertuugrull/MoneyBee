using MoneyBee.Transfer.Services.Models;

namespace MoneyBee.Transfer.Services.Interfaces;

public interface ICustomerVerificationService
{
    Task<CustomerVerificationResult> VerifyCustomerAsync(Guid customerId);
}
