using System;
using System.Threading.Tasks;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Metadatas;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class CurrencyRepository(AppDbContext context) : ICurrencyRepository
{
    public async Task CreateCurrencyAsync(Currency currency, CancellationToken cancellationToken)
    {
        await context.Currencies.AddAsync(currency, cancellationToken);
    }

    public Task<bool> CurrencyCodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        return context.Currencies.AnyAsync(x => x.Code == code, cancellationToken);
    }

    public IQueryable<Currency> GetCurrenciesAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var query = context.Currencies
            .AsNoTracking()
            .AsQueryable();

        if(isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }
        
        query = query.OrderBy(x => x.Code);

        return query;
    }

    public async Task<Currency?> GetCurrencyAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Currency?> GetCurrencyForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Currencies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);   
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
