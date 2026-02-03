using System;
using Domain.Models;

namespace Application.Core.Interfaces.IRepositories;

public interface IGeographyRepository
{
    Task<List<Country>> GetCountriesAsync(CancellationToken cancellationToken);
    Task<List<County>> GetCountiesByCountryAsync(Guid countryId, CancellationToken cancellationToken);  
    Task<List<City>> GetCitiesByCountyAsync(Guid countyId, CancellationToken cancellationToken);
}
