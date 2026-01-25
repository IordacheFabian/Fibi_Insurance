using System;
using Application.Addresses.DTOs;
using Application.Buildings.Commands;
using Application.Buildings.DTOs.Request;
using Application.Clients.DTOs;
using Application.Core;
using Application.Tests.TestHelpers;
using AutoMapper;
using Domain.Models;
using Domain.Models.Clients;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Buildings;

public class CreateBuildingsTests
{
    private readonly IMapper _mapper = TestMapper.Instance;

    [Fact]
    public async Task Handle_Should_Create_Building_For_Existing_Client()
    {
        using var context = TestDbContextFactory.Create();

        var country = new Country { Id = Guid.NewGuid(), Name = "Romania" };
        var county = new County { Id = Guid.NewGuid(), Name = "Bucharest", CountryId = country.Id, Country = country };
        var city = new City { Id = Guid.NewGuid(), Name = "Bucharest", CountyId = county.Id, County = county };

        context.Countries.Add(country);
        context.Counties.Add(county);   
        context.Cities.Add(city);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Test Client",
            ClientType = (Domain.Models.Clients.Type)ClientType.Individual,
            IdentificationNumber = "1234567890123",
            Email = "test@test.com",
            PhoneNumber = "0712345678"
        };
        context.Clients.Add(client);

        await context.SaveChangesAsync();   

        var handler = new CreateBuilding.Handler(context, _mapper);

        var dto = new CreateBuildingDto
        {
            ConstructionYear = 2000,
            BuildingType = BuildingType.Residential,
            NumberOfFloors = 3,
            SurfaceArea = 184,
            InsuredValue = 12000,
            Address = new CreateAddressDto
            {
                Street = "Test Street",
                Number = "10",
                CityId = city.Id
            }
        };

        var command = new CreateBuilding.Command
        {
            ClientId = client.Id,
            BuildingDto = dto
        };

        var buildingIdString = await handler.Handle(command, CancellationToken.None);
        var buildingId = Guid.Parse(buildingIdString);

        var building = await context.Buildings
            .Include(b => b.Address)
            .FirstOrDefaultAsync(b => b.Id == buildingId);


        building.Should().NotBeNull();
        building!.ClientId.Should().Be(client.Id);
        building.Address.Should().NotBeNull();
        building.Address.CityId.Should().Be(city.Id);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_For_NonExisting_Client()
    {
        using var context = TestDbContextFactory.Create();

        var handler = new CreateBuilding.Handler(context, _mapper);

        var dto = new CreateBuildingDto
        {
            ConstructionYear = 2000,
            BuildingType = BuildingType.Residential,
            NumberOfFloors = 3,
            SurfaceArea = 184,
            InsuredValue = 12000,
            Address = new CreateAddressDto
            {
                Street = "Test",
                Number = "1",
                CityId = Guid.NewGuid()
            }
        };

        var command = new CreateBuilding.Command
        {
            ClientId = Guid.NewGuid(),
            BuildingDto = dto
        };

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
