using FluentValidation;
using MoneyBee.Transfer.Models;

namespace MoneyBee.Transfer.Validators;

public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferRequestValidator()
    {
        RuleFor(x => x.SenderCustomerId)
            .NotEmpty().WithMessage("Sender customer ID is required");

        RuleFor(x => x.ReceiverCustomerId)
            .NotEmpty().WithMessage("Receiver customer ID is required")
            .NotEqual(x => x.SenderCustomerId).WithMessage("Sender and receiver cannot be the same");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (e.g., TRY, USD, EUR)");
    }
}
