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

public class GetPoliciesByCityReport
{
    public class Query : IRequest<PagedResult<PoliciesByCityListDto>>
    {
        public PoliciesByCityReportDto ReportRequest { get; set; } = default!;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class Handler(IReportsRepository reportsRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<PoliciesByCityListDto>>
    {
        public async Task<PagedResult<PoliciesByCityListDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            if(request.ReportRequest.To < request.ReportRequest.From)
            {
                throw new BadRequestException("The 'To' date must be greater than or equal to the 'From' date.");
            }

            var query = reportsRepository.GetPoliciesByCityReport(request.ReportRequest, cancellationToken);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);

            var itemDto = mapper.Map<List<PoliciesByCityListDto>>(items);

            return new PagedResult<PoliciesByCityListDto>
            {
                Items = itemDto,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
