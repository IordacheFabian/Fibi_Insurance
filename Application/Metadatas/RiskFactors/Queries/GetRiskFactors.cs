using System;
using Application.Core.Interfaces.IRepositories;
using Application.Metadatas.RiskFactors.DTOs.Response;
using AutoMapper;
using Domain.Models.Metadatas;
using MediatR;

namespace Application.Metadatas.RiskFactors.Queries;

public class GetRiskFactors
{
    public class Query : IRequest<List<RiskFactorDto>>
    {
        public bool? IsActive { get; set; }
        public RiskLevel? RiskLevel { get; set; }
    }

    public class Handler(IRiskFactorRepository riskFactorRepository, IMapper mapper) : IRequestHandler<Query, List<RiskFactorDto>>
    {
        public async Task<List<RiskFactorDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var riskFactors = await riskFactorRepository.GetRiskFactorConfigurationsAsync(
                request.IsActive,
                request.RiskLevel,
                cancellationToken);

            return mapper.Map<List<RiskFactorDto>>(riskFactors);
        }
    }
}
