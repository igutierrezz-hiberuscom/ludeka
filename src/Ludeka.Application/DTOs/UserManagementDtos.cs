using System;
using System.Collections.Generic;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

public record AppUserDto(
    string Id,
    string UserName,
    string Email,
    UserRole Role,
    string RoleDisplayName,
    UserStatus Status,
    string StatusDisplayName,
    ModeratorPermission Permissions,
    IReadOnlyList<string> PermissionNames,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record UserFilterDto(
    string? Search = null,
    UserRole? Role = null,
    UserStatus? Status = null
);

public record CreateUserCommand(
    string Id,
    string UserName,
    string Email,
    UserRole Role = UserRole.CommunityUser,
    ModeratorPermission Permissions = ModeratorPermission.None
);

public record UpdateUserRoleAndPermissionsCommand(
    string UserId,
    UserRole Role,
    ModeratorPermission Permissions
);

public record UpdateUserStatusCommand(
    string UserId,
    UserStatus Status
);
