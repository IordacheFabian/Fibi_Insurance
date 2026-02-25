using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Reports.DTOs.Request;
using Application.Reports.DTOs.Response;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Reports.Queries;

public class GetPoliciesByBrokerReport
{
    public class Query : IRequest<PagedResult<PoliciesByBrokerListDto>>
    {
        public PoliciesByBrokerReportDto ReportDto { get; set; } = default!;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    
    }

    public class Handler(IReportsRepository reportsRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<PoliciesByBrokerListDto>>
    {
        public async Task<PagedResult<PoliciesByBrokerListDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            if (request.ReportDto.To < request.ReportDto.From)
            {
                throw new BadRequestException("The 'To' date must be greater than or equal to the 'From' date.");
            }

            var query = reportsRepository.GetPoliciesByBrokerReport(request.ReportDto, cancellationToken);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);

            var itemDto = mapper.Map<List<PoliciesByBrokerListDto>>(items);

            return new PagedResult<PoliciesByBrokerListDto>
            {
                Items = itemDto,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
