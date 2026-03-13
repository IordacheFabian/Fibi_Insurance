using System.Linq;
using Application.Core;
using Application.Policies.Command;
using Application.Policies.DTOs.Requests;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Policies;
using FluentAssertions;
using Moq;

namespace Application.Tests.Policies;

public class CancelPolicyHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCancelActivePolicy()
    {
        var policy = CreatePolicy();
        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CancelPolicy.Handler(repo.Object);
        var dto = new CancelPolicyDto
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CancellationReason = "Customer request"
        };

        await handler.Handle(new CancelPolicy.Command { PolicyId = policy.Id, CancelPolicyDto = dto }, CancellationToken.None);

        policy.PolicyStatus.Should().Be(PolicyStatus.Cancelled);
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
        var policy = CreatePolicy(p => p.PolicyStatus = PolicyStatus.Draft);

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
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenCancellationDateBeforeStart()
    {
        var futureStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var policy = CreatePolicy(versionConfig: v =>
        {
            v.StartDate = futureStart;
            v.EndDate = futureStart.AddDays(10);
        });

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new CancelPolicy.Handler(repo.Object);
        var dto = new CancelPolicyDto
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CancellationReason = "Too early"
        };

        var act = async () => await handler.Handle(new CancelPolicy.Command { PolicyId = policy.Id, CancelPolicyDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenCancellationDateAfterEnd()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var policy = CreatePolicy(versionConfig: v =>
        {
            v.StartDate = today.AddDays(-2);
            v.EndDate = today.AddDays(1);
        });

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new CancelPolicy.Handler(repo.Object);
        var dto = new CancelPolicyDto
        {
            CancellationDate = policy.PolicyVersions.Single().EndDate.AddDays(1),
            CancellationReason = "Too late"
        };

        var act = async () => await handler.Handle(new CancelPolicy.Command { PolicyId = policy.Id, CancelPolicyDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenReasonMissing()
    {
        var policy = CreatePolicy();

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var handler = new CancelPolicy.Handler(repo.Object);
        var dto = new CancelPolicyDto
        {
            CancellationReason = " ",
            CancellationDate = policy.PolicyVersions.Single().StartDate.AddDays(1)
        };

        var act = async () => await handler.Handle(new CancelPolicy.Command { PolicyId = policy.Id, CancelPolicyDto = dto }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldAllowNullCancellationDate()
    {
        var policy = CreatePolicy();

        var repo = new Mock<IPolicyRepository>();
        repo.Setup(x => x.GetPolicyForCancellationAsync(policy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CancelPolicy.Handler(repo.Object);
        var dto = new CancelPolicyDto
        {
            CancellationReason = "Customer request",
            CancellationDate = null
        };

        await handler.Handle(new CancelPolicy.Command { PolicyId = policy.Id, CancelPolicyDto = dto }, CancellationToken.None);

        policy.PolicyStatus.Should().Be(PolicyStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenActiveVersionMissing()
    {
        var policy = CreatePolicy(versionConfig: v => v.IsActiveVersion = false);

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
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Policy CreatePolicy(Action<Policy>? policyConfig = null, Action<PolicyVersion>? versionConfig = null)
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyNumber = "POL-TEST",
            PolicyStatus = PolicyStatus.Active,
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
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            BasePremium = 1000m,
            FinalPremium = 1200m,
            CurrencyId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "UnitTests",
            IsActiveVersion = true
        };

        policy.PolicyVersionId = activeVersion.Id;
        policy.PolicyVersions = new List<PolicyVersion> { activeVersion };

        versionConfig?.Invoke(activeVersion);
        policyConfig?.Invoke(policy);

        return policy;
    }
}