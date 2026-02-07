using System;
using Domain.Models.Metadatas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Context.Configurations.Converters;

namespace Persistence.Context.Configurations.Metadatas;

public class FeeConfigurationConfiguration : IEntityTypeConfiguration<FeeConfiguration>
{
    public void Configure(EntityTypeBuilder<FeeConfiguration> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(x => x.FeeType)
            .IsRequired();
        
        builder.Property(x => x.Percentage)
            .HasPrecision(9, 6);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .HasConversion(DateOnlyConverters.DateOnlyToStringConverter)
            .Metadata.SetValueComparer(DateOnlyConverters.DateOnlyComparer);

        builder.Property(x => x.EffectiveTo)
            .HasConversion(DateOnlyConverters.NullableDateonlyToStringConverter)
            .Metadata.SetValueComparer(DateOnlyConverters.NullableDateOnlyConverter);

        builder.HasIndex(x => new { x.FeeType, x.EffectiveFrom });


    }
}
