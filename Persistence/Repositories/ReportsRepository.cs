using System;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Reports.DTOs.Request;
using Application.Reports.DTOs.Response;
using AutoMapper;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class ReportsRepository(AppDbContext context) : IReportsRepository
{
    public IQueryable<PoliciesByBrokerListDto> GetPoliciesByBrokerReport(PoliciesByBrokerReportDto reportRequest, CancellationToken cancellationToken)
    {
        var policies = context.Policies
            .AsNoTracking()
            .Select(p => new
            {
                Policy = p,
                ActiveVersion = p.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => new
                    {
                        v.StartDate,
                        v.EndDate,
                        v.FinalPremium,
                        v.CurrencyId,
                        CurrencyCode = v.Currency.Code,
                        ExchangeRateToBase = v.Currency.ExchangeRateToBase
                    })
                    .SingleOrDefault()
            })
            .Where(x => x.ActiveVersion != null);

        policies = policies.Where(x => 
            x.ActiveVersion!.StartDate >= reportRequest.From && 
            x.ActiveVersion.EndDate <= reportRequest.To);
        
        if(reportRequest.PolicyStatus.HasValue)
        {
            policies = policies.Where(p => p.Policy.PolicyStatus == reportRequest.PolicyStatus);
        }

        if(!string.IsNullOrWhiteSpace(reportRequest.Currency))
        {
            var code = reportRequest.Currency.Trim().ToUpper();
            policies = policies.Where(p => p.ActiveVersion!.CurrencyCode == code);
        }

        if(reportRequest.BuildingType.HasValue)
        {
            policies = policies.Where(p => p.Policy.Building.BuildingType == reportRequest.BuildingType);
        }

        var result = policies
            .GroupBy(p => new
            {
                p.Policy.BrokerId,
                BrokerName = p.Policy.Broker.Name,
                p.ActiveVersion!.CurrencyId,
                p.ActiveVersion.CurrencyCode
            })
            .Select(g => new PoliciesByBrokerListDto
            {
                BrokerId = g.Key.BrokerId,
                BrokerName = g.Key.BrokerName,
                CurrencyId = g.Key.CurrencyId,
                CurrencyCode = g.Key.CurrencyCode,
                PoliciesCount = g.Count(),
                FinalPremium = g.Sum(x => x.ActiveVersion!.FinalPremium),
                FinalPremiumBaseCurrency = Math.Round(g.Sum(x => x.ActiveVersion!.FinalPremium *
                    x.ActiveVersion!.ExchangeRateToBase), 2)
            })
            .OrderBy(x => x.BrokerName)
            .ThenBy(x => x.CurrencyCode);

        return result;
    }

    public IQueryable<PoliciesByCityListDto> GetPoliciesByCityReport(PoliciesByCityReportDto reportRequest, CancellationToken cancellationToken)
    {
        var policies = context.Policies
            .AsNoTracking()
            .Select(p => new
            {
                Policy = p,
                ActiveVersion = p.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => new
                    {
                        v.StartDate,
                        v.EndDate,
                        v.FinalPremium,
                        v.CurrencyId,
                        CurrencyCode = v.Currency.Code,
                        ExchangeRateToBase = v.Currency.ExchangeRateToBase
                    })
                    .SingleOrDefault()
            })
            .Where(x => x.ActiveVersion != null);

        policies = policies.Where(x =>
        x.ActiveVersion!.StartDate >= reportRequest.From &&
        x.ActiveVersion!.EndDate <= reportRequest.To);

        if (reportRequest.PolicyStatus.HasValue)
        {
            policies = policies.Where(p => p.Policy.PolicyStatus == reportRequest.PolicyStatus);
        }

        if(!string.IsNullOrWhiteSpace(reportRequest.Currency))
        {
            var code = reportRequest.Currency.Trim().ToUpper();
            policies = policies.Where(p => p.ActiveVersion!.CurrencyCode == code);
        }

        if(reportRequest.BuildingType.HasValue)
        {
            policies = policies.Where(p => p.Policy.Building.BuildingType == reportRequest.BuildingType);
        }

        var query = policies
            .GroupBy(p => new
            {
                p.Policy.Building.Address.CityId,
                CityName = p.Policy.Building.Address.City.Name,
                p.Policy.Building.Address.City.CountyId,
                CountyName = p.Policy.Building.Address.City.County.Name,
                p.Policy.Building.Address.City.County.CountryId,
                CountryName = p.Policy.Building.Address.City.County.Country.Name,
                p.ActiveVersion!.CurrencyId,
                p.ActiveVersion.CurrencyCode
            })
            .Select(g => new PoliciesByCityListDto
            {
                CityId = g.Key.CityId,
                CityName = g.Key.CityName,
                CountyId = g.Key.CountyId,
                CountyName = g.Key.CountyName,
                CountryId = g.Key.CountryId,
                CountryName = g.Key.CountryName,
                CurrencyId = g.Key.CurrencyId,
                CurrencyCode = g.Key.CurrencyCode,
                PoliciesCount = g.Count(),
                FinalPremium = g.Sum(x => x.ActiveVersion!.FinalPremium),
                FinalPremiumBaseCurrency = Math.Round(g.Sum(p => p.ActiveVersion!.FinalPremium * p.ActiveVersion!.ExchangeRateToBase), 2)
            })
            .OrderBy(x => x.CityName)
            .ThenBy(x => x.CurrencyCode);

        return query;
    }

    public IQueryable<PoliciesByCountryListDto> GetPoliciesByCountryReport(PoliciesByCountryReportDto reportRequest, CancellationToken cancellationToken)
    {
        var policies = context.Policies
            .AsNoTracking()
            .Select(p => new
            {
                Policy = p,
                ActiveVersion = p.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => new
                    {
                        v.StartDate,
                        v.EndDate,
                        v.FinalPremium,
                        v.CurrencyId,
                        CurrencyCode = v.Currency.Code,
                        ExchangeRateToBase = v.Currency.ExchangeRateToBase
                    })
                    .SingleOrDefault()
            })
            .Where(x => x.ActiveVersion != null);

        policies = policies.Where(x =>
        x.ActiveVersion!.StartDate >= reportRequest.From &&
        x.ActiveVersion!.EndDate <= reportRequest.To);

        if (reportRequest.PolicyStatus.HasValue)
        {
            policies = policies.Where(p => p.Policy.PolicyStatus == reportRequest.PolicyStatus);
        }

        if(!string.IsNullOrWhiteSpace(reportRequest.Currency))
        {
            var code = reportRequest.Currency.Trim().ToUpper();
            policies = policies.Where(p => p.ActiveVersion!.CurrencyCode == code);
        }

        if(reportRequest.BuildingType.HasValue)
        {
            policies = policies.Where(p => p.Policy.Building.BuildingType == reportRequest.BuildingType);
        }

        var query = policies
            .GroupBy(p => new
            {
                p.Policy.Building.Address.City.County.CountryId,
                CountryName = p.Policy.Building.Address.City.County.Country.Name,
                p.ActiveVersion!.CurrencyId,
                p.ActiveVersion.CurrencyCode
            })
            .Select(g => new PoliciesByCountryListDto
            {
                CountryId = g.Key.CountryId,
                CountryName = g.Key.CountryName,
                CurrencyId = g.Key.CurrencyId,
                CurrencyCode = g.Key.CurrencyCode,
                PoliciesCount = g.Count(),
                FinalPremium = g.Sum(x => x.ActiveVersion!.FinalPremium),
                FinalPremiumBaseCurrency = Math.Round(g.Sum(p => p.ActiveVersion!.FinalPremium * p.ActiveVersion!.ExchangeRateToBase), 2)
            })
            .OrderBy(x => x.CountryName)
            .ThenBy(x => x.CurrencyCode);

        return query;
    }

    public IQueryable<PoliciesByCountyListDto> GetPoliciesByCountyReport(PoliciesByCountyReportDto reportRequest, CancellationToken cancellationToken)
    {
        var policies = context.Policies
            .AsNoTracking()
            .Select(p => new
            {
                Policy = p,
                ActiveVersion = p.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => new
                    {
                        v.StartDate,
                        v.EndDate,
                        v.FinalPremium,
                        v.CurrencyId,
                        CurrencyCode = v.Currency.Code,
                        ExchangeRateToBase = v.Currency.ExchangeRateToBase
                    })
                    .SingleOrDefault()
            })
            .Where(x => x.ActiveVersion != null);

        policies = policies.Where(x =>
        x.ActiveVersion!.StartDate >= reportRequest.From &&
        x.ActiveVersion!.EndDate <= reportRequest.To);

        if (reportRequest.PolicyStatus.HasValue)
        {
            policies = policies.Where(p => p.Policy.PolicyStatus == reportRequest.PolicyStatus);
        }

        if (!string.IsNullOrWhiteSpace(reportRequest.Currency))
        {
            var code = reportRequest.Currency.Trim().ToUpper();
            policies = policies.Where(p => p.ActiveVersion!.CurrencyCode == code);
        }

        if (reportRequest.BuildingType.HasValue)
        {
            policies = policies.Where(p => p.Policy.Building.BuildingType == reportRequest.BuildingType);
        }

        var query = policies
            .GroupBy(p => new
            {
                p.Policy.Building.Address.City.County.CountryId,
                CountryName = p.Policy.Building.Address.City.County.Country.Name,
                p.Policy.Building.Address.City.CountyId,
                CountyName = p.Policy.Building.Address.City.County.Name,
                p.ActiveVersion!.CurrencyId,
                p.ActiveVersion.CurrencyCode
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
                FinalPremium = g.Sum(x => x.ActiveVersion!.FinalPremium),
                FinalPremiumBaseCurrency = Math.Round(g.Sum(p => p.ActiveVersion!.FinalPremium * p.ActiveVersion!.ExchangeRateToBase), 2)
            })
            .OrderBy(x => x.CountryName)
            .ThenBy(x => x.CurrencyCode);

            return query;
    }
}
