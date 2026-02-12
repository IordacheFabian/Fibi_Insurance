using System;
using Application.Brokers.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Brokers;
using MediatR;

namespace Application.Brokers.Command;

public class ActivateBroker
{
    public class Command : IRequest<BrokerDto>
    {
        public Guid Id { get; set; }
    }

    public class Handler(IBrokerRepository brokerRepository, IMapper mapper) : IRequestHandler<Command, BrokerDto>
    {
        public async Task<BrokerDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var broker = await brokerRepository.GetBrokerForUpdateAsync(request.Id, cancellationToken);
            if (broker == null)
                throw new NotFoundException("Broker not found");
            
            if(broker.BrokerStatus != BrokerStatus.Active)
            {
                broker.BrokerStatus = BrokerStatus.Active;
                broker.UpdatedAt = DateTime.UtcNow;

                var result = await brokerRepository.SaveChangesAsync(cancellationToken);
                if(!result) throw new BadRequestException("Failed to activate broker");
            }

            return mapper.Map<BrokerDto>(broker);   
        }
    }
}
