using Application.Clients.DTOs;
using Application.Clients.DTOs.Response;
using Application.Core;
using AutoMapper;
using Domain.Models.Clients;
using FluentAssertions;

namespace Application.Tests.Core;

public class MappingProfilesTests
{
    private readonly MapperConfiguration _configuration;

    public MappingProfilesTests()
    {
        _configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>());
    }

    [Fact]
    public void MappingProfiles_ShouldBeValid()
    {
        _configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void ClientSearchMapping_ShouldMaskIdentificationNumber()
    {
        var mapper = _configuration.CreateMapper();
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            IdentificationNumber = "1234567890123"
        };

        var dto = mapper.Map<ClientSearchDto>(client);

        dto.IdentificationMasked.Should().Be("*********0123");
    }
}
