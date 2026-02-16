using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Metadatas;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class FeeConfigurationRepository(AppDbContext context) : IFeeConfigurationRepository
{
    public async Task CreateFeeConfigurationAsync(FeeConfiguration feeConfiguration, CancellationToken cancellationToken)
    {
        await context.FeeConfigurations.AddAsync(feeConfiguration, cancellationToken);
    }

    public async Task<bool> ExistsOverlappingAsync(FeeType feeType, DateOnly effectiveFrom, DateOnly? effectiveTo, CancellationToken cancellationToken)
    {
        return await context.FeeConfigurations
            .AsNoTracking()
            .AnyAsync(
                x => x.FeeType == feeType && 
                (x.EffectiveTo == null || x.EffectiveTo.Value >= effectiveFrom) &&
                (effectiveTo == null || x.EffectiveFrom <= effectiveTo.Value),
                cancellationToken);
    }

    public async Task<List<FeeConfiguration>> GetActiveFeeConfigurationsAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var query = context.FeeConfigurations
            .Where(x => x.IsActive 
                && x.EffectiveFrom <= date 
                && (x.EffectiveTo == null || x.EffectiveTo.Value >= date));

        return await query
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);

    }

    public async Task<FeeConfiguration?> GetFeeConfigurationAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.FeeConfigurations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<FeeConfiguration>> GetFeeConfigurationsAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var query = context.FeeConfigurations
            .AsNoTracking();

        if(isActive.HasValue  )
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(x => x.FeeType)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
