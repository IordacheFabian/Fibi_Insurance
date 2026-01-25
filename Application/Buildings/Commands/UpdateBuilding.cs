using System;
using Application.Buildings.DTOs.Request;
using Application.Core;
using AutoMapper;
using MediatR;
using Persistence.Context;

namespace Application.Buildings.Commands;

public class UpdateBuilding
{
    public class Command : IRequest<Unit>
    {
        public required UpdateBuildingDto BuildingDto { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var building = await context.Buildings.FindAsync([request.BuildingDto.Id], cancellationToken);

            if(building == null) throw new NotFoundException("Building not found");

            mapper.Map(request.BuildingDto, building);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if(!result) throw new BadRequestException("Failed to update building");

            return Unit.Value;
        }
    }

}
