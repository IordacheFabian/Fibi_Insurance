using System;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Context.Configurations.Converters;

namespace Persistence.Context.Configurations.Policies;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PolicyNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.PolicyNumber)
            .IsUnique();

        builder.Property(x => x.ClientId).IsRequired();
        builder.Property(x => x.BuildingId).IsRequired();
        builder.Property(x => x.CurrencyId).IsRequired();

        builder.Property(x => x.PolicyStatus)
            .IsRequired();
        
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

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);
        
        builder.HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Building)
            .WithMany()
            .HasForeignKey(x => x.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.Currency)
            .WithMany(x => x.Policies)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.PolicyAdjustements)
            .WithOne(x => x.Policy)
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
