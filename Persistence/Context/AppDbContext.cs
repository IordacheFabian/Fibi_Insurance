using System;
using Domain.Models;
using Domain.Models.AppUsers;
using Domain.Models.Brokers;
using Domain.Models.Buildings;
using Domain.Models.Claims;
using Domain.Models.Clients;
using Domain.Models.Geography.Address;
using Domain.Models.Metadatas;
using Domain.Models.Payments;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Seeds;

namespace Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Building> Buildings { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<Broker> Brokers { get; set; } = null!;

    public DbSet<City> Cities { get; set; } = null!;
    public DbSet<County> Counties { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;

    public DbSet<Policy> Policies { get; set; } = null!;
    public DbSet<PolicyAdjustment> PolicyAdjustments { get; set; } = null!;
    public DbSet<PolicyVersion> PolicyVersions { get; set; } = null!;
    public DbSet<PolicyEndorsement> PolicyEndorsements { get; set; } = null!;

    public DbSet<Currency> Currencies { get; set; } = null!;
    public DbSet<RiskFactorConfiguration> RiskFactorConfigurations { get; set; } = null!;
    public DbSet<FeeConfiguration> FeeConfigurations { get; set; } = null!;

    public DbSet<AppUser> Users { get; set; } = null!;

    public DbSet<Payment> Payments { get; set; } = null!;

    public DbSet<Claim> Claims { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        GeographySeed.Seed(modelBuilder);
        MetadataSeed.Seed(modelBuilder);
        BrokerSeed.Seed(modelBuilder);
    }
}
