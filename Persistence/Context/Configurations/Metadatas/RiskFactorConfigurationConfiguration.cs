using System;
using Domain.Models.Metadatas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.Metadatas;

public class RiskFactorConfigurationConfiguration : IEntityTypeConfiguration<RiskFactorConfiguration>
{
    public void Configure(EntityTypeBuilder<RiskFactorConfiguration> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RiskLevel)
            .IsRequired();
        
        builder.Property(x => x.ReferenceID);

        builder.Property(x => x.BuildingType);

        builder.Property(x => x.AdjustementPercentage)
            .HasPrecision(9, 6);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => new { x.RiskLevel, x.ReferenceID });

        builder.HasIndex(x => new { x.RiskLevel, x.BuildingType });
            
    }
}
