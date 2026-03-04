using System;
using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.TestInfrastructure;
using Application.Policies.DTOs.Response;
using Domain.Models.Brokers;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Xunit;

namespace API.IntegrationTests.Policies;

public class Policies_FullFlow_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _http;

    public Policies_FullFlow_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _http = factory.CreateClient();
    }

    [Fact]
    public async Task Should_CreateClientBuildingPolicy_AndActivate_When_RequestIsValid()
    {
        await ResetDatabaseAsync();

        var brokerId = await GetActiveBrokerIdAsync();
        var clientId = await CreateClientAsync();
        var buildingId = await CreateBuildingAsync(clientId);
        var (policyId, _, _) = await CreatePolicyDraftAsync(clientId, buildingId, brokerId);

        await ActivatePolicyAsync(policyId);

        var details = await GetPolicyDetailsAsync(policyId);

        Assert.Equal(policyId, details.Id);
        Assert.Equal(PolicyStatus.Active, details.PolicyStatus);
        Assert.Equal(clientId, details.Client.Id);
        Assert.Equal(buildingId, details.Building.Id);
        Assert.Equal(brokerId, details.Broker.Id);
        Assert.True(details.FinalPremium >= details.BasePremium);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ClientMissing_OnPolicyCreate()
    {
        await ResetDatabaseAsync();

        var brokerId = await GetActiveBrokerIdAsync();
        var payload = new
        {
            clientId = Guid.NewGuid(),
            buildingId = Guid.NewGuid(),
            currencyId = TestConstants.CurrencyId,
            basePremium = 1000m,
            startDate = new DateOnly(2026, 2, 12),
            endDate = new DateOnly(2026, 3, 12),
            brokerId = brokerId
        };

        var response = await _http.PostAsJsonAsync("/api/brokers/policies", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_BuildingMissing_OnPolicyCreate()
    {
        await ResetDatabaseAsync();

        var brokerId = await GetActiveBrokerIdAsync();
        var clientId = await CreateClientAsync();
        var payload = new
        {
            clientId = clientId,
            buildingId = Guid.NewGuid(),
            currencyId = TestConstants.CurrencyId,
            basePremium = 1000m,
            startDate = new DateOnly(2026, 2, 12),
            endDate = new DateOnly(2026, 3, 12),
            brokerId = brokerId
        };

        var response = await _http.PostAsJsonAsync("/api/brokers/policies", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_BasePremiumInvalid()
    {
        await ResetDatabaseAsync();

        var brokerId = await GetActiveBrokerIdAsync();
        var clientId = await CreateClientAsync();
        var buildingId = await CreateBuildingAsync(clientId);

        var payload = new
        {
            clientId,
            buildingId,
            currencyId = TestConstants.CurrencyId,
            basePremium = -100m,
            startDate = new DateOnly(2026, 2, 12),
            endDate = new DateOnly(2026, 3, 12),
            brokerId
        };

        var response = await _http.PostAsJsonAsync("/api/brokers/policies", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ActivatingMissingPolicy()
    {
        await ResetDatabaseAsync();

        var response = await _http.PostAsync($"/api/brokers/policies/{Guid.NewGuid()}/activate", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ActivatingPolicyNotInDraft()
    {
        await ResetDatabaseAsync();

        var brokerId = await GetActiveBrokerIdAsync();
        var clientId = await CreateClientAsync();
        var buildingId = await CreateBuildingAsync(clientId);
        var (policyId, _, _) = await CreatePolicyDraftAsync(clientId, buildingId, brokerId);

        await ActivatePolicyAsync(policyId);

        var response = await _http.PostAsync($"/api/brokers/policies/{policyId}/activate", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_CancelPolicy_When_RequestValid()
    {
        await ResetDatabaseAsync();

        var brokerId = await GetActiveBrokerIdAsync();
        var clientId = await CreateClientAsync();
        var buildingId = await CreateBuildingAsync(clientId);
        var (policyId, startDate, _) = await CreatePolicyDraftAsync(clientId, buildingId, brokerId);

        await ActivatePolicyAsync(policyId);

        var cancelDate = startDate.AddDays(5);
        var cancelPayload = new
        {
            cancellationDate = cancelDate,
            cancellationReason = "Customer request"
        };

        var cancelResponse = await _http.PostAsJsonAsync($"/api/brokers/policies/{policyId}/cancel", cancelPayload);

        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var details = await GetPolicyDetailsAsync(policyId);
        Assert.Equal(PolicyStatus.Cancelled, details.PolicyStatus);
        Assert.Equal(cancelDate, details.CancelledAt);
        Assert.Equal("Customer request", details.CancellationReason);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_CancelDateBeforeStart()
    {
        await ResetDatabaseAsync();

        var brokerId = await GetActiveBrokerIdAsync();
        var clientId = await CreateClientAsync();
        var buildingId = await CreateBuildingAsync(clientId);
        var (policyId, startDate, _) = await CreatePolicyDraftAsync(clientId, buildingId, brokerId);

        await ActivatePolicyAsync(policyId);

        var cancelPayload = new
        {
            cancellationDate = startDate.AddDays(-1),
            cancellationReason = "Too early"
        };

        var response = await _http.PostAsJsonAsync($"/api/brokers/policies/{policyId}/cancel", cancelPayload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_CancelDateAfterEnd()
    {
        await ResetDatabaseAsync();

        var brokerId = await GetActiveBrokerIdAsync();
        var clientId = await CreateClientAsync();
        var buildingId = await CreateBuildingAsync(clientId);
        var (policyId, startDate, endDate) = await CreatePolicyDraftAsync(clientId, buildingId, brokerId, startDate: new DateOnly(2026, 2, 1), endDate: new DateOnly(2026, 2, 28));

        await ActivatePolicyAsync(policyId);

        var cancelPayload = new
        {
            cancellationDate = endDate.AddDays(1),
            cancellationReason = "Late request"
        };

        var response = await _http.PostAsJsonAsync($"/api/brokers/policies/{policyId}/cancel", cancelPayload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_RejectPolicyCreation_When_BrokerInactive()
    {
        await ResetDatabaseAsync();

        var brokerId = await GetActiveBrokerIdAsync();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var broker = await db.Brokers.FirstAsync(x => x.Id == brokerId);
            broker.BrokerStatus = BrokerStatus.Inactive;
            await db.SaveChangesAsync();
        }

        var clientId = await CreateClientAsync();
        var buildingId = await CreateBuildingAsync(clientId);

        var payload = new
        {
            clientId,
            buildingId,
            currencyId = TestConstants.CurrencyId,
            basePremium = 1000m,
            startDate = new DateOnly(2026, 2, 12),
            endDate = new DateOnly(2026, 3, 12),
            brokerId
        };

        var response = await _http.PostAsJsonAsync("/api/brokers/policies", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnPolicyDetails_WithLinkedData()
    {
        await ResetDatabaseAsync();

        var brokerId = await GetActiveBrokerIdAsync();
        var clientId = await CreateClientAsync();
        var buildingId = await CreateBuildingAsync(clientId);
        var (policyId, _, _) = await CreatePolicyDraftAsync(clientId, buildingId, brokerId);

        var details = await GetPolicyDetailsAsync(policyId);

        Assert.Equal(clientId, details.Client.Id);
        Assert.Equal(buildingId, details.Building.Id);
        Assert.Equal(brokerId, details.Broker.Id);
        Assert.NotEmpty(details.PolicyAdjustments);
        Assert.All(details.PolicyAdjustments, adj => Assert.True(adj.Amount >= 0));
    }

    private async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        TestSeed.Seed(db);
    }

    private async Task<Guid> GetActiveBrokerIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Brokers
            .Where(b => b.BrokerStatus == BrokerStatus.Active)
            .Select(b => b.Id)
            .FirstAsync();
    }

    private async Task<Guid> CreateClientAsync()
    {
        var request = new
        {
            clientType = 0,
            name = $"Client {Guid.NewGuid():N}",
            identificationNumber = GenerateValidCnp(),
            email = $"client_{Guid.NewGuid():N}@test.com",
            phoneNumber = "0700000000",
            address = new
            {
                street = "Seed Street",
                number = "1",
                cityId = TestConstants.CityId
            }
        };

        var response = await _http.PostAsJsonAsync("/api/brokers/clients", request);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"CreateClient failed: {(int)response.StatusCode} {response.StatusCode}\n{err}");
        }

        return await ParseGuidResponseAsync(response);
    }

    private async Task<Guid> CreateBuildingAsync(Guid clientId)
    {
        var request = new
        {
            clientId,
            address = new
            {
                street = "Iris",
                number = "123",
                cityId = TestConstants.CityId
            },
            constructionYear = 2010,
            buildingType = 0,
            numberOfFloors = 2,
            surfaceArea = 120,
            insuredValue = 250000,
            riskIndicators = "none"
        };

        var response = await _http.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", request);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"CreateBuilding failed: {(int)response.StatusCode} {response.StatusCode}\n{err}");
        }

        return await ParseGuidResponseAsync(response);
    }

    private async Task<(Guid PolicyId, DateOnly StartDate, DateOnly EndDate)> CreatePolicyDraftAsync(
        Guid clientId,
        Guid buildingId,
        Guid brokerId,
        decimal basePremium = 1000m,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var start = startDate ?? new DateOnly(2026, 2, 12);
        var end = endDate ?? start.AddMonths(1);

        var request = new
        {
            clientId,
            buildingId,
            currencyId = TestConstants.CurrencyId,
            policyNumber = (string?)null,
            basePremium = basePremium,
            startDate = start,
            endDate = end,
            brokerId
        };

        var response = await _http.PostAsJsonAsync("/api/brokers/policies", request);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"CreatePolicy failed: {(int)response.StatusCode} {response.StatusCode}\n{err}");
        }

        var dto = await response.Content.ReadFromJsonAsync<PolicyDetailsDto>();
        if (dto == null) throw new Xunit.Sdk.XunitException("Policy response body was empty");

        return (dto.Id, start, end);
    }

    private async Task ActivatePolicyAsync(Guid policyId)
    {
        var response = await _http.PostAsync($"/api/brokers/policies/{policyId}/activate", null);
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"ActivatePolicy failed: {(int)response.StatusCode} {response.StatusCode}\n{err}");
        }
    }

    private async Task<PolicyDetailsDto> GetPolicyDetailsAsync(Guid policyId)
    {
        var response = await _http.GetAsync($"/api/brokers/policies/{policyId}");
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PolicyDetailsDto>();
        if (dto == null) throw new Xunit.Sdk.XunitException("Policy details response body was empty");
        return dto;
    }

    private static async Task<Guid> ParseGuidResponseAsync(HttpResponseMessage response)
    {
        var raw = await response.Content.ReadAsStringAsync();
        return Guid.Parse(raw.Trim('\"'));
    }

    private static string GenerateValidCnp()
    {
        Span<int> digits = stackalloc int[13];
        digits[0] = 1;
        for (var i = 1; i < 12; i++)
        {
            digits[i] = Random.Shared.Next(0, 10);
        }

        var controlKey = new[] { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            sum += digits[i] * controlKey[i];
        }

        var controlDigit = sum % 11;
        if (controlDigit == 10) controlDigit = 1;
        digits[12] = controlDigit;

        Span<char> buffer = stackalloc char[13];
        for (var i = 0; i < 13; i++)
        {
            buffer[i] = (char)('0' + digits[i]);
        }

        return new string(buffer);
    }
}