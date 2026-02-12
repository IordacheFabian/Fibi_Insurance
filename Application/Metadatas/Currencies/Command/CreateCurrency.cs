using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Metadatas.Currencies.DTOs.Request;
using Application.Metadatas.Currencies.DTOs.Response;
using AutoMapper;
using Domain.Models.Metadatas;
using MediatR;

namespace Application.Metadatas.Currencies.Command;

public class CreateCurrency
{
    public class Command : IRequest<CurrencyDto>
    {
        public required CreateCurrencyDto CreateCurrencyDto { get; set; }
    }

    public class Handler(ICurrencyRepository currencyRepository, IMapper mapper) : IRequestHandler<Command, CurrencyDto>
    {
        public async Task<CurrencyDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var code = request.CreateCurrencyDto.Code.Trim().ToUpperInvariant();
            var name = request.CreateCurrencyDto.Name.Trim();

            var exists = await currencyRepository.CurrencyCodeExistsAsync(code, cancellationToken);
            if (exists)
            {
                throw new Exception($"Currency with code '{code}' already exists.");
            } 
            
            var currency = mapper.Map<Currency>(request.CreateCurrencyDto); {
                currency.CreatedAt = DateTime.UtcNow;
                currency.UpdatedAt = DateTime.UtcNow;
            }
            
            await currencyRepository.CreateCurrencyAsync(currency, cancellationToken);

            var result = await currencyRepository.SaveChangesAsync(cancellationToken);
            if (!result) throw new BadRequestException("Failed to create currency.");

            return mapper.Map<CurrencyDto>(currency);
        }
    }
}
