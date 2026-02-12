using System;
using Application.Brokers.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using MediatR;

namespace Application.Brokers.Queries;

public class GetBrokerDetails
{
    public class Query : IRequest<BrokerDetailsDto>
    {
        public Guid Id { get; set; }
    }

    public class Handler(IBrokerRepository brokerRepository, IMapper mapper) : IRequestHandler<Query, BrokerDetailsDto>
    {
        public async Task<BrokerDetailsDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var broker = await brokerRepository.GetBrokerAsync(request.Id, cancellationToken);
            if (broker == null)
                throw new NotFoundException("Broker not found");

            return mapper.Map<BrokerDetailsDto>(broker);
        }
    }
}
