using System;
using System.Globalization;
using System.Security;
using System.Text;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Features.Community;

public class SocialCardService : ISocialCardService
{
    public GeneratedSocialCardDto GenerateCard(SocialCardDataDto data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var svg = GenerateSvg(data);
        var caption = GenerateInstagramCaption(data);
        var safeTitle = SecurityElement.Escape(data.GameTitle)
            .Replace(" ", "-")
            .ToLowerInvariant();
        var fileName = $"ludeka-card-{safeTitle}.svg";

        return new GeneratedSocialCardDto(svg, caption, fileName);
    }

    public string GenerateSvg(SocialCardDataDto data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var title = EscapeXml(data.GameTitle);
        var originalTitle = EscapeXml(data.OriginalTitle);
        var designer = EscapeXml(data.Designer);
        var publisher = EscapeXml(data.Publisher);
        var style = EscapeXml(data.StyleText);
        var ideal = EscapeXml(data.IdealPlayersText);
        var ratingStr = data.Rating.ToString("0.0", CultureInfo.InvariantCulture);
        var coverUrl = !string.IsNullOrWhiteSpace(data.CoverImageUrl)
            ? EscapeXml(data.CoverImageUrl)
            : string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine(@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 1080 1080"" width=""1080"" height=""1080"">");
        sb.AppendLine(@"  <defs>");
        sb.AppendLine(@"    <linearGradient id=""bgGrad"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""100%"">");
        sb.AppendLine(@"      <stop offset=""0%"" stop-color=""#0B0F17""/>");
        sb.AppendLine(@"      <stop offset=""60%"" stop-color=""#131B2A""/>");
        sb.AppendLine(@"      <stop offset=""100%"" stop-color=""#1A2234""/>");
        sb.AppendLine(@"    </linearGradient>");
        sb.AppendLine(@"    <linearGradient id=""orangeGrad"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""0%"">");
        sb.AppendLine(@"      <stop offset=""0%"" stop-color=""#F97316""/>");
        sb.AppendLine(@"      <stop offset=""100%"" stop-color=""#EA580C""/>");
        sb.AppendLine(@"    </linearGradient>");
        sb.AppendLine(@"    <linearGradient id=""coverShadow"" x1=""0%"" y1=""0%"" x2=""0%"" y2=""100%"">");
        sb.AppendLine(@"      <stop offset=""0%"" stop-color=""#000000"" stop-opacity=""0""/>");
        sb.AppendLine(@"      <stop offset=""100%"" stop-color=""#000000"" stop-opacity=""0.85""/>");
        sb.AppendLine(@"    </linearGradient>");
        sb.AppendLine(@"    <filter id=""dropShadow"" x=""-10%"" y=""-10%"" width=""120%"" height=""120%"">");
        sb.AppendLine(@"      <feDropShadow dx=""0"" dy=""16"" stdDeviation=""24"" flood-color=""#000000"" flood-opacity=""0.6""/>");
        sb.AppendLine(@"    </filter>");
        sb.AppendLine(@"    <clipPath id=""coverClip"">");
        sb.AppendLine(@"      <rect x=""140"" y=""120"" width=""800"" height=""450"" rx=""24""/>");
        sb.AppendLine(@"    </clipPath>");
        sb.AppendLine(@"  </defs>");

        // Background
        sb.AppendLine(@"  <!-- Fondo principal -->");
        sb.AppendLine(@"  <rect width=""1080"" height=""1080"" fill=""url(#bgGrad)""/>");

        // Glow decorativo
        sb.AppendLine(@"  <circle cx=""540"" cy=""120"" r=""400"" fill=""#F97316"" opacity=""0.07""/>");

        // Cabecera de marca
        sb.AppendLine(@"  <!-- Cabecera de Marca -->");
        sb.AppendLine(@"  <g transform=""translate(140, 64)"">");
        sb.AppendLine(@"    <text x=""0"" y=""28"" fill=""#FFFFFF"" font-family=""'Inter', 'Segoe UI', system-ui, sans-serif"" font-size=""32"" font-weight=""900"" letter-spacing=""-0.5"">Ludeka<tspan fill=""#F97316"">.</tspan></text>");
        sb.AppendLine(@"    <text x=""135"" y=""26"" fill=""#94A3B8"" font-family=""'Inter', 'Segoe UI', system-ui, sans-serif"" font-size=""16"" font-weight=""600"" letter-spacing=""0.5"">COMUNIDAD Y CATÁLOGO DE JUEGOS</text>");
        sb.AppendLine(@"  </g>");

        // Carátula / Imagen
        sb.AppendLine(@"  <!-- Carátula oficial -->");
        sb.AppendLine(@"  <rect x=""140"" y=""120"" width=""800"" height=""450"" rx=""24"" fill=""#0F172A"" stroke=""#334155"" stroke-width=""2"" filter=""url(#dropShadow)""/>");
        if (!string.IsNullOrWhiteSpace(coverUrl))
        {
            sb.AppendLine($@"  <image href=""{coverUrl}"" x=""140"" y=""120"" width=""800"" height=""450"" preserveAspectRatio=""xMidYMid slice"" clip-path=""url(#coverClip)""/>");
            sb.AppendLine(@"  <rect x=""140"" y=""120"" width=""800"" height=""450"" fill=""url(#coverShadow)"" clip-path=""url(#coverClip)""/>");
        }

        // Badges flotantes en la imagen
        sb.AppendLine(@"  <!-- Badges flotantes -->");
        sb.AppendLine(@"  <g transform=""translate(170, 510)"">");
        sb.AppendLine(@"    <rect x=""0"" y=""0"" width=""110"" height=""38"" rx=""19"" fill=""#0B0F17"" fill-opacity=""0.85"" stroke=""#F59E0B"" stroke-width=""1.5""/>");
        sb.AppendLine($@"    <text x=""55"" y=""25"" fill=""#FBBF24"" font-family=""'Inter', 'Segoe UI', sans-serif"" font-size=""18"" font-weight=""900"" text-anchor=""middle"">★ {ratingStr}</text>");
        sb.AppendLine(@"  </g>");

        if (!string.IsNullOrWhiteSpace(data.FoundingVerdictBadge))
        {
            var badgeText = EscapeXml(data.FoundingVerdictBadge);
            sb.AppendLine(@"  <g transform=""translate(295, 510)"">");
            sb.AppendLine(@"    <rect x=""0"" y=""0"" width=""260"" height=""38"" rx=""19"" fill=""#7C2D12"" fill-opacity=""0.9"" stroke=""#F97316"" stroke-width=""1.5""/>");
            sb.AppendLine($@"    <text x=""130"" y=""24"" fill=""#FFEDD5"" font-family=""'Inter', 'Segoe UI', sans-serif"" font-size=""14"" font-weight=""800"" text-anchor=""middle"">{badgeText}</text>");
            sb.AppendLine(@"  </g>");
        }

        // Bloque de Texto e Identidad
        sb.AppendLine(@"  <!-- Identidad del Juego -->");
        sb.AppendLine(@"  <g transform=""translate(140, 620)"">");
        sb.AppendLine($@"    <text x=""0"" y=""40"" fill=""#FFFFFF"" font-family=""'Inter', 'Segoe UI', system-ui, sans-serif"" font-size=""44"" font-weight=""900"" letter-spacing=""-1"">{title}</text>");
        sb.AppendLine($@"    <text x=""0"" y=""76"" fill=""#94A3B8"" font-family=""'Inter', 'Segoe UI', system-ui, sans-serif"" font-size=""20"" font-weight=""500"">{designer} &bull; {publisher} ({data.YearPublished})</text>");
        sb.AppendLine(@"  </g>");

        // Píldoras de ADN Lúdico
        sb.AppendLine(@"  <!-- Píldoras ADN Lúdico -->");
        sb.AppendLine(@"  <g transform=""translate(140, 730)"">");
        // Píldora 1: Estilo
        sb.AppendLine(@"    <rect x=""0"" y=""0"" width=""240"" height=""48"" rx=""12"" fill=""#1E293B"" stroke=""#475569"" stroke-width=""1""/>");
        sb.AppendLine($@"    <text x=""120"" y=""30"" fill=""#E2E8F0"" font-family=""'Inter', 'Segoe UI', sans-serif"" font-size=""16"" font-weight=""700"" text-anchor=""middle"">{style}</text>");

        // Píldora 2: Escalabilidad
        sb.AppendLine(@"    <rect x=""260"" y=""0"" width=""260"" height=""48"" rx=""12"" fill=""#064E3B"" stroke=""#10B981"" stroke-width=""1""/>");
        sb.AppendLine($@"    <text x=""390"" y=""30"" fill=""#6EE7B7"" font-family=""'Inter', 'Segoe UI', sans-serif"" font-size=""16"" font-weight=""700"" text-anchor=""middle"">{ideal}</text>");

        // Píldora 3: Duración
        if (data.EstimatedDurationPerPlayer.HasValue)
        {
            sb.AppendLine(@"    <rect x=""540"" y=""0"" width=""260"" height=""48"" rx=""12"" fill=""#1E293B"" stroke=""#475569"" stroke-width=""1""/>");
            sb.AppendLine($@"    <text x=""670"" y=""30"" fill=""#E2E8F0"" font-family=""'Inter', 'Segoe UI', sans-serif"" font-size=""16"" font-weight=""700"" text-anchor=""middle"">⏱️ ~{data.EstimatedDurationPerPlayer} min/jugador</text>");
        }
        sb.AppendLine(@"  </g>");

        // Pie de Marca y Llamada a la Acción
        sb.AppendLine(@"  <!-- Pie de Tarjeta -->");
        sb.AppendLine(@"  <line x1=""140"" y1=""840"" x2=""940"" y2=""840"" stroke=""#334155"" stroke-width=""1"" stroke-dasharray=""4 4""/>");
        sb.AppendLine(@"  <g transform=""translate(140, 890)"">");
        sb.AppendLine(@"    <rect x=""0"" y=""0"" width=""800"" height=""110"" rx=""20"" fill=""#0F172A"" stroke=""#1E293B"" stroke-width=""1""/>");
        sb.AppendLine(@"    <text x=""40"" y=""46"" fill=""#F97316"" font-family=""'Inter', 'Segoe UI', sans-serif"" font-size=""18"" font-weight=""800"">LUDEKA</text>");
        sb.AppendLine(@"    <text x=""40"" y=""76"" fill=""#E2E8F0"" font-family=""'Inter', 'Segoe UI', sans-serif"" font-size=""20"" font-weight=""700"">Ficha completa, semáforo de mesa y fotos reales en ludeka.app</text>");
        sb.AppendLine(@"  </g>");

        sb.AppendLine(@"</svg>");
        return sb.ToString();
    }

    public string GenerateInstagramCaption(SocialCardDataDto data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var durationText = data.EstimatedDurationPerPlayer.HasValue
            ? $"⏱️ Duración: ~{data.EstimatedDurationPerPlayer} min por jugador\n"
            : string.Empty;

        var verdictText = !string.IsNullOrWhiteSpace(data.FoundingVerdictBadge)
            ? $"🛡️ Veredicto de la Mesa: {data.FoundingVerdictBadge}\n"
            : string.Empty;

        return $@"🎲 {data.GameTitle} ({data.YearPublished})

{verdictText}⭐️ Valoración: {data.Rating.ToString("0.0", CultureInfo.InvariantCulture)} / 10
⚙️ Estilo: {data.StyleText} ({data.ConfrontationText})
👥 Escalabilidad: {data.IdealPlayersText}
{durationText}
Diseñado por {data.Designer} y publicado por {data.Publisher}.

👉 Consulta la ficha inteligente con semáforo hasta 7+ comensales, guía de fundas y dudas de reglas resueltas en ludeka.app (enlace en bio).

---
#juegosdemesa #boardgames #ludeka #juegosdemesaespaña #bgg #juegosdecartas #{data.Publisher.Replace(" ", "").ToLowerInvariant()} #jugones #eurogames";
    }

    private static string EscapeXml(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
