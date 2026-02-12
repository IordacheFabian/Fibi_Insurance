using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Metadatas.Fees.DTOs.Request;
using Application.Metadatas.Fees.DTOs.Response;
using AutoMapper;
using Domain.Models.Metadatas;
using MediatR;

namespace Application.Metadatas.Fees.Command;

public class CreateFeeConfiguration
{
    public class Command : IRequest<FeeConfigurationDto>
    {
        public required CreateFeeConfigurationDto CreateFeeConfigurationDto { get; set; }
    }

    public class Handler(IFeeConfigurationRepository feeConfigurationRepository, IMapper mapper) : IRequestHandler<Command, FeeConfigurationDto>
    {
        public async Task<FeeConfigurationDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var feeConfiguration = request.CreateFeeConfigurationDto;

            var overlaps = feeConfigurationRepository.ExistsOverlappingAsync(feeConfiguration.FeeType, feeConfiguration.EffectiveFrom, feeConfiguration.EffectiveTo, cancellationToken);

            if (overlaps.Result) throw new InvalidOperationException("Overlapping fee configuration exists for the same fee type and effective period.");

            var fee = mapper.Map<FeeConfiguration>(feeConfiguration); {
                fee.CreatedAt = DateTime.UtcNow;
                fee.UpdatedAt = DateTime.UtcNow;
            }

            await feeConfigurationRepository.CreateFeeConfigurationAsync(fee, cancellationToken);

            var result = await feeConfigurationRepository.SaveChangesAsync(cancellationToken);

            if(!result) throw new BadRequestException("Failed to create fee configuration.");

            return mapper.Map<FeeConfigurationDto>(fee);
        }
    }
}
