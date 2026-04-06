using System;
using Application.Policies.DTOs.Response;
using Domain.Models.Policies;

namespace Application.Core.Interfaces.IRepositories;

public interface IPolicyRepository
{
    Task<Policy?> GetPolicyAsync(Guid id, CancellationToken cancellationToken);
    Task<PolicyDetailsDto?> GetPolicyDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<Policy?> GetPolicyForActivationAsync(Guid id,  CancellationToken cancellationToken);
    Task<Policy?> GetPolicyForCancellationAsync(Guid id, CancellationToken cancellationToken);
    Task CreatePolicyAsync(Policy policy, CancellationToken cancellationToken);
    IQueryable<Policy> ListPolicyAsync(
        Guid? clientId,
        Guid? brokerId,
        PolicyStatus? policyStatus,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken
    );
    Task<Policy?> GetPolicyForEndorsementAsync(Guid id, CancellationToken cancellationToken);
    Task<List<PolicyEndorsementsDto>> GetPolicyEndorsementsAsync(CancellationToken cancellationToken);
    Task<List<PolicyEndorsementsDto>> GetPolicyEndorsementForPolicyAsync(Guid policyId, CancellationToken cancellationToken);
    Task<List<PolicyVersionsDto>> GetPolicyVersionsAsync(Guid policyId, CancellationToken cancellationToken);
    Task CreatePolicyEndorsementAsync(PolicyEndorsement policyEndorsement, CancellationToken cancellationToken);
    Task CreatePolicyVersionAsync(PolicyVersion policyVersion, CancellationToken cancellationToken);    
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
