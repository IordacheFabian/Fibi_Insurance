using Application.Clients.Commands;
using Application.Clients.DTOs;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using AutoMapper;
using Domain.Models.Clients;
using FluentAssertions;
using Moq;

namespace Application.Tests.Clients;

public class CreateClientHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CreateClient.Handler _handler;

    public CreateClientHandlerTests()
    {
        _handler = new CreateClient.Handler(_clientRepository.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldPersistClient_WhenRequestValid()
    {
        var dto = new CreateClientDto { Name = "John", Email = "john@test.com", IdentificationNumber = "1234567890123", PhoneNumber = "0712345678" };
        var entity = new Client { Id = Guid.NewGuid(), Name = dto.Name };

        _mapper.Setup(x => x.Map<Client>(dto)).Returns(entity);
        _clientRepository.Setup(x => x.AddClientAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _clientRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(new CreateClient.Command { ClientDto = dto }, CancellationToken.None);

        result.Should().Be(entity.Id.ToString());
        _clientRepository.Verify(x => x.AddClientAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _clientRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenSaveFails()
    {
        var dto = new CreateClientDto { Name = "Broken", IdentificationNumber = "1234567890123" };
        var entity = new Client { Id = Guid.NewGuid() };

        _mapper.Setup(x => x.Map<Client>(dto)).Returns(entity);
        _clientRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await _handler.Handle(new CreateClient.Command { ClientDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
