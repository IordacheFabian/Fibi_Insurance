using System;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Context.Configurations.Converters;

namespace Persistence.Context.Configurations.Policies;

public class PolicyVersionConfiguration : IEntityTypeConfiguration<PolicyVersion>
{
    public void Configure(EntityTypeBuilder<PolicyVersion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StartDate)
            .HasConversion(DateOnlyConverters.DateOnlyToStringConverter)
            .Metadata.SetValueComparer(DateOnlyConverters.DateOnlyComparer);

        builder.Property(x => x.EndDate)
            .HasConversion(DateOnlyConverters.DateOnlyToStringConverter)
            .Metadata.SetValueComparer(DateOnlyConverters.DateOnlyComparer);

        builder.Property(x => x.BasePremium)
            .HasPrecision(18, 2);

        builder.Property(x => x.FinalPremium)
            .HasPrecision(18, 2);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
