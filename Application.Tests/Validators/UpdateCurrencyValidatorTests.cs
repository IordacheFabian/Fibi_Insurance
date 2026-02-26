using Application.Metadatas.Currencies.Command;
using Application.Metadatas.Currencies.DTOs.Request;
using Application.Validators;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class UpdateCurrencyValidatorTests
{
    private readonly UpdateCurrencyValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_WhenNameMissing()
    {
        var command = BuildCommand(dto => dto.Name = string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UpdateCurrencyDto.Name);
    }

    [Fact]
    public void Validator_ShouldFail_WhenExchangeRateNotPositive()
    {
        var command = BuildCommand(dto => dto.ExchangeRateToBase = 0m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UpdateCurrencyDto.ExchangeRateToBase);
    }

    private static UpdateCurrency.Command BuildCommand(Action<UpdateCurrencyDto>? configure = null)
    {
        var dto = new UpdateCurrencyDto { Id = Guid.NewGuid(), Name = "Euro", ExchangeRateToBase = 4.95m };
        configure?.Invoke(dto);
        return new UpdateCurrency.Command { UpdateCurrencyDto = dto };
    }
}
