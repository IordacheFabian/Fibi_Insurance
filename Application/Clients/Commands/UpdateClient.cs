using System;
using Application.Clients.DTOs;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using MediatR;

namespace Application.Clients.Commands;

public class UpdateClient
{
    public class Command : IRequest<Unit>
    {
        public Guid Id { get; init; }
        public required UpdateClientDto ClientDto { get; set; }
    }

    public class Handler(IClientRepository clientRepository, IMapper mapper) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var client = await clientRepository.GetClientAsync(request.Id, cancellationToken);
                            
            if (client == null) throw new Exception("Client not found");

            mapper.Map(request.ClientDto, client);

            var result = await clientRepository.SaveChangesAsync(cancellationToken);

            if(!result) throw new BadRequestException("Failed to update client details");

            return Unit.Value;
        }
    }
}
