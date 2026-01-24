using System;
using Application.Clients.DTOs.Response;
using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Models.Clients;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Clients.Queries;

public class GetClient
{
    public class Query : IRequest<List<ClientSearchDto>>
    {
        public string? Name { get; init; }
        public string? Identifier { get; init; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Query, List<ClientSearchDto>>
    {
        public async Task<List<ClientSearchDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = context.Clients
                .AsNoTracking()
                .AsQueryable();

            if(!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(x => x.Name.Contains(request.Name));
            }

            if(!string.IsNullOrWhiteSpace(request.Identifier))
            {
                query = query.Where(x => x.IdentificationNumber == request.Identifier);
            }

            var clients = await query 
                .Include(x => x.Buildings)
                    .ThenInclude(x => x.Address)
                        .ThenInclude(x => x.City)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<ClientSearchDto>>(clients);
        }
    }
}
