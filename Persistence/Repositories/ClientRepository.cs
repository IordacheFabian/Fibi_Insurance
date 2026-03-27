using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Clients;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories.Clients;

public class ClientRepository(AppDbContext context) : IClientRepository
{
    public async Task AddClientAsync(Client client, CancellationToken cancellationToken)
    {
        await context.Clients.AddAsync(client, cancellationToken);
    }

    public async Task<Client?> GetClientAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Clients
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Client?> GetClientDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Clients
            .AsNoTracking()
            .Include(x => x.Buildings)
                .ThenInclude(x => x.Address)
                    .ThenInclude(x => x.City)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public IQueryable<Client> ClientSearchAsync(string? name, string? identifier, CancellationToken cancellationToken)
    {
        var query = context.Clients
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => EF.Functions.Like(x.Name, $"%{name}%"));
        }

        if (!string.IsNullOrWhiteSpace(identifier))
        {
            query = query.Where(x => x.IdentificationNumber == identifier);
        }

        return query
            .Include(x => x.Buildings)
                .ThenInclude(x => x.Address)
                    .ThenInclude(x => x.City)
            .OrderBy(x => x.Name);
            
    }

    public async Task<bool> IdentifierExistsAsync(string identifier, CancellationToken cancellationToken)
    {
        return await context.Clients
            .AsNoTracking()
            .AnyAsync(x => x.IdentificationNumber == identifier, cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
