using System;
using Application.Core;
using Application.Geographies.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Geographies.Queries;

public class GetCounties
{
    public class Query : IRequest<List<CountyDto>>
    {
        public Guid CountyId { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Query, List<CountyDto>>
    {
        public Task<List<CountyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var counties = context.Counties
                .Where(x => x.CountryId == request.CountyId);
            
            if(counties == null) throw new NotFoundException("Counties not found for the specified country");

            return mapper.ProjectTo<CountyDto>(counties).ToListAsync(cancellationToken);
        }
    }
}
