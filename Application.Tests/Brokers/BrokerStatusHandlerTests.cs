using Application.Brokers.Command;
using Application.Brokers.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Brokers;
using FluentAssertions;
using Moq;

namespace Application.Tests.Brokers;

public class BrokerStatusHandlerTests
{
    private readonly Mock<IBrokerRepository> _brokerRepository = new();
    private readonly Mock<IMapper> _mapper = new();

    [Fact]
    public async Task Activate_ShouldThrowNotFound_WhenBrokerMissing()
    {
        var handler = new ActivateBroker.Handler(_brokerRepository.Object, _mapper.Object);
        _brokerRepository.Setup(x => x.GetBrokerForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Broker?)null);

        var act = async () => await handler.Handle(new ActivateBroker.Command { Id = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Activate_ShouldPersistStatusChange_WhenBrokerInactive()
    {
        var broker = new Broker { Id = Guid.NewGuid(), BrokerStatus = BrokerStatus.Inactive };
        var handler = new ActivateBroker.Handler(_brokerRepository.Object, _mapper.Object);
        _brokerRepository.Setup(x => x.GetBrokerForUpdateAsync(broker.Id, It.IsAny<CancellationToken>())).ReturnsAsync(broker);
        _brokerRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mapper.Setup(x => x.Map<BrokerDto>(broker)).Returns(new BrokerDto { Id = broker.Id });

        var result = await handler.Handle(new ActivateBroker.Command { Id = broker.Id }, CancellationToken.None);

        broker.BrokerStatus.Should().Be(BrokerStatus.Active);
        result.Id.Should().Be(broker.Id);
        _brokerRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Activate_ShouldSkipSave_WhenAlreadyActive()
    {
        var broker = new Broker { Id = Guid.NewGuid(), BrokerStatus = BrokerStatus.Active };
        var handler = new ActivateBroker.Handler(_brokerRepository.Object, _mapper.Object);
        _brokerRepository.Setup(x => x.GetBrokerForUpdateAsync(broker.Id, It.IsAny<CancellationToken>())).ReturnsAsync(broker);
        _mapper.Setup(x => x.Map<BrokerDto>(broker)).Returns(new BrokerDto { Id = broker.Id });

        await handler.Handle(new ActivateBroker.Command { Id = broker.Id }, CancellationToken.None);

        _brokerRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Deactivate_ShouldThrowNotFound_WhenBrokerMissing()
    {
        var handler = new DeactivateBroker.Handler(_brokerRepository.Object, _mapper.Object);
        _brokerRepository.Setup(x => x.GetBrokerForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Broker?)null);

        var act = async () => await handler.Handle(new DeactivateBroker.Command { Id = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Deactivate_ShouldPersistStatusChange_WhenBrokerActive()
    {
        var broker = new Broker { Id = Guid.NewGuid(), BrokerStatus = BrokerStatus.Active };
        var handler = new DeactivateBroker.Handler(_brokerRepository.Object, _mapper.Object);
        _brokerRepository.Setup(x => x.GetBrokerForUpdateAsync(broker.Id, It.IsAny<CancellationToken>())).ReturnsAsync(broker);
        _brokerRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mapper.Setup(x => x.Map<BrokerDto>(broker)).Returns(new BrokerDto { Id = broker.Id });

        await handler.Handle(new DeactivateBroker.Command { Id = broker.Id }, CancellationToken.None);

        broker.BrokerStatus.Should().Be(BrokerStatus.Inactive);
        _brokerRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deactivate_ShouldSkipSave_WhenAlreadyInactive()
    {
        var broker = new Broker { Id = Guid.NewGuid(), BrokerStatus = BrokerStatus.Inactive };
        var handler = new DeactivateBroker.Handler(_brokerRepository.Object, _mapper.Object);
        _brokerRepository.Setup(x => x.GetBrokerForUpdateAsync(broker.Id, It.IsAny<CancellationToken>())).ReturnsAsync(broker);
        _mapper.Setup(x => x.Map<BrokerDto>(broker)).Returns(new BrokerDto { Id = broker.Id });

        await handler.Handle(new DeactivateBroker.Command { Id = broker.Id }, CancellationToken.None);

        _brokerRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
