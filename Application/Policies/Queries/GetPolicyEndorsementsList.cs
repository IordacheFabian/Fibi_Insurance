using Application.Core.Interfaces.IRepositories;
using Application.Policies.DTOs.Response;
using MediatR;

namespace Application.Policies.Queries;

public class GetPolicyEndorsementsList
{
    public class Query : IRequest<List<PolicyEndorsementsDto>>
    {
        public Guid? BrokerId { get; set; }
    }

    public class Handler(IPolicyRepository policyRepository) : IRequestHandler<Query, List<PolicyEndorsementsDto>>
    {
        public async Task<List<PolicyEndorsementsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await policyRepository.GetPolicyEndorsementsAsync(request.BrokerId, cancellationToken);
        }
    }
}