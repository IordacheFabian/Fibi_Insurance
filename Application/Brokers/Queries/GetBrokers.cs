using System;
using Application.Brokers.DTOs.Response;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Brokers.Queries;

public class GetBrokers
{
    public class Query : IRequest<PagedResult<BrokerDto>>
    {
        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(IBrokerRepository brokerRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<BrokerDto>>
    {
        public async Task<PagedResult<BrokerDto>> Handle(Query request, CancellationToken cancellationToken)
        {   
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            var brokers = brokerRepository.GetBrokersAsync(request.IsActive, cancellationToken);
            
            var totalCount = await brokers.CountAsync(cancellationToken);

            var items = await brokers
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);
            
            var itemDto = mapper.Map<List<BrokerDto>>(items);

            return new PagedResult<BrokerDto>
            {
                Items = itemDto,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
