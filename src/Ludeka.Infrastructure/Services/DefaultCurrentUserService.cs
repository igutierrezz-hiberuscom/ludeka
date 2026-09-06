using System;
using System.Collections.Generic;
using System.Linq;
using Ludeka.Application.Contracts;

namespace Ludeka.Infrastructure.Services;

public class DefaultCurrentUserService : ICurrentUserService
{
    public const string DefaultUserId = "usuario-fundador-ludeka";
    public const string DefaultUserName = "Mesa Fundadora (Demo)";

    private readonly List<string> _roles = ["FoundingTeam", "Moderator"];
    private string _currentUserId = DefaultUserId;
    private string _currentUserName = DefaultUserName;

    public string UserId => _currentUserId;
    public string UserName => _currentUserName;
    public IReadOnlyList<string> Roles => _roles.AsReadOnly();
    public bool IsFoundingTeam => IsInRole("FoundingTeam");

    public bool IsInRole(string role)
    {
        return _roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
    }

    public void SwitchRole(string role)
    {
        _roles.Clear();
        if (string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
        {
            _roles.Add("User");
            _currentUserId = "jugador-comunitario";
            _currentUserName = "Jugador Comunitario";
        }
        else
        {
            _roles.Add("FoundingTeam");
            _roles.Add("Moderator");
            _currentUserId = DefaultUserId;
            _currentUserName = DefaultUserName;
        }
    }
}
