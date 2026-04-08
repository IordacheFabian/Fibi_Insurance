using System;
using System.Globalization;
using Application.Core.Interfaces.IRepositories;
using Application.Core.PagedResults;
using Application.Reports.DTOs.Request;
using Application.Reports.DTOs.Response;
using Application.Core;
using AutoMapper;
using Domain.Models.Claims;
using Domain.Models.Metadatas;
using Domain.Models.Payments;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class ReportsRepository(AppDbContext context) : IReportsRepository
{
    public async Task<ReportsAnalyticsDto> GetAnalyticsAsync(DateOnly from, DateOnly to, Guid? brokerId, string? currencyCode, bool filterByCurrency, CancellationToken cancellationToken)
    {
        var targetCurrency = await ResolveTargetCurrencyAsync(currencyCode, cancellationToken);
        var targetCurrencyRate = targetCurrency.ExchangeRateToBase;
        var normalizedCurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode.Trim().ToUpperInvariant();

        var monthBuckets = CreateMonthBuckets(from, to);
        var monthMap = monthBuckets.ToDictionary(
            bucket => bucket.Key,
            bucket => new ReportsMonthlyPointDto
            {
                Month = bucket.Label,
                Premiums = 0,
                Claims = 0,
                NewPolicies = 0,
            });

        var policiesQuery = context.Policies
            .AsNoTracking()
            .Where(p => !brokerId.HasValue || p.BrokerId == brokerId.Value)
            .Select(p => new
            {
                p.Id,
                p.BrokerId,
                BrokerName = p.Broker.Name,
                CommissionPercentage = p.Broker.CommissionPercentage,
                p.PolicyStatus,
                Region = p.Building.Address.City.Name,
                StartDate = p.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => (DateOnly?)v.StartDate)
                    .SingleOrDefault(),
                EndDate = p.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => (DateOnly?)v.EndDate)
                    .SingleOrDefault(),
                FinalPremium = p.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => (decimal?)v.FinalPremium)
                    .SingleOrDefault(),
                CurrencyCode = p.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => v.Currency.Code)
                    .SingleOrDefault(),
                ExchangeRateToBase = p.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => (decimal?)v.Currency.ExchangeRateToBase)
                    .SingleOrDefault()
            })
            .Where(p => p.StartDate.HasValue && p.EndDate.HasValue && p.FinalPremium.HasValue && p.ExchangeRateToBase.HasValue);

        if (filterByCurrency && normalizedCurrencyCode is not null)
        {
            policiesQuery = policiesQuery.Where(p => p.CurrencyCode == normalizedCurrencyCode);
        }

        var policies = await policiesQuery.ToListAsync(cancellationToken);

        var policiesInRange = policies
            .Where(p => p.StartDate!.Value <= to && p.EndDate!.Value >= from)
            .Select(p => new
            {
                p.Id,
                p.BrokerId,
                p.BrokerName,
                CommissionPercentage = p.CommissionPercentage ?? 0m,
                p.PolicyStatus,
                p.Region,
                StartDate = p.StartDate!.Value,
                EndDate = p.EndDate!.Value,
                FinalPremium = p.FinalPremium!.Value,
                CurrencyCode = p.CurrencyCode!,
                FinalPremiumBase = p.FinalPremium!.Value * p.ExchangeRateToBase!.Value,
            })
            .ToList();

        foreach (var policy in policiesInRange)
        {
            var monthKey = GetMonthKey(policy.StartDate);
            if (monthMap.TryGetValue(monthKey, out var bucket))
            {
                bucket.NewPolicies += 1;
            }
        }

        var payments = await context.Payments
            .AsNoTracking()
            .Where(p => (!brokerId.HasValue || p.Policy.BrokerId == brokerId.Value) && p.PaymentDate >= from.ToDateTime(TimeOnly.MinValue) && p.PaymentDate <= to.ToDateTime(TimeOnly.MaxValue))
            .Select(p => new
            {
                p.PolicyId,
                BrokerId = p.Policy.BrokerId,
                BrokerName = p.Policy.Broker.Name,
                CommissionPercentage = p.Policy.Broker.CommissionPercentage,
                p.PaymentDate,
                p.Amount,
                p.Status,
                CurrencyCode = p.Currency.Code,
                ExchangeRateToBase = p.Currency.ExchangeRateToBase,
            })
            .Where(p => !filterByCurrency || normalizedCurrencyCode == null || p.CurrencyCode == normalizedCurrencyCode)
            .ToListAsync(cancellationToken);

        foreach (var payment in payments.Where(p => p.Status == PaymentStatus.Completed))
        {
            var monthKey = GetMonthKey(DateOnly.FromDateTime(payment.PaymentDate));
            if (monthMap.TryGetValue(monthKey, out var bucket))
            {
                bucket.Premiums += Math.Round(payment.Amount * payment.ExchangeRateToBase, 2);
            }
        }

        var claims = await context.Claims
            .AsNoTracking()
            .Where(c => (!brokerId.HasValue || c.Policy.BrokerId == brokerId.Value) && c.CreatedAt >= from && c.CreatedAt <= to)
            .Select(c => new
            {
                c.PolicyId,
                c.Status,
                c.CreatedAt,
                c.EstimatedDamage,
                c.ApprovedAmount,
                CurrencyCode = c.Policy.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => v.Currency.Code)
                    .SingleOrDefault(),
                ExchangeRateToBase = c.Policy.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => (decimal?)v.Currency.ExchangeRateToBase)
                    .SingleOrDefault() ?? 1m,
            })
            .Where(c => !filterByCurrency || normalizedCurrencyCode == null || c.CurrencyCode == normalizedCurrencyCode)
            .ToListAsync(cancellationToken);

        foreach (var claim in claims)
        {
            var monthKey = GetMonthKey(claim.CreatedAt);
            if (monthMap.TryGetValue(monthKey, out var bucket))
            {
                var claimAmount = GetClaimAmount(claim.Status, claim.EstimatedDamage, claim.ApprovedAmount);
                bucket.Claims += Math.Round(claimAmount * claim.ExchangeRateToBase, 2);
            }
        }

        var claimsByPolicyId = claims
            .GroupBy(c => c.PolicyId)
            .ToDictionary(group => group.Key, group => group.Count());

        var geographic = policiesInRange
            .GroupBy(policy => policy.Region)
            .Select(group => new ReportsGeographicPointDto
            {
                Region = group.Key,
                Policies = group.Count(),
                Premium = ConvertFromBase(group.Sum(item => item.FinalPremiumBase), targetCurrencyRate),
                Claims = group.Sum(item => claimsByPolicyId.GetValueOrDefault(item.Id, 0)),
            })
            .OrderByDescending(item => item.Premium)
            .Take(8)
            .ToList();

        var completedPayments = payments
            .Where(payment => payment.Status == PaymentStatus.Completed)
            .Select(payment => new
            {
                payment.PolicyId,
                payment.BrokerId,
                payment.BrokerName,
                CommissionPercentage = payment.CommissionPercentage ?? 0m,
                AmountBase = Math.Round(payment.Amount * payment.ExchangeRateToBase, 2),
            })
            .ToList();

        var collectedPremiumByBrokerId = completedPayments
            .GroupBy(payment => payment.BrokerId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(payment => payment.AmountBase));

        var brokerEarningsByBrokerId = completedPayments
            .GroupBy(payment => payment.BrokerId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(payment => Math.Round(payment.AmountBase * payment.CommissionPercentage / 100m, 2)));

        var brokerPerformance = policiesInRange
            .GroupBy(policy => new
            {
                policy.BrokerId,
                policy.BrokerName,
            })
            .Select(group =>
            {
                var commissionPercentage = Math.Round(group.Max(policy => policy.CommissionPercentage), 2);
                var writtenPremiumBase = group.Sum(policy => policy.FinalPremiumBase);
                var collectedPremiumBase = collectedPremiumByBrokerId.GetValueOrDefault(group.Key.BrokerId, 0m);
                var brokerEarningsBase = brokerEarningsByBrokerId.GetValueOrDefault(group.Key.BrokerId, 0m);

                return new ReportsBrokerPerformancePointDto
                {
                    BrokerId = group.Key.BrokerId,
                    BrokerName = group.Key.BrokerName,
                    TotalPolicies = group.Count(),
                    ActivePolicies = group.Count(policy => policy.PolicyStatus == PolicyStatus.Active),
                    WrittenPremium = ConvertFromBase(writtenPremiumBase, targetCurrencyRate),
                    CollectedPremium = ConvertFromBase(collectedPremiumBase, targetCurrencyRate),
                    BrokerEarnings = ConvertFromBase(brokerEarningsBase, targetCurrencyRate),
                    CommissionPercentage = commissionPercentage,
                };
            })
            .OrderByDescending(item => item.BrokerEarnings)
            .ThenByDescending(item => item.WrittenPremium)
            .ToList();

        var claimsBreakdown = claims
            .GroupBy(claim => claim.Status)
            .Select(group => new ReportsClaimsBreakdownPointDto
            {
                Name = ToClaimStatusLabel(group.Key),
                Value = group.Count(),
            })
            .OrderByDescending(item => item.Value)
            .ToList();

        var totalPolicyPremium = policiesInRange.Sum(policy => policy.FinalPremiumBase);
        var totalPremiumRevenue = completedPayments.Sum(payment => payment.AmountBase);
        var totalBrokerEarnings = brokerEarningsByBrokerId.Values.Sum();
        var totalClaimsIncurred = claims
            .Sum(claim => Math.Round(GetClaimAmount(claim.Status, claim.EstimatedDamage, claim.ApprovedAmount) * claim.ExchangeRateToBase, 2));

        var previousFrom = from.AddYears(-1);
        var previousTo = to.AddYears(-1);
        var currentGrowthCount = policies.Count(policy => policy.StartDate.HasValue && policy.StartDate.Value >= from && policy.StartDate.Value <= to);
        var previousGrowthCount = policies.Count(policy => policy.StartDate.HasValue && policy.StartDate.Value >= previousFrom && policy.StartDate.Value <= previousTo);

        var portfolioGrowth = previousGrowthCount == 0
            ? currentGrowthCount > 0 ? 100m : 0m
            : Math.Round(((decimal)(currentGrowthCount - previousGrowthCount) / previousGrowthCount) * 100m, 2);

        return new ReportsAnalyticsDto
        {
            CurrencyCode = targetCurrency.Code,
            CurrencyName = targetCurrency.Name,
            Summary = new ReportsSummaryDto
            {
                TotalWrittenPremium = ConvertFromBase(totalPolicyPremium, targetCurrencyRate),
                TotalPremiumRevenue = ConvertFromBase(totalPremiumRevenue, targetCurrencyRate),
                TotalBrokerEarnings = ConvertFromBase(totalBrokerEarnings, targetCurrencyRate),
                ClaimsRatio = totalPolicyPremium == 0 ? 0 : Math.Round((totalClaimsIncurred / totalPolicyPremium) * 100m, 2),
                PortfolioGrowth = portfolioGrowth,
                CollectionRate = totalPolicyPremium == 0 ? 0 : Math.Round((totalPremiumRevenue / totalPolicyPremium) * 100m, 2),
                TotalPolicies = policiesInRange.Count,
                TotalClaimsIncurred = ConvertFromBase(totalClaimsIncurred, targetCurrencyRate),
            },
            Monthly = monthBuckets.Select(bucket => new ReportsMonthlyPointDto
            {
                Month = monthMap[bucket.Key].Month,
                Premiums = ConvertFromBase(monthMap[bucket.Key].Premiums, targetCurrencyRate),
                Claims = ConvertFromBase(monthMap[bucket.Key].Claims, targetCurrencyRate),
                NewPolicies = monthMap[bucket.Key].NewPolicies,
            }).ToList(),
            Geographic = geographic,
            ClaimsBreakdown = claimsBreakdown,
            BrokerPerformance = brokerPerformance,
        };
    }

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

    private static List<(string Key, string Label)> CreateMonthBuckets(DateOnly from, DateOnly to)
    {
        var buckets = new List<(string Key, string Label)>();
        var cursor = new DateOnly(from.Year, from.Month, 1);
        var end = new DateOnly(to.Year, to.Month, 1);

        while (cursor <= end)
        {
            buckets.Add((GetMonthKey(cursor), cursor.ToString("MMM", CultureInfo.InvariantCulture)));
            cursor = cursor.AddMonths(1);
        }

        return buckets;
    }

    private static string GetMonthKey(DateOnly value)
    {
        return $"{value.Year}-{value.Month:D2}";
    }

    private static decimal GetClaimAmount(ClaimStatus status, decimal estimatedDamage, decimal? approvedAmount)
    {
        if (status == ClaimStatus.Approved || status == ClaimStatus.Paid)
        {
            return approvedAmount ?? estimatedDamage;
        }

        return 0m;
    }

    private static string ToClaimStatusLabel(ClaimStatus status)
    {
        return status switch
        {
            ClaimStatus.Submitted => "Submitted",
            ClaimStatus.UnderReview => "Under Review",
            ClaimStatus.Approved => "Approved",
            ClaimStatus.Rejected => "Rejected",
            ClaimStatus.Paid => "Paid",
            _ => status.ToString(),
        };
    }

    private async Task<Currency> ResolveTargetCurrencyAsync(string? currencyCode, CancellationToken cancellationToken)
    {
        var normalizedCode = string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode.Trim().ToUpperInvariant();

        var query = context.Currencies.AsNoTracking().Where(currency => currency.IsActive);

        Currency? targetCurrency;
        if (normalizedCode is not null)
        {
            targetCurrency = await query.FirstOrDefaultAsync(currency => currency.Code == normalizedCode, cancellationToken);
            if (targetCurrency is null)
            {
                throw new BadRequestException("Selected currency is not available.");
            }

            return targetCurrency;
        }

        var activeCurrencies = await query.ToListAsync(cancellationToken);
        targetCurrency = activeCurrencies
            .OrderBy(currency => Math.Abs((double)(currency.ExchangeRateToBase - 1m)))
            .ThenBy(currency => currency.Code)
            .FirstOrDefault();
        if (targetCurrency is null)
        {
            throw new BadRequestException("No active currencies are configured.");
        }

        return targetCurrency;
    }

    private static decimal ConvertFromBase(decimal amount, decimal targetExchangeRate)
    {
        if (targetExchangeRate == 0)
        {
            return 0;
        }

        return Math.Round(amount / targetExchangeRate, 2);
    }
}
