using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Geographies.DTOs;
using AutoMapper;
using MediatR;

namespace Application.Geographies.Queries;

public class GetCities
{
    public class Query : IRequest<List<CityDto>>
    {
        public Guid CityId { get; set; }
    }

    public class Handler(IGeographyRepository geographyRepository, IMapper mapper) : IRequestHandler<Query, List<CityDto>>
    {
        public async Task<List<CityDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var cities = await geographyRepository.GetCitiesByCountyAsync(request.CityId, cancellationToken);
            
            if(cities == null) throw new NotFoundException("Cities not found for the specified county");

            return mapper.Map<List<CityDto>>(cities);
        }
    }
}
