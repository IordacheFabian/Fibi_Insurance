using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Geographies.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Geographies.Queries;

public class GetCounties
{
    public class Query : IRequest<PagedResult<CountyDto>>
    {
        public Guid CountyId { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(IGeographyRepository geographyRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<CountyDto>>
    {
        public async Task<PagedResult<CountyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            var counties = geographyRepository.GetCountiesByCountryAsync(request.CountyId, cancellationToken);
            
            if(counties == null) throw new NotFoundException("Counties not found for the specified country");

            var totalCount = await counties.CountAsync(cancellationToken);

            var items = await counties
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);
            
            var itemsDto = mapper.Map<List<CountyDto>>(items);

            return new PagedResult<CountyDto>
            {
                Items = itemsDto,
                TotalCount = totalCount,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize
            };
        }
    }
}
