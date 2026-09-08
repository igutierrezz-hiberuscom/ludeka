namespace Ludeka.Core.Enums;

/// <summary>
/// Estado de actividad de una cuenta de usuario en Ludeka.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Cuenta activa y operativa.
    /// </summary>
    Active,

    /// <summary>
    /// Cuenta suspendida. Se inhabilitan inmediatamente todos los privilegios y accesos de moderación.
    /// </summary>
    Suspended
}
