using System;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Metadatas.RiskFactors.DTOs.Response;
using AutoMapper;
using Domain.Models.Metadatas;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Metadatas.RiskFactors.Queries;

public class GetRiskFactors
{
    public class Query : IRequest<PagedResult<RiskFactorDto>>
    {
        public bool? IsActive { get; set; }
        public RiskLevel? RiskLevel { get; set; }

        public int PageNumber { get; set; } = 1;    
        public int PageSize { get; set; } = 10;
    }

    public class Handler(IRiskFactorRepository riskFactorRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<RiskFactorDto>>
    {
        public async Task<PagedResult<RiskFactorDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            var riskFactors =  riskFactorRepository.GetRiskFactorConfigurationsAsync(
                request.IsActive,
                request.RiskLevel,
                cancellationToken);

            var totalCount = await riskFactors.CountAsync(cancellationToken);

            var items = await riskFactors
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);

            var itemsDto = mapper.Map<List<RiskFactorDto>>(items);

            return new PagedResult<RiskFactorDto>
            {
                Items = itemsDto,
                TotalCount = totalCount,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize
            };
        }
    }
}
