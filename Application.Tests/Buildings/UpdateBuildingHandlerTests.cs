using Application.Buildings.Commands;
using Application.Buildings.DTOs.Request;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Buildings;
using FluentAssertions;
using Moq;

namespace Application.Tests.Buildings;

public class UpdateBuildingHandlerTests
{
    private readonly Mock<IBuildingRepository> _buildingRepository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly UpdateBuilding.Handler _handler;

    public UpdateBuildingHandlerTests()
    {
        _handler = new UpdateBuilding.Handler(_buildingRepository.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateBuilding_WhenEntityExists()
    {
        var dto = new UpdateBuildingDto { Id = Guid.NewGuid(), ConstructionYear = 2005 };
        var entity = new Building { Id = dto.Id, ConstructionYear = 1998 };

        _buildingRepository.Setup(x => x.GetBuildingAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _mapper.Setup(x => x.Map(dto, entity)).Callback(() => entity.ConstructionYear = dto.ConstructionYear);
        _buildingRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _handler.Handle(new UpdateBuilding.Command { BuildingDto = dto }, CancellationToken.None);

        entity.ConstructionYear.Should().Be(2005);
        _buildingRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenBuildingMissing()
    {
        var dto = new UpdateBuildingDto { Id = Guid.NewGuid() };
        _buildingRepository.Setup(x => x.GetBuildingAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Building?)null);

        var act = async () => await _handler.Handle(new UpdateBuilding.Command { BuildingDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenSaveFails()
    {
        var dto = new UpdateBuildingDto { Id = Guid.NewGuid() };
        var entity = new Building { Id = dto.Id };

        _buildingRepository.Setup(x => x.GetBuildingAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _buildingRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await _handler.Handle(new UpdateBuilding.Command { BuildingDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
