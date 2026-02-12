using System;
using Application.Brokers.DTOs.Response;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using MediatR;

namespace Application.Brokers.Queries;

public class GetBrokers
{
    public class Query : IRequest<List<BrokerDto>>
    {
        public bool? IsActive { get; set; }
    }

    public class Handler(IBrokerRepository brokerRepository, IMapper mapper) : IRequestHandler<Query, List<BrokerDto>>
    {
        public async Task<List<BrokerDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var brokers = await brokerRepository.GetBrokersAsync(request.IsActive, cancellationToken);
            return mapper.Map<List<BrokerDto>>(brokers);
        }
    }
}
