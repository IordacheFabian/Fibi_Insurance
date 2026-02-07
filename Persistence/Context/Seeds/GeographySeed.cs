using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context.Seeds;

public static class GeographySeed
{
    public static void Seed(ModelBuilder builder)
    {

        var romaniaId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var bucharestCountyId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var bucharestCityId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var clujCountyId = Guid.Parse("22222222-2222-2222-2222-222222222223");
        var timisCountyId = Guid.Parse("22222222-2222-2222-2222-222222222224");
        var iasiCountyId = Guid.Parse("22222222-2222-2222-2222-222222222225");
        var brasovCountyId = Guid.Parse("22222222-2222-2222-2222-222222222226");

        var clujNapocaCityId = Guid.Parse("33333333-3333-3333-3333-333333333334");
        var florestiCityId = Guid.Parse("33333333-3333-3333-3333-333333333335");

        var timisoaraCityId = Guid.Parse("33333333-3333-3333-3333-333333333336");
        var lugojCityId = Guid.Parse("33333333-3333-3333-3333-333333333337");

        var iasiCityId = Guid.Parse("33333333-3333-3333-3333-333333333338");

        var brasovCityId = Guid.Parse("33333333-3333-3333-3333-333333333339");

        builder.Entity<Country>().HasData(
            new Country { Id = romaniaId, Name = "Romania" }
        );

        builder.Entity<County>().HasData(
            new County { Id = bucharestCountyId, CountryId = romaniaId, Name = "Bucharest" },

            new County { Id = clujCountyId, CountryId = romaniaId, Name = "Cluj" },
            new County { Id = timisCountyId, CountryId = romaniaId, Name = "Timiș" },
            new County { Id = iasiCountyId, CountryId = romaniaId, Name = "Iași" },
            new County { Id = brasovCountyId, CountryId = romaniaId, Name = "Brașov" }
        );

        builder.Entity<City>().HasData(
            new City { Id = bucharestCityId, CountyId = bucharestCountyId, Name = "Bucharest" },

            // Cluj
            new City { Id = clujNapocaCityId, CountyId = clujCountyId, Name = "Cluj-Napoca" },
            new City { Id = florestiCityId, CountyId = clujCountyId, Name = "Florești" },

            // Timiș
            new City { Id = timisoaraCityId, CountyId = timisCountyId, Name = "Timișoara" },
            new City { Id = lugojCityId, CountyId = timisCountyId, Name = "Lugoj" },

            // Iași
            new City { Id = iasiCityId, CountyId = iasiCountyId, Name = "Iași" },

            // Brașov
            new City { Id = brasovCityId, CountyId = brasovCountyId, Name = "Brașov" }
        );
    }
}
