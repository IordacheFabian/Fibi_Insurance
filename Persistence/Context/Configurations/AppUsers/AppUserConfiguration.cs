using System;
using Domain.Models.AppUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Configurations.AppUsers;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.isActive) 
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Broker)
            .WithMany()
            .HasForeignKey(x => x.BrokerId)
            .OnDelete(DeleteBehavior.Restrict);
    
    }
}
