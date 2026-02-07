using System;
using Domain.Models.Brokers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.Brokers;

public class BrokerConfiguration : IEntityTypeConfiguration<Broker>
{
    public void Configure(EntityTypeBuilder<Broker> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BrokerCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(b => b.BrokerCode)
            .IsUnique();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(b => b.BrokerStatus)
            .IsRequired();

        builder.Property(b => b.CommissionPrecentage)
            .HasPrecision(5, 2);

        builder.HasMany(b => b.Policies)
            .WithOne(p => p.Broker)
            .HasForeignKey(p => p.BrokerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
