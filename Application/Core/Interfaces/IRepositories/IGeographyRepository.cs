using System;
using Domain.Models;

namespace Application.Core.Interfaces.IRepositories;

public interface IGeographyRepository
{
    IQueryable<Country> GetCountriesAsync(CancellationToken cancellationToken);
    IQueryable<County> GetCountiesByCountryAsync(Guid countryId, CancellationToken cancellationToken);  
    IQueryable<City> GetCitiesByCountyAsync(Guid countyId, CancellationToken cancellationToken);
}
