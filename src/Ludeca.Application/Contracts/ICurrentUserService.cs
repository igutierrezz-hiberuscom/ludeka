using System.Collections.Generic;

namespace Ludeca.Application.Contracts;

public interface ICurrentUserService
{
    string UserId { get; }
    string UserName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsFoundingTeam { get; }
    bool IsInRole(string role);
    void SwitchRole(string role);
}
