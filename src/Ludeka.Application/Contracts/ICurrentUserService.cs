using System.Collections.Generic;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Contracts;

public interface ICurrentUserService
{
    string UserId { get; }
    string UserName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsFoundingTeam { get; }
    bool IsInRole(string role);
    void SwitchRole(string role);

    bool HasPermission(ModeratorPermission permission)
    {
        if (IsFoundingTeam) return true;
        return IsInRole("Moderator");
    }

    void SwitchUser(string userId)
    {
    }
}
