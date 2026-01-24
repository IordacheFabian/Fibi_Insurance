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
    public class Query : IRequest<Result<List<BuildingListDto>>>
    {
        public Guid ClientId { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Query, Result<List<BuildingListDto>>>
    {
        public async Task<Result<List<BuildingListDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var client = await context.Clients
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.ClientId, cancellationToken);

            if(!client) return Result<List<BuildingListDto>>.Failure("Client not found", 404);

            var buildings = await context.Buildings
                .AsNoTracking()
                .Where(x => x.ClientId == request.ClientId)
                .Include(x => x.Address)
                    .ThenInclude(x => x.City)
                .OrderBy(x => x.Address.Street)
                .Select(x => mapper.Map<BuildingListDto>(x))
                .ToListAsync(cancellationToken);

            return Result<List<BuildingListDto>>.Success(buildings);
        }
    }
}
