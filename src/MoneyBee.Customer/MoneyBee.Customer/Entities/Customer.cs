using MoneyBee.Shared.Models;

namespace MoneyBee.Customer.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public CustomerType Type { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public string? TaxNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
