using FluentValidation;
using MoneyBee.Customer.Models;
using MoneyBee.Shared.Models;
using MoneyBee.Shared.Utilities;

namespace MoneyBee.Customer.Validators;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Surname is required")
            .MaximumLength(100).WithMessage("Surname cannot exceed 100 characters");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("National ID is required")
            .Must(NationalIdValidator.IsValid).WithMessage("National ID is invalid");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Phone number format is invalid");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required")
            .Must(BeAtLeast18YearsOld).WithMessage("Customer must be at least 18 years old");

        RuleFor(x => x.TaxNumber)
            .NotEmpty().When(x => x.Type == CustomerType.Corporate)
            .WithMessage("Tax number is required for corporate customers");
    }

    private static bool BeAtLeast18YearsOld(DateTime birthDate)
    {
        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;
        return age >= 18;
    }
}
