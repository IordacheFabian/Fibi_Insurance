using System;
using Application.Claims.Request;
using Application.Claims.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Claims;
using MediatR;

namespace Application.Claims.Command;

public class ApproveClaim
{
    public class Command : IRequest<ApprovedClaimDto>
    {
        public Guid ClaimId { get; set; }
        public ApproveClaimDto ApproveClaimDto { get; set; } = default!;
    }

    public class Handler(IClaimRepository claimRepository) : IRequestHandler<Command, ApprovedClaimDto>
    {
        public async Task<ApprovedClaimDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var claim = await claimRepository.GetClaimByIdAsync(request.ClaimId, cancellationToken);

            if(claim == null) 
            {
                throw new NotFoundException("Claim not found");
            }

            if(claim.Status != ClaimStatus.Submitted && claim.Status != ClaimStatus.UnderReview)
            {
                throw new BadRequestException("Only claims with status Submitted or Under Review can be approved");
            }

            claim.Status = ClaimStatus.Approved;
            claim.ApprovedAmount = request.ApproveClaimDto.ApprovedAmount;
            claim.ApprovedAt = DateOnly.FromDateTime(DateTime.UtcNow);
            claim.ReviewedAt = claim.ApprovedAt;
            claim.RejectionReason = null;

            await claimRepository.SaveChangesAsync(cancellationToken);

            return new ApprovedClaimDto
            {
                Id = claim.Id,
                PolicyId = claim.PolicyId,
                EstimatedDamage = claim.EstimatedDamage,
                ApprovedAmount = claim.ApprovedAmount,
                ApprovedAt = claim.ApprovedAt
            };
        }
    }
}
