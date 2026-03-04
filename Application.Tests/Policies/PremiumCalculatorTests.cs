// using Application.Core.Interfaces.IRepositories;
// using Domain.Models;
// using Domain.Models.Buildings;
// using Domain.Models.Geography.Address;
// using Domain.Models.Metadatas;
// using Domain.Models.Policies;
// using FluentAssertions;
// using Moq;
// using Persistence.Repositories;

// namespace Application.Tests.Policies;

// public class PremiumCalculatorTests
// {
//     private readonly Mock<IRiskFactorRepository> _riskFactors = new();
//     private readonly Mock<IFeeConfigurationRepository> _fees = new();
//     private readonly PremiumCalculator _sut;

//     public PremiumCalculatorTests()
//     {
//         _sut = new PremiumCalculator(_riskFactors.Object, _fees.Object);
//     }

//     [Fact]
//     public async Task CalculateAsync_ShouldApplyCityRiskFactor_WhenCityMatches()
//     {
//         var cityId = Guid.NewGuid();
//         var countyId = Guid.NewGuid();
//         var countryId = Guid.NewGuid();
//         var building = CreateBuilding(cityId, countyId, countryId, BuildingType.Residential);

//         _fees.Setup(x => x.GetActiveFeeConfigurationsAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(new List<FeeConfiguration>());

//         _riskFactors.Setup(x => x.GetActiveRiskFactorConfigurationsAsync(It.IsAny<CancellationToken>()))
//             .ReturnsAsync(
//                 new List<RiskFactorConfiguration>
//                 {
//                     new()
//                     {
//                         Id = Guid.NewGuid(),
//                         RiskLevel = RiskLevel.City,
//                         ReferenceID = cityId,
//                         AdjustementPercentage = 5m
//                     }
//                 });

//         var (finalPremium, adjustments) = await _sut.CalculateAsync(building, 1000m, DateOnly.FromDateTime(DateTime.UtcNow), CancellationToken.None);

//         finalPremium.Should().Be(1050m);
//         adjustments.Should().ContainSingle(adj =>
//             adj.AdjustmentType == AdjustmentType.RiskAdjustement &&
//             adj.Percentage == 5m &&
//             adj.Amount == 50m);
//     }

//     [Fact]
//     public async Task CalculateAsync_ShouldMatchCountyCountryAndBuildingTypeRiskFactors()
//     {
//         var cityId = Guid.NewGuid();
//         var countyId = Guid.NewGuid();
//         var countryId = Guid.NewGuid();
//         var building = CreateBuilding(cityId, countyId, countryId, BuildingType.Commercial);

//         _fees.Setup(x => x.GetActiveFeeConfigurationsAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(new List<FeeConfiguration>());

//         _riskFactors.Setup(x => x.GetActiveRiskFactorConfigurationsAsync(It.IsAny<CancellationToken>()))
//             .ReturnsAsync(
//                 new List<RiskFactorConfiguration>
//                 {
//                     new()
//                     {
//                         Id = Guid.NewGuid(),
//                         RiskLevel = RiskLevel.BuildingType,
//                         BuildingType = BuildingType.Commercial,
//                         AdjustementPercentage = 3m
//                     },
//                     new()
//                     {
//                         Id = Guid.NewGuid(),
//                         RiskLevel = RiskLevel.County,
//                         ReferenceID = countyId,
//                         AdjustementPercentage = 2m
//                     },
//                     new()
//                     {
//                         Id = Guid.NewGuid(),
//                         RiskLevel = RiskLevel.Country,
//                         ReferenceID = countryId,
//                         AdjustementPercentage = 1m
//                     }
//                 });

//         var (_, adjustments) = await _sut.CalculateAsync(building, 1000m, DateOnly.FromDateTime(DateTime.UtcNow), CancellationToken.None);

//         adjustments.Should().HaveCount(3);
//         adjustments.Sum(a => a.Amount).Should().Be(60m);
//     }

//     [Fact]
//     public async Task CalculateAsync_ShouldRoundWithMultipleFeesAndRiskFactors()
//     {
//         var cityId = Guid.NewGuid();
//         var countyId = Guid.NewGuid();
//         var countryId = Guid.NewGuid();
//         var building = CreateBuilding(cityId, countyId, countryId, BuildingType.MixedUse);

//         _fees.Setup(x => x.GetActiveFeeConfigurationsAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(
//                 new List<FeeConfiguration>
//                 {
//                     new() { Id = Guid.NewGuid(), Name = "Admin", FeeType = FeeType.AdminFee, Percentage = 2.5m },
//                     new() { Id = Guid.NewGuid(), Name = "Broker", FeeType = FeeType.BrokerComission, Percentage = 1.75m }
//                 });

//         _riskFactors.Setup(x => x.GetActiveRiskFactorConfigurationsAsync(It.IsAny<CancellationToken>()))
//             .ReturnsAsync(
//                 new List<RiskFactorConfiguration>
//                 {
//                     new()
//                     {
//                         Id = Guid.NewGuid(),
//                         RiskLevel = RiskLevel.City,
//                         ReferenceID = cityId,
//                         AdjustementPercentage = 1.33m
//                     }
//                 });

//         var (finalPremium, adjustments) = await _sut.CalculateAsync(building, 1234.56m, DateOnly.FromDateTime(DateTime.UtcNow), CancellationToken.None);

//         finalPremium.Should().Be(1303.44m);
//         adjustments.Should().HaveCount(3);
//         adjustments.Sum(a => a.Amount).Should().Be(68.88m);
//     }

//     [Fact]
//     public async Task CalculateAsync_ShouldRoundMidpointsAwayFromZero()
//     {
//         var ids = (City: Guid.NewGuid(), County: Guid.NewGuid(), Country: Guid.NewGuid());
//         var building = CreateBuilding(ids.City, ids.County, ids.Country, BuildingType.MixedUse);

//         _fees.Setup(x => x.GetActiveFeeConfigurationsAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(new List<FeeConfiguration>());

//         _riskFactors.Setup(x => x.GetActiveRiskFactorConfigurationsAsync(It.IsAny<CancellationToken>()))
//             .ReturnsAsync(
//                 new List<RiskFactorConfiguration>
//                 {
//                     new()
//                     {
//                         Id = Guid.NewGuid(),
//                         RiskLevel = RiskLevel.City,
//                         ReferenceID = ids.City,
//                         AdjustementPercentage = 1.0005m
//                     }
//                 });

//         var (finalPremium, adjustments) = await _sut.CalculateAsync(building, 1000m, DateOnly.FromDateTime(DateTime.UtcNow), CancellationToken.None);

//         adjustments.Should().ContainSingle().Which.Amount.Should().Be(10.01m);
//         finalPremium.Should().Be(1010.01m);
//     }

//     [Fact]
//     public async Task CalculateAsync_ShouldPreventNegativeFinalPremium()
//     {
//         var ids = (City: Guid.NewGuid(), County: Guid.NewGuid(), Country: Guid.NewGuid());
//         var building = CreateBuilding(ids.City, ids.County, ids.Country, BuildingType.Residential);

//         _fees.Setup(x => x.GetActiveFeeConfigurationsAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(new List<FeeConfiguration>());

//         _riskFactors.Setup(x => x.GetActiveRiskFactorConfigurationsAsync(It.IsAny<CancellationToken>()))
//             .ReturnsAsync(
//                 new List<RiskFactorConfiguration>
//                 {
//                     new()
//                     {
//                         Id = Guid.NewGuid(),
//                         RiskLevel = RiskLevel.City,
//                         ReferenceID = ids.City,
//                         AdjustementPercentage = -250m
//                     }
//                 });

//         var (finalPremium, adjustments) = await _sut.CalculateAsync(building, 100m, DateOnly.FromDateTime(DateTime.UtcNow), CancellationToken.None);

//         adjustments.Should().ContainSingle().Which.Amount.Should().Be(-250m);
//         finalPremium.Should().Be(0m);
//     }

//     private static Building CreateBuilding(Guid cityId, Guid countyId, Guid countryId, BuildingType type)
//     {
//         var country = new Country { Id = countryId, Name = "Test Country" };
//         var county = new County { Id = countyId, CountryId = countryId, Name = "Test County", Country = country };
//         var city = new City { Id = cityId, CountyId = countyId, Name = "Test City", County = county };
//         var address = new Address
//         {
//             Id = Guid.NewGuid(),
//             Street = "Test",
//             Number = "1",
//             CityId = cityId,
//             City = city
//         };

//         return new Building
//         {
//             Id = Guid.NewGuid(),
//             ClientId = Guid.NewGuid(),
//             AddressId = address.Id,
//             BuildingType = type,
//             Address = address
//         };
//     }
// }