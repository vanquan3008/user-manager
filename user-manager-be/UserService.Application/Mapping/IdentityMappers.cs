using UserService.Application.DTOs.ApplicationRoles;
using UserService.Application.DTOs.Roles;
using UserService.Application.DTOs.Users;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Mapping;

public static class IdentityUserMapper
{
    public static IdentityUserDto ToDto(Guid id, string userName, string email, bool emailConfirmed, bool isActive, bool isBanned, string? banReason, DateTime createdAt, Guid? userProfileId, bool isSSOAccount) =>
        new IdentityUserDto(
            id,
            userName,
            email,
            emailConfirmed,
            isActive,
            isBanned,
            banReason,
            RefreshToken: null, 
            RefreshTokenExpiryTime: null,
            createdAt,
            userProfileId,
            IsSSOAccount: isSSOAccount
        );
}

public static class RoleDtoMapper
{
    public static RoleDto ToRoleDto(Guid id, string name, string? description, bool isSystemRole, string? permissionsCsv, DateTime createdAt, DateTime? updatedAt) =>
        new RoleDto(
            id,
            name,
            description,
            isSystemRole,
            permissionsCsv ?? string.Empty,
            createdAt,
            updatedAt
        );

    public static RoleBasicDto ToRoleBasicDto(Guid id, string name, string? description, bool isSystemRole) =>
        new RoleBasicDto(
            id,
            name,
            description,
            isSystemRole
        );

    public static ApplicationRoleDto ToApplicationRoleDto(Guid id, string name, string? description, DateTime createdAt, DateTime? updatedAt) =>
        new ApplicationRoleDto(
            id,
            name,
            description,
            createdAt,
            updatedAt
        );

    public static ApplicationRoleDto ToApplicationRoleDto(RoleLookup role, ApplicationRole mapping) =>
        new ApplicationRoleDto(
            role.Id,
            role.Name ?? string.Empty,
            role.Description,
            mapping.CreatedAt,
            mapping.UpdatedAt
        );
}
