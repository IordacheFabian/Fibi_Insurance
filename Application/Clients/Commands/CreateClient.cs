using System;
using Application.Clients.DTOs;
using Application.Core;
using AutoMapper;
using Domain.Models.Clients;
using MediatR;
using Persistence.Context;

namespace Application.Clients.Commands;

public class CreateClient
{
    public class Command : IRequest<string>
    {
        public required CreateClientDto ClientDto { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command, string>
    {
        public async Task<string> Handle(Command request, CancellationToken cancellationToken)
        {
            var client = mapper.Map<Client>(request.ClientDto);

            context.Clients.Add(client);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if(!result) throw new BadRequestException("Failed to create client");

            return client.Id.ToString();
        }
    }
}
