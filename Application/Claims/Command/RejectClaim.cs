using System;
using Application.Claims.Request;
using Application.Claims.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Claims;
using MediatR;

namespace Application.Claims.Command;

public class RejectClaim
{
    public class Command : IRequest<RejectedClaimDto>
    {
        public Guid ClaimId { get; set; }
        public RejectClaimDto RejectClaimDto { get; set; } = default!;
    }

    public class Handler(IClaimRepository claimRepository) : IRequestHandler<Command, RejectedClaimDto>
    {
        public async Task<RejectedClaimDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var claim = await claimRepository.GetClaimByIdAsync(request.ClaimId, cancellationToken);

            if(claim == null) 
            {
                throw new NotFoundException("Claim not found");
            }

            if(claim.Status != ClaimStatus.Submitted && claim.Status != ClaimStatus.UnderReview)
            {
                throw new BadRequestException("Only claims with status Submitted or Under Review can be rejected");
            }

            claim.Status = ClaimStatus.Rejected;
            claim.RejectionReason = request.RejectClaimDto.Reason;
            claim.ReviewedAt = DateOnly.FromDateTime(DateTime.UtcNow);
            claim.ApprovedAmount = null;

            await claimRepository.SaveChangesAsync(cancellationToken);

            return new RejectedClaimDto
            {
                Id = claim.Id,
                PolicyId = claim.PolicyId,
                EstimatedDamage = claim.EstimatedDamage,
                Status = claim.Status,
                RejectionReason = claim.RejectionReason,
                ReviewedAt = claim.ReviewedAt
            };
        }
    }
}
