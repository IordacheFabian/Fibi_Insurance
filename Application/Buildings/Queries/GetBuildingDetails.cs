using System;
using Application.Buildings.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using MediatR;

namespace Application.Buildings.Queries;

public class GetBuildingDetails
{
    public class Query : IRequest<BuildingDetailsDto>
    {
        public Guid Id { get; set; }
        public Guid BrokerId { get; set; }
    }

    public class Handler(IBuildingRepository buildingRepository, IMapper mapper) : IRequestHandler<Query, BuildingDetailsDto>
    {
        public async Task<BuildingDetailsDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var building = await buildingRepository.GetBuildingDetailsAsync(request.Id, request.BrokerId, cancellationToken);

            if(building == null) throw new NotFoundException("Building not found");

            var result = mapper.Map<BuildingDetailsDto>(building);

            return result;
        }
    }
}
