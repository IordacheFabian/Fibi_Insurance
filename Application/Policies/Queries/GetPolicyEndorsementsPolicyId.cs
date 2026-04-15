using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Policies.DTOs.Response;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Application.Policies.Queries;

public class GetPolicyEndorsementsPolicyId
{
    public class Query : IRequest<List<PolicyEndorsementsDto>>
    {
        public Guid PolicyId { get; set; }
        public Guid? BrokerId { get; set; }
    }

    public class Handler(IPolicyRepository policyRepository, IMapper mapper) : IRequestHandler<Query, List<PolicyEndorsementsDto>>
    {
        public async Task<List<PolicyEndorsementsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var endorsements = await policyRepository.GetPolicyEndorsementForPolicyAsync(request.PolicyId, request.BrokerId, cancellationToken);

            var result = mapper.Map<List<PolicyEndorsementsDto>>(endorsements);
            return result;

        }
    }
}
