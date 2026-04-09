using System;
using Application.Claims.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Claims;
using MediatR;

namespace Application.Claims.Command;

public class MoveToReview
{
    public class Command : IRequest<MoveToReviewDto>
    {
        public Guid ClaimId { get; set; }
        public decimal ApprovedAmount { get; set; }
        public string ReviewedBy { get; set; } = "Admin";
    }

    public class Handler(IClaimRepository claimRepository) : IRequestHandler<Command, MoveToReviewDto>
    {
        public async Task<MoveToReviewDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var claim = await claimRepository.GetClaimByIdAsync(request.ClaimId, null, cancellationToken);

            if(claim == null) 
            {
                throw new NotFoundException("Claim not found");
            }

            if(claim.Status != ClaimStatus.Submitted && claim.Status != ClaimStatus.UnderReview)
            {
                throw new BadRequestException("Only claims with status Submitted or UnderReview can be moved to review");
            }

            claim.Status = ClaimStatus.UnderReview;
            claim.ReviewedAt = DateOnly.FromDateTime(DateTime.UtcNow);

            var result = await claimRepository.SaveChangesAsync(cancellationToken);
            if (!result)
            {
                throw new Exception("Failed to move claim to review");
            }

            return new MoveToReviewDto
            {
                Id = claim.Id,
                PolicyId = claim.PolicyId,
                Status = claim.Status,
                ReviewedAt = claim.ReviewedAt,
            };
        }
    }
}
