using System;
using Application.Buildings.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using AutoMapper;
using Domain.Models.Buildings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Buildings.Queries;

public class GetBuildingsList
{
    public class Query : IRequest<PagedResult<BuildingListDto>>
    {
        public Guid ClientId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(IBuildingRepository buildingRepository, IClientRepository clientRepository, IMapper mapper) : IRequestHandler<Query, PagedResult<BuildingListDto>>
    {
        public async Task<PagedResult<BuildingListDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paging = new PagingParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            paging.Normalize();

            var client = await clientRepository.GetClientAsync(request.ClientId, cancellationToken);

            if(client == null) throw new NotFoundException("Client not found");

            var buildings = buildingRepository.GetBuildingForClientAsync(request.ClientId, cancellationToken);

            var totalCount = await buildings.CountAsync(cancellationToken);

            var items = await buildings
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);
            
            var itemsDto = mapper.Map<List<BuildingListDto>>(items);

            return new PagedResult<BuildingListDto>
            {
                Items = itemsDto,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
