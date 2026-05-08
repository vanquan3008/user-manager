using System;

namespace UserService.Api.Authorization.Validation;

public sealed record MagicLinkGrantInput(string? Token, string? ApplicationId)
{
    public Guid? ParseApplicationId()
        => Guid.TryParse(ApplicationId, out var applicationId) ? applicationId : null;
}

