using System;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

/// <summary>
/// Contrato para el extractor de títulos de juegos en publicaciones y novedades editoriales.
/// </summary>
public interface INewsGameExtractor
{
    /// <summary>
    /// Extrae el título candidato de un juego a partir del titular y notas de una novedad editorial.
    /// </summary>
    string? ExtractGameTitle(string newsTitle, string? newsNotes = null);

    /// <summary>
    /// Analiza un lanzamiento específico, buscando coincidencia en el catálogo local de Ludeka
    /// o encolando en BGG si es un juego no catalogado.
    /// </summary>
    Task<NewsExtractionResultDto> ProcessReleaseAsync(WeeklyRelease release, CancellationToken ct = default);

    /// <summary>
    /// Escanea y procesa todos los lanzamientos editoriales pendientes de vinculación en Ludeka.
    /// </summary>
    Task<int> DiscoverAndEnqueueFromReleasesAsync(CancellationToken ct = default);
}
