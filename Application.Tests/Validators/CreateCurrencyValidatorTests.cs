using Application.Metadatas.Currencies.DTOs.Request;
using Application.Validators;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class CreateCurrencyValidatorTests
{
    private readonly CreateCurrencyValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_WhenCodeNotThreeLetters()
    {
        var dto = BuildDto(c => c.Code = "EU");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validator_ShouldFail_WhenCodeNotUppercase()
    {
        var dto = BuildDto(c => c.Code = "eur");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validator_ShouldFail_WhenExchangeRateNotPositive()
    {
        var dto = BuildDto(c => c.ExchangeRateToBase = 0);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ExchangeRateToBase);
    }

    private static CreateCurrencyDto BuildDto(Action<CreateCurrencyDto>? configure = null)
    {
        var dto = new CreateCurrencyDto { Code = "EUR", Name = "Euro", ExchangeRateToBase = 4.95m };
        configure?.Invoke(dto);
        return dto;
    }
}
