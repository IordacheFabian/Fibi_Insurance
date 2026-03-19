using System;
using Application.Claims.Command;
using Application.Claims.Request;
using FluentValidation;

namespace Application.Validators;

public class ApproveClaimValidator : AbstractValidator<ApproveClaim.Command>
{
    public ApproveClaimValidator()
    {
        RuleFor(x => x.ApproveClaimDto.ApprovedAmount)
            .GreaterThan(0).WithMessage("Approved amount must be greater than zero.");
    }
}
