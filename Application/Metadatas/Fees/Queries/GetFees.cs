using System;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Metadatas.Fees.DTOs.Response;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Metadatas.Fees.Queries;

public class GetFees
{
    public class Query : IRequest<PagedResult<FeeConfigurationDto>>
    {
        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(IFeeConfigurationRepository feeConfigurationRepository, IMapper mapper)
        : IRequestHandler<Query, PagedResult<FeeConfigurationDto>>
    {
        public async Task<PagedResult<FeeConfigurationDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            var fees = feeConfigurationRepository.GetFeeConfigurationsAsync(request.IsActive, cancellationToken);

            var totalCount = await fees.CountAsync(cancellationToken);  

            var items = await fees
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);
                
            var itemsDto = mapper.Map<List<FeeConfigurationDto>>(items);

            return new PagedResult<FeeConfigurationDto>
            {
                Items = itemsDto,
                TotalCount = totalCount,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize
            };
        }
    }
}
