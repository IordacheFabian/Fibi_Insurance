using System;
using Application.Brokers.DTOs.Request;
using Application.Validators.Extensions;
using FluentValidation;

namespace Application.Validators;

public class UpdateBrokerValidator : AbstractValidator<UpdateBrokerDto>
{   
    public UpdateBrokerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Email) 
            .ValidEmail();
        
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(50).WithMessage("Phone number must not exceed 50 characters.");

        RuleFor(x => x.CommissionPercentage) 
            .GreaterThanOrEqualTo(0).WithMessage("Commission percentage must be greater than or equal to 0.")
            .LessThanOrEqualTo(100).WithMessage("Commission percentage must be less than or equal to 100.")
            .When(x => x.CommissionPercentage.HasValue);
    }
}
