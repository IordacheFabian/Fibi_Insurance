using System;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Policies.DTOs.Response;
using AutoMapper;
using Domain.Models.Policies;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Policies.Queries;

public class GetPolicies 
{
    public class Query : IRequest<PagedResult<PolicyListItemDto>>
    {
        public Guid? ClientId { get; set; }
        public Guid? BrokerId { get; set; }
        public PolicyStatus? PolicyStatus { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(IPolicyRepository policyRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<PolicyListItemDto>>
    {
        public async Task<PagedResult<PolicyListItemDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize(maxPageSize: 100);

            var query = policyRepository.ListPolicyAsync(request.ClientId, request.BrokerId, request.PolicyStatus, request.StartDate, request.EndDate, cancellationToken);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);    

            var itemsDto = mapper.Map<List<PolicyListItemDto>>(items);

            return new PagedResult<PolicyListItemDto>
            {
                Items = itemsDto,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
