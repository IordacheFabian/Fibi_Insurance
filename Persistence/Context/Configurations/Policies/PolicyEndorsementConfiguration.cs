using System;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Context.Configurations.Converters;

namespace Persistence.Context.Configurations.Policies;

public class PolicyEndorsementConfiguration : IEntityTypeConfiguration<PolicyEndorsement>
{
    public void Configure(EntityTypeBuilder<PolicyEndorsement> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EffectiveDate)
            .HasConversion(DateOnlyConverters.DateOnlyToStringConverter)
            .Metadata.SetValueComparer(DateOnlyConverters.DateOnlyComparer);
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
