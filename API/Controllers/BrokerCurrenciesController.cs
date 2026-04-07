using System;
using API.Controllers.BaseControllers;
using Application.Core.PagedResults;
using Application.Metadatas.Currencies.DTOs.Response;
using Application.Metadatas.Currencies.Query;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BrokerCurrenciesController : BrokerBaseController
{
    [HttpGet("currencies")]
    public async Task<ActionResult<PagedResult<CurrencyDto>>> GetCurrenciesAsync([FromQuery] bool? isActive, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var currencies = await Mediator.Send(new GetCurrencies.Query { IsActive = isActive, PageNumber = pageNumber, PageSize = pageSize });

        return Ok(currencies);
    }
}