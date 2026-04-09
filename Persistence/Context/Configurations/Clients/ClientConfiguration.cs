using System;
using Domain.Models.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.Clients;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IdentificationNumber)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.HasIndex(x => x.IdentificationNumber)
            .IsUnique();

        builder.HasOne(x => x.Broker)
            .WithMany(x => x.Clients)
            .HasForeignKey(x => x.BrokerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.Buildings) 
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
