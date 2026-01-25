using System;
using Application.Buildings.DTOs.Response;
using Application.Core;
using AutoMapper;
using Domain.Models.Buildings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Buildings.Queries;

public class GetBuildingsList
{
    public class Query : IRequest<List<BuildingListDto>>
    {
        public Guid ClientId { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Query, List<BuildingListDto>>
    {
        public async Task<List<BuildingListDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var client = await context.Clients
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.ClientId, cancellationToken);

            if(!client) throw new NotFoundException("Client not found");

            var buildings = await context.Buildings
                .AsNoTracking()
                .Where(x => x.ClientId == request.ClientId)
                .Include(x => x.Address)
                    .ThenInclude(x => x.City)
                .OrderBy(x => x.Address.Street)
                .Select(x => mapper.Map<BuildingListDto>(x))
                .ToListAsync(cancellationToken);

            return buildings;
        }
    }
}
