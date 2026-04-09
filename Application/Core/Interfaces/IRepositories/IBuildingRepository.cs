using System;
using Domain.Models.Buildings;

namespace Application.Core.Interfaces.IRepositories;

public interface IBuildingRepository
{
    Task<Building?> GetBuildingAsync(Guid id, Guid? brokerId, CancellationToken cancellationToken);
    Task<Building?> GetBuildingDetailsAsync(Guid id, Guid? brokerId, CancellationToken cancellationToken);  
    IQueryable<Building> GetBuildingForClientAsync(Guid id, Guid? brokerId, CancellationToken cancellationToken);
    Task AddBuildingAsync(Building building, CancellationToken cancellationToken);  
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}   
