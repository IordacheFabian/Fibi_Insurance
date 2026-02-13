using System;
using Domain.Models.Metadatas;

namespace Application.Core.Interfaces.IRepositories;

public interface ICurrencyRepository
{
    Task<List<Currency>> GetCurrenciesAsync(bool? isActive, CancellationToken cancellationToken);
    Task<Currency?> GetCurrencyAsync(Guid id, CancellationToken cancellationToken);
    Task<Currency?> GetCurrencyForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task CreateCurrencyAsync(Currency currency, CancellationToken cancellationToken);
    Task<bool> CurrencyCodeExistsAsync(string code, CancellationToken cancellationToken);
    // Task RemoveCurrencyAsync(Currency currency, CancellationToken cancellationToken);   
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);   
}
