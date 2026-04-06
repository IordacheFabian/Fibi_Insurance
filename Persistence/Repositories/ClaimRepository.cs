using System;
using Application.Core.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Domain.Models.Claims;

namespace Persistence.Repositories;

public class ClaimRepository(AppDbContext context) : IClaimRepository
{
    public async Task AddAsync(Claim claim, CancellationToken cancellationToken)
    {
        await context.Claims.AddAsync(claim, cancellationToken);
    }

    public IQueryable<Claim> GetAllClaimsAsync(CancellationToken cancellationToken)
    {
        return context.Claims
            .AsNoTracking()
            .Include(c => c.Policy)
                .ThenInclude(p => p.Client);
    }

    public async Task<Claim?> GetClaimByIdAsync(Guid claimId, CancellationToken cancellationToken)
    {
        return await context.Claims
            .FirstOrDefaultAsync(x => x.Id == claimId, cancellationToken);
    }

    public Task<List<Claim>> GetClaimsByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        return context.Claims
            .AsNoTracking()
            .Where(claim => claim.PolicyId == policyId)
            .OrderByDescending(claim => claim.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
