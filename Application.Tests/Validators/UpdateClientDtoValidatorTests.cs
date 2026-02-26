using Application.Clients.Commands;
using Application.Clients.DTOs;
using Application.Validators;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class UpdateClientDtoValidatorTests
{
    private readonly UpdateClientDtoValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_WhenDtoMissing()
    {
        var command = new UpdateClient.Command { Id = Guid.NewGuid(), ClientDto = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClientDto);
    }

    [Fact]
    public void Validator_ShouldFail_WhenNameEmpty()
    {
        var command = BuildCommand(dto => dto.Name = string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClientDto!.Name);
    }

    [Fact]
    public void Validator_ShouldFail_WhenEmailInvalid()
    {
        var command = BuildCommand(dto => dto.Email = "bad");

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

    private static UpdateClient.Command BuildCommand(Action<UpdateClientDto>? configure = null)
    {
        var dto = new UpdateClientDto
        {
            Name = "Test",
            Email = "test@test.com",
            PhoneNumber = "0711111111"
        };
        configure?.Invoke(dto);
        return new UpdateClient.Command { Id = Guid.NewGuid(), ClientDto = dto };
    }
}
