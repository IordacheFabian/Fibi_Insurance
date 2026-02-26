using Application.Metadatas.Fees.DTOs.Request;
using Application.Validators;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class CreateFeeConfigurationValidatorTests
{
    private readonly CreateFeeConfigurationValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_WhenPercentageNotPositive()
    {
        var dto = BuildDto(f => f.Percentage = 0m);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Percentage);
    }

    [Fact]
    public void Validator_ShouldFail_WhenPercentageAboveLimit()
    {
        var dto = BuildDto(f => f.Percentage = 150m);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Percentage);
    }

    [Fact]
    public void Validator_ShouldFail_WhenEffectiveToBeforeEffectiveFrom()
    {
        var dto = BuildDto(f => f.EffectiveTo = f.EffectiveFrom.AddDays(-1));

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EffectiveTo);
    }

    private static CreateFeeConfigurationDto BuildDto(Action<CreateFeeConfigurationDto>? configure = null)
    {
        var dto = new CreateFeeConfigurationDto
        {
            Name = "Admin",
            FeeType = Domain.Models.Metadatas.FeeType.AdminFee,
            Percentage = 1.5m,
            EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        configure?.Invoke(dto);
        return dto;
    }
}
