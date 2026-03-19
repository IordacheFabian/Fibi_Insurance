using System;
using Domain.Models.Claims;
namespace Application.Core.Interfaces.IRepositories;

public interface IClaimRepository
{
    Task<Claim?> GetClaimByIdAsync(Guid claimId, CancellationToken cancellationToken);
    Task AddAsync (Claim claim, CancellationToken cancellationToken);
    Task<List<Claim>> GetAllClaimsAsync(CancellationToken cancellationToken);
    Task<List<Claim>> GetClaimsByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
