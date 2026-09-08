using System;
using System.Globalization;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Infrastructure.Services;

/// <summary>
/// Generador heurístico determinista de síntesis editorial para juegos de mesa.
/// Provee síntesis en español de alta calidad para desarrollo offline, pruebas y resiliencia ante caídas de API.
/// </summary>
public static class HeuristicGameSummaryGenerator
{
    public static AiGameSummaryDto Generate(Game game, string modelName = "Heurística Editorial")
    {
        ArgumentNullException.ThrowIfNull(game);

        // 1. Veredicto de Escalabilidad
        string scalabilitySummary;
        if (game.Scalability.Count > 0)
        {
            scalabilitySummary = $"{game.IdealPlayerCountText}. El consenso del hobby internacional confirma que en este rango el flujo de turnos es óptimo, con tensión competitiva balanceada y mínimo tiempo de inactividad entre rondas.";
        }
        else
        {
            scalabilitySummary = $"Escalabilidad oficial de {game.CalculateIdealPlayerCountText()} según fabricante. Adecuado tanto para partidas introductorias como para grupos estables.";
        }

        // 2. Accesibilidad & Edad
        string ageSummary;
        if (game.Age.IsAccessibleEarlier)
        {
            ageSummary = $"Caja marcada para {game.Age.BoxAge}+ años por normativa de piezas, pero la comunidad lo juega con éxito desde los {game.Age.CommunityAge}+ años bajo explicación guiada gracias a su iconografía clara.";
        }
        else if (game.Age.CommunityAge > game.Age.BoxAge)
        {
            ageSummary = $"Recomendado a partir de los {game.Age.CommunityAge}+ años (la caja indica {game.Age.BoxAge}+). Requiere madurez analítica y gestión de opciones que pueden abrumar a jugadores más jóvenes.";
        }
        else
        {
            ageSummary = $"Edad recomendada: {game.Age.CommunityAge}+ años. Nivel de abstracción y toma de decisiones plenamente accesible para público familiar y juvenil.";
        }

        // 3. Huella en Mesa y Duración
        string footprintName = game.Footprint switch
        {
            TableFootprint.SmallTable => "Mesa pequeña / cafetería (despliegue compacto sin tableros masivos)",
            TableFootprint.StandardTable => "Mesa de comedor estándar (espacio suficiente para tablero central y tableros personales)",
            TableFootprint.TableMonster => "Monstruo de mesa (requiere mesa amplia para reservas, mercados y áreas individuales)",
            _ => "Mesa de comedor estándar"
        };

        string footprintSummary = $"{footprintName}. Duración estimada de ~{game.Duration.EstimatedPerPlayerMinutes} min por comensal (partidas completas de {game.Duration.MinMinutes} a {game.Duration.MaxMinutes} minutos).";

        // 4. Veredicto General
        string confrontationDesc = game.Confrontation switch
        {
            ConfrontationType.Cooperative => "experiencia colaborativa donde todos ganan o pierden juntos",
            ConfrontationType.Competitive => "duelo competitivo con interacción directa o de tablero",
            ConfrontationType.HiddenRolesOrTeams => "dinámica de identidades secretas, faroleo y deducción social",
            ConfrontationType.SemiCooperative => "tensión semi-cooperativa con agenda individual oculta",
            _ => "dinámica estratégica"
        };

        string authorEditorial = (!string.IsNullOrWhiteSpace(game.Designer) && !string.IsNullOrWhiteSpace(game.Publisher))
            ? $" Diseñado por {game.Designer} y editado por {game.Publisher}."
            : string.Empty;

        string generalVerdict = $"{game.SpanishTitle} ({game.YearPublished}) es un referente del estilo {game.Style} enfocado en una {confrontationDesc}.{authorEditorial} Cuenta con una valoración de {game.BggRating:0.0}/10 en BGG. Síntesis objetiva generada automáticamente para orientar a la comunidad mientras se formaliza el veredicto oficial de la mesa fundadora.";

        return new AiGameSummaryDto(
            GameId: game.Id,
            GameTitle: game.SpanishTitle,
            ScalabilitySummary: scalabilitySummary,
            AgeSummary: ageSummary,
            FootprintSummary: footprintSummary,
            GeneralVerdict: generalVerdict,
            Model: modelName,
            GeneratedAt: DateTime.UtcNow
        );
    }
}
