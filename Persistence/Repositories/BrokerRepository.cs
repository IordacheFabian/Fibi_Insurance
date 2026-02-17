using System;
using System.Net.Quic;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Brokers;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class BrokerRepository(AppDbContext context) : IBrokerRepository
{
    public async Task<bool> BrokerCodeExistsAsync(string brokerCode, CancellationToken cancellationToken)
    {
        return await context.Brokers
            .AsNoTracking()
            .AnyAsync(x => x.BrokerCode == brokerCode, cancellationToken);
    }

    public async Task CreateBrokerAsync(Broker broker, CancellationToken cancellationToken)
    {
        await context.Brokers.AddAsync(broker, cancellationToken);
    }

    public async Task<Broker?> GetBrokerAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Brokers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Broker?> GetBrokerForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Brokers
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public IQueryable<Broker> GetBrokersAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var query = context.Brokers
            .AsNoTracking()
            .AsQueryable();

        
        if(isActive.HasValue)
        {
            query = query.Where(x => isActive.Value
                ? x.BrokerStatus == BrokerStatus.Active
                : x.BrokerStatus == BrokerStatus.Inactive);
        }

        query = query.OrderBy(x => x.BrokerCode);
        
        return query;
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
