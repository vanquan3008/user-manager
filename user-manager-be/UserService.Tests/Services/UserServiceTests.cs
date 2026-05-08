using UserService.Application.Mapping;
using UserService.Domain.Entities;
using UserService.Domain.Events;
using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;

namespace UserService.Tests.Services;

public class UserServiceTests
{
    private static readonly DateTime UtcNow = new(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidInput_CreatesUserAggregate()
    {
        var id = Guid.NewGuid();

        var user = CreateUser(id);

        user.Id.Should().Be(id);
        user.Username.Value.Should().Be("user1");
        user.Email.Value.Should().Be("u@test.com");
        user.Name.DisplayName.Should().Be("First Last");
        user.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WhenUserIsUnder13_ThrowsDomainException()
    {
        var dateOfBirth = DateOnly.FromDateTime(UtcNow).AddYears(-12);

        var act = () => User.Create(
            Guid.NewGuid(),
            Username.Create("user1"),
            Email.Create("u@test.com"),
            PersonName.Create("First", "Last"),
            dateOfBirth,
            UtcNow);

        act.Should().Throw<InvalidDomainObjectException>()
            .WithMessage("*at least 13 years old*");
    }

    [Fact]
    public void UpdateProfile_ChangesNameAndProfileFields()
    {
        var user = CreateUser();

        user.UpdateProfile(
            PersonName.Create("New", "Name", "Display"),
            avatarUrl: " https://cdn.example.com/avatar.png ",
            bio: " Bio ");

        user.Name.DisplayName.Should().Be("Display");
        user.AvatarUrl.Should().Be("https://cdn.example.com/avatar.png");
        user.Bio.Should().Be("Bio");
    }

    [Fact]
    public void VerifyEmail_MarksEmailVerifiedAndAddsDomainEvent()
    {
        var user = CreateUser();

        user.VerifyEmail();

        user.IsEmailVerified.Should().BeTrue();
        user.DomainEvents.Should().ContainSingle(e => e is EmailVerifiedEvent);
    }

    [Fact]
    public void FromCreateRequest_MapsDtoToDomainUser()
    {
        var request = new CreateUserRequest(
            "u@test.com",
            "user1",
            "First",
            "Last",
            new DateOnly(1990, 1, 1),
            "Display");

        var user = UserDtoMapper.FromCreateRequest(request, UtcNow);

        user.Email.Value.Should().Be(request.Email);
        user.Username.Value.Should().Be(request.Username);
        user.Name.DisplayName.Should().Be("Display");
    }

    [Fact]
    public void ToDto_MapsDomainUserToUserDto()
    {
        var user = CreateUser();

        var dto = UserDtoMapper.ToDto(user);

        dto.Id.Should().Be(user.Id.ToString());
        dto.Email.Should().Be(user.Email.Value);
        dto.Username.Should().Be(user.Username.Value);
        dto.DisplayName.Should().Be(user.Name.DisplayName);
    }

    private static User CreateUser(Guid? id = null)
        => User.Create(
            id ?? Guid.NewGuid(),
            Username.Create("user1"),
            Email.Create("u@test.com"),
            PersonName.Create("First", "Last"),
            new DateOnly(1990, 1, 1),
            UtcNow);
}
