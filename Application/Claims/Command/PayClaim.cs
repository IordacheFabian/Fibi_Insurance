using System;
using Application.Claims.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Claims;
using MediatR;

namespace Application.Claims.Command;

public class PayClaim 
{
    public class Command : IRequest<PaidClaimDto>
    {
        public Guid ClaimId { get; set; }
        public string PaidBy { get; set; } = default!;
    }

    public class Handler(IClaimRepository claimRepository) : IRequestHandler<Command, PaidClaimDto>
    {
        public async Task<PaidClaimDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var claim = await claimRepository.GetClaimByIdAsync(request.ClaimId, cancellationToken);

            if(claim == null) 
            {
                throw new NotFoundException("Claim not found");
            }

            if(claim.Status != ClaimStatus.Approved)
            {
                throw new BadRequestException("Only claims with status Approved can be paid");
            }

            if(!claim.ApprovedAmount.HasValue || claim.ApprovedAmount.Value <= 0)
            {
                throw new BadRequestException("Approved amount must be greater than zero to pay the claim");
            }

            claim.Status = ClaimStatus.Paid;
            claim.PaidAt = DateOnly.FromDateTime(DateTime.UtcNow);
            var result = await claimRepository.SaveChangesAsync(cancellationToken);
            if (!result)
            {
                throw new Exception("Failed to pay claim");
            }
        
            return new PaidClaimDto
            {
                Id = claim.Id,
                PolicyId = claim.PolicyId,
                EstimatedDamage = claim.EstimatedDamage,
                ApprovedAmount = claim.ApprovedAmount,
                ApprovedAt = claim.ApprovedAt,
                PaidAt = claim.PaidAt
            };
        }
    }
}
