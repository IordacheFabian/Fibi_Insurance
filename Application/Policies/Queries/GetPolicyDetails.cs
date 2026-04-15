using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Policies.DTOs.Response;
using AutoMapper;
using Domain.Models.Policies;
using MediatR;

namespace Application.Policies.Queries;

public class GetPolicyDetails
{
    public class Query : IRequest<PolicyDetailsDto>
    {
        public Guid PolicyId { get; set; }
        public Guid? BrokerId { get; set; }
    }

    public class Handler(IPolicyRepository policyRepository, IMapper mapper) : IRequestHandler<Query, PolicyDetailsDto>
    {
        public async Task<PolicyDetailsDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var policy = await policyRepository.GetPolicyDetailsAsync(request.PolicyId, request.BrokerId, cancellationToken);

            if(policy == null) 
                throw new NotFoundException("Policy not found");

            return mapper.Map<PolicyDetailsDto>(policy);
        }
    }
}
