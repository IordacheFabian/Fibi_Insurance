using System;
using Application.Claims.Response;
using Application.Core.Interfaces.IRepositories;
using MediatR;

namespace Application.Claims.Query;

public class GetPolicyClaims
{
    public class Query : IRequest<List<ClaimDto>>
    {
        public Guid PolicyId { get; set; }
        public Guid BrokerId { get; set; }
    }

    public class Handler(IClaimRepository claimRepository) : IRequestHandler<Query, List<ClaimDto>>
    {

        public async Task<List<ClaimDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var claims = await claimRepository.GetClaimsByPolicyIdAsync(request.PolicyId, request.BrokerId, cancellationToken);
            return claims.Select(claim => new ClaimDto
            {
                Id = claim.Id,
                PolicyId = claim.PolicyId,
                Description = claim.Description,
                IncidentDate = claim.IncidentDate,
                EstimatedDamage = claim.EstimatedDamage,
                Status = claim.Status.ToString(),
                ApprovedAmount = claim.ApprovedAmount,
                CreatedAt = claim.CreatedAt,
                ReviewedAt = claim.ReviewedAt,
                ApprovedAt = claim.ApprovedAt,
                RejectionReason = claim.RejectionReason,
                RejectedAt = claim.RejectedAt,
                PaidAt = claim.PaidAt
            }).ToList();
        }
    }
}
