using System;
using Application.Clients.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Models.Clients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Clients.Queries;

public class GetClient
{
    public class Query : IRequest<PagedResult<ClientSearchDto>>
    {
        public Guid BrokerId { get; init; }
        public string? Name { get; init; }
        public string? Identifier { get; init; }
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(IClientRepository clientRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<ClientSearchDto>>
    {
        public async Task<PagedResult<ClientSearchDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            var clients = clientRepository.ClientSearchAsync(request.Name, request.Identifier, request.BrokerId, cancellationToken);
            var totalCount = await clients.CountAsync(cancellationToken);

            var items = await clients
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);
            
            var itemsDto = mapper.Map<List<ClientSearchDto>>(items);

            return new PagedResult<ClientSearchDto>
            {
                Items = itemsDto,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
