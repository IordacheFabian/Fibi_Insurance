using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Policies.DTOs.Requests;
using Domain.Models.Policies;
using MediatR;

namespace Application.Policies.Command;

public class CancelPolicy
{
    public class Command : IRequest<Unit>
    {
        public required Guid PolicyId { get; set; }
        public required CancelPolicyDto CancelPolicyDto { get; set; }
    }

    public class Handler(IPolicyRepository policyRepository) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var policy = await policyRepository.GetPolicyForCancellationAsync(request.PolicyId, cancellationToken);
            if (policy == null) throw new NotFoundException("Policy not found");

            if (policy.PolicyStatus != PolicyStatus.Active) 
                throw new BadRequestException("Only active policies can be cancelled");
            
            var activeVersion = policy.PolicyVersions.SingleOrDefault(v => v.IsActiveVersion);
            if (activeVersion == null)
                throw new BadRequestException("Policy does not have an active version and cannot be cancelled");

            if (string.IsNullOrWhiteSpace(request.CancelPolicyDto.CancellationReason))
                throw new BadRequestException("Cancellation reason is required");   

            var effectiveDate = request.CancelPolicyDto.CancellationDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            if(effectiveDate < activeVersion.StartDate)
                throw new BadRequestException("Cancellation date cannot be before policy start date");

            if(effectiveDate > activeVersion.EndDate)
                throw new BadRequestException("Cancellation date cannot be after policy end date");

            policy.PolicyStatus = PolicyStatus.Cancelled;
            // policy.CancelledAt = effectiveDate;
            // policy.CancellationReason = request.CancelPolicyDto.CancellationReason;
            // policy.UpdatedAt = DateTime.UtcNow;

            var result = await policyRepository.SaveChangesAsync(cancellationToken);
            if(!result) throw new Exception("Failed to cancel policy");

            return Unit.Value;
        }
    }
}
