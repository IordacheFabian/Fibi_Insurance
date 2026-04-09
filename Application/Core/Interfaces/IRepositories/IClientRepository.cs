using System;
using Domain.Models.Clients;

namespace Application.Core.Interfaces.IRepositories;

public interface IClientRepository
{
    Task<Client?> GetClientAsync(Guid id, Guid? brokerId, CancellationToken cancellationToken);
    IQueryable<Client> ClientSearchAsync(string? name, string? identifier, Guid? brokerId, CancellationToken cancellationToken);
    Task<Client?> GetClientDetailsAsync(Guid id, Guid? brokerId, CancellationToken cancellationToken);
    Task AddClientAsync(Client client, CancellationToken cancellationToken);    
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
    Task<bool> IdentifierExistsAsync(string identifier, CancellationToken cancellationToken);
}
