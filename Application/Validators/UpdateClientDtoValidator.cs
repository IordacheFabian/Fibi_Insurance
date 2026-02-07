using System;
using Application.Clients.Commands;
using Application.Validators.Extensions;
using FluentValidation;

namespace Application.Validators;

public class UpdateClientDtoValidator : AbstractValidator<UpdateClient.Command>
{   
    public UpdateClientDtoValidator()
    {
        RuleFor(x => x.ClientDto).NotNull().WithMessage("Client data is required.");

        When(x => x.ClientDto != null, () =>
        {
            RuleFor(x => x.ClientDto.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200);

            RuleFor(x => x.ClientDto.Email).ValidEmail();

            RuleFor(x => x.ClientDto.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required");
        });
    }
}
