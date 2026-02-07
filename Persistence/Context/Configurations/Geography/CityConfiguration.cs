using System;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.Geography;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CountyId)
            .IsRequired();

        builder.HasOne(x => x.County)
            .WithMany(x => x.Cities)
            .HasForeignKey(x => x.CountyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
