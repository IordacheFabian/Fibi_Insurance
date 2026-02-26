using Application.Core;
using Application.Policies.DTOs.Command;
using Application.Policies.DTOs.Requests;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Policies;
using FluentAssertions;
using Moq;

namespace Application.Tests.Policies;

public class CancelPolicyHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCancelActivePolicyAndPersistFields()
    {
        var effectiveDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyStatus = PolicyStatus.Active,
            StartDate = effectiveDate.AddDays(-5),
            EndDate = effectiveDate.AddDays(5),
            ClientId = Guid.NewGuid(),
            BuildingId = Guid.NewGuid(),
            BrokerId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid(),
            BasePremium = 1000m,
            FinalPremium = 1200m,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-2),
            PolicyAdjustements = new List<PolicyAdjustement>()
        };

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CancelPolicy.Handler(repo.Object);
        var dto = new CancelPolicyDto
        {
            CancellationDate = effectiveDate,
            CancellationReason = "Customer request"
        };

        var previousUpdatedAt = policy.UpdatedAt;

        await handler.Handle(new CancelPolicy.Command { PolicyId = policy.Id, CancelPolicyDto = dto }, CancellationToken.None);

        policy.PolicyStatus.Should().Be(PolicyStatus.Cancelled);
        policy.CancelledAt.Should().Be(effectiveDate);
        policy.CancellationReason.Should().Be("Customer request");
        policy.UpdatedAt.Should().BeAfter(previousUpdatedAt);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenPolicyMissing()
    {
        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy?)null);

        var handler = new CancelPolicy.Handler(repo.Object);

        var act = async () => await handler.Handle(new CancelPolicy.Command
        {
            PolicyId = Guid.NewGuid(),
            CancelPolicyDto = new CancelPolicyDto { CancellationReason = "reason" }
        }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenPolicyNotActive()
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyStatus = PolicyStatus.Draft,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
        };

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new CancelPolicy.Handler(repo.Object);

        var act = async () => await handler.Handle(new CancelPolicy.Command
        {
            PolicyId = policy.Id,
            CancelPolicyDto = new CancelPolicyDto { CancellationReason = "reason" }
        }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenCancellationDateOutsidePolicy()
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyStatus = PolicyStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
        };

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new CancelPolicy.Handler(repo.Object);
        var dto = new CancelPolicyDto
        {
            CancellationDate = policy.StartDate.AddDays(-1),
            CancellationReason = "Too early"
        };

        var act = async () => await handler.Handle(new CancelPolicy.Command { PolicyId = policy.Id, CancelPolicyDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenReasonMissing()
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyStatus = PolicyStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))
        };

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new CancelPolicy.Handler(repo.Object);
        var dto = new CancelPolicyDto
        {
            CancellationReason = " ",
            CancellationDate = policy.StartDate.AddDays(1)
        };

        var act = async () => await handler.Handle(new CancelPolicy.Command { PolicyId = policy.Id, CancelPolicyDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldDefaultCancellationDate_WhenNotProvided()
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyStatus = PolicyStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            ClientId = Guid.NewGuid(),
            BuildingId = Guid.NewGuid(),
            BrokerId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid(),
            BasePremium = 100m,
            FinalPremium = 120m
        };

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new CancelPolicy.Handler(repo.Object);
        var dto = new CancelPolicyDto
        {
            CancellationReason = "Customer request",
            CancellationDate = null
        };

        var expectedDate = DateOnly.FromDateTime(DateTime.UtcNow);

        await handler.Handle(new CancelPolicy.Command { PolicyId = policy.Id, CancelPolicyDto = dto }, CancellationToken.None);

        policy.CancelledAt.Should().Be(expectedDate);
    }
}