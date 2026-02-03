using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Geography.Address;
using Persistence.Context;

namespace Persistence.Repositories;

public class AddressRepository(AppDbContext context) : IAddressRepository
{
    public async Task AddAddressAsync(Address address, CancellationToken cancellationToken)
    {
        await context.Addresses.AddAsync(address, cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
