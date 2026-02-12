using System;
using Domain.Models.Metadatas;

namespace Application.Core.Interfaces.IRepositories;

public interface IFeeConfigurationRepository
{
    Task<List<FeeConfiguration>> GetFeeConfigurationsAsync(bool? isActive, CancellationToken cancellationToken);
    Task CreateFeeConfigurationAsync(FeeConfiguration feeConfiguration, CancellationToken cancellationToken);
    Task<FeeConfiguration?> GetFeeConfigurationAsync(Guid id, CancellationToken cancellationToken);
    Task<List<FeeConfiguration>> GetActiveFeeConfigurationsAsync(DateOnly date, CancellationToken cancellationToken);
    Task<bool> ExistsOverlappingAsync(FeeType feeType, DateOnly effectiveFrom, DateOnly? effectiveTo, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);   
}
