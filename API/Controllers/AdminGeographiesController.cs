using System;
using API.Controllers.BaseControllers;
using Application.Core.PagedResults;
using Application.Geographies.DTOs;
using Application.Geographies.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AdminGeographiesController : AdminBaseController
{
    [HttpGet("countries")]
    public async Task<ActionResult<PagedResult<CountryDto>>> GetCountriesAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var countries = await Mediator.Send(new GetCountries.Query { PageNumber = pageNumber, PageSize = pageSize });
        return Ok(countries);
    }

    [HttpGet("countries/{countryId:guid}/counties")]
    public async Task<ActionResult<PagedResult<CountyDto>>> GetCountiesAsync(Guid countryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var counties = await Mediator.Send(new GetCounties.Query { CountyId = countryId, PageNumber = pageNumber, PageSize = pageSize });
        return Ok(counties);
    }

    [HttpGet("counties/{countyId:guid}/cities")]
    public async Task<ActionResult<PagedResult<CityDto>>> GetCitiesAsync(Guid countyId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var cities = await Mediator.Send(new GetCities.Query { CityId = countyId, PageNumber = pageNumber, PageSize = pageSize });
        return Ok(cities);
    }
}