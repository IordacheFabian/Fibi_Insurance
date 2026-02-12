using System;
using Application.Metadatas.Fees.DTOs.Request;
using FluentValidation;

namespace Application.Validators;

public class CreateFeeConfigurationValidator : AbstractValidator<CreateFeeConfigurationDto>
{
    public CreateFeeConfigurationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Fee configuration name is required.")
            .MaximumLength(200).WithMessage("Fee configuration name cannot exceed 200 characters.");
        
        RuleFor(x => x.Percentage)   
            .GreaterThan(0).WithMessage("Percentage must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Percentage cannot exceed 100.");

        RuleFor(x => x.EffectiveTo)
            .Must((dto, effectiveTo) => !effectiveTo.HasValue || effectiveTo.Value >= dto.EffectiveFrom)
            .WithMessage("EffectiveTo must be greater than or equal to EffectiveFrom.");
    }
}
