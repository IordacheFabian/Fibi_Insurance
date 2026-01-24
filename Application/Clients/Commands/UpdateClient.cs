using System;
using Application.Clients.DTOs;
using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Clients.Commands;

public class UpdateClient
{
    public class Command : IRequest<Unit>
    {
        public Guid Id { get; init; }
        public required UpdateClientDto ClientDto { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var client = await context.Clients
                            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
                            
            if (client == null) throw new NotFoundException("Client not found");

            mapper.Map(request.ClientDto, client);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if(!result) throw new BadRequestException("Failed to update client details");

            return Unit.Value;
        }
    }
}
