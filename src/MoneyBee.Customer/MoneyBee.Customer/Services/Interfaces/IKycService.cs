using MoneyBee.Customer.Services.Models;

namespace MoneyBee.Customer.Services.Interfaces;

public interface IKycService
{
    Task<KycVerificationResult> VerifyCustomerAsync(Guid customerId, string nationalId, DateTime birthDate);
}
