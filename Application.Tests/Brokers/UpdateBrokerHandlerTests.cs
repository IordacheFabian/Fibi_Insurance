using Application.Brokers.Command;
using Application.Brokers.DTOs.Request;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Brokers;
using FluentAssertions;
using Moq;

namespace Application.Tests.Brokers;

public class UpdateBrokerHandlerTests
{
    private readonly Mock<IBrokerRepository> _brokerRepository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly UpdateBroker.Handler _handler;

    public UpdateBrokerHandlerTests()
    {
        _handler = new UpdateBroker.Handler(_brokerRepository.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenBrokerMissing()
    {
        var dto = new UpdateBrokerDto { Name = "Missing" };
        _brokerRepository.Setup(x => x.GetBrokerForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Broker?)null);

        var act = async () => await _handler.Handle(new UpdateBroker.Command { Id = Guid.NewGuid(), UpdateBrokerDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldUpdateBroker_WhenEntityExists()
    {
        var dto = new UpdateBrokerDto { Name = "Updated", Email = "updated@test.com" };
        var entity = new Broker { Id = Guid.NewGuid(), Name = "Old" };

        _brokerRepository.Setup(x => x.GetBrokerForUpdateAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _mapper.Setup(x => x.Map(dto, entity)).Callback(() => entity.Name = dto.Name);
        _mapper.Setup(x => x.Map<UpdateBrokerDto>(entity)).Returns(new UpdateBrokerDto
        {
            Name = dto.Name,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CommissionPercentage = dto.CommissionPercentage
        });
        _brokerRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(new UpdateBroker.Command { Id = entity.Id, UpdateBrokerDto = dto }, CancellationToken.None);

        result.Name.Should().Be("Updated");
        _brokerRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenSaveFails()
    {
        var dto = new UpdateBrokerDto { Name = "Fail" };
        var entity = new Broker { Id = Guid.NewGuid() };

        _brokerRepository.Setup(x => x.GetBrokerForUpdateAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _brokerRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await _handler.Handle(new UpdateBroker.Command { Id = entity.Id, UpdateBrokerDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
