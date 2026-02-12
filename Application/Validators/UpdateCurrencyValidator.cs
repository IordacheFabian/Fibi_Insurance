using System;
using Application.Metadatas.Currencies.Command;
using FluentValidation;

namespace Application.Validators;

public class UpdateCurrencyValidator : AbstractValidator<UpdateCurrency.Command>
{
    public UpdateCurrencyValidator()
    {
        RuleFor(x => x.UpdateCurrencyDto.Name)
            .NotEmpty().WithMessage("Currency name is required.")
            .MaximumLength(100).WithMessage("Currency name cannot exceed 100 characters.");

        RuleFor(x => x.UpdateCurrencyDto.ExchangeRateToBase)
            .GreaterThan(0).WithMessage("Exchange rate to base currency must be greater than zero.");
    }
}
