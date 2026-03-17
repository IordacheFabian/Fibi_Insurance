using System;
using Domain.Models.AppUsers;
using Domain.Models.Brokers;
using Microsoft.VisualBasic;

namespace Application.Core.Interfaces.IRepositories;

public interface IAuthorizationRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<AppUser?> GetUserAsync(string email, CancellationToken cancellationToken);
    Task<Broker?> GetBrokerAsync(Guid id, CancellationToken cancellationToken); 
    Task AddUserAsync(AppUser user, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
