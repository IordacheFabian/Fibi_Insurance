using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.util;
using Application.Policies.DTOs.Requests;
using AutoMapper;
using Domain.Models.Policies;
using MediatR;

namespace Application.Policies.Command;

public class CreatePolicyEndorsement
{
    public class Command : IRequest<Unit>
    {
        public Guid PolicyId { get; set; }
        public CreatePolicyEndorsementDto CreatePolicyEndorsementDto { get; set; } = default!;
        public string CreatedBy { get; set; } = default!;
    }

    public class Handler(
        IPolicyRepository policyRepository,
        IPremiumCalculator premiumCalculator) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var policy = await policyRepository.GetPolicyForEndorsementAsync(request.PolicyId, cancellationToken);

            if (policy == null)
                throw new NotFoundException("Policy not found");

            if (policy.PolicyStatus != PolicyStatus.Active)
                throw new BadRequestException("Only active policies can be endorsed");

            var activeVersion = policy.PolicyVersions.SingleOrDefault(v => v.IsActiveVersion);
            if (activeVersion == null)
                throw new BadRequestException("Policy does not have an active version and cannot be endorsed");

            var dto = request.CreatePolicyEndorsementDto;

            if (dto.EffectiveDate < activeVersion.StartDate || dto.EffectiveDate > activeVersion.EndDate)
                throw new BadRequestException("Endorsement effective date must be within the policy period");

            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new BadRequestException("Endorsement reason is required");

            var newVersionNumber = activeVersion.VersionNumber + 1;
            var createdBy = string.IsNullOrWhiteSpace(request.CreatedBy) || request.CreatedBy == "Unknown"
                ? policy.Broker?.Name ?? "Unknown"
                : request.CreatedBy;

            var newPolicyVersion = new PolicyVersion
            {
                Id = Guid.NewGuid(),
                PolicyId = policy.Id,
                VersionNumber = newVersionNumber,
                StartDate = activeVersion.StartDate,
                EndDate = activeVersion.EndDate,
                BasePremium = activeVersion.BasePremium,
                FinalPremium = activeVersion.FinalPremium,
                CurrencyId = activeVersion.CurrencyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsActiveVersion = true,
                PolicyAdjustments = new List<PolicyAdjustment>()
            };

            ApplyEndorsement.ApplyEndorsementChanges(dto, newPolicyVersion);

            var (finalPremium, policyAdjustments) = await premiumCalculator.CalculateAsync(
                policy.Building,
                newPolicyVersion.BasePremium,
                newPolicyVersion.StartDate,
                cancellationToken);

            newPolicyVersion.FinalPremium = finalPremium;

            foreach (var adjustment in policyAdjustments)
            {
                adjustment.PolicyVersion = newPolicyVersion;
                newPolicyVersion.PolicyAdjustments.Add(adjustment);
            }

            activeVersion.IsActiveVersion = false;

            await policyRepository.CreatePolicyVersionAsync(newPolicyVersion, cancellationToken);

            var policyEndorsement = new PolicyEndorsement
            {
                Id = Guid.NewGuid(),
                PolicyId = policy.Id,
                Policy = policy,
                EndorsementType = dto.EndorsementType,
                EffectiveDate = dto.EffectiveDate,
                Reason = dto.Reason,
                OldVersionNumber = activeVersion.VersionNumber,
                NewVersionNumber = newVersionNumber,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await policyRepository.CreatePolicyEndorsementAsync(policyEndorsement, cancellationToken);

            var result = await policyRepository.SaveChangesAsync(cancellationToken);
            if (!result)
                throw new Exception("Failed to create policy endorsement");

            return Unit.Value;

        }
    }
}
