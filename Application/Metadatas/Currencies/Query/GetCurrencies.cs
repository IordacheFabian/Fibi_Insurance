using System;
using Application.Core.Interfaces.IRepositories;
using Application.Metadatas.Currencies.DTOs.Response;
using AutoMapper;
using MediatR;

namespace Application.Metadatas.Currencies.Query;

public class GetCurrencies
{
    public class Query : IRequest<List<CurrencyDto>>
    {
        public bool? IsActive { get; set; }
    }

    public class Handler(ICurrencyRepository currencyRepository, IMapper mapper) : IRequestHandler<Query, List<CurrencyDto>>
    {
        public async Task<List<CurrencyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var currencies = await currencyRepository.GetCurrenciesAsync(request.IsActive, cancellationToken);

            return mapper.Map<List<CurrencyDto>>(currencies);
        }
    }
}
