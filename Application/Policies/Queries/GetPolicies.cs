using System;
using Application.Core.Interfaces.IRepositories;
using Application.Policies.DTOs.Response;
using AutoMapper;
using Domain.Models.Policies;
using MediatR;

namespace Application.Policies.Queries;

public class GetPolicies 
{
    public class Query : IRequest<List<PolicyListItemDto>>
    {
        public Guid? ClientId { get; set; }
        public Guid? BrokerId { get; set; }
        public PolicyStatus? PolicyStatus { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

    public class Handler(IPolicyRepository policyRepository, IMapper mapper) : IRequestHandler<Query, List<PolicyListItemDto>>
    {
        public async Task<List<PolicyListItemDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var policies = await policyRepository.ListPolicyAsync(request.ClientId, request.BrokerId, request.PolicyStatus, request.StartDate, request.EndDate, cancellationToken);
            return mapper.Map<List<PolicyListItemDto>>(policies);
        }
    }
}
