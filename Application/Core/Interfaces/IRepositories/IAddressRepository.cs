using System;
using Domain.Models.Geography.Address;

namespace Application.Core.Interfaces.IRepositories;

public interface IAddressRepository
{
    Task AddAddressAsync(Address address, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
