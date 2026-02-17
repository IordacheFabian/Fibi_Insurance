using Application.Policies.DTOs.Requests;
using Application.Validators;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class CreatePolicyDraftValidatorTests
{
    private readonly CreatePolicyDraftValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_ForNegativeBasePremium()
    {
        var dto = BuildDto();
        dto.BasePremium = -1m;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BasePremium);
    }

    [Fact]
    public void Validator_ShouldFail_WhenMandatoryIdsMissing()
    {
        var dto = BuildDto();
        dto.ClientId = Guid.Empty;
        dto.BrokerId = Guid.Empty;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ClientId);
        result.ShouldHaveValidationErrorFor(x => x.BrokerId);
    }

    private static CreatePolicyDraftDto BuildDto() => new()
    {
        ClientId = Guid.NewGuid(),
        BuildingId = Guid.NewGuid(),
        CurrencyId = Guid.NewGuid(),
        BrokerId = Guid.NewGuid(),
        BasePremium = 100m,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        PolicyNumber = null
    };
}