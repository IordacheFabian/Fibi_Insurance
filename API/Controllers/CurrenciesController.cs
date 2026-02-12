using System;
using API.Controllers.BaseControllers;
using Application.Metadatas.Currencies.Command;
using Application.Metadatas.Currencies.DTOs.Request;
using Application.Metadatas.Currencies.DTOs.Response;
using Application.Metadatas.Currencies.Query;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CurrenciesController : AdminBaseController
{
    [HttpGet("currencies")]
    public async Task<ActionResult<List<CurrencyDto>>> GetCurrenciesAsync([FromQuery] bool? isActive)
    {
        var currencies = await Mediator.Send(new GetCurrencies.Query { IsActive = isActive });

        return Ok(currencies);
    }

    [HttpPost("currencies")]
    public async Task<ActionResult<CurrencyDto>> CreateCurrencyAsync([FromBody] CreateCurrencyDto createCurrencyDto)
    {
        var currency = await Mediator.Send(new CreateCurrency.Command { CreateCurrencyDto = createCurrencyDto });

        return CreatedAtAction(nameof(GetCurrenciesAsync), new { id = currency.Id }, currency); 
    }

    [HttpPut("currencies/{currencyId:guid}")] 
    public async Task<ActionResult> UpdateCurrencyAsync(Guid currencyId, [FromBody] UpdateCurrencyDto updateCurrencyDto)
    {
        var currency = await Mediator.Send(new UpdateCurrency.Command {  UpdateCurrencyDto = updateCurrencyDto });

        return Ok(currency);
    }
}
