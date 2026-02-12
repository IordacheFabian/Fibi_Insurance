using System;
using Application.Metadatas.Fees.Command;
using Application.Metadatas.Fees.DTOs.Request;
using FluentValidation;

namespace Application.Validators;

public class UpdateFeeConfigurationValidator : AbstractValidator<UpdateFeeConfigurationDto>
{
    public UpdateFeeConfigurationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Fee configuration name is required.")
            .MaximumLength(200).WithMessage("Fee configuration name cannot exceed 200 characters.");

        RuleFor(x => x.Percentage)
            .GreaterThanOrEqualTo(0).WithMessage("Percentage must be greater than or equal to zero.");

        RuleFor(x => x.EffectiveTo)
            .LessThanOrEqualTo(x => x.EffectiveTo ?? DateOnly.MaxValue)
            .WithMessage("Effective from date must be before effective to date.");
    }
}
