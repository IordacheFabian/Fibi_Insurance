using System;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.Policies;

public class PolicyAdjustementConfiguration : IEntityTypeConfiguration<PolicyAdjustement>
{
    public void Configure(EntityTypeBuilder<PolicyAdjustement> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PolicyId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AdjustementType)
            .IsRequired();

        builder.Property(x => x.Percentage)
            .HasPrecision(9, 6);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.HasIndex(x => new { x.PolicyId, x.AdjustementType });
            
    }
}
