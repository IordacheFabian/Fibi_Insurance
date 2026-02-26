using Application.Metadatas.RiskFactors.DTOs.Request;
using Application.Validators;
using Domain.Models.Buildings;
using Domain.Models.Metadatas;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class CreateRiskFactorValidatorTests
{
    private readonly CreateRiskFactorValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_WhenNameMissing()
    {
        var dto = BuildDto(r => r.Name = string.Empty);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_ShouldFail_WhenPercentageOutOfRange()
    {
        var dto = BuildDto(r => r.AdjustementPercentage = 150);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AdjustementPercentage);
    }

    [Fact]
    public void Validator_ShouldFail_WhenBuildingTypeRuleViolated()
    {
        var dto = BuildDto(r =>
        {
            r.Level = RiskLevel.City;
            r.BuildingType = BuildingType.Residential;
        });

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BuildingType);
    }

    [Fact]
    public void Validator_ShouldFail_WhenReferenceMissingForGeoLevel()
    {
        var dto = BuildDto(r =>
        {
            r.Level = RiskLevel.County;
            r.ReferenceId = null;
        });

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ReferenceId);
    }

    private static CreateRiskFactorDto BuildDto(Action<CreateRiskFactorDto>? configure = null)
    {
        var dto = new CreateRiskFactorDto
        {
            Name = "Risk",
            Level = RiskLevel.City,
            ReferenceId = Guid.NewGuid(),
            BuildingType = null,
            AdjustementPercentage = 5m
        };
        configure?.Invoke(dto);
        return dto;
    }
}
