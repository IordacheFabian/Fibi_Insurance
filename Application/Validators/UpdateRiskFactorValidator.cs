using System;
using Application.Metadatas.RiskFactors.DTOs.Request;
using FluentValidation;

namespace Application.Validators;

public class UpdateRiskFactorValidator : AbstractValidator<UpdateRiskFactorDto>
{
    public UpdateRiskFactorValidator()
    {
        RuleFor(x => x.Name)
           .NotEmpty()
           .MaximumLength(200);

        RuleFor(x => x.AdjustementPercentage)
         .GreaterThanOrEqualTo(-100m)
         .LessThanOrEqualTo(100m);
    }
}
