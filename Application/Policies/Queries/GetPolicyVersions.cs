using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Policies.DTOs.Response;
using AutoMapper;
using Domain.Models.Policies;
using MediatR;

namespace Application.Policies.Queries;

public class GetPolicyVersions
{
    public class Query : IRequest<List<PolicyVersionsDto>>
    {
        public Guid PolicyId { get; set; }
    }

    public class Handler(IPolicyRepository policyRepository, IMapper mapper) : IRequestHandler<Query, List<PolicyVersionsDto>>
    {
        public async Task<List<PolicyVersionsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var versions = await policyRepository.GetPolicyVersionsAsync(request.PolicyId, cancellationToken);
            if (versions == null)
                throw new NotFoundException("Policy not found");

            var result = mapper.Map<List<PolicyVersionsDto>>(versions);
            return result;
        }
    }
}
