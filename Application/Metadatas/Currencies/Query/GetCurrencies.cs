using System;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Metadatas.Currencies.DTOs.Response;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Metadatas.Currencies.Query;

public class GetCurrencies
{
    public class Query : IRequest<PagedResult<CurrencyDto>>
    {
        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(ICurrencyRepository currencyRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<CurrencyDto>>
    {
        public async Task<PagedResult<CurrencyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            var currencies = currencyRepository.GetCurrenciesAsync(request.IsActive, cancellationToken);

            var totalCount = await currencies.CountAsync(cancellationToken);
            
            var items = await currencies
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);

            var itemsDto = mapper.Map<List<CurrencyDto>>(items);

            return new PagedResult<CurrencyDto>
            {
                Items = itemsDto,
                TotalCount = totalCount,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize
            };
        }
    }
}
