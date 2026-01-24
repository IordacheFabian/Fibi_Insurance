using System;
using Application.Clients.DTOs.Response;
using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Clients.Queries;

public class GetClientDetails
{
    public class Query : IRequest<ClientDetailsDto>
    {
        public Guid Id { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Query, ClientDetailsDto>
    {
        public async Task<ClientDetailsDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var client = await context.Clients
                .AsNoTracking()
                .Include(x => x.Buildings)
                    .ThenInclude(x => x.Address)
                        .ThenInclude(x => x.City)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (client is null)
                throw new NotFoundException("Client not found");

            return mapper.Map<ClientDetailsDto>(client);
        }
    }
}
