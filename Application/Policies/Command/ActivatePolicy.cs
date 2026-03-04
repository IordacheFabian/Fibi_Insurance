using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Policies;
using MediatR;

namespace Application.Policies.Command;

public class ActivatePolicy
{
    public class Command : IRequest<Unit> 
    {
        public required Guid PolicyId { get; set; } 
    }

    public class Handler(IPolicyRepository policyRepository) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var policy = await policyRepository.GetPolicyForActivationAsync(request.PolicyId, cancellationToken);
            if (policy == null) throw new NotFoundException("Policy not found");

            if (policy.PolicyStatus != PolicyStatus.Draft) 
                throw new BadRequestException("Only policies in draft status can be activated");

            var activeVersion = policy.PolicyVersions.SingleOrDefault(v => v.IsActiveVersion);
            if (activeVersion == null)
                throw new BadRequestException("Policy does not have an active version and cannot be activated");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if(activeVersion.StartDate < today) 
                throw new BadRequestException("Policy start date cannot be in the future");

            if (policy.ClientId == Guid.Empty ||
                policy.BuildingId == Guid.Empty ||
                activeVersion.CurrencyId == Guid.Empty ||
                policy.BrokerId == Guid.Empty)
            {
                throw new BadRequestException("Policy is missing mandatory fields and cannot be activated");
            }

            if (activeVersion.EndDate <= activeVersion.StartDate)
                throw new BadRequestException("Policy period is invalid");

            if (activeVersion.BasePremium <= 0 || activeVersion.FinalPremium <= 0)
                throw new BadRequestException("Policy premium values are invalid");

            policy.PolicyStatus = PolicyStatus.Active;
            // policy.UpdatedAt = DateTime.UtcNow;

            var result = await policyRepository.SaveChangesAsync(cancellationToken);
            if(!result) throw new Exception("Failed to activate policy");   

            return Unit.Value;
        }
    }
}
