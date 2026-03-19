using System;
using Application.Payments.Command;
using FluentValidation;

namespace Application.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePayment.Command>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.Payment.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");
        RuleFor(x => x.Payment.CurrencyId)
            .NotEmpty().WithMessage("CurrencyId is required.");
        RuleFor(x => x.Payment.Method)
            .NotEmpty().WithMessage("Payment method is required.");
        RuleFor(x => x.Payment.Status)
            .NotEmpty().WithMessage("Payment status is required.");
    }
}
