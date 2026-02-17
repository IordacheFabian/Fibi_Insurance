using System;
using Application.Buildings.DTOs.Request;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Buildings;
using Domain.Models.Metadatas;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class RiskFactorRepository(AppDbContext context) : IRiskFactorRepository
{
    public async Task CreateRiskFactorAsync(RiskFactorConfiguration riskFactorConfiguration, CancellationToken cancellationToken)
    {
        await context.RiskFactorConfigurations.AddAsync(riskFactorConfiguration, cancellationToken);
    }

    public async Task<bool> ExistsOverlappingAsync(RiskLevel riskLevel, Guid? referenceId, BuildingType? buildingType, CancellationToken cancellationToken)
    {
        return await context.RiskFactorConfigurations
            .AsNoTracking()
            .AnyAsync(
                x => x.RiskLevel == riskLevel && 
                x.ReferenceID == referenceId &&
                x.BuildingType == buildingType,
                cancellationToken
            );
    }

    public async Task<List<RiskFactorConfiguration>> GetActiveRiskFactorConfigurationsAsync(CancellationToken cancellationToken)
    {
        var query = context.RiskFactorConfigurations
            .AsNoTracking()
            .Where(x => x.IsActive);

        return await query
            .ToListAsync(cancellationToken);

    }

    public IQueryable<RiskFactorConfiguration> GetRiskFactorConfigurationsAsync(bool? isActive, RiskLevel? riskLevel, CancellationToken cancellationToken)
    {
        var query = context.RiskFactorConfigurations
            .AsNoTracking()
            .AsQueryable();

        if (isActive.HasValue) 
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (riskLevel.HasValue)
        {
            query = query.Where(x => x.RiskLevel == riskLevel.Value);
        }

        query = query.OrderBy(x => x.RiskLevel).ThenBy(x => x.ReferenceID).ThenBy(x => x.BuildingType);

        return query;
    }

    public async Task<RiskFactorConfiguration?> GetRiskFactorForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.RiskFactorConfigurations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
