using Application.Brokers.Command;
using Application.Brokers.DTOs.Request;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Brokers;
using FluentAssertions;
using Moq;

namespace Application.Tests.Brokers;

public class CreateBrokerHandlerTests
{
    private readonly Mock<IBrokerRepository> _brokerRepository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CreateBroker.Handler _handler;

    public CreateBrokerHandlerTests()
    {
        _handler = new CreateBroker.Handler(_brokerRepository.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldRejectDuplicateBrokerCode()
    {
        var dto = new CreateBrokerDto { BrokerCode = " brk-001 ", Name = "Test" };
        _brokerRepository.Setup(x => x.BrokerCodeExistsAsync("BRK-001", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = async () => await _handler.Handle(new CreateBroker.Command { CreateBrokerDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        _brokerRepository.Verify(x => x.BrokerCodeExistsAsync("BRK-001", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPersistBroker_WhenCodeAvailable()
    {
        var dto = new CreateBrokerDto { BrokerCode = " brk-002 ", Name = "Broker" };
        var entity = new Broker { Id = Guid.NewGuid(), BrokerCode = "BRK-002" };
        var responseDto = new CreateBrokerDto { BrokerCode = entity.BrokerCode, Name = entity.Name };

        _brokerRepository.Setup(x => x.BrokerCodeExistsAsync("BRK-002", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mapper.Setup(x => x.Map<Broker>(dto)).Returns(entity);
        _mapper.Setup(x => x.Map<CreateBrokerDto>(entity)).Returns(responseDto);
        _brokerRepository.Setup(x => x.CreateBrokerAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _brokerRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(new CreateBroker.Command { CreateBrokerDto = dto }, CancellationToken.None);

        result.Should().Be(responseDto);
        _brokerRepository.Verify(x => x.CreateBrokerAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenSaveFails()
    {
        var dto = new CreateBrokerDto { BrokerCode = "brk-003", Name = "Test" };
        var entity = new Broker { Id = Guid.NewGuid() };

        _brokerRepository.Setup(x => x.BrokerCodeExistsAsync("BRK-003", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mapper.Setup(x => x.Map<Broker>(dto)).Returns(entity);
        _brokerRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await _handler.Handle(new CreateBroker.Command { CreateBrokerDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
