using System;
using Application.Buildings.DTOs.Request;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using MediatR;

namespace Application.Buildings.Commands;

public class UpdateBuilding
{
    public class Command : IRequest<Unit>
    {
        public Guid BrokerId { get; set; }
        public required UpdateBuildingDto BuildingDto { get; set; }
    }

    public class Handler(IBuildingRepository buildingRepository, ICurrencyRepository currencyRepository, IMapper mapper) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var building = await buildingRepository.GetBuildingAsync(request.BuildingDto.Id, request.BrokerId, cancellationToken);
            if(building == null) throw new NotFoundException("Building not found");

            var currency = await currencyRepository.GetCurrencyAsync(request.BuildingDto.CurrencyId, cancellationToken);
            if(currency == null || !currency.IsActive) throw new NotFoundException("Currency not found");

            mapper.Map(request.BuildingDto, building);

            var result = await buildingRepository.SaveChangesAsync(cancellationToken);

            if(!result) throw new BadRequestException("Failed to update building");

            return Unit.Value;
        }
    }

}
