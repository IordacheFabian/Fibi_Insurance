using System;
using Application.Buildings.DTOs.Request;
using Domain.Models.Buildings;
using Domain.Models.Metadatas;

namespace Application.Core.Interfaces.IRepositories;

public interface IRiskFactorRepository
{
    Task<List<RiskFactorConfiguration>> GetRiskFactorConfigurationsAsync(
        bool? isActive, 
        RiskLevel? riskLevel,
        CancellationToken cancellationToken);
    Task<RiskFactorConfiguration?> GetRiskFactorForUpdateAsync(Guid id, CancellationToken cancellationToken);    
    Task<List<RiskFactorConfiguration>> GetActiveRiskFactorConfigurationsAsync(CancellationToken cancellationToken);
    Task CreateRiskFactorAsync(RiskFactorConfiguration riskFactorConfiguration, CancellationToken cancellationToken);
    Task<bool> ExistsOverlappingAsync(RiskLevel riskLevel, Guid? referenceId, BuildingType? buildingType, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
