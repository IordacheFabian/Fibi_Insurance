using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Geographies.DTOs;
using AutoMapper;
using MediatR;

namespace Application.Geographies.Queries;

public class GetCounties
{
    public class Query : IRequest<List<CountyDto>>
    {
        public Guid CountyId { get; set; }
    }

    public class Handler(IGeographyRepository geographyRepository, IMapper mapper) : IRequestHandler<Query, List<CountyDto>>
    {
        public async Task<List<CountyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var counties = await geographyRepository.GetCountiesByCountryAsync(request.CountyId, cancellationToken);
            
            if(counties == null) throw new NotFoundException("Counties not found for the specified country");

            return mapper.Map<List<CountyDto>>(counties);
        }
    }
}
