using System;
using Application.Addresses;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Metadatas.Currencies.DTOs.Request;
using Application.Metadatas.Currencies.DTOs.Response;
using AutoMapper;
using Domain.Models.Metadatas;
using MediatR;

namespace Application.Metadatas.Currencies.Command;

public class UpdateCurrency
{
    public class Command : IRequest<CurrencyDto>
    {
        public required UpdateCurrencyDto UpdateCurrencyDto { get; set; }
    }

    public class Handler(ICurrencyRepository currencyRepository, IMapper mapper) : IRequestHandler<Command, CurrencyDto>
    {
        public async Task<CurrencyDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var currency = await currencyRepository.GetCurrencyForUpdateAsync(request.UpdateCurrencyDto.Id, cancellationToken);
            if (currency == null)
                throw new Exception($"Currency with id '{request.UpdateCurrencyDto.Id}' not found.");
            
            mapper.Map(request.UpdateCurrencyDto, currency);
            {
                currency.Name = request.UpdateCurrencyDto.Name.Trim();
                currency.UpdatedAt = DateTime.UtcNow;
            }

            var result = await currencyRepository.SaveChangesAsync(cancellationToken);  
            if (!result) throw new BadRequestException("Failed to update currency.");

            return mapper.Map<CurrencyDto>(currency);

        }
    }
}
