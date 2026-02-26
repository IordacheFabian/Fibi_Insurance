using System;
using Application.Policies.DTOs.Command;
using FluentValidation;

namespace Application.Validators;

public class CancelPolicyValidator : AbstractValidator<CancelPolicy.Command>
{
    public CancelPolicyValidator()
    {
        RuleFor(x => x.PolicyId) 
            .NotEmpty().WithMessage("PolicyId is required");

        RuleFor(x => x.CancelPolicyDto)
            .NotNull().WithMessage("CancelPolicyDto is required");

        When(x => x.CancelPolicyDto is not null, () =>
        {
            RuleFor(x => x.CancelPolicyDto!.CancellationReason)
                .NotEmpty().WithMessage("Cancellation reason is required")
                .MaximumLength(500).WithMessage("Cancellation reason cannot exceed 500 characters");
        });
    }
}
