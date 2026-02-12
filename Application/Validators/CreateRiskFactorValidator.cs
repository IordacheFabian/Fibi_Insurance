using System;
using Application.Metadatas.RiskFactors.DTOs.Request;
using Domain.Models.Metadatas;
using FluentValidation;

namespace Application.Validators;

public class CreateRiskFactorValidator : AbstractValidator<CreateRiskFactorDto>
{
    public CreateRiskFactorValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AdjustementPercentage)
            .GreaterThanOrEqualTo(-100m)
            .LessThanOrEqualTo(100m);

        When(x => x.Level == RiskLevel.County || x.Level == RiskLevel.City, () =>
        {
            RuleFor(x => x.ReferenceId)
                .NotNull()
                .WithMessage("ReferenceId is required for County/City risk factors.");

            RuleFor(x => x.BuildingType)
                .Null()
                .WithMessage("BuildingType must be null for County/City risk factors.");
        });

        When(x => x.Level == RiskLevel.BuildingType, () =>
        {
            RuleFor(x => x.BuildingType)
                .NotNull()
                .WithMessage("BuildingType is required when Level is BuildingType.");

            RuleFor(x => x.ReferenceId)
                .Null()
                .WithMessage("ReferenceId must be null when Level is BuildingType.");
        }); RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        
        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("Level is required.");
    }
}
