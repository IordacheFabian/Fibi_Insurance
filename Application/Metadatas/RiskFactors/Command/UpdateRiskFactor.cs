using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Metadatas.RiskFactors.DTOs.Request;
using Application.Metadatas.RiskFactors.DTOs.Response;
using AutoMapper;
using Domain.Models.Metadatas;
using MediatR;

namespace Application.Metadatas.RiskFactors.Command;

public class UpdateRiskFactor
{
    public class Command : IRequest<RiskFactorDto>
    {
        public Guid Id { get; set; }
        public UpdateRiskFactorDto UpdateRiskFactorDto { get; set; } = null!;
    }

    public class Handler(IRiskFactorRepository riskFactorRepository, IMapper mapper) : IRequestHandler<Command, RiskFactorDto>
    {
        public async Task<RiskFactorDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var riskFactor = await riskFactorRepository.GetRiskFactorForUpdateAsync(request.Id, cancellationToken);

            if(riskFactor == null)
                throw new  NotFoundException("Risk factor configuration not found.");
            
            mapper.Map(request.UpdateRiskFactorDto, riskFactor);

            var result = await riskFactorRepository.SaveChangesAsync(cancellationToken);
            if(!result) 
                throw new BadRequestException("Failed to update risk factor configuration.");

            return mapper.Map<RiskFactorDto>(riskFactor);
        }
    }
}
