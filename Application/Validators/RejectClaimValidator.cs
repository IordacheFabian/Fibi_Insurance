using System;
using Application.Claims.Command;
using FluentValidation;

namespace Application.Validators;

public class RejectClaimValidator : AbstractValidator<RejectClaim.Command>   
{
    public RejectClaimValidator()
    {
        RuleFor(x => x.RejectClaimDto.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters.");
    }
}
