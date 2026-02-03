using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class GeographyRepository(AppDbContext context) : IGeographyRepository
{
    public async Task<List<City>> GetCitiesByCountyAsync(Guid countyId, CancellationToken cancellationToken)
    {
        return await context.Cities
            .AsNoTracking()
            .Where(x => x.CountyId == countyId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<County>> GetCountiesByCountryAsync(Guid countryId, CancellationToken cancellationToken)
    {
        return await context.Counties
            .AsNoTracking()
            .Where(x => x.CountryId == countryId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Country>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        return await context.Countries
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
