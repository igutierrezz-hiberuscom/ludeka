namespace Ludeka.Web.Components.Shared;

/// <summary>
/// Catálogo whitelist de iconos Lucide (lucide-static v0.525.0, licencia ISC) consumido
/// por <see cref="Icon"/>. Clave: nombre kebab-case de Lucide. Valor: marcado interno de
/// paths (contenido controlado por el equipo, nunca entrada de usuario).
///
/// Regla de iconos futuros (spec iconography-lucide): todo icono nuevo se añade aquí
/// con su path oficial y se consume vía <c>&lt;Icon Name="..." /&gt;</c>. La interfaz no
/// incorpora emojis como iconografía.
/// </summary>
public static class IconCatalog
{
    public static readonly IReadOnlyDictionary<string, string> Icons = new Dictionary<string, string>
    {
        ["search"] = @"<path d=""m21 21-4.34-4.34"" /><circle cx=""11"" cy=""11"" r=""8"" />",
        ["dices"] = @"<rect width=""12"" height=""12"" x=""2"" y=""10"" rx=""2"" ry=""2"" /><path d=""m17.92 14 3.5-3.5a2.24 2.24 0 0 0 0-3l-5-4.92a2.24 2.24 0 0 0-3 0L10 6"" /><path d=""M6 18h.01"" /><path d=""M10 14h.01"" /><path d=""M15 6h.01"" /><path d=""M18 9h.01"" />",
        ["gift"] = @"<rect x=""3"" y=""8"" width=""18"" height=""4"" rx=""1"" /><path d=""M12 8v13"" /><path d=""M19 12v7a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2v-7"" /><path d=""M7.5 8a2.5 2.5 0 0 1 0-5A4.8 8 0 0 1 12 8a4.8 8 0 0 1 4.5-5 2.5 2.5 0 0 1 0 5"" />",
        ["newspaper"] = @"<path d=""M15 18h-5"" /><path d=""M18 14h-8"" /><path d=""M4 22h16a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2H8a2 2 0 0 0-2 2v16a2 2 0 0 1-4 0v-9a2 2 0 0 1 2-2h2"" /><rect width=""8"" height=""4"" x=""10"" y=""6"" rx=""1"" />",
        ["tent"] = @"<path d=""M3.5 21 14 3"" /><path d=""M20.5 21 10 3"" /><path d=""M15.5 21 12 15l-3.5 6"" /><path d=""M2 21h20"" />",
        ["trophy"] = @"<path d=""M10 14.66v1.626a2 2 0 0 1-.976 1.696A5 5 0 0 0 7 21.978"" /><path d=""M14 14.66v1.626a2 2 0 0 0 .976 1.696A5 5 0 0 1 17 21.978"" /><path d=""M18 9h1.5a1 1 0 0 0 0-5H18"" /><path d=""M4 22h16"" /><path d=""M6 9a6 6 0 0 0 12 0V3a1 1 0 0 0-1-1H7a1 1 0 0 0-1 1z"" /><path d=""M6 9H4.5a1 1 0 0 1 0-5H6"" />",
        ["star"] = @"<path d=""M11.525 2.295a.53.53 0 0 1 .95 0l2.31 4.679a2.123 2.123 0 0 0 1.595 1.16l5.166.756a.53.53 0 0 1 .294.904l-3.736 3.638a2.123 2.123 0 0 0-.611 1.878l.882 5.14a.53.53 0 0 1-.771.56l-4.618-2.428a2.122 2.122 0 0 0-1.973 0L6.396 21.01a.53.53 0 0 1-.77-.56l.881-5.139a2.122 2.122 0 0 0-.611-1.879L2.16 9.795a.53.53 0 0 1 .294-.906l5.165-.755a2.122 2.122 0 0 0 1.597-1.16z"" />",
        ["timer"] = @"<line x1=""10"" x2=""14"" y1=""2"" y2=""2"" /><line x1=""12"" x2=""15"" y1=""14"" y2=""11"" /><circle cx=""12"" cy=""14"" r=""8"" />",
        ["rocket"] = @"<path d=""M4.5 16.5c-1.5 1.26-2 5-2 5s3.74-.5 5-2c.71-.84.7-2.13-.09-2.91a2.18 2.18 0 0 0-2.91-.09z"" /><path d=""m12 15-3-3a22 22 0 0 1 2-3.95A12.88 12.88 0 0 1 22 2c0 2.72-.78 7.5-6 11a22.35 22.35 0 0 1-4 2z"" /><path d=""M9 12H4s.55-3.03 2-4c1.62-1.08 5 0 5 0"" /><path d=""M12 15v5s3.03-.55 4-2c1.08-1.62 0-5 0-5"" />",
        ["refresh-cw"] = @"<path d=""M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8"" /><path d=""M21 3v5h-5"" /><path d=""M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16"" /><path d=""M8 16H3v5"" />",
        ["sparkles"] = @"<path d=""M9.937 15.5A2 2 0 0 0 8.5 14.063l-6.135-1.582a.5.5 0 0 1 0-.962L8.5 9.936A2 2 0 0 0 9.937 8.5l1.582-6.135a.5.5 0 0 1 .963 0L14.063 8.5A2 2 0 0 0 15.5 9.937l6.135 1.581a.5.5 0 0 1 0 .964L15.5 14.063a2 2 0 0 0-1.437 1.437l-1.582 6.135a.5.5 0 0 1-.963 0z"" /><path d=""M20 3v4"" /><path d=""M22 5h-4"" /><path d=""M4 17v2"" /><path d=""M5 18H3"" />",
        ["calendar-days"] = @"<path d=""M8 2v4"" /><path d=""M16 2v4"" /><rect width=""18"" height=""18"" x=""3"" y=""4"" rx=""2"" /><path d=""M3 10h18"" /><path d=""M8 14h.01"" /><path d=""M12 14h.01"" /><path d=""M16 14h.01"" /><path d=""M8 18h.01"" /><path d=""M12 18h.01"" /><path d=""M16 18h.01"" />",
        ["calendar"] = @"<path d=""M8 2v4"" /><path d=""M16 2v4"" /><rect width=""18"" height=""18"" x=""3"" y=""4"" rx=""2"" /><path d=""M3 10h18"" />",
        ["map-pin"] = @"<path d=""M20 10c0 4.993-5.539 10.193-7.399 11.799a1 1 0 0 1-1.202 0C9.539 20.193 4 14.993 4 10a8 8 0 0 1 16 0"" /><circle cx=""12"" cy=""10"" r=""3"" />",
        ["globe"] = @"<circle cx=""12"" cy=""12"" r=""10"" /><path d=""M12 2a14.5 14.5 0 0 0 0 20 14.5 14.5 0 0 0 0-20"" /><path d=""M2 12h20"" />",
        ["puzzle"] = @"<path d=""M15.39 4.39a1 1 0 0 0 1.68-.474 2.5 2.5 0 1 1 3.014 3.015 1 1 0 0 0-.474 1.68l1.683 1.682a2.414 2.414 0 0 1 0 3.414L19.61 15.39a1 1 0 0 1-1.68-.474 2.5 2.5 0 1 0-3.014 3.015 1 1 0 0 1 .474 1.68l-1.683 1.682a2.414 2.414 0 0 1-3.414 0L8.61 19.61a1 1 0 0 0-1.68.474 2.5 2.5 0 1 1-3.014-3.015 1 1 0 0 0 .474-1.68l-1.683-1.682a2.414 2.414 0 0 1 0-3.414L4.39 8.61a1 1 0 0 1 1.68.474 2.5 2.5 0 1 0 3.014-3.015 1 1 0 0 1-.474-1.68l1.683-1.682a2.414 2.414 0 0 1 3.414 0z"" />",
    };
}
