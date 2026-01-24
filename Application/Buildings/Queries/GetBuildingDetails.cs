using System;
using Application.Buildings.DTOs.Response;
using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Buildings.Queries;

public class GetBuildingDetails
{
    public class Query : IRequest<Result<BuildingDetailsDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Query, Result<BuildingDetailsDto>>
    {
        public async Task<Result<BuildingDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var building = await context.Buildings
                .AsNoTracking()
                .Include(x => x.Client)
                .Include(x => x.Address)
                    .ThenInclude(x => x.City)
                    .ThenInclude(x => x.County)
                    .ThenInclude(x => x.Country)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if(building == null) return Result<BuildingDetailsDto>.Failure("Building not found", 404);

            var result = mapper.Map<BuildingDetailsDto>(building);

            return Result<BuildingDetailsDto>.Success(result);
        }
    }
}
