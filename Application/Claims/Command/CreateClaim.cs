using System;
using Application.Claims.Request;
using Application.Claims.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Claims;
using Domain.Models.Policies;
using MediatR;

namespace Application.Claims.Command;

public class CreateClaim
{
    public class Command : IRequest<ClaimDto>
    {
        public Guid PolicyId { get; set; }
        public CreateClaimDto Claim { get; set; } = default!;
    }

    public class Handler(IClaimRepository claimRepository, IPolicyRepository policyRepository) : IRequestHandler<Command, ClaimDto>
    {
        public async Task<ClaimDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var policy = await policyRepository.GetPolicyAsync(request.PolicyId, cancellationToken);

            if(policy == null)
            {
                throw new NotFoundException("Policy not found");
            }

            if(policy.PolicyStatus != PolicyStatus.Active)
            {
                throw new BadRequestException("Claims can only be made for active policies");
            }

            var claim = new Claim
            {
                Id = Guid.NewGuid(),
                PolicyId = request.PolicyId,
                Description = request.Claim.Description,
                IncidentDate = request.Claim.IncidentDate,
                EstimatedDamage = request.Claim.EstimatedDamage,
                Status = ClaimStatus.Submitted,
                CreatedAt = DateTime.UtcNow
            };

            await claimRepository.AddAsync(claim, cancellationToken);
            var result = await claimRepository.SaveChangesAsync(cancellationToken);
            if(!result)
            {
                throw new BadRequestException("Failed to create claim");
            }
            
            return new ClaimDto
            {
                Id = claim.Id,
                PolicyId = claim.PolicyId,
                Description = claim.Description,
                IncidentDate = claim.IncidentDate,
                EstimatedDamage = claim.EstimatedDamage,
                ApprovedAmount = claim.ApprovedAmount,
                Status = claim.Status.ToString(),
                RejectionReason = claim.RejectionReason,
                CreatedAt = claim.CreatedAt,
                ReviewedAt = claim.ReviewedAt
            };

        }
    }
}
