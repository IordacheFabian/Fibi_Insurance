using System;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.Geography;

public class CountyConfiguration : IEntityTypeConfiguration<County>
{
    public void Configure(EntityTypeBuilder<County> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.CountryId)
            .IsRequired();

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Counties)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
