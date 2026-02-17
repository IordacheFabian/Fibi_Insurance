using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class GeographyRepository(AppDbContext context) : IGeographyRepository
{
    public IQueryable<City> GetCitiesByCountyAsync(Guid countyId, CancellationToken cancellationToken)
    {
        return context.Cities
            .AsNoTracking()
            .AsQueryable()
            .Where(x => x.CountyId == countyId)
            .OrderBy(x => x.Name);
    }

    public IQueryable<County> GetCountiesByCountryAsync(Guid countryId, CancellationToken cancellationToken)
    {
        return context.Counties
            .AsNoTracking()
            .AsQueryable()
            .Where(x => x.CountryId == countryId)
            .OrderBy(x => x.Name);
    }

    public IQueryable<Country> GetCountriesAsync(CancellationToken cancellationToken)
    {
        return context.Countries
            .AsNoTracking()
            .AsQueryable()
            .OrderBy(x => x.Name);
    }
}
