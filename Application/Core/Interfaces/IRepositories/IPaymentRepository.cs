using System;
using Domain.Models.Metadatas;
using Domain.Models.Payments;
using Domain.Models.Policies;

namespace Application.Core.Interfaces.IRepositories;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken);
    Task<List<Payment>> GetPaymentsByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken);
    Task<List<Payment>> GetAllPaymentsAsync(CancellationToken cancellationToken);
    Task<decimal> GetCompletedPaidAmountAsync(Guid policyId, CancellationToken cancellationToken);

    Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken);
    Task<bool> GetCurrencyByIdAsync(Guid currencyId, CancellationToken cancellationToken);
    Task<bool> PolicyExistsAsync(Guid policyId, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);   
}
