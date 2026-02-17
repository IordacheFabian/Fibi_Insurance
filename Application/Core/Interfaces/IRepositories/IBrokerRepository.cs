using System;
using Domain.Models.Brokers;

namespace Application.Core.Interfaces.IRepositories;

public interface IBrokerRepository
{
    IQueryable<Broker> GetBrokersAsync(bool? isActive, CancellationToken cancellationToken);
    Task<Broker?> GetBrokerAsync(Guid id, CancellationToken cancellationToken);
    Task<Broker?> GetBrokerForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> BrokerCodeExistsAsync(string brokerCode, CancellationToken cancellationToken);   
    Task CreateBrokerAsync(Broker broker, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);   
}   
