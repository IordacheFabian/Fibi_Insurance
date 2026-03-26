using System;
using System.Security.Claims;
using Application.Claims.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Claims.Query;

public class GetClaimsList
{
    public class Query : IRequest<PagedResult<ClaimListDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(IClaimRepository claimRepository) : IRequestHandler<Query, PagedResult<ClaimListDto>>
    {
        public async Task<PagedResult<ClaimListDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            var claims =  claimRepository.GetAllClaimsAsync(cancellationToken);

            if(claims == null)
            {
                throw new NotFoundException("No claims found.");
            }

            var totalCount = await claims.CountAsync(cancellationToken);
            var items = await claims
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);

            var itemDto = items.Select(c => new ClaimListDto
            {
                Id = c.Id,
                PolicyId = c.PolicyId,
                PolicyNumber = c.Policy.PolicyNumber,
                ClientName = c.Policy.Client.Name,
                ApprovedAmount = c.ApprovedAmount,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            }).ToList();

            return new PagedResult<ClaimListDto>
            {
                Items = itemDto,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalCount = totalCount
            };


        }
    }
}
