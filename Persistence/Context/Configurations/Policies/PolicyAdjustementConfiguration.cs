using System;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.Policies;

public class PolicyAdjustmentConfiguration : IEntityTypeConfiguration<PolicyAdjustment>
{
    public void Configure(EntityTypeBuilder<PolicyAdjustment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PolicyVersionId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AdjustmentType)
            .IsRequired();

        builder.Property(x => x.Percentage)
            .HasPrecision(9, 6);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.HasIndex(x => new { x.PolicyVersionId, x.AdjustmentType });
            
    }
}
