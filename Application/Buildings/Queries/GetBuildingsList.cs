using System;
using Application.Buildings.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Buildings;
using MediatR;

namespace Application.Buildings.Queries;

public class GetBuildingsList
{
    public class Query : IRequest<List<BuildingListDto>>
    {
        public Guid ClientId { get; set; }
    }

    public class Handler(IBuildingRepository buildingRepository, IClientRepository clientRepository, IMapper mapper) : IRequestHandler<Query, List<BuildingListDto>>
    {
        public async Task<List<BuildingListDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var client = await clientRepository.GetClientAsync(request.ClientId, cancellationToken);

            if(client == null) throw new NotFoundException("Client not found");

            var buildings = await buildingRepository.GetBuildingForClientAsync(request.ClientId, cancellationToken);

            return mapper.Map<List<BuildingListDto>>(buildings);
        }
    }
}
