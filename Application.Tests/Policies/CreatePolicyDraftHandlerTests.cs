using System.Linq;
using Application.Brokers.DTOs.Response;
using Application.Buildings.DTOs.Response;
using Application.Clients.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Policies.DTOs.Command;
using Application.Policies.DTOs.Requests;
using Application.Policies.DTOs.Response;
using Application.Metadatas.Currencies.DTOs.Response;
using AutoMapper;
using Domain.Models;
using Domain.Models.Brokers;
using Domain.Models.Buildings;
using Domain.Models.Clients;
using Domain.Models.Geography.Address;
using Domain.Models.Metadatas;
using Domain.Models.Policies;
using FluentAssertions;
using Moq;

namespace Application.Tests.Policies;

public class CreatePolicyDraftHandlerTests
{
    private readonly Mock<IPolicyRepository> _policyRepo = new();
    private readonly Mock<IClientRepository> _clientRepo = new();
    private readonly Mock<IBuildingRepository> _buildingRepo = new();
    private readonly Mock<ICurrencyRepository> _currencyRepo = new();
    private readonly Mock<IBrokerRepository> _brokerRepo = new();
    private readonly Mock<IPremiumCalculator> _calculator = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CreatePolicyDraft.Handler _handler;

    public CreatePolicyDraftHandlerTests()
    {
        _handler = new CreatePolicyDraft.Handler(
            _policyRepo.Object,
            _clientRepo.Object,
            _buildingRepo.Object,
            _currencyRepo.Object,
            _brokerRepo.Object,
            _calculator.Object,
            _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenClientIsMissing()
    {
        var dto = BuildDto();
        _clientRepo.Setup(x => x.GetClientAsync(dto.ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var command = new CreatePolicyDraft.Command { CreatePolicyDraftDto = dto };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenBuildingIsMissing()
    {
        var dto = BuildDto();
        _clientRepo.Setup(x => x.GetClientAsync(dto.ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client { Id = dto.ClientId });

        _buildingRepo.Setup(x => x.GetBuildingAsync(dto.BuildingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Models.Buildings.Building?)null);

        var command = new CreatePolicyDraft.Command { CreatePolicyDraftDto = dto };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCreateDraftAndReturnDetails()
    {
        var dto = BuildDto();
        var client = new Client { Id = dto.ClientId, Name = "John" };
        var country = new Country { Id = Guid.NewGuid(), Name = "Country" };
        var county = new County { Id = Guid.NewGuid(), CountryId = country.Id, Name = "County", Country = country };
        var city = new City { Id = Guid.NewGuid(), CountyId = county.Id, Name = "City", County = county };
        var address = new Address { Id = Guid.NewGuid(), CityId = city.Id, City = city, Street = "Main", Number = "1" };
        var building = new Building { Id = dto.BuildingId, AddressId = address.Id, Address = address, BuildingType = BuildingType.Residential };
        var currency = new Currency { Id = dto.CurrencyId, Code = "EUR", Name = "Euro" };
        var broker = new Broker { Id = dto.BrokerId, BrokerStatus = BrokerStatus.Active, BrokerCode = "BRK-1", Name = "Broker", Email = "broker@test.com" };

        _clientRepo.Setup(x => x.GetClientAsync(dto.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        _buildingRepo.Setup(x => x.GetBuildingAsync(dto.BuildingId, It.IsAny<CancellationToken>())).ReturnsAsync(building);
        _currencyRepo.Setup(x => x.GetCurrencyAsync(dto.CurrencyId, It.IsAny<CancellationToken>())).ReturnsAsync(currency);
        _brokerRepo.Setup(x => x.GetBrokerAsync(dto.BrokerId, It.IsAny<CancellationToken>())).ReturnsAsync(broker);

        var adjustments = new List<PolicyAdjustement>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                AdjustementType = AdjustementType.AdminFee,
                Percentage = 2m,
                Amount = 20m
            }
        };

        _calculator.Setup(x => x.CalculateAsync(building, dto.BasePremium, dto.StartDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((dto.BasePremium + 50m, adjustments));

        var clientDto = new ClientDetailsDto { Id = client.Id, Name = "John" };
        var buildingDto = new BuildingDetailsDto { Id = building.Id };
        var currencyDto = new CurrencyDto { Id = currency.Id, Code = currency.Code, Name = currency.Name };
        var brokerDto = new BrokerDetailsDto { Id = broker.Id, BrokerCode = broker.BrokerCode, Name = broker.Name, Email = broker.Email, BrokerStatus = broker.BrokerStatus };
        var policyDetailsDto = new PolicyDetailsDto();

        _mapper.Setup(x => x.Map<ClientDetailsDto>(client)).Returns(clientDto);
        _mapper.Setup(x => x.Map<BuildingDetailsDto>(building)).Returns(buildingDto);
        _mapper.Setup(x => x.Map<CurrencyDto>(currency)).Returns(currencyDto);
        _mapper.Setup(x => x.Map<BrokerDetailsDto>(broker)).Returns(brokerDto);
        _mapper.Setup(x => x.Map<PolicyDetailsDto>(It.IsAny<Policy>())).Returns(policyDetailsDto);

        Policy? capturedPolicy = null;
        _policyRepo.Setup(x => x.CreatePolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
            .Callback<Policy, CancellationToken>((policy, _) => capturedPolicy = policy)
            .Returns(Task.CompletedTask);
        _policyRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(new CreatePolicyDraft.Command { CreatePolicyDraftDto = dto }, CancellationToken.None);

        result.Should().BeSameAs(policyDetailsDto);
        result.Client.Should().Be(clientDto);
        result.Building.Should().Be(buildingDto);
        result.CurrencyCode.Should().Be(currencyDto.Code);

        _calculator.Verify(x => x.CalculateAsync(building, dto.BasePremium, dto.StartDate, It.IsAny<CancellationToken>()), Times.Once);
        _policyRepo.Verify(x => x.CreatePolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()), Times.Once);
        _policyRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        capturedPolicy.Should().NotBeNull();
        capturedPolicy!.PolicyStatus.Should().Be(PolicyStatus.Draft);
        capturedPolicy.PolicyAdjustements.Should().HaveCount(1);
        capturedPolicy.PolicyAdjustements.All(a => a.Policy == capturedPolicy).Should().BeTrue();
    }

    private static CreatePolicyDraftDto BuildDto() => new()
    {
        ClientId = Guid.NewGuid(),
        BuildingId = Guid.NewGuid(),
        BrokerId = Guid.NewGuid(),
        CurrencyId = Guid.NewGuid(),
        BasePremium = 1000m,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
        PolicyNumber = null
    };
}