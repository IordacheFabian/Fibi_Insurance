using System;
using API.Controllers.BaseControllers;
using Application.Core.PagedResults;
using Application.Metadatas.Currencies.Command;
using Application.Metadatas.Currencies.DTOs.Request;
using Application.Metadatas.Currencies.DTOs.Response;
using Application.Metadatas.Currencies.Query;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CurrenciesController : AdminBaseController
{
    [HttpGet("currencies")]
    public async Task<ActionResult<PagedResult<CurrencyDto>>> GetCurrenciesAsync([FromQuery] bool? isActive, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var currencies = await Mediator.Send(new GetCurrencies.Query { IsActive = isActive, PageNumber = pageNumber, PageSize = pageSize });

        return Ok(currencies);
    }

    [HttpPost("currencies")]
    public async Task<ActionResult<CurrencyDto>> CreateCurrencyAsync([FromBody] CreateCurrencyDto createCurrencyDto)
    {
        var currency = await Mediator.Send(new CreateCurrency.Command { CreateCurrencyDto = createCurrencyDto });

        return Ok(currency);
    }

    [HttpPut("currencies/{currencyId:guid}")] 
    public async Task<ActionResult> UpdateCurrencyAsync(Guid currencyId, [FromBody] UpdateCurrencyDto updateCurrencyDto)
    {
        var currency = await Mediator.Send(new UpdateCurrency.Command {  UpdateCurrencyDto = updateCurrencyDto });

        return Ok(currency);
    }
}
