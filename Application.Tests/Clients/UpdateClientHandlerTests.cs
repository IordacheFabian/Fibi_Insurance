using Application.Clients.Commands;
using Application.Clients.DTOs;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Clients;
using FluentAssertions;
using Moq;

namespace Application.Tests.Clients;

public class UpdateClientHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly UpdateClient.Handler _handler;

    public UpdateClientHandlerTests()
    {
        _handler = new UpdateClient.Handler(_clientRepository.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateClient_WhenEntityExists()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateClientDto { Name = "Updated", Email = "updated@test.com", PhoneNumber = "0700000000" };
        var entity = new Client { Id = id, Name = "Old" };

        _clientRepository.Setup(x => x.GetClientAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _mapper.Setup(x => x.Map(dto, entity)).Callback(() => entity.Name = dto.Name);
        _clientRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _handler.Handle(new UpdateClient.Command { Id = id, ClientDto = dto }, CancellationToken.None);

        entity.Name.Should().Be("Updated");
        _clientRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenClientMissing()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateClientDto { Name = "Missing" };
        _clientRepository.Setup(x => x.GetClientAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

        var act = async () => await _handler.Handle(new UpdateClient.Command { Id = id, ClientDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Client not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenSaveFails()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateClientDto { Name = "Broken" };
        var entity = new Client { Id = id };

        _clientRepository.Setup(x => x.GetClientAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _clientRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await _handler.Handle(new UpdateClient.Command { Id = id, ClientDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
