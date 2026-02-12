using System;
using Application.Core.Interfaces.IRepositories;
using Application.Metadatas.Fees.DTOs.Response;
using AutoMapper;
using MediatR;

namespace Application.Metadatas.Fees.Queries;

public class GetFees
{
    public class Query : IRequest<List<FeeConfigurationDto>>
    {
        public bool? IsActive { get; set; }
    }

    public class Handler(IFeeConfigurationRepository feeConfigurationRepository, IMapper mapper)
        : IRequestHandler<Query, List<FeeConfigurationDto>>
    {
        public async Task<List<FeeConfigurationDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var fees = await feeConfigurationRepository.GetFeeConfigurationsAsync(request.IsActive, cancellationToken);
            return mapper.Map<List<FeeConfigurationDto>>(fees);
        }
    }
}
