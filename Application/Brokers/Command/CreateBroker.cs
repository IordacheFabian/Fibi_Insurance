using System;
using Application.Brokers.DTOs.Request;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Brokers;
using MediatR;

namespace Application.Brokers.Command;

public class CreateBroker
{
    public class Command : IRequest<CreateBrokerDto>
    {
        public required CreateBrokerDto CreateBrokerDto { get; set; } 
    }

    public class Handler(IBrokerRepository brokerRepository, IMapper mapper) : IRequestHandler<Command, CreateBrokerDto>
    {
        public async Task<CreateBrokerDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var createBrokerDto = request.CreateBrokerDto;

            var brokerCode = createBrokerDto.BrokerCode.Trim().ToUpperInvariant();

            if(await brokerRepository.BrokerCodeExistsAsync(brokerCode, cancellationToken))
                throw new BadRequestException("Broker code already exists");

            var broker = mapper.Map<Broker>(createBrokerDto);

            await brokerRepository.CreateBrokerAsync(broker, cancellationToken);

            var result = await brokerRepository.SaveChangesAsync(cancellationToken);
            if(!result) throw new BadRequestException("Failed to create broker");

            return mapper.Map<CreateBrokerDto>(broker);
        }
    }


}
