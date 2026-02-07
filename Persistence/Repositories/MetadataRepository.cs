using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Metadatas;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class MetadataRepository(AppDbContext context) : IMetadataRepository
{
    public async Task<List<FeeConfiguration>> GetActiveFeeConfigurationsAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var query =  context.FeeConfigurations
            .AsNoTracking()
            .Where(x => x.IsActive && x.EffectiveFrom <= date && x.EffectiveTo >= date);

        return await query
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RiskFactorConfiguration>> GetActiveRiskFactorConfigurationsAsync(CancellationToken cancellationToken)
    {
        var query = context.RiskFactorConfigurations
            .AsNoTracking()
            .Where(x => x.IsActive);

        return await query
            .ToListAsync(cancellationToken);
    }

    public async Task<Currency?> GetCurrencyAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
