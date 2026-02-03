using System;
using Application.Clients.DTOs;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Clients;
using MediatR;

namespace Application.Clients.Commands;

public class CreateClient
{
    public class Command : IRequest<string>
    {
        public required CreateClientDto ClientDto { get; set; }
    }

    public class Handler(IClientRepository clientRepository, IMapper mapper) : IRequestHandler<Command, string>
    {
        public async Task<string> Handle(Command request, CancellationToken cancellationToken)
        {
            var client = mapper.Map<Client>(request.ClientDto);

            await clientRepository.AddClientAsync(client, cancellationToken);

            var result = await clientRepository.SaveChangesAsync(cancellationToken);
            if(!result) throw new BadRequestException("Failed to create client");

            return client.Id.ToString();
        }
    }
}
