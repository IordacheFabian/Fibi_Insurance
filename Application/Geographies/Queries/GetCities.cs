using System;
using Application.Core;
using Application.Geographies.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Geographies.Queries;

public class GetCities
{
    public class Query : IRequest<List<CityDto>>
    {
        public Guid CityId { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Query, List<CityDto>>
    {
        public Task<List<CityDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var cities = context.Cities
                .Where(x => x.CountyId == request.CityId);
            
            if(cities == null) throw new NotFoundException("Cities not found for the specified county");

            return mapper.ProjectTo<CityDto>(cities).ToListAsync(cancellationToken);
        }
    }
}
