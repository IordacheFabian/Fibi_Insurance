using System;
using Application.Clients.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;


namespace Application.Clients.Queries;

public class GetClientDetails
{
    public class Query : IRequest<ClientDetailsDto>
    {
        public Guid Id { get; set; }
        public Guid BrokerId { get; set; }
    }

    public class Handler(IClientRepository clientRepository, IMapper mapper) : IRequestHandler<Query, ClientDetailsDto>
    {
        public async Task<ClientDetailsDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var client = await clientRepository.GetClientDetailsAsync(request.Id, request.BrokerId, cancellationToken);
            if (client is null)
                throw new NotFoundException("Client not found");

            return mapper.Map<ClientDetailsDto>(client);
        }
    }
}
