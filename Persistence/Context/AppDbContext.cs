using System;
using Domain.Models;
using Domain.Models.Buildings;
using Domain.Models.Clients;
using Domain.Models.Geography.Address;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Address> Addresses { get; set; }

    public DbSet<City> Cities { get; set; }
    public DbSet<County> Counties { get; set; }
    public DbSet<Country> Countries { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Client>()
            .HasMany(x => x.Buildings)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId);

        builder.Entity<Building>()
            .HasOne(x => x.Address)
            .WithOne(x => x.Building)
            .HasForeignKey<Building>(x => x.AddressId);

        builder.Entity<Address>()
            .HasOne(x => x.City)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.CityId);

        builder.Entity<County>()
            .HasOne(x => x.Country)
            .WithMany(x => x.Counties)
            .HasForeignKey(x => x.CountryId);

        builder.Entity<City>()
            .HasOne(x => x.County)
            .WithMany(x => x.Cities)
            .HasForeignKey(x => x.CountyId);

        var countryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var countyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var cityId = Guid.Parse("33333333-3333-3333-3333-333333333333");


        builder.Entity<Country>().HasData(new Country
        {
            Id = countryId,
            Name = "Romania"
        });

        builder.Entity<County>().HasData(new County
        {
            Id = countyId,
            CountryId = countryId,
            Name = "Bucharest"
        });

        builder.Entity<City>().HasData(new City
        {
            Id = cityId,
            CountyId = countyId,
            Name = "Bucharest"
        });


    }
}
