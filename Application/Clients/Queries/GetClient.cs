using System;
using Application.Clients.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Models.Clients;
using MediatR;

namespace Application.Clients.Queries;

public class GetClient
{
    public class Query : IRequest<List<ClientSearchDto>>
    {
        public string? Name { get; init; }
        public string? Identifier { get; init; }
    }

    public class Handler(IClientRepository clientRepository, IMapper mapper) : IRequestHandler<Query, List<ClientSearchDto>>
    {
        public async Task<List<ClientSearchDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var clients = await clientRepository.ClientSearchAsync(request.Name, request.Identifier, cancellationToken);

            return mapper.Map<List<ClientSearchDto>>(clients);
        }
    }
}
