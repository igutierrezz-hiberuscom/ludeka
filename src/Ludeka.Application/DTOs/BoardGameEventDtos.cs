using System;

namespace Ludeka.Application.DTOs;

/// <summary>
/// Petición para crear un nuevo evento o feria lúdica.
/// </summary>
public record CreateBoardGameEventRequest(
    string Title,
    string Description,
    string ImageUrl,
    DateOnly StartDate,
    DateOnly EndDate,
    string Location,
    string? WebsiteUrl = null,
    string Organizer = "",
    bool IsOfficial = true,
    string Country = "España"
);

/// <summary>
/// Petición para actualizar un evento o feria lúdica existente.
/// </summary>
public record UpdateBoardGameEventRequest(
    string Title,
    string Description,
    string ImageUrl,
    DateOnly StartDate,
    DateOnly EndDate,
    string Location,
    string? WebsiteUrl = null,
    string Organizer = "",
    bool IsOfficial = true,
    string Country = "España"
);
