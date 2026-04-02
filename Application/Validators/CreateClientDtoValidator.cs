using System;
using Application.Clients.Commands;
using Application.Core.Interfaces.IRepositories;
using Application.Validators.Extensions;
using FluentValidation;

namespace Application.Clients.DTOs.Validators;

public class CreateClientDtoValidator : AbstractValidator<CreateClient.Command>
{
    public CreateClientDtoValidator(IClientRepository clientRepository)
    {
        RuleFor(x => x.ClientDto).NotNull().WithMessage("Client data is required.");

        When(x => x.ClientDto != null, () =>
        {
            RuleFor(x => x.ClientDto.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200);

            RuleFor(x => x.ClientDto.IdentificationNumber).ValidCnp();

            RuleFor(x => x.ClientDto.IdentificationNumber)
                .MustAsync(async (identifier, cancellationToken) =>
                    !await clientRepository.IdentifierExistsAsync(identifier, cancellationToken))
                .WithMessage("A client with this identification number already exists.");

            RuleFor(x => x.ClientDto.Email).ValidEmail();

            RuleFor(x => x.ClientDto.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required");
        });
    }
}
