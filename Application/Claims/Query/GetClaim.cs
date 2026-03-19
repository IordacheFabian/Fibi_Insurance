using System;
using Application.Claims.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using MediatR;

namespace Application.Claims.Query;

public class GetClaim
{
    public class Query : IRequest<ClaimDto>
    {
        public Guid ClaimId { get; set; }
    }

    public class Handler(IClaimRepository claimRepository) : IRequestHandler<Query, ClaimDto>
    {
        public async Task<ClaimDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var claim = await claimRepository.GetClaimByIdAsync(request.ClaimId, cancellationToken);
            if(claim == null)
            {
                throw new NotFoundException("Claim not found");
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
                ReviewedAt = claim.ReviewedAt,
                PaidAt = claim.PaidAt
            };
        }
    }
}
