using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Metadatas.RiskFactors.DTOs.Request;
using Application.Metadatas.RiskFactors.DTOs.Response;
using AutoMapper;
using Domain.Models.Metadatas;
using MediatR;

namespace Application.Metadatas.RiskFactors.Command;

public class CreateRiskFactor
{
    public class Command : IRequest<RiskFactorDto>
    {
        public CreateRiskFactorDto CreateRiskFactorDto { get; set; }
    }

    public class Handler(IRiskFactorRepository riskFactorRepository, IMapper mapper) : IRequestHandler<Command, RiskFactorDto>
    {
        public async Task<RiskFactorDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var riskFactorDto = request.CreateRiskFactorDto;

            var overlaps = await riskFactorRepository.ExistsOverlappingAsync(
                riskFactorDto.Level, riskFactorDto.ReferenceId, riskFactorDto.BuildingType, cancellationToken);

            if (overlaps)
                throw new InvalidOperationException("A risk factor with the same level, reference ID, and building type already exists.");
            
            var riskFactor = mapper.Map<RiskFactorConfiguration>(riskFactorDto);

            await riskFactorRepository.CreateRiskFactorAsync(riskFactor, cancellationToken);

            var result = await riskFactorRepository.SaveChangesAsync(cancellationToken); 
            if(!result)
                throw new BadRequestException("Failed to create risk factor configuration.");
            
            return mapper.Map<RiskFactorDto>(riskFactor);
        }
    }
}
