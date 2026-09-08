using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ludeka.Application.Contracts;

/// <summary>
/// Estado de ubicación geográfica del usuario.
/// </summary>
public record UserLocationState(
    string? UserCountry,
    string? DetectedCountry,
    string? EffectiveCountry
);

/// <summary>
/// Contrato de servicio para gestionar la ubicación activa del usuario y aplicar ordenación prioritaria territorial.
/// </summary>
public interface IUserLocationService
{
    /// <summary>
    /// País seleccionado expresamente por el usuario (null si no ha seleccionado ninguno).
    /// </summary>
    string? CurrentCountry { get; }

    /// <summary>
    /// País detectado automáticamente por el dispositivo/navegador.
    /// </summary>
    string? DetectedCountry { get; }

    /// <summary>
    /// País efectivo para la sesión (CurrentCountry ?? DetectedCountry).
    /// </summary>
    string? EffectiveCountry { get; }

    /// <summary>
    /// Determina si hay un filtrado territorial estricto activo.
    /// </summary>
    bool HasActiveCountry => !string.IsNullOrWhiteSpace(CurrentCountry);

    /// <summary>
    /// Establece el país seleccionado por el usuario.
    /// </summary>
    void SetUserCountry(string? country);

    /// <summary>
    /// Alias de compatibilidad para SetUserCountry.
    /// </summary>
    void SetCurrentCountry(string? country) => SetUserCountry(country);

    /// <summary>
    /// Establece el país detectado por el dispositivo/navegador.
    /// </summary>
    void SetDetectedCountry(string? country);

    /// <summary>
    /// Obtiene el país efectivo para la sesión (preferencia de usuario autenticado o selección de sesión).
    /// </summary>
    Task<string?> GetEffectiveCountryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el país detectado por el dispositivo en la sesión activa.
    /// </summary>
    Task<string?> GetDetectedCountryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el estado consolidado de ubicación del usuario.
    /// </summary>
    Task<UserLocationState> GetUserLocationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ordena una colección situando en las primeras posiciones los elementos correspondientes
    /// al país especificado (o internacional) y manteniendo después los del resto de territorios.
    /// </summary>
    IEnumerable<T> PrioritizeByCountry<T>(IEnumerable<T> items, Func<T, string?> countrySelector, string? preferredCountry = null);

    /// <summary>
    /// Ordena una colección usando el selector de país o inspección automática de propiedad 'Country'.
    /// </summary>
    IEnumerable<T> PrioritizeByCountry<T>(IEnumerable<T> items, string? preferredCountry = null, Func<T, string?>? countrySelector = null);
}
