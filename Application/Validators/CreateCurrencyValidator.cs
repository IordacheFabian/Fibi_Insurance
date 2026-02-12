using System;
using Application.Metadatas.Currencies.DTOs.Request;
using FluentValidation;

namespace Application.Validators;

public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyDto>
{
    public CreateCurrencyValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Currency code is required.")
            .Length(3).WithMessage("Currency code must be exactly 3 characters.")
            .Matches("^[A-Z]{3}$").WithMessage("Currency code must consist of 3 uppercase letters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Currency name is required.")
            .MaximumLength(100).WithMessage("Currency name cannot exceed 100 characters.");

        RuleFor(x => x.ExchangeRateToBase)
            .GreaterThan(0).WithMessage("Exchange rate to base currency must be greater than zero.");
    }

}
