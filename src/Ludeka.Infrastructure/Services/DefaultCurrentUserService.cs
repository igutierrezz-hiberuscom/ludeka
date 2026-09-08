using System;
using System.Collections.Generic;
using System.Linq;
using Ludeka.Application.Contracts;
using Ludeka.Core.Enums;

namespace Ludeka.Infrastructure.Services;

public class DefaultCurrentUserService : ICurrentUserService
{
    public const string DefaultUserId = "usuario-fundador-ludeka";
    public const string DefaultUserName = "Mesa Fundadora (Demo)";

    private readonly List<string> _roles = ["FoundingTeam", "Moderator"];
    private string _currentUserId = DefaultUserId;
    private string _currentUserName = DefaultUserName;
    private ModeratorPermission _permissions = ModeratorPermission.All;
    private bool _isSuspended = false;

    public string UserId => _currentUserId;
    public string UserName => _currentUserName;
    public IReadOnlyList<string> Roles => _roles.AsReadOnly();
    public bool IsFoundingTeam => !_isSuspended && IsInRole("FoundingTeam");

    public bool IsInRole(string role)
    {
        if (_isSuspended) return false;
        return _roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasPermission(ModeratorPermission permission)
    {
        if (_isSuspended)
            return false;

        if (IsFoundingTeam)
            return true;

        if (IsInRole("Moderator"))
        {
            if (permission == ModeratorPermission.None)
                return true;

            return (_permissions & permission) == permission;
        }

        return false;
    }

    public void SwitchRole(string role)
    {
        _roles.Clear();
        _isSuspended = false;

        if (string.Equals(role, "User", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "CommunityUser", StringComparison.OrdinalIgnoreCase))
        {
            _roles.Add("User");
            _roles.Add("CommunityUser");
            _permissions = ModeratorPermission.None;
            _currentUserId = "jugador-comunitario";
            _currentUserName = "Jugador Comunitario";
        }
        else if (string.Equals(role, "Moderator", StringComparison.OrdinalIgnoreCase))
        {
            _roles.Add("Moderator");
            _permissions = ModeratorPermission.CanEditGames | ModeratorPermission.CanUploadImages;
            _currentUserId = "laura_mod";
            _currentUserName = "Laura Moderadora";
        }
        else
        {
            _roles.Add("FoundingTeam");
            _roles.Add("Moderator");
            _permissions = ModeratorPermission.All;
            _currentUserId = DefaultUserId;
            _currentUserName = DefaultUserName;
        }
    }

    public void SwitchUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;

        var clean = userId.Trim().ToLowerInvariant();
        _roles.Clear();
        _isSuspended = false;

        switch (clean)
        {
            case "usuario-fundador-ludeka":
            case "carlos_fundador":
                _roles.Add("FoundingTeam");
                _roles.Add("Moderator");
                _permissions = ModeratorPermission.All;
                _currentUserId = clean;
                _currentUserName = "Carlos Fundador";
                break;

            case "laura_mod":
                _roles.Add("Moderator");
                _permissions = ModeratorPermission.CanEditGames | ModeratorPermission.CanUploadImages;
                _currentUserId = "laura_mod";
                _currentUserName = "Laura (Mod Fichas e Imágenes)";
                break;

            case "pablo_editoriales":
                _roles.Add("Moderator");
                _permissions = ModeratorPermission.CanManagePublishers | ModeratorPermission.CanManageCreators | ModeratorPermission.CanManageStoreLinks;
                _currentUserId = "pablo_editoriales";
                _currentUserName = "Pablo (Mod Industria y Tiendas)";
                break;

            case "marta_media":
                _roles.Add("Moderator");
                _permissions = ModeratorPermission.CanApproveMedia | ModeratorPermission.CanResolveReports;
                _currentUserId = "marta_media";
                _currentUserName = "Marta (Mod Media y Reportes)";
                break;

            case "susana_suspendida":
                _roles.Add("Moderator");
                _permissions = ModeratorPermission.All;
                _isSuspended = true;
                _currentUserId = "susana_suspendida";
                _currentUserName = "Susana (Cuenta Suspendida)";
                break;

            case "jugador-comunitario":
            case "david_comunidad":
            default:
                _roles.Add("CommunityUser");
                _roles.Add("User");
                _permissions = ModeratorPermission.None;
                _currentUserId = clean;
                _currentUserName = clean == "david_comunidad" ? "David Jugador" : "Usuario Comunitario";
                break;
        }
    }
}
