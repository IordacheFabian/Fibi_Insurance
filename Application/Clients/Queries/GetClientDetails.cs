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
    public class Query : IRequest<Result<ClientSearchDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Query, Result<ClientSearchDto>>
    {
        public async Task<Result<ClientSearchDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var client = await context.Clients
                .FirstOrDefaultAsync(x => request.Id == x.Id, cancellationToken);

            if(client == null) return Result<ClientSearchDto>.Failure("Client not found", 404);


            return Result<ClientSearchDto>.Success(
                mapper.Map<ClientSearchDto>(client)
            );
        }
    }
}
