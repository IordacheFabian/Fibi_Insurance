using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.AppUsers;
using Domain.Models.Brokers;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class AuthorizationRepository(AppDbContext context) : IAuthorizationRepository
{
    public async Task AddUserAsync(AppUser user, CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(user, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<Broker?> GetBrokerAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Brokers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<AppUser?> GetUserAsync(string email, CancellationToken cancellationToken)
    {
        return context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
