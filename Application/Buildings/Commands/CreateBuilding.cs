using System;
using Application.Addresses.DTOs;
using Application.Buildings.DTOs.Request;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Buildings;
using Domain.Models.Geography.Address;
using MediatR;

namespace Application.Buildings.Commands;

public class CreateBuilding
{
    public class Command : IRequest<string>
    {
        public Guid ClientId { get; set; }  
        public required CreateBuildingDto BuildingDto { get; set; }
    }

    public class Handler(IBuildingRepository buildingRepository, IClientRepository clientRepository, IAddressRepository addressRepository, IMapper mapper) : IRequestHandler<Command, string>
    {
        public async Task<string> Handle(Command request, CancellationToken cancellationToken)
        {
            var client = await clientRepository.GetClientAsync(request.ClientId, cancellationToken);

            if(client == null) throw new NotFoundException("Client not found");

            var address = mapper.Map<Address>(request.BuildingDto.Address);

            await addressRepository.AddAddressAsync(address, cancellationToken);
            var savedAddress = await addressRepository.SaveChangesAsync(cancellationToken);

            if (!savedAddress || address.Id == Guid.Empty) 
                throw new BadRequestException("Failed to create address for building");

            var building = mapper.Map<Building>(request.BuildingDto);
            building.AddressId = address.Id;
            building.ClientId = request.ClientId;

            await buildingRepository.AddBuildingAsync(building, cancellationToken);

            var result = await buildingRepository.SaveChangesAsync(cancellationToken);

            if(!result) throw new BadRequestException("Failed to create building");

            return building.Id.ToString();
        }
    }

}
