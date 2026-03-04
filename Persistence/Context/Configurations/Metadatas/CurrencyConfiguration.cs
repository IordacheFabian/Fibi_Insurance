using System;
using Domain.Models.Metadatas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.Metadatas;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(3);

        builder.HasIndex(x => x.Code)
            .IsUnique();
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ExchangeRateToBase)
            .HasPrecision(18, 6);

        // builder.HasMany(x => x.Policies)
        //     .WithOne(x => x.Currency)
        //     .HasForeignKey(x => x.CurrencyId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}
