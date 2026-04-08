using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using API.IntegrationTests.TestInfrastructure;
using Application.Core.PagedResults;
using Application.Reports.DTOs.Response;
using Domain.Models;
using Domain.Models.Brokers;
using Domain.Models.Buildings;
using Domain.Models.Clients;
using Domain.Models.Geography.Address;
using Domain.Models.Metadatas;
using Domain.Models.Payments;
using Domain.Models.Policies;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Xunit;
using Xunit.Sdk;

namespace API.IntegrationTests.Reports;

public class ReportsController_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private const string AnalyticsPath = "/api/admin/reports/analytics";
    private const string CountryPath = "/api/admin/policies-by-country";
    private const string CountyPath = "/api/admin/policies-by-county";
    private const string CityPath = "/api/admin/policies-by-city";
    private const string BrokerPath = "/api/admin/policies-by-broker";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _http;

    public ReportsController_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _http = factory.CreateClient();
    }

    [Fact]
    public async Task PoliciesByCountry_Should_ReturnGroupedTotals()
    {
        await AssertReportTotalsAsync(CountryPath, BuildExpectedByCountry, AssertCountryRows);
    }

    [Fact]
    public async Task PoliciesByCounty_Should_ReturnGroupedTotals()
    {
        await AssertReportTotalsAsync(CountyPath, BuildExpectedByCounty, AssertCountyRows);
    }

    [Fact]
    public async Task PoliciesByCity_Should_ReturnGroupedTotals()
    {
        await AssertReportTotalsAsync(CityPath, BuildExpectedByCity, AssertCityRows);
    }

    [Fact]
    public async Task PoliciesByBroker_Should_ReturnGroupedTotals()
    {
        await AssertReportTotalsAsync(BrokerPath, BuildExpectedByBroker, AssertBrokerRows);
    }

    [Fact]
    public async Task PoliciesByCountry_Should_ApplyFilters()
    {
        await AssertReportFiltersAsync(CountryPath, BuildExpectedByCountry, AssertCountryRows);
    }

    [Fact]
    public async Task PoliciesByCounty_Should_ApplyFilters()
    {
        await AssertReportFiltersAsync(CountyPath, BuildExpectedByCounty, AssertCountyRows);
    }

    [Fact]
    public async Task PoliciesByCity_Should_ApplyFilters()
    {
        await AssertReportFiltersAsync(CityPath, BuildExpectedByCity, AssertCityRows);
    }

    [Fact]
    public async Task PoliciesByBroker_Should_ApplyFilters()
    {
        await AssertReportFiltersAsync(BrokerPath, BuildExpectedByBroker, AssertBrokerRows);
    }

    [Fact]
    public async Task PoliciesByCountry_Should_ReturnEmptyList_WhenFiltersExcludeAll()
    {
        await AssertReportEmptyResultsAsync<PoliciesByCountryListDto>(CountryPath);
    }

    [Fact]
    public async Task PoliciesByCounty_Should_ReturnEmptyList_WhenFiltersExcludeAll()
    {
        await AssertReportEmptyResultsAsync<PoliciesByCountyListDto>(CountyPath);
    }

    [Fact]
    public async Task PoliciesByCity_Should_ReturnEmptyList_WhenFiltersExcludeAll()
    {
        await AssertReportEmptyResultsAsync<PoliciesByCityListDto>(CityPath);
    }

    [Fact]
    public async Task PoliciesByBroker_Should_ReturnEmptyList_WhenFiltersExcludeAll()
    {
        await AssertReportEmptyResultsAsync<PoliciesByBrokerListDto>(BrokerPath);
    }

    [Fact]
    public async Task ReportsAnalytics_Should_ReturnBrokerEarningsAndPerformance()
    {
        var seeded = await ResetAndSeedPoliciesAsync();

        var alphaRon = seeded.Single(policy => policy.BrokerName == "Alpha Broker" && policy.CurrencyCode == "RON" && policy.FinalPremium == 1000m);
        var betaRon = seeded.Single(policy => policy.BrokerName == "Beta Broker" && policy.CurrencyCode == "RON" && policy.FinalPremium == 700m);
        var betaEur = seeded.Single(policy => policy.BrokerName == "Beta Broker" && policy.CurrencyCode == "EUR" && policy.FinalPremium == 400m);

        await SeedPaymentsAsync(
            CreatePayment(alphaRon, 400m, new DateTime(2026, 1, 20), PaymentStatus.Completed),
            CreatePayment(betaRon, 250m, new DateTime(2026, 2, 18), PaymentStatus.Pending),
            CreatePayment(betaEur, 100m, new DateTime(2026, 2, 25), PaymentStatus.Completed));

        var response = await GetAnalyticsAsync(new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), "RON");

        Assert.Equal("RON", response.CurrencyCode);
        Assert.Equal(8738.32m, response.Summary.TotalWrittenPremium);
        Assert.Equal(890m, response.Summary.TotalPremiumRevenue);
        Assert.Equal(89m, response.Summary.TotalBrokerEarnings);
        Assert.Equal(7, response.Summary.TotalPolicies);

        Assert.Collection(
            response.BrokerPerformance,
            beta =>
            {
                Assert.Equal("Beta Broker", beta.BrokerName);
                Assert.Equal(3, beta.TotalPolicies);
                Assert.Equal(2, beta.ActivePolicies);
                Assert.Equal(49m, beta.BrokerEarnings);
                Assert.Equal(490m, beta.CollectedPremium);
                Assert.Equal(10m, beta.CommissionPercentage);
            },
            alpha =>
            {
                Assert.Equal("Alpha Broker", alpha.BrokerName);
                Assert.Equal(4, alpha.TotalPolicies);
                Assert.Equal(2, alpha.ActivePolicies);
                Assert.Equal(40m, alpha.BrokerEarnings);
                Assert.Equal(400m, alpha.CollectedPremium);
                Assert.Equal(10m, alpha.CommissionPercentage);
            });
    }

    private async Task AssertReportTotalsAsync<TDto>(
        string path,
        Func<IEnumerable<PolicySeedResult>, List<TDto>> expectedBuilder,
        Action<IReadOnlyList<TDto>, IReadOnlyList<TDto>> assertRows)
    {
        var seeded = await ResetAndSeedPoliciesAsync();

        var from = new DateOnly(2025, 12, 1);
        var to = new DateOnly(2026, 12, 31);

        var response = await GetReportAsync<TDto>(path, from, to);
        var expected = expectedBuilder(FilterPolicies(seeded, from, to).ToList());

        AssertPagedResult(response, expected.Count, 1, 50);
        assertRows(ToListOrEmpty(response.Items), expected);
    }

    private async Task AssertReportFiltersAsync<TDto>(
        string path,
        Func<IEnumerable<PolicySeedResult>, List<TDto>> expectedBuilder,
        Action<IReadOnlyList<TDto>, IReadOnlyList<TDto>> assertRows)
    {
        var seeded = await ResetAndSeedPoliciesAsync();
        await RunFilterMatrixAsync(path, seeded, expectedBuilder, assertRows);
    }

    private async Task AssertReportEmptyResultsAsync<TDto>(string path)
    {
        await ResetAndSeedPoliciesAsync();

        var outsideRange = await GetReportAsync<TDto>(path, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));
        AssertPagedResult(outsideRange, 0, 1, 50);
        Assert.Empty(ToListOrEmpty(outsideRange.Items));

        var missingCurrency = await GetReportAsync<TDto>(path, new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), currency: "USD");
        AssertPagedResult(missingCurrency, 0, 1, 50);
        Assert.Empty(ToListOrEmpty(missingCurrency.Items));
    }

    private async Task<List<PolicySeedResult>> ResetAndSeedPoliciesAsync()
    {
        await ResetDatabaseAsync();
        return await SeedDefaultPoliciesAsync();
    }

    private async Task RunFilterMatrixAsync<TDto>(
        string path,
        List<PolicySeedResult> seeded,
        Func<IEnumerable<PolicySeedResult>, List<TDto>> expectedBuilder,
        Action<IReadOnlyList<TDto>, IReadOnlyList<TDto>> assertRows)
    {
        var defaultFrom = new DateOnly(2026, 1, 1);
        var defaultTo = new DateOnly(2026, 3, 31);

        await AssertScenario(defaultFrom, defaultTo, null, null, null);

        foreach (var status in new[] { PolicyStatus.Active, PolicyStatus.Cancelled, PolicyStatus.Expired })
        {
            await AssertScenario(defaultFrom, defaultTo, status, null, null);
        }

        foreach (var currency in new[] { "RON", "EUR" })
        {
            await AssertScenario(defaultFrom, defaultTo, null, currency, null);
        }

        foreach (var buildingType in new[] { BuildingType.Residential, BuildingType.Commercial })
        {
            await AssertScenario(defaultFrom, defaultTo, null, null, buildingType);
        }

        await AssertScenario(defaultFrom, defaultTo, PolicyStatus.Active, "RON", BuildingType.Residential);

        await AssertScenario(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), null, null, null);
        await AssertScenario(new DateOnly(2026, 3, 31), new DateOnly(2026, 3, 31), null, null, null);

        async Task AssertScenario(DateOnly from, DateOnly to, PolicyStatus? status, string? currency, BuildingType? buildingType)
        {
            var filtered = FilterPolicies(seeded, from, to, status, currency, buildingType).ToList();
            var expected = expectedBuilder(filtered);
            var response = await GetReportAsync<TDto>(path, from, to, status, currency, buildingType);

            AssertPagedResult(response, expected.Count, 1, 50);
            assertRows(ToListOrEmpty(response.Items), expected);
        }
    }

    private async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        TestSeed.Seed(db);
    }

    private async Task<List<PolicySeedResult>> SeedDefaultPoliciesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var builder = new ReportingDataBuilder(db);

        var policies = new List<PolicySeedResult>
        {
            builder.AddPolicy(new PolicySeedOptions(
                "Romania",
                "Cluj",
                "Cluj-Napoca",
                "Alpha Broker",
                "RON",
                1m,
                PolicyStatus.Active,
                BuildingType.Residential,
                new DateOnly(2026, 1, 15),
                new DateOnly(2026, 3, 1),
                1000m,
                900m)),
            builder.AddPolicy(new PolicySeedOptions(
                "Romania",
                "Cluj",
                "Cluj-Napoca",
                "Alpha Broker",
                "EUR",
                4.9m,
                PolicyStatus.Cancelled,
                BuildingType.Commercial,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 1, 31),
                333.33m,
                300m)),
            builder.AddPolicy(new PolicySeedOptions(
                "Romania",
                "Brasov",
                "Brasov",
                "Beta Broker",
                "RON",
                1m,
                PolicyStatus.Active,
                BuildingType.Commercial,
                new DateOnly(2026, 2, 5),
                new DateOnly(2026, 5, 5),
                700m,
                640m)),
            builder.AddPolicy(new PolicySeedOptions(
                "Romania",
                "Brasov",
                "Brasov",
                "Beta Broker",
                "EUR",
                4.9m,
                PolicyStatus.Expired,
                BuildingType.Residential,
                new DateOnly(2026, 2, 1),
                new DateOnly(2026, 2, 28),
                400m,
                360m)),
            builder.AddPolicy(new PolicySeedOptions(
                "Bulgaria",
                "Sofia",
                "Sofia",
                "Alpha Broker",
                "RON",
                1m,
                PolicyStatus.Active,
                BuildingType.Residential,
                new DateOnly(2026, 3, 15),
                new DateOnly(2026, 5, 15),
                600m,
                550m)),
            builder.AddPolicy(new PolicySeedOptions(
                "Bulgaria",
                "Varna",
                "Varna",
                "Beta Broker",
                "EUR",
                4.9m,
                PolicyStatus.Active,
                BuildingType.Commercial,
                new DateOnly(2025, 12, 15),
                new DateOnly(2026, 1, 15),
                550m,
                500m)),
            builder.AddPolicy(new PolicySeedOptions(
                "Bulgaria",
                "Varna",
                "Varna",
                "Alpha Broker",
                "RON",
                1m,
                PolicyStatus.Cancelled,
                BuildingType.Commercial,
                new DateOnly(2026, 3, 31),
                new DateOnly(2026, 3, 31),
                150m,
                140m))
        };

        await db.SaveChangesAsync();
        return policies;
    }

    private async Task<PagedResult<T>> GetReportAsync<T>(
        string path,
        DateOnly from,
        DateOnly to,
        PolicyStatus? status = null,
        string? currency = null,
        BuildingType? buildingType = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["From"] = from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["To"] = to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["pageNumber"] = pageNumber.ToString(CultureInfo.InvariantCulture),
            ["pageSize"] = pageSize.ToString(CultureInfo.InvariantCulture)
        };

        if (status.HasValue)
        {
            query["PolicyStatus"] = ((int)status.Value).ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(currency))
        {
            query["Currency"] = currency;
        }

        if (buildingType.HasValue)
        {
            query["BuildingType"] = ((int)buildingType.Value).ToString(CultureInfo.InvariantCulture);
        }

        var url = QueryHelpers.AddQueryString(path, query!);
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<T>>();
        return payload ?? throw new XunitException("Response body was empty.");
    }

    private async Task<ReportsAnalyticsDto> GetAnalyticsAsync(
        DateOnly from,
        DateOnly to,
        string? currency = null,
        bool filterByCurrency = false)
    {
        var query = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["From"] = from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["To"] = to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["filterByCurrency"] = filterByCurrency.ToString()
        };

        if (!string.IsNullOrWhiteSpace(currency))
        {
            query["Currency"] = currency;
        }

        var url = QueryHelpers.AddQueryString(AnalyticsPath, query!);
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ReportsAnalyticsDto>();
        return payload ?? throw new XunitException("Response body was empty.");
    }

    private async Task SeedPaymentsAsync(params Payment[] payments)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Payments.AddRangeAsync(payments);
        await db.SaveChangesAsync();
    }

    private static Payment CreatePayment(PolicySeedResult policy, decimal amount, DateTime paymentDate, PaymentStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            PolicyId = policy.PolicyId,
            Amount = amount,
            CurrencyId = policy.CurrencyId,
            PaymentDate = DateTime.SpecifyKind(paymentDate, DateTimeKind.Utc),
            Method = PaymentMethod.BankTransfer,
            Status = status,
            CreatedAt = DateTime.SpecifyKind(paymentDate, DateTimeKind.Utc),
        };

    private static IEnumerable<PolicySeedResult> FilterPolicies(
        IEnumerable<PolicySeedResult> policies,
        DateOnly from,
        DateOnly to,
        PolicyStatus? status = null,
        string? currency = null,
        BuildingType? buildingType = null)
    {
        var filtered = policies.Where(p => p.StartDate >= from && p.EndDate <= to);

        if (status.HasValue)
        {
            filtered = filtered.Where(p => p.PolicyStatus == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(currency))
        {
            var normalized = currency.Trim().ToUpperInvariant();
            filtered = filtered.Where(p => string.Equals(p.CurrencyCode, normalized, StringComparison.OrdinalIgnoreCase));
        }

        if (buildingType.HasValue)
        {
            filtered = filtered.Where(p => p.BuildingType == buildingType.Value);
        }

        return filtered;
    }

    private static List<PoliciesByCountryListDto> BuildExpectedByCountry(IEnumerable<PolicySeedResult> policies) =>
        policies
            .GroupBy(p => new { p.CountryId, p.CountryName, p.CurrencyId, p.CurrencyCode })
            .Select(g => new PoliciesByCountryListDto
            {
                CountryId = g.Key.CountryId,
                CountryName = g.Key.CountryName,
                CurrencyId = g.Key.CurrencyId,
                CurrencyCode = g.Key.CurrencyCode,
                PoliciesCount = g.Count(),
                FinalPremium = g.Sum(x => x.FinalPremium),
                FinalPremiumBaseCurrency = Math.Round(g.Sum(x => x.FinalPremium * x.ExchangeRateToBase), 2)
            })
            .OrderBy(x => x.CountryName)
            .ThenBy(x => x.CurrencyCode)
            .ToList();

    private static List<PoliciesByCountyListDto> BuildExpectedByCounty(IEnumerable<PolicySeedResult> policies) =>
        policies
            .GroupBy(p => new { p.CountryId, p.CountryName, p.CountyId, p.CountyName, p.CurrencyId, p.CurrencyCode })
            .Select(g => new PoliciesByCountyListDto
            {
                CountryId = g.Key.CountryId,
                CountryName = g.Key.CountryName,
                CountyId = g.Key.CountyId,
                CountyName = g.Key.CountyName,
                CurrencyId = g.Key.CurrencyId,
                CurrencyCode = g.Key.CurrencyCode,
                PoliciesCount = g.Count(),
                FinalPremium = g.Sum(x => x.FinalPremium),
                FinalPremiumBaseCurrency = Math.Round(g.Sum(x => x.FinalPremium * x.ExchangeRateToBase), 2)
            })
            .OrderBy(x => x.CountyName)
            .ThenBy(x => x.CurrencyCode)
            .ToList();

    private static List<PoliciesByCityListDto> BuildExpectedByCity(IEnumerable<PolicySeedResult> policies) =>
        policies
            .GroupBy(p => new
            {
                p.CountryId,
                p.CountryName,
                p.CountyId,
                p.CountyName,
                p.CityId,
                p.CityName,
                p.CurrencyId,
                p.CurrencyCode
            })
            .Select(g => new PoliciesByCityListDto
            {
                CountryId = g.Key.CountryId,
                CountryName = g.Key.CountryName,
                CountyId = g.Key.CountyId,
                CountyName = g.Key.CountyName,
                CityId = g.Key.CityId,
                CityName = g.Key.CityName,
                CurrencyId = g.Key.CurrencyId,
                CurrencyCode = g.Key.CurrencyCode,
                PoliciesCount = g.Count(),
                FinalPremium = g.Sum(x => x.FinalPremium),
                FinalPremiumBaseCurrency = Math.Round(g.Sum(x => x.FinalPremium * x.ExchangeRateToBase), 2)
            })
            .OrderBy(x => x.CityName)
            .ThenBy(x => x.CurrencyCode)
            .ToList();

    private static List<PoliciesByBrokerListDto> BuildExpectedByBroker(IEnumerable<PolicySeedResult> policies) =>
        policies
            .GroupBy(p => new { p.BrokerId, p.BrokerName, p.CurrencyId, p.CurrencyCode })
            .Select(g => new PoliciesByBrokerListDto
            {
                BrokerId = g.Key.BrokerId,
                BrokerName = g.Key.BrokerName,
                CurrencyId = g.Key.CurrencyId,
                CurrencyCode = g.Key.CurrencyCode,
                PoliciesCount = g.Count(),
                FinalPremium = g.Sum(x => x.FinalPremium),
                FinalPremiumBaseCurrency = Math.Round(g.Sum(x => x.FinalPremium * x.ExchangeRateToBase), 2)
            })
            .OrderBy(x => x.BrokerName)
            .ThenBy(x => x.CurrencyCode)
            .ToList();

    private static void AssertCountryRows(IReadOnlyList<PoliciesByCountryListDto> actual, IReadOnlyList<PoliciesByCountryListDto> expected)
    {
        var orderingKeys = actual.Select(x => (x.CountryName, x.CurrencyCode)).ToList();
        var sortedKeys = orderingKeys
            .OrderBy(k => k.CountryName, StringComparer.Ordinal)
            .ThenBy(k => k.CurrencyCode, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(sortedKeys, orderingKeys);

        var normalizedActual = actual
            .OrderBy(x => x.CountryName, StringComparer.Ordinal)
            .ThenBy(x => x.CurrencyCode, StringComparer.Ordinal)
            .ToList();
        var normalizedExpected = expected
            .OrderBy(x => x.CountryName, StringComparer.Ordinal)
            .ThenBy(x => x.CurrencyCode, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(normalizedExpected.Count, normalizedActual.Count);
        for (var i = 0; i < normalizedExpected.Count; i++)
        {
            Assert.Equal(normalizedExpected[i].CountryId, normalizedActual[i].CountryId);
            Assert.Equal(normalizedExpected[i].CountryName, normalizedActual[i].CountryName);
            Assert.Equal(normalizedExpected[i].CurrencyId, normalizedActual[i].CurrencyId);
            Assert.Equal(normalizedExpected[i].CurrencyCode, normalizedActual[i].CurrencyCode);
            Assert.Equal(normalizedExpected[i].PoliciesCount, normalizedActual[i].PoliciesCount);
            Assert.Equal(normalizedExpected[i].FinalPremium, normalizedActual[i].FinalPremium);
            Assert.Equal(normalizedExpected[i].FinalPremiumBaseCurrency, normalizedActual[i].FinalPremiumBaseCurrency);
        }
    }

    private static void AssertCountyRows(IReadOnlyList<PoliciesByCountyListDto> actual, IReadOnlyList<PoliciesByCountyListDto> expected)
    {
        var orderingKeys = actual.Select(x => (x.CountryName, x.CurrencyCode)).ToList();
        var sortedKeys = orderingKeys
            .OrderBy(k => k.CountryName, StringComparer.Ordinal)
            .ThenBy(k => k.CurrencyCode, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(sortedKeys, orderingKeys);

        var normalizedActual = actual
            .OrderBy(x => x.CountryName, StringComparer.Ordinal)
            .ThenBy(x => x.CountyName, StringComparer.Ordinal)
            .ThenBy(x => x.CurrencyCode, StringComparer.Ordinal)
            .ToList();
        var normalizedExpected = expected
            .OrderBy(x => x.CountryName, StringComparer.Ordinal)
            .ThenBy(x => x.CountyName, StringComparer.Ordinal)
            .ThenBy(x => x.CurrencyCode, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(normalizedExpected.Count, normalizedActual.Count);
        for (var i = 0; i < normalizedExpected.Count; i++)
        {
            Assert.Equal(normalizedExpected[i].CountryId, normalizedActual[i].CountryId);
            Assert.Equal(normalizedExpected[i].CountryName, normalizedActual[i].CountryName);
            Assert.Equal(normalizedExpected[i].CountyId, normalizedActual[i].CountyId);
            Assert.Equal(normalizedExpected[i].CountyName, normalizedActual[i].CountyName);
            Assert.Equal(normalizedExpected[i].CurrencyId, normalizedActual[i].CurrencyId);
            Assert.Equal(normalizedExpected[i].CurrencyCode, normalizedActual[i].CurrencyCode);
            Assert.Equal(normalizedExpected[i].PoliciesCount, normalizedActual[i].PoliciesCount);
            Assert.Equal(normalizedExpected[i].FinalPremium, normalizedActual[i].FinalPremium);
            Assert.Equal(normalizedExpected[i].FinalPremiumBaseCurrency, normalizedActual[i].FinalPremiumBaseCurrency);
        }
    }

    private static void AssertCityRows(IReadOnlyList<PoliciesByCityListDto> actual, IReadOnlyList<PoliciesByCityListDto> expected)
    {
        var orderingKeys = actual.Select(x => (x.CityName, x.CurrencyCode)).ToList();
        var sortedKeys = orderingKeys
            .OrderBy(k => k.CityName, StringComparer.Ordinal)
            .ThenBy(k => k.CurrencyCode, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(sortedKeys, orderingKeys);

        var normalizedActual = actual
            .OrderBy(x => x.CountryName, StringComparer.Ordinal)
            .ThenBy(x => x.CountyName, StringComparer.Ordinal)
            .ThenBy(x => x.CityName, StringComparer.Ordinal)
            .ThenBy(x => x.CurrencyCode, StringComparer.Ordinal)
            .ToList();
        var normalizedExpected = expected
            .OrderBy(x => x.CountryName, StringComparer.Ordinal)
            .ThenBy(x => x.CountyName, StringComparer.Ordinal)
            .ThenBy(x => x.CityName, StringComparer.Ordinal)
            .ThenBy(x => x.CurrencyCode, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(normalizedExpected.Count, normalizedActual.Count);
        for (var i = 0; i < normalizedExpected.Count; i++)
        {
            Assert.Equal(normalizedExpected[i].CountryId, normalizedActual[i].CountryId);
            Assert.Equal(normalizedExpected[i].CountryName, normalizedActual[i].CountryName);
            Assert.Equal(normalizedExpected[i].CountyId, normalizedActual[i].CountyId);
            Assert.Equal(normalizedExpected[i].CountyName, normalizedActual[i].CountyName);
            Assert.Equal(normalizedExpected[i].CityId, normalizedActual[i].CityId);
            Assert.Equal(normalizedExpected[i].CityName, normalizedActual[i].CityName);
            Assert.Equal(normalizedExpected[i].CurrencyId, normalizedActual[i].CurrencyId);
            Assert.Equal(normalizedExpected[i].CurrencyCode, normalizedActual[i].CurrencyCode);
            Assert.Equal(normalizedExpected[i].PoliciesCount, normalizedActual[i].PoliciesCount);
            Assert.Equal(normalizedExpected[i].FinalPremium, normalizedActual[i].FinalPremium);
            Assert.Equal(normalizedExpected[i].FinalPremiumBaseCurrency, normalizedActual[i].FinalPremiumBaseCurrency);
        }
    }

    private static void AssertBrokerRows(IReadOnlyList<PoliciesByBrokerListDto> actual, IReadOnlyList<PoliciesByBrokerListDto> expected)
    {
        var orderingKeys = actual.Select(x => (x.BrokerName, x.CurrencyCode)).ToList();
        var sortedKeys = orderingKeys
            .OrderBy(k => k.BrokerName, StringComparer.Ordinal)
            .ThenBy(k => k.CurrencyCode, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(sortedKeys, orderingKeys);

        var normalizedActual = actual
            .OrderBy(x => x.BrokerName, StringComparer.Ordinal)
            .ThenBy(x => x.CurrencyCode, StringComparer.Ordinal)
            .ToList();
        var normalizedExpected = expected
            .OrderBy(x => x.BrokerName, StringComparer.Ordinal)
            .ThenBy(x => x.CurrencyCode, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(normalizedExpected.Count, normalizedActual.Count);
        for (var i = 0; i < normalizedExpected.Count; i++)
        {
            Assert.Equal(normalizedExpected[i].BrokerId, normalizedActual[i].BrokerId);
            Assert.Equal(normalizedExpected[i].BrokerName, normalizedActual[i].BrokerName);
            Assert.Equal(normalizedExpected[i].CurrencyId, normalizedActual[i].CurrencyId);
            Assert.Equal(normalizedExpected[i].CurrencyCode, normalizedActual[i].CurrencyCode);
            Assert.Equal(normalizedExpected[i].PoliciesCount, normalizedActual[i].PoliciesCount);
            Assert.Equal(normalizedExpected[i].FinalPremium, normalizedActual[i].FinalPremium);
            Assert.Equal(normalizedExpected[i].FinalPremiumBaseCurrency, normalizedActual[i].FinalPremiumBaseCurrency);
        }
    }

    private static void AssertPagedResult<T>(PagedResult<T> result, int expectedCount, int expectedPageNumber, int expectedPageSize)
    {
        Assert.Equal(expectedPageNumber, result.PageNumber);
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(expectedCount, result.TotalCount);
        Assert.NotNull(result.Items);
        Assert.Equal(expectedCount, result.Items.Count);
    }

    private static List<T> ToListOrEmpty<T>(IReadOnlyList<T>? source) => source?.ToList() ?? new List<T>();

    private sealed class ReportingDataBuilder
    {
        private readonly AppDbContext _db;
        private readonly Dictionary<string, Country> _countries = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<(Guid CountryId, string County), County> _counties = new();
        private readonly Dictionary<(Guid CountyId, string City), City> _cities = new();
        private readonly Dictionary<string, Currency> _currencies = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Broker> _brokers = new(StringComparer.OrdinalIgnoreCase);

        public ReportingDataBuilder(AppDbContext db)
        {
            _db = db;
        }

        public PolicySeedResult AddPolicy(PolicySeedOptions options)
        {
            var country = EnsureCountry(options.CountryName);
            var county = EnsureCounty(country, options.CountyName);
            var city = EnsureCity(county, options.CityName);
            var currency = EnsureCurrency(options.CurrencyCode, options.ExchangeRateToBase);
            var broker = EnsureBroker(options.BrokerName);

            var client = new Client
            {
                Id = Guid.NewGuid(),
                ClientType = ClientType.Individual,
                Name = $"Client {Guid.NewGuid():N}".Substring(0, 12),
                IdentificationNumber = GenerateNumericString(13),
                Email = $"reports_{Guid.NewGuid():N}@test.local",
                PhoneNumber = "+40111222333"
            };

            var address = new Address
            {
                Id = Guid.NewGuid(),
                CityId = city.Id,
                Street = options.Street ?? "Test Street",
                Number = options.Number ?? "1",
                IsPrimary = true
            };

            var building = new Building
            {
                Id = Guid.NewGuid(),
                ClientId = client.Id,
                AddressId = address.Id,
                CurrencyId = currency.Id,
                Currency = currency,
                ConstructionYear = 2015,
                BuildingType = options.BuildingType,
                NumberOfFloors = 5,
                SurfaceArea = 140,
                InsuredValue = 250000,
                RiskIndicatiors = "Low"
            };

            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                PolicyNumber = $"POL-{Guid.NewGuid():N}",
                BrokerId = broker.Id,
                Broker = broker,
                ClientId = client.Id,
                Client = client,
                BuildingId = building.Id,
                Building = building,
                PolicyStatus = options.PolicyStatus,
                CurrentVersionNumber = 1
            };

            var policyVersion = new PolicyVersion
            {
                Id = Guid.NewGuid(),
                PolicyId = policy.Id,
                Policy = policy,
                VersionNumber = policy.CurrentVersionNumber,
                StartDate = options.StartDate,
                EndDate = options.EndDate,
                BasePremium = options.BasePremium ?? options.FinalPremium - 100m,
                FinalPremium = options.FinalPremium,
                CurrencyId = currency.Id,
                Currency = currency,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = broker.Name,
                IsActiveVersion = true
            };

            policy.PolicyVersionId = policyVersion.Id;
            policy.PolicyVersions = new List<PolicyVersion> { policyVersion };

            _db.Clients.Add(client);
            _db.Addresses.Add(address);
            _db.Buildings.Add(building);
            _db.Policies.Add(policy);
            _db.PolicyVersions.Add(policyVersion);

            return new PolicySeedResult(
                policy.Id,
                country.Id,
                country.Name,
                county.Id,
                county.Name,
                city.Id,
                city.Name,
                broker.Id,
                broker.Name,
                currency.Id,
                currency.Code,
                currency.ExchangeRateToBase,
                options.PolicyStatus,
                options.BuildingType,
                options.StartDate,
                options.EndDate,
                options.FinalPremium);
        }

        private Country EnsureCountry(string name)
        {
            if (_countries.TryGetValue(name, out var existing))
            {
                return existing;
            }

            var country = _db.Countries.Local.FirstOrDefault(c => c.Name == name) ??
                          _db.Countries.FirstOrDefault(c => c.Name == name);
            if (country == null)
            {
                country = new Country { Id = Guid.NewGuid(), Name = name };
                _db.Countries.Add(country);
            }

            _countries[name] = country;
            return country;
        }

        private County EnsureCounty(Country country, string name)
        {
            var key = (country.Id, name);
            if (_counties.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var county = _db.Counties.Local.FirstOrDefault(c => c.CountryId == country.Id && c.Name == name) ??
                         _db.Counties.FirstOrDefault(c => c.CountryId == country.Id && c.Name == name);
            if (county == null)
            {
                county = new County { Id = Guid.NewGuid(), CountryId = country.Id, Name = name };
                _db.Counties.Add(county);
            }

            _counties[key] = county;
            return county;
        }

        private City EnsureCity(County county, string name)
        {
            var key = (county.Id, name);
            if (_cities.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var city = _db.Cities.Local.FirstOrDefault(c => c.CountyId == county.Id && c.Name == name) ??
                       _db.Cities.FirstOrDefault(c => c.CountyId == county.Id && c.Name == name);
            if (city == null)
            {
                city = new City { Id = Guid.NewGuid(), CountyId = county.Id, Name = name };
                _db.Cities.Add(city);
            }

            _cities[key] = city;
            return city;
        }

        private Currency EnsureCurrency(string code, decimal exchangeRate)
        {
            if (_currencies.TryGetValue(code, out var existing))
            {
                existing.ExchangeRateToBase = exchangeRate;
                return existing;
            }

            var currency = _db.Currencies.Local.FirstOrDefault(c => c.Code == code) ??
                           _db.Currencies.FirstOrDefault(c => c.Code == code);
            if (currency == null)
            {
                currency = new Currency
                {
                    Id = Guid.NewGuid(),
                    Code = code.ToUpperInvariant(),
                    Name = $"{code.ToUpperInvariant()} Currency",
                    ExchangeRateToBase = exchangeRate,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _db.Currencies.Add(currency);
            }
            else
            {
                currency.ExchangeRateToBase = exchangeRate;
            }

            _currencies[code] = currency;
            return currency;
        }

        private Broker EnsureBroker(string name)
        {
            if (_brokers.TryGetValue(name, out var existing))
            {
                return existing;
            }

            var broker = _db.Brokers.Local.FirstOrDefault(b => b.Name == name) ??
                         _db.Brokers.FirstOrDefault(b => b.Name == name);
            if (broker == null)
            {
                var sanitizedName = name.Replace(" ", string.Empty, StringComparison.Ordinal);
                broker = new Broker
                {
                    Id = Guid.NewGuid(),
                    BrokerCode = $"{sanitizedName.ToUpperInvariant()}-{Guid.NewGuid():N}",
                    Name = name,
                    Email = $"{sanitizedName.ToLowerInvariant()}@reports.test",
                    BrokerStatus = BrokerStatus.Active,
                    CommissionPercentage = 10m,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Brokers.Add(broker);
            }

            _brokers[name] = broker;
            return broker;
        }
    }

    private sealed record PolicySeedOptions(
        string CountryName,
        string CountyName,
        string CityName,
        string BrokerName,
        string CurrencyCode,
        decimal ExchangeRateToBase,
        PolicyStatus PolicyStatus,
        BuildingType BuildingType,
        DateOnly StartDate,
        DateOnly EndDate,
        decimal FinalPremium,
        decimal? BasePremium = null,
        string? Street = null,
        string? Number = null);

    private sealed record PolicySeedResult(
        Guid PolicyId,
        Guid CountryId,
        string CountryName,
        Guid CountyId,
        string CountyName,
        Guid CityId,
        string CityName,
        Guid BrokerId,
        string BrokerName,
        Guid CurrencyId,
        string CurrencyCode,
        decimal ExchangeRateToBase,
        PolicyStatus PolicyStatus,
        BuildingType BuildingType,
        DateOnly StartDate,
        DateOnly EndDate,
        decimal FinalPremium);

    private static string GenerateNumericString(int length)
    {
        Span<char> buffer = stackalloc char[length];
        for (var i = 0; i < length; i++)
        {
            buffer[i] = (char)('0' + Random.Shared.Next(0, 10));
        }

        return new string(buffer);
    }
}
