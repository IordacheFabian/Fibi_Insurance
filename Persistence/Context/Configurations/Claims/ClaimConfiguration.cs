using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Models.Claims;
namespace Persistence.Context.Configurations.Claims;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.EstimatedDamage)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(x => x.ApprovedAmount)
            .HasPrecision(18, 2);
        
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Policy)
            .WithMany(x => x.Claims)
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
