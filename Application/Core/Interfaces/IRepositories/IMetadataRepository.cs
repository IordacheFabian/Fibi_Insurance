using System;
using Domain.Models.Metadatas;

namespace Application.Core.Interfaces.IRepositories;

public interface IMetadataRepository
{
    Task<Currency?> GetCurrencyAsync(Guid id, CancellationToken cancellationToken);
    Task<List<FeeConfiguration>> GetActiveFeeConfigurationsAsync(DateOnly date, CancellationToken cancellationToken);
    Task<List<RiskFactorConfiguration>> GetActiveRiskFactorConfigurationsAsync(CancellationToken cancellationToken);
}
