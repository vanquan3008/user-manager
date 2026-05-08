using MediatR;
using UserService.Application.DTOs.Applications;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Mapping;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, CreateApplicationResponse>
{
    private readonly IApplicationRepository _applications;
    private readonly IUserRepository _users;
    private readonly IApplicationRoleService _applicationRoleService;
    private readonly IHashingService _hashingService;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CreateApplicationCommandHandler(
        IApplicationRepository applications,
        IUserRepository users,
        IApplicationRoleService applicationRoleService,
        IHashingService hashingService,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _applications = applications;
        _users = users;
        _applicationRoleService = applicationRoleService;
        _hashingService = hashingService;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateApplicationResponse> Handle(CreateApplicationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new BusinessRuleValidationException("ApplicationNameRequired", "Application name is required.");

        var owner = await _users.GetByIdAsync(command.OwnerId, cancellationToken);
        if (owner == null)
            throw new UserNotFoundException(command.OwnerId);

        var name = command.Name.Trim();
        var nameTaken = await _applications.IsNameTakenAsync(command.OwnerId, name, cancellationToken);
        if (nameTaken)
            throw new BusinessRuleValidationException("ApplicationNameTaken", $"Application name '{name}' is already used by this owner.");

        var appName = AppName.Create(name);
        var appDescription = AppDescription.Create(command.Description?.Trim() ?? string.Empty);
        UrlValue? logoUrl = !string.IsNullOrWhiteSpace(command.LogoUrl) ? UrlValue.Create(command.LogoUrl.Trim()) : null;

        var apiKey = ApiKey.Generate();
        var plainSecret = ApiSecretGenerator.Generate();
        var secretHash = ApiSecretHash.Create(_hashingService.Hash(plainSecret));

        var app = AppApplication.Create(
            Guid.NewGuid(),
            command.OwnerId,
            appName,
            appDescription,
            apiKey,
            secretHash,
            _clock.UtcNow,
            logoUrl,
            command.SubscriptionPlanId,
            command.DefaultLicensePackageId);

        if (!string.IsNullOrWhiteSpace(command.AllowedRedirectUris))
        {
            foreach (var uri in command.AllowedRedirectUris
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                app.AddRedirectUri(UrlValue.Create(uri));
            }
        }

        if (!string.IsNullOrWhiteSpace(command.BackchannelLogoutUri))
        {
            app.SetBackchannelLogoutUri(UrlValue.Create(command.BackchannelLogoutUri.Trim()));
        }

        await _applications.AddAsync(app, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Seed default application-scoped roles for newly created applications.
        var adminRoleResult = await _applicationRoleService.CreateRoleAsync(app.Id, "Admin", "Application administrator", cancellationToken);
        if (!adminRoleResult.Succeeded)
        {
            var firstError = adminRoleResult.Errors?.SelectMany(kv => kv.Value).FirstOrDefault() ?? "Unknown error";
            throw new BusinessRuleValidationException("SeedDefaultRoleFailed", $"Failed to create default role 'Admin': {firstError}");
        }

        var guestRoleResult = await _applicationRoleService.CreateRoleAsync(app.Id, "Guest", "Default guest role", cancellationToken);
        if (!guestRoleResult.Succeeded)
        {
            var firstError = guestRoleResult.Errors?.SelectMany(kv => kv.Value).FirstOrDefault() ?? "Unknown error";
            throw new BusinessRuleValidationException("SeedDefaultRoleFailed", $"Failed to create default role 'Guest': {firstError}");
        }

        // "Sa" in the UI corresponds to SuperAdmin in system/global naming.
        var superAdminRoleResult = await _applicationRoleService.CreateRoleAsync(
            app.Id,
            "SuperAdmin",
            "Application super administrator",
            cancellationToken);
        if (!superAdminRoleResult.Succeeded)
        {
            var firstError = superAdminRoleResult.Errors?.SelectMany(kv => kv.Value).FirstOrDefault() ?? "Unknown error";
            throw new BusinessRuleValidationException("SeedDefaultRoleFailed", $"Failed to create default role 'SuperAdmin': {firstError}");
        }

        return ApplicationEntityMapper.ToCreateApplicationResponse(app, owner);
    }
}
