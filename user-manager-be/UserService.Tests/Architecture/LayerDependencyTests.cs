using FluentAssertions;
using UserService.Application.DependencyInjection;
using UserService.Domain.Entities;

namespace UserService.Tests.Architecture;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_Should_Not_Reference_AspNetCore_Or_EntityFramework()
    {
        var references = typeof(User).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        references.Should().NotContain(name =>
            name.StartsWith("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Application_Should_Not_Reference_EntityFramework_Or_Infrastructure()
    {
        var references = typeof(ApplicationServiceCollectionExtensions).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        references.Should().NotContain(name =>
            name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("UserService.Infrastructure", StringComparison.OrdinalIgnoreCase));
    }
}
