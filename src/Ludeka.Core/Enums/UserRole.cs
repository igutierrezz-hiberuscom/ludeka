namespace Ludeka.Core.Enums;

/// <summary>
/// Roles principales del sistema de usuarios de Ludeka.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Miembro de la Mesa Fundadora con privilegios absolutos de gobernanza, configuración y auditoría.
    /// </summary>
    FoundingTeam,

    /// <summary>
    /// Moderador con permisos específicos y granulares asignados por la Mesa Fundadora.
    /// </summary>
    Moderator,

    /// <summary>
    /// Usuario regular de la comunidad (crea listas, reseñas, reportes, ludoteca personal).
    /// </summary>
    CommunityUser
}
