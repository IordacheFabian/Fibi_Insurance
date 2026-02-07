using System;
using Application.Policies.DTOs.Requests;
using FluentValidation;

namespace Application.Validators;

public class CreatePolicyDraftValidator : AbstractValidator<CreatePolicyDraftDto>
{
    public CreatePolicyDraftValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty().WithMessage("ClientId is required");
        RuleFor(x => x.BuildingId).NotEmpty();
        RuleFor(x => x.CurrencyId).NotEmpty();
        RuleFor(x => x.BrokerId).NotEmpty().WithMessage("BrokerId is required");

        RuleFor(x => x.BasePremium).GreaterThan(0);

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate).WithMessage("StartDate must be before EndDate");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("EndDate must be after StartDate");
    }
}
