using Ludeca.Core.Enums;

namespace Ludeca.Application.DTOs;

public record GameFilterCriteria(
    string? SearchTerm = null,
    int? PlayerCount = null,
    GameStyle? Style = null,
    ConfrontationType? Confrontation = null,
    int? MaxDurationMinutes = null,
    bool EspecialParejas = false,
    bool MesaFamiliar = false,
    bool SoloTop = false
);
