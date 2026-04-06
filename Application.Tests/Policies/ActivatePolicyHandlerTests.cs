using Application.Core;
using Application.Policies.Command;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Policies;
using FluentAssertions;
using Moq;

namespace Application.Tests.Policies;

public class ActivatePolicyHandlerTests
{
    [Fact]
    public async Task Handle_ShouldActivateDraftPolicy()
    {
        var policy = CreatePolicy();

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForActivationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new ActivatePolicy.Handler(repo.Object);

        await handler.Handle(new ActivatePolicy.Command { PolicyId = policy.Id }, CancellationToken.None);

        policy.PolicyStatus.Should().Be(PolicyStatus.Active);
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
        var policy = CreatePolicy(policy => policy.PolicyStatus = PolicyStatus.Active);

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
        var policy = CreatePolicy(version: v => v.StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)));

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForActivationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new ActivatePolicy.Handler(repo.Object);

        var act = async () => await handler.Handle(new ActivatePolicy.Command { PolicyId = policy.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenMandatoryReferencesMissing()
    {
        var policy = CreatePolicy(policy => policy.ClientId = Guid.Empty);

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForActivationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new ActivatePolicy.Handler(repo.Object);

        var act = async () => await handler.Handle(new ActivatePolicy.Command { PolicyId = policy.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenPolicyPeriodInvalid()
    {
        var policy = CreatePolicy(version: v => v.EndDate = v.StartDate);

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForActivationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new ActivatePolicy.Handler(repo.Object);

        var act = async () => await handler.Handle(new ActivatePolicy.Command { PolicyId = policy.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenPremiumValuesInvalid()
    {
        var policy = CreatePolicy(version: v =>
        {
            v.BasePremium = 0m;
            v.FinalPremium = 10m;
        });

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForActivationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new ActivatePolicy.Handler(repo.Object);

        var act = async () => await handler.Handle(new ActivatePolicy.Command { PolicyId = policy.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenActiveVersionMissing()
    {
        var policy = CreatePolicy(version: v => v.IsActiveVersion = false);

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForActivationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new ActivatePolicy.Handler(repo.Object);

        var act = async () => await handler.Handle(new ActivatePolicy.Command { PolicyId = policy.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Policy CreatePolicy(Action<Policy>? policyConfig = null, Action<PolicyVersion>? version = null)
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyNumber = "POL-TEST",
            PolicyStatus = PolicyStatus.Draft,
            BrokerId = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            BuildingId = Guid.NewGuid(),
            CurrentVersionNumber = 1
        };

        var activeVersion = new PolicyVersion
        {
            Id = Guid.NewGuid(),
            PolicyId = policy.Id,
            Policy = policy,
            VersionNumber = policy.CurrentVersionNumber,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            BasePremium = 500m,
            FinalPremium = 650m,
            CurrencyId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            CreatedBy = "UnitTests",
            IsActiveVersion = true
        };

        policy.PolicyVersionId = activeVersion.Id;
        policy.PolicyVersions = new List<PolicyVersion> { activeVersion };

        version?.Invoke(activeVersion);
        policyConfig?.Invoke(policy);

        return policy;
    }
}