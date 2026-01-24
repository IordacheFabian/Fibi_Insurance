using System;
using Application.Geographies.DTOs;
using Application.Geographies.Queries;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class GeographiesController : BaseApiController
{
    [HttpGet("countries")]
    public async Task<ActionResult<List<CountryDto>>> GetCountriesAsync()
    {
        var countries = await Mediator.Send(new GetCountries.Query());
        return Ok(countries);
    }

    [HttpGet("countries/{countryId:guid}/counties")]
    public async Task<ActionResult<List<CountyDto>>> GetCountiesAsync(Guid countryId)
    {
        var counties = await Mediator.Send(new GetCounties.Query { CountyId = countryId });
        return Ok(counties);
    }

    [HttpGet("counties/{countyId:guid}/cities")]
    public async Task<ActionResult<List<CityDto>>> GetCitiesAsync(Guid countyId)
    {
        var cities = await Mediator.Send(new GetCities.Query { CityId = countyId });
        return Ok(cities);
    }
}
