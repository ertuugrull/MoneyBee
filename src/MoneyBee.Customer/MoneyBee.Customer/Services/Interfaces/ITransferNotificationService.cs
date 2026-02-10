namespace MoneyBee.Customer.Services.Interfaces;

public interface ITransferNotificationService
{
    Task NotifyCustomerStatusChangedAsync(Guid customerId, MoneyBee.Shared.Models.CustomerStatus newStatus);
}
