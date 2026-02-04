using System;
using FluentValidation;
using FluentValidation.Validators;

namespace Application.Clients.DTOs.Validators.Extensions;

public static class EmailValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email address is not valid");
    }
}
