using MoneyBee.Shared.Models;

namespace MoneyBee.Customer.Models;

public class CreateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public CustomerType Type { get; set; }
    public string? TaxNumber { get; set; }
}

public class UpdateCustomerStatusRequest
{
    public CustomerStatus Status { get; set; }
}

public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public CustomerType Type { get; set; }
    public CustomerStatus Status { get; set; }
    public string? TaxNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerVerificationResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public CustomerStatus Status { get; set; }
}
