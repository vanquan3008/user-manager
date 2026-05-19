using Microsoft.Extensions.DependencyInjection;
using MediatR;
using FluentValidation;
using UserService.Application.Behaviors;
using UserService.Application.Features.OAuth.Policies;

namespace UserService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly));
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped<IOAuthRedirectUriPolicy, OAuthRedirectUriPolicy>();

        return services
            .AddIdentityModule()
            .AddApplicationsModule()
            .AddAuthorizationModule()
            .AddSubscriptionsModule();
    }
}
