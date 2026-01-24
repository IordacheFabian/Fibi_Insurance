using System;
using Application.Buildings.DTOs.Request;
using Application.Core;
using AutoMapper;
using MediatR;
using Persistence.Context;

namespace Application.Buildings.Commands;

public class UpdateBuilding
{
    public class Command : IRequest<Result<Unit>>
    {
        public required UpdateBuildingDto BuildingDto { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var building = await context.Buildings.FindAsync([request.BuildingDto.Id], cancellationToken);

            if(building == null) return Result<Unit>.Failure("Building not found", 404);

            mapper.Map(request.BuildingDto, building);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if(!result) return Result<Unit>.Failure("Failed to update the building", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }

}
