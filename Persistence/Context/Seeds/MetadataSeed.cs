using System;
using Domain.Models.Metadatas;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context.Seeds;

public class MetadataSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var ronId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var eurId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        modelBuilder.Entity<Currency>().HasData(
            new Currency
            {
                Id = ronId,
                Code = "RON",
                Name = "Romanian Leu",
                ExchangeRateToBase = 1m
            },
            new Currency
            {
                Id = eurId,
                Code = "EUR",
                Name = "Euro",
                ExchangeRateToBase = 4.95m
            }
        );

        modelBuilder.Entity<FeeConfiguration>().HasData(
            new FeeConfiguration
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Default broker commission",
                FeeType = FeeType.BrokerComission,
                Percentage = 0.10m,
                EffectiveFrom = new DateOnly(2026, 1, 1),
                EffectiveTo = null,
                IsActive = true
            }
        );

        var cityId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<RiskFactorConfiguration>().HasData(
            new RiskFactorConfiguration
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                RiskLevel = RiskLevel.City,
                ReferenceID = cityId,
                BuildingType = null,
                AdjustementPercentage = 0.05m,
                IsActive = true
            }
        );
        
    }
}
