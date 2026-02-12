using System;
using Application.Brokers.DTOs.Request;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using MediatR;

namespace Application.Brokers.Command;

public class UpdateBroker
{
    public class Command : IRequest<UpdateBrokerDto>
    {
        public Guid Id { get; set; }
        public UpdateBrokerDto UpdateBrokerDto { get; set; } = null!;
    }

    public class Handler(IBrokerRepository brokerRepository, IMapper mapper) : IRequestHandler<Command, UpdateBrokerDto>
    {
        public async Task<UpdateBrokerDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var broker = await brokerRepository.GetBrokerForUpdateAsync(request.Id, cancellationToken);
            if(broker == null) 
                throw new NotFoundException ("Broker not found");

            mapper.Map(request.UpdateBrokerDto, broker);

            var result = await brokerRepository.SaveChangesAsync(cancellationToken);
            if(!result) 
                throw new BadRequestException("Failed to update broker details");

            return mapper.Map<UpdateBrokerDto>(broker);
        }
    }
}
