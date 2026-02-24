using System;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Reports.DTOs.Request;
using Application.Reports.DTOs.Response;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class ReportsRepository(AppDbContext context) : IReportsRepository
{
    public IQueryable<PoliciesByCountryListDto> GetPoliciesByCountryReport(PoliciesByCountryReportDto reportRequest, CancellationToken cancellationToken)
    {
        var policies = context.Policies
            .AsNoTracking()
            .AsQueryable()
            .Where(p => p.StartDate >= reportRequest.From && p.EndDate <= reportRequest.To);

        if(reportRequest.PolicyStatus.HasValue)
        {
            policies = policies.Where(p => p.PolicyStatus == reportRequest.PolicyStatus);
        }

        if(!string.IsNullOrWhiteSpace(reportRequest.Currency))
        {
            var code = reportRequest.Currency.Trim().ToUpper();
            policies = policies.Where(p => p.Currency.Code == code);
        }

        if(reportRequest.BuildingType.HasValue)
        {
            policies = policies.Where(p => p.Building.BuildingType == reportRequest.BuildingType);
        }

        var query = policies
            .GroupBy(p => new
            {
                CountryId = p.Building.Address.City.County.CountryId,
                CountryName = p.Building.Address.City.County.Country.Name,
                CurrencyId = p.CurrencyId,
                CurrencyCode = p.Currency.Code
            })
            .Select(g => new PoliciesByCountryListDto
            {
                CountryId = g.Key.CountryId,
                CountryName = g.Key.CountryName,
                CurrencyId = g.Key.CurrencyId,
                CurrencyCode = g.Key.CurrencyCode,
                PoliciesCount = g.Count(),
                FinalPremium = g.Sum(x => x.FinalPremium)
            })
            .OrderBy(x => x.CountryName)
            .ThenBy(x => x.CurrencyCode);

        return query;
    }

    public IQueryable<PoliciesByCountyListDto> GetPoliciesByCountyReport(PoliciesByCountyReportDto reportRequest, CancellationToken cancellationToken)
    {
        var policies = context.Policies
            .AsNoTracking()
            .AsQueryable()
            .Where(p => p.StartDate >= reportRequest.From && p.EndDate <= reportRequest.To);

        if (reportRequest.PolicyStatus.HasValue)
        {
            policies = policies.Where(p => p.PolicyStatus == reportRequest.PolicyStatus);
        }

        if (!string.IsNullOrWhiteSpace(reportRequest.Currency))
        {
            var code = reportRequest.Currency.Trim().ToUpper();
            policies = policies.Where(p => p.Currency.Code == code);
        }

        if (reportRequest.BuildingType.HasValue)
        {
            policies = policies.Where(p => p.Building.BuildingType == reportRequest.BuildingType);
        }

        var query = policies
            .GroupBy(p => new
            {
                CountryId = p.Building.Address.City.County.CountryId,
                CountryName = p.Building.Address.City.County.Country.Name,
                CountyId = p.Building.Address.City.CountyId,
                CountyName = p.Building.Address.City.County.Name,
                CurrencyId = p.CurrencyId,
                CurrencyCode = p.Currency.Code
            })
            .Select(g => new PoliciesByCountyListDto
            {
                CountryId = g.Key.CountryId,
                CountryName = g.Key.CountryName,
                CountyId = g.Key.CountyId,
                CountyName = g.Key.CountyName,
                CurrencyId = g.Key.CurrencyId,
                CurrencyCode = g.Key.CurrencyCode,
                PoliciesCount = g.Count(),
                FinalPremium = g.Sum(x => x.FinalPremium)
            })
            .OrderBy(x => x.CountryName)
            .ThenBy(x => x.CurrencyCode);

            return query;
    }
}
