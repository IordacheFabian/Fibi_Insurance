using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Metadatas;
using Domain.Models.Payments;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class PaymentRepository(AppDbContext context) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        await context.Payments.AddAsync(payment, cancellationToken);   
    }

    public async Task<List<Payment>> GetAllPaymentsAsync(CancellationToken cancellationToken)
    {
        return await context.Payments
            .AsNoTracking()
            .Include(x => x.Policy)
                .ThenInclude(x => x.Client)
            .Include(x => x.Currency)
            .OrderByDescending(x => x.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetCompletedPaidAmountAsync(Guid policyId, CancellationToken cancellationToken)
    {
        return await context.Payments
            .Where(x => x.PolicyId == policyId && x.Status == PaymentStatus.Completed)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
    }

    public async Task<bool> GetCurrencyByIdAsync(Guid currencyId, CancellationToken cancellationToken)
    {
        return await context.Currencies
            .AnyAsync(x => x.Id == currencyId && x.IsActive, cancellationToken);
    }

    public async Task<List<Payment>> GetPaymentsByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        return await context.Payments
            .AsNoTracking()
            .Include(x => x.Policy)
                .ThenInclude(x => x.Client)
            .Include(x => x.Currency)
            .Where(x => x.PolicyId == policyId)
            .OrderByDescending(x => x.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        return await context.Policies
        .Include(x => x.Client)
        .Include(x => x.PolicyVersions)
            .ThenInclude(x => x.Currency)
        .FirstOrDefaultAsync(x => x.Id == policyId, cancellationToken);
    }

    public async Task<bool> PolicyExistsAsync(Guid policyId, CancellationToken cancellationToken)
    {
        return await context.Policies.AnyAsync(x => x.Id == policyId, cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}
