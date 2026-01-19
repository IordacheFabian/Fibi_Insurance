using System;
using Application.Clients.DTOs.Response;
using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Clients.Queries;

public class GetClient
{
    public class Query : IRequest<Result<List<ClientSearchDto>>>
    {
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Query,Result<List<ClientSearchDto>>>
    {
        public async Task<Result<List<ClientSearchDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var clients = await context.Clients
                .ProjectTo<ClientSearchDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<ClientSearchDto>>.Success(clients);
        }
    }
}
