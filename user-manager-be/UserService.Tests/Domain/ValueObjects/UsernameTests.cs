using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;

namespace UserService.Tests.Domain.ValueObjects;

public class UsernameTests
{
    [Theory]
    [InlineData("john_doe")]
    [InlineData("User123")]
    [InlineData("test_user_456")]
    [InlineData("abc")]
    [InlineData("UserName_With_30_Chars_Max")]
    public void Create_WithValidUsername_ShouldReturnUsername(string validUsername)
    {
        // Act
        var username = Username.Create(validUsername);

        // Assert
        username.Should().NotBeNull();
        username.Value.Should().Be(validUsername);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyUsername_ShouldThrowException(string? emptyUsername)
    {
        // Act
        Action act = () => Username.Create(emptyUsername!);

        // Assert
        act.Should().Throw<InvalidDomainObjectException>()
            .WithMessage("*Username cannot be empty*");
    }

    [Theory]
    [InlineData("ab")] // Too short
    [InlineData("ThisUsernameIsWayTooLongAndExceeds30Characters")] // Too long
    public void Create_WithInvalidLength_ShouldThrowException(string invalidUsername)
    {
        // Act
        Action act = () => Username.Create(invalidUsername);

        // Assert
        act.Should().Throw<InvalidDomainObjectException>()
            .WithMessage("*must be 3-30 characters*");
    }

    [Theory]
    [InlineData("user name")] // Space
    [InlineData("user-name")] // Hyphen
    [InlineData("user@name")] // Special char
    [InlineData("user.name")] // Dot
    public void Create_WithInvalidCharacters_ShouldThrowException(string invalidUsername)
    {
        // Act
        Action act = () => Username.Create(invalidUsername);

        // Assert
        act.Should().Throw<InvalidDomainObjectException>()
            .WithMessage("*contain only letters, numbers, and underscores*");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var username1 = Username.Create("john_doe");
        var username2 = Username.Create("JOHN_DOE");

        // Act & Assert
        username1.Should().Be(username2);
        (username1 == username2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var username1 = Username.Create("john_doe");
        var username2 = Username.Create("jane_doe");

        // Act & Assert
        username1.Should().NotBe(username2);
        (username1 != username2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var username = Username.Create("john_doe");

        // Act
        string usernameString = username;

        // Assert
        usernameString.Should().Be("john_doe");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var username = Username.Create("john_doe");

        // Act
        var result = username.ToString();

        // Assert
        result.Should().Be("john_doe");
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHash()
    {
        // Arrange
        var username1 = Username.Create("john_doe");
        var username2 = Username.Create("JOHN_DOE");

        // Act & Assert
        username1.GetHashCode().Should().Be(username2.GetHashCode());
    }

    [Theory]
    [InlineData("user123")]
    [InlineData("USER123")]
    [InlineData("User123")]
    public void Equals_IsCaseInsensitive(string value)
    {
        // Arrange
        var username1 = Username.Create("user123");
        var username2 = Username.Create(value);

        // Act & Assert
        username1.Should().Be(username2);
    }
}
