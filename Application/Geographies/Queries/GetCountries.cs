using System;
using Application.Core;
using Application.Geographies.DTOs;
using AutoMapper;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Geographies.Queries;

public class GetCountries
{
    public class Query : IRequest<List<CountryDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Query, List<CountryDto>>
    {
        public Task<List<CountryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var countries = context.Countries;

            if(countries == null) throw new NotFoundException("Countries not found");

            return mapper.ProjectTo<CountryDto>(countries).ToListAsync(cancellationToken);
        }
    }
}
