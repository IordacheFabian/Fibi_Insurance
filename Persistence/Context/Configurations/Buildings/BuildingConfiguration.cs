using System;
using Domain.Models.Buildings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.Buildings;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId)
            .IsRequired();

        builder.Property(x => x.AddressId)
            .IsRequired();

        builder.Property(x => x.CurrencyId)
            .IsRequired();

        builder.HasOne(x => x.Address)
            .WithOne(x => x.Building)
            .HasForeignKey<Building>(x => x.AddressId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.Buildings)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
