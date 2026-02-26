using Application.Brokers.DTOs.Request;
using Application.Validators;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class UpdateBrokerValidatorTests
{
    private readonly UpdateBrokerValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_WhenNameMissing()
    {
        var dto = BuildDto(b => b.Name = string.Empty);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_ShouldFail_WhenEmailInvalid()
    {
        var dto = BuildDto(b => b.Email = "bad");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_ShouldFail_WhenCommissionNegative()
    {
        var dto = BuildDto(b => b.CommissionPercentage = -5);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CommissionPercentage);
    }

    private static UpdateBrokerDto BuildDto(Action<UpdateBrokerDto>? configure = null)
    {
        var dto = new UpdateBrokerDto
        {
            Name = "Broker",
            Email = "broker@test.com",
            PhoneNumber = "0700000000",
            CommissionPercentage = 10
        };
        configure?.Invoke(dto);
        return dto;
    }
}
