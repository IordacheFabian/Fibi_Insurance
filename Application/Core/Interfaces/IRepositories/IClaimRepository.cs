using System;
using Domain.Models.Claims;
namespace Application.Core.Interfaces.IRepositories;

public interface IClaimRepository
{
    Task<Claim?> GetClaimByIdAsync(Guid claimId, Guid? brokerId, CancellationToken cancellationToken);
    Task AddAsync (Claim claim, CancellationToken cancellationToken);
    IQueryable<Claim> GetAllClaimsAsync(CancellationToken cancellationToken);
    Task<List<Claim>> GetClaimsByPolicyIdAsync(Guid policyId, Guid? brokerId, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
