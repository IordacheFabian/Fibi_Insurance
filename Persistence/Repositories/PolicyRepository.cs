using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class PolicyRepository(AppDbContext context) : IPolicyRepository
{
    public async Task CreatePolicyAsync(Policy policy, CancellationToken cancellationToken)
    {
        await context.Policies.AddAsync(policy, cancellationToken);
    }

    public async Task<Policy?> GetPolicyAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Policies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
 
    public async Task<Policy?> GetPolicyDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Policies
            .AsNoTracking()
            .Include(x => x.Client)
            .Include(x => x.Building)
                .ThenInclude(x => x.Address)
                    .ThenInclude(x => x.City)
                        .ThenInclude(x => x.County)
                            .ThenInclude(x => x.Country)
            .Include(x => x.Broker)
            .Include(x => x.Currency)
            .Include(x => x.PolicyAdjustements)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    }

    public async Task<Policy?> GetPolicyForActivationAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Policies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Policy?> GetPolicyForCancellationAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Policies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);   
    }

    public IQueryable<Policy> ListPolicyAsync(Guid? clientId, Guid? brokerId, PolicyStatus? policyStatus, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken)
    {
        var query = context.Policies
            .AsNoTracking()
            .AsQueryable();

            if(clientId.HasValue) 
                query = query.Where(x => x.ClientId == clientId.Value);
            
            if(brokerId.HasValue)
                query = query.Where(x => x.BrokerId == brokerId.Value);

            if(policyStatus.HasValue)
                query = query.Where(x => x.PolicyStatus == policyStatus.Value);

            if(startDate.HasValue) 
                query = query.Where(x => x.StartDate >= startDate.Value);

            if(endDate.HasValue)
                query = query.Where(x => x.EndDate <= endDate.Value);

            query = query
                .Include(x => x.Client)
                .Include(x => x.Building)
                    .ThenInclude(x => x.Address)
                        .ThenInclude(x => x.City)
                .Include(x => x.Currency);

            return query; 
            
        }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
