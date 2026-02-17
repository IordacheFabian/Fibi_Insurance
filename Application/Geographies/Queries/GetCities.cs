using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Geographies.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Geographies.Queries;

public class GetCities
{
    public class Query : IRequest<PagedResult<CityDto>>
    {
        public Guid CityId { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(IGeographyRepository geographyRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<CityDto>>
    {
        public async Task<PagedResult<CityDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            var cities = geographyRepository.GetCitiesByCountyAsync(request.CityId, cancellationToken);

            if (cities == null) throw new NotFoundException("Cities not found for the specified county");

            var totalCount = await cities.CountAsync(cancellationToken);

            var items = await cities
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);

            var itemsDto = mapper.Map<List<CityDto>>(items);

            return new PagedResult<CityDto>
            {
                Items = itemsDto,
                TotalCount = totalCount,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize
            };
        }
    }
}
