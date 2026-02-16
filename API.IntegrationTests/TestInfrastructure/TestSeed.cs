using System;
using Domain.Models;
using Domain.Models.Brokers;
using Domain.Models.Metadatas;
using Persistence.Context;


namespace API.IntegrationTests.TestInfrastructure;

public class TestSeed
{
    private static readonly Guid CountryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid CountyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid CityId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid CurrencyId = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");

    public static void Seed(AppDbContext db)
    {
        if (!db.Countries.Any())
        {
            db.Countries.Add(new Country { Id = CountryId, Name = "Romania" });
            db.Counties.Add(new County { Id = CountyId, Name = "Cluj", CountryId = CountryId });
            db.Cities.Add(new City { Id = CityId, Name = "Cluj-Napoca", CountyId = CountyId });
        }

        if (!db.Currencies.Any())
        {
            db.Currencies.Add(new Currency
            {
                Id = CurrencyId,
                Code = "RON",
                Name = "Romanian Leu",
                ExchangeRateToBase = 1m,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!db.FeeConfigurations.Any())
        {
            db.FeeConfigurations.Add(new FeeConfiguration
            {
                Id = Guid.NewGuid(),
                Name = "Admin fee",
                FeeType = FeeType.AdminFee,
                Percentage = 5m,
                EffectiveFrom = new DateOnly(2020, 1, 1),
                EffectiveTo = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!db.Brokers.Any())
        {
            db.Brokers.Add(new Broker
            {
                Id = Guid.NewGuid(),
                BrokerCode = "BRK-SEED",
                Name = "Seeded Broker",
                Email = "seeded.broker@test.local",
                PhoneNumber = "+40123123123",
                BrokerStatus = BrokerStatus.Active,
                CommissionPercentage = 10m,
                CreatedAt = DateTime.UtcNow
            });
        }

        db.SaveChanges();
    }
}
