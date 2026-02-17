using Application.Core;
using Application.Policies.DTOs.Command;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Policies;
using FluentAssertions;
using Moq;

namespace Application.Tests.Policies;

public class ActivatePolicyHandlerTests
{
    [Fact]
    public async Task Handle_ShouldActivateDraftPolicyAndUpdateTimestamp()
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyStatus = PolicyStatus.Draft,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            ClientId = Guid.NewGuid(),
            BuildingId = Guid.NewGuid(),
            BrokerId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid(),
            BasePremium = 500m,
            FinalPremium = 650m,
            PolicyAdjustements = new List<PolicyAdjustement>(),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForActivationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new ActivatePolicy.Handler(repo.Object);
        var previousUpdatedAt = policy.UpdatedAt;

        await handler.Handle(new ActivatePolicy.Command { PolicyId = policy.Id }, CancellationToken.None);

        policy.PolicyStatus.Should().Be(PolicyStatus.Active);
        policy.UpdatedAt.Should().BeAfter(previousUpdatedAt);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenPolicyIsMissing()
    {
        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForActivationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy?)null);

        var handler = new ActivatePolicy.Handler(repo.Object);

        var act = async () => await handler.Handle(new ActivatePolicy.Command { PolicyId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenPolicyNotDraft()
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyStatus = PolicyStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            ClientId = Guid.NewGuid(),
            BuildingId = Guid.NewGuid(),
            BrokerId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid(),
            BasePremium = 400m,
            FinalPremium = 450m
        };

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForActivationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new ActivatePolicy.Handler(repo.Object);

        var act = async () => await handler.Handle(new ActivatePolicy.Command { PolicyId = policy.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenStartDateIsInFuture()
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyStatus = PolicyStatus.Draft,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            ClientId = Guid.NewGuid(),
            BuildingId = Guid.NewGuid(),
            BrokerId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid(),
            BasePremium = 500m,
            FinalPremium = 600m
        };

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForActivationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new ActivatePolicy.Handler(repo.Object);

        var act = async () => await handler.Handle(new ActivatePolicy.Command { PolicyId = policy.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}