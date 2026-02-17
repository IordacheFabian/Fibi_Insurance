using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Geographies.DTOs;
using AutoMapper;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Geographies.Queries;

public class GetCountries
{
    public class Query : IRequest<PagedResult<CountryDto>>
    {
        public Guid Id { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(IGeographyRepository geographyRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<CountryDto>>
    {
        public async Task<PagedResult<CountryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            var countries = geographyRepository.GetCountriesAsync(cancellationToken);

            if(countries == null) throw new NotFoundException("Countries not found");

            var totalCount = await countries.CountAsync(cancellationToken);

            var items = await countries
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);

            var itemsDto = mapper.Map<List<CountryDto>>(items);

            return new PagedResult<CountryDto>
            {
                Items = itemsDto,
                TotalCount = totalCount,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize
            };
        }
    }
}
