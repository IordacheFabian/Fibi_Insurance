using System;
using Application.Clients.DTOs;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Clients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Clients.Commands;

public class CreateClient
{
    public class Command : IRequest<string>
    {
        public Guid BrokerId { get; set; }
        public required CreateClientDto ClientDto { get; set; }
    }

    public class Handler(IClientRepository clientRepository, IMapper mapper) : IRequestHandler<Command, string>
    {
        public async Task<string> Handle(Command request, CancellationToken cancellationToken)
        {
            var client = mapper.Map<Client>(request.ClientDto);
            client.BrokerId = request.BrokerId;

            await clientRepository.AddClientAsync(client, cancellationToken);

            try
            {
                var result = await clientRepository.SaveChangesAsync(cancellationToken);
                if(!result) throw new BadRequestException("Failed to create client");
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) == true)
            {
                throw new BadRequestException("A client with this identification number already exists.");
            }

            return client.Id.ToString();
        }
    }
}
