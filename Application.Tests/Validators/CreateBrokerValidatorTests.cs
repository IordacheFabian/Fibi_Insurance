using Application.Brokers.DTOs.Request;
using Application.Validators;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class CreateBrokerValidatorTests
{
    private readonly CreateBrokerValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_WhenCodeMissing()
    {
        var dto = BuildDto(b => b.BrokerCode = string.Empty);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BrokerCode);
    }

    [Fact]
    public void Validator_ShouldFail_WhenEmailInvalid()
    {
        var dto = BuildDto(b => b.Email = "invalid");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_ShouldFail_WhenCommissionOutOfRange()
    {
        var dto = BuildDto(b => b.CommissionPercentage = 150);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CommissionPercentage);
    }

    private static CreateBrokerDto BuildDto(Action<CreateBrokerDto>? configure = null)
    {
        var dto = new CreateBrokerDto
        {
            BrokerCode = "BRK-01",
            Name = "Broker",
            Email = "broker@test.com",
            PhoneNumber = "0700000000",
            CommissionPercentage = 10
        };
        configure?.Invoke(dto);
        return dto;
    }
}
