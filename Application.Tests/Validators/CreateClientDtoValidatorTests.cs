using Application.Clients.Commands;
using Application.Clients.DTOs;
using Application.Clients.DTOs.Validators;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class CreateClientDtoValidatorTests
{
    private readonly CreateClientDtoValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_WhenPayloadMissing()
    {
        var command = new CreateClient.Command { ClientDto = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClientDto);
    }

    [Fact]
    public void Validator_ShouldFail_WhenCnpInvalid()
    {
        var command = BuildCommand(dto => dto.IdentificationNumber = "123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClientDto!.IdentificationNumber);
    }

    [Fact]
    public void Validator_ShouldFail_WhenEmailInvalid()
    {
        var command = BuildCommand(dto => dto.Email = "not-an-email");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClientDto!.Email);
    }

    [Fact]
    public void Validator_ShouldFail_WhenPhoneMissing()
    {
        var command = BuildCommand(dto => dto.PhoneNumber = string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClientDto!.PhoneNumber);
    }

    private static CreateClient.Command BuildCommand(Action<CreateClientDto>? configure = null)
    {
        var dto = new CreateClientDto
        {
            Name = "Test",
            IdentificationNumber = "1234567890123",
            Email = "test@test.com",
            PhoneNumber = "0712345678"
        };
        configure?.Invoke(dto);
        return new CreateClient.Command { ClientDto = dto };
    }
}
