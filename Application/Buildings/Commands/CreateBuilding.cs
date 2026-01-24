using System;
using Application.Addresses.DTOs;
using Application.Buildings.DTOs.Request;
using Application.Core;
using AutoMapper;
using Domain.Models.Buildings;
using Domain.Models.Geography.Address;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Buildings.Commands;

public class CreateBuilding
{
    public class Command : IRequest<string>
    {
        public Guid ClientId { get; set; }  
        public required CreateBuildingDto BuildingDto { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command, string>
    {
        public async Task<string> Handle(Command request, CancellationToken cancellationToken)
        {
            var client = await context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ClientId, cancellationToken);

            if(client == null) throw new NotFoundException("Client not found");

            var address = mapper.Map<Address>(request.BuildingDto.Address);
            context.Addresses.Add(address);
            await context.SaveChangesAsync(cancellationToken);

            if(address.Id == Guid.Empty) 
                throw new BadRequestException("Failed to create address for building");

            var building = mapper.Map<Building>(request.BuildingDto);
            building.AddressId = address.Id;
            building.ClientId = request.ClientId;

            context.Buildings.Add(building);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if(!result) throw new BadRequestException("Failed to create building");

            return building.Id.ToString();
        }
    }

}
