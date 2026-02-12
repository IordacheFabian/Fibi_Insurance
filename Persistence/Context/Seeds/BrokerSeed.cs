using System;
using Domain.Models.Brokers;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context.Seeds;

public class BrokerSeed
{
    public static readonly Guid DefaultBrokerId =
            Guid.Parse("99999999-9999-9999-9999-999999999999");

    public static void Seed(ModelBuilder builder)
    {
        builder.Entity<Broker>().HasData(
            new Broker
            {
                Id = DefaultBrokerId,
                BrokerCode = "BRK-001",
                Name = "Default Broker",
                Email = "broker@insurance.local",
                PhoneNumber = "0700000000",
                BrokerStatus = BrokerStatus.Active,
                CommissionPercentage = 10.00m
            }
        );
    }
}
