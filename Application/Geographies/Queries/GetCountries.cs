using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Geographies.DTOs;
using AutoMapper;
using Domain.Models;
using MediatR;

namespace Application.Geographies.Queries;

public class GetCountries
{
    public class Query : IRequest<List<CountryDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(IGeographyRepository geographyRepository, IMapper mapper) : IRequestHandler<Query, List<CountryDto>>
    {
        public async Task<List<CountryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var countries = await geographyRepository.GetCountriesAsync(cancellationToken);

            if(countries == null) throw new NotFoundException("Countries not found");

            return mapper.Map<List<CountryDto>>(countries);
        }
    }
}
