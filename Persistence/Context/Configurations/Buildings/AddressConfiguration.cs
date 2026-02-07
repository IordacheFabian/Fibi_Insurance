using System;
using Domain.Models.Geography.Address;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.Buildings;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Street)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(x => x.CityId)
            .IsRequired();
        
        builder.HasOne(x => x.City) 
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
