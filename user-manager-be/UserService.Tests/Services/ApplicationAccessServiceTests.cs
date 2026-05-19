using FluentAssertions;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Tests.Services;

public sealed class ApplicationAccessServiceTests
{
    private readonly Mock<IIdentityService> _identityService = new();
    private readonly Mock<IApplicationMembershipRepository> _memberships = new();
    private readonly Mock<ISubscriptionRepository> _subscriptions = new();

    [Fact]
    public async Task HasAccessAsync_WhenMembershipAndSubscriptionAreActive_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var service = CreateService();

        _identityService
            .Setup(service => service.GetUserRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());
        _memberships
            .Setup(repository => repository.GetByUserAndAppAsync(userId, applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApplicationMembership.Join(Guid.NewGuid(), userId, applicationId, Guid.NewGuid(), DateTime.UtcNow));
        _subscriptions
            .Setup(repository => repository.GetActiveSubscriptionAsync(userId, applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSubscription(userId, applicationId));

        var hasAccess = await service.HasAccessAsync(userId, applicationId);

        hasAccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenUserIsSuperAdmin_ReturnsSystemAccessWithoutRepositoryCalls()
    {
        var userId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var service = CreateService();

        var evaluation = await service.EvaluateAsync(userId, applicationId, new[] { RoleNames.SuperAdmin });

        evaluation.HasAccess.Should().BeTrue();
        evaluation.HasSystemRoleAccess.Should().BeTrue();
        _memberships.Verify(repository => repository.GetByUserAndAppAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _subscriptions.Verify(repository => repository.GetActiveSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private ApplicationAccessService CreateService()
        => new(_identityService.Object, _memberships.Object, _subscriptions.Object);

    private static Subscription CreateSubscription(Guid userId, Guid applicationId)
    {
        var plan = SubscriptionPlan.Create(
            Guid.NewGuid(),
            applicationId,
            PlanName.Create("Starter Plan"),
            PlanDescription.Create("Default plan"),
            Money.Create(0, "USD"),
            Money.Create(0, "USD"));

        return Subscription.AssignPlan(
            Guid.NewGuid(),
            userId,
            applicationId,
            plan,
            Array.Empty<string>(),
            SubscriptionType.Premium,
            DateTime.UtcNow);
    }
}
