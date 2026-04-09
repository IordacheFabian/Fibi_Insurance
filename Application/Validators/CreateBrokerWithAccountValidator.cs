using Application.Brokers.DTOs.Request;
using Application.Validators.Extensions;
using FluentValidation;

namespace Application.Validators;

public class CreateBrokerWithAccountValidator : AbstractValidator<CreateBrokerWithAccountDto>
{
    public CreateBrokerWithAccountValidator()
    {
        RuleFor(x => x.BrokerCode)
            .NotEmpty().WithMessage("Broker code is required.")
            .MaximumLength(50).WithMessage("Broker code must not exceed 50 characters.");

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

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}