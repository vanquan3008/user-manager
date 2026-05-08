using UserService.Domain.Exceptions;

namespace UserService.Tests.Domain.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void EntityNotFoundException_WithGuid_ShouldSetProperties()
    {
        // Arrange
        var entityName = "User";
        var entityId = Guid.NewGuid();

        // Act
        var exception = new EntityNotFoundException(entityName, entityId);

        // Assert
        exception.EntityName.Should().Be(entityName);
        exception.EntityId.Should().Be(entityId);
        exception.Message.Should().Contain(entityName);
        exception.Message.Should().Contain(entityId.ToString());
    }

    [Fact]
    public void EntityNotFoundException_WithIdentifier_ShouldSetProperties()
    {
        // Arrange
        var entityName = "User";
        var identifier = "john_doe";

        // Act
        var exception = new EntityNotFoundException(entityName, identifier);

        // Assert
        exception.EntityName.Should().Be(entityName);
        exception.Message.Should().Contain(entityName);
        exception.Message.Should().Contain(identifier);
    }

    [Fact]
    public void DuplicateUserException_ShouldSetProperties()
    {
        // Arrange
        var duplicateField = "Email";
        var duplicateValue = "user@example.com";

        // Act
        var exception = new DuplicateUserException(duplicateField, duplicateValue);

        // Assert
        exception.DuplicateField.Should().Be(duplicateField);
        exception.DuplicateValue.Should().Be(duplicateValue);
        exception.Message.Should().Contain(duplicateField);
        exception.Message.Should().Contain(duplicateValue);
    }

    [Fact]
    public void AuthenticationException_ShouldSetProperties()
    {
        // Arrange
        var reason = "Invalid password";

        // Act
        var exception = new AuthenticationException(reason);

        // Assert
        exception.Reason.Should().Be(reason);
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void BusinessRuleValidationException_WithRuleName_ShouldSetProperties()
    {
        // Arrange
        var ruleName = "UserMustBeActive";
        var message = "User account is suspended";

        // Act
        var exception = new BusinessRuleValidationException(ruleName, message);

        // Assert
        exception.RuleName.Should().Be(ruleName);
        exception.Message.Should().Contain(ruleName);
        exception.Message.Should().Contain(message);
    }

    [Fact]
    public void InvalidDomainObjectException_WithPropertyDetails_ShouldSetProperties()
    {
        // Arrange
        var propertyName = "Username";
        var attemptedValue = "ab";
        var message = "Username too short";

        // Act
        var exception = new InvalidDomainObjectException(propertyName, attemptedValue, message);

        // Assert
        exception.PropertyName.Should().Be(propertyName);
        exception.AttemptedValue.Should().Be(attemptedValue);
        exception.Message.Should().Contain(propertyName);
        exception.Message.Should().Contain(message);
    }

    [Fact]
    public void DomainOperationNotAllowedException_ShouldSetProperties()
    {
        // Arrange
        var operation = "DeleteUser";
        var reason = "User has active sessions";

        // Act
        var exception = new DomainOperationNotAllowedException(operation, reason);

        // Assert
        exception.Operation.Should().Be(operation);
        exception.Reason.Should().Be(reason);
        exception.Message.Should().Contain(operation);
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void DomainException_ShouldBeException()
    {
        // Arrange & Act
        var exception = new EntityNotFoundException("Test", Guid.NewGuid());

        // Assert
        exception.Should().BeAssignableTo<Exception>();
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void AuthenticationException_WithMultipleReasons_ShouldCombineMessages()
    {
        // Arrange
        var reasons = new[]
        {
            new AuthenticationException("Invalid email"),
            new AuthenticationException("Account locked"),
            new AuthenticationException("Password expired")
        };

        // Act & Assert
        foreach (var exception in reasons)
        {
            exception.Message.Should().Contain("Authentication failed");
            exception.Reason.Should().NotBeNullOrEmpty();
        }
    }
}
