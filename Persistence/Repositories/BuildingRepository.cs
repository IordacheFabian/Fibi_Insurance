using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Buildings;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class BuildingRepository(AppDbContext context) : IBuildingRepository
{
    public async Task AddBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        await context.Buildings.AddAsync(building, cancellationToken);
    }

    public async Task<Building?> GetBuildingAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Buildings
            .AsNoTracking()
            .Include(b => b.Address)
                .ThenInclude(a => a.City)
                    .ThenInclude(c => c.County)
                        .ThenInclude(co => co.Country)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Building?> GetBuildingDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Buildings
            .AsNoTracking()
            .Include(x => x.Client)
            .Include(x => x.Address)
                .ThenInclude(x => x.City)
                    .ThenInclude(x => x.County)
                        .ThenInclude(x => x.Country)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Building>> GetBuildingForClientAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Buildings
            .AsNoTracking()
            .Where(x => x.ClientId == id)
            .Include(x => x.Address)
                .ThenInclude(x => x.City)
            .OrderBy(x => x.Address.Street)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
