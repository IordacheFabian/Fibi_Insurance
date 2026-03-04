using System;
using Application.Policies.Command;
using FluentValidation;

namespace Application.Validators;

public class ActivatePolicyValidator : AbstractValidator<ActivatePolicy.Command>
{
    public ActivatePolicyValidator()
    {
        RuleFor(x => x.PolicyId)
            .NotEmpty().WithMessage("PolicyId is required");
    }
}
