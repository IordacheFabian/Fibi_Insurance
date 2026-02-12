using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Metadatas.Fees.DTOs.Request;
using Application.Metadatas.Fees.DTOs.Response;
using AutoMapper;
using Domain.Models.Metadatas;
using MediatR;

namespace Application.Metadatas.Fees.Command;

public class UpdateFeeConfiguration
{
    public class Command : IRequest<FeeConfigurationDto>
    {
        public required UpdateFeeConfigurationDto UpdateDto { get; set; }
    }

    public class Handler(IFeeConfigurationRepository feeConfigurationRepository, IMapper mapper)
        : IRequestHandler<Command, FeeConfigurationDto>
    {
        public async Task<FeeConfigurationDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var fee = await feeConfigurationRepository.GetFeeConfigurationAsync(request.UpdateDto.Id, cancellationToken);
            if (fee == null)
                throw new Exception($"Fee configuration with id '{request.UpdateDto.Id}' not found.");
            
            mapper.Map(request.UpdateDto, fee); {
                fee.Name = request.UpdateDto.Name.Trim();
                fee.UpdatedAt = DateTime.UtcNow;
            }
            
            var result = await feeConfigurationRepository.SaveChangesAsync(cancellationToken);
            if (!result) throw new BadRequestException("Failed to update fee configuration.");

            return mapper.Map<FeeConfigurationDto>(fee);
        }
    }
}
