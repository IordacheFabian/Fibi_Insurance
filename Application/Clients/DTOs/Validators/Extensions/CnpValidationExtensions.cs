using System;
using FluentValidation;

namespace Application.Clients.DTOs.Validators.Extensions;

public static class CnpValidationExtensions
{
    private static readonly int[] ControlKey = { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };

    public static IRuleBuilderOptions<T, string> ValidCnp<T> (this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("CNP is required.")
            .Length(13).WithMessage("CNP must be 13 characters long.")
            .Matches(@"^\d{13}$").WithMessage("CNP must contain only digits.")
            .Must(IsValidCnp)
            .WithMessage("CNP is not valid");
    }

    private static bool IsValidCnp(string cnp)
    {
        if(string.IsNullOrWhiteSpace(cnp) || cnp.Length != 13) return false;

        var sum = 0;
        for(int i = 0; i < 12; i++ )
        {
            sum += (cnp[i] - '0') * ControlKey[i];
        }

        var controlDigit = sum % 11;
        if(controlDigit == 10) controlDigit = 1;

        return controlDigit == (cnp[12] - '0');
    }
}
