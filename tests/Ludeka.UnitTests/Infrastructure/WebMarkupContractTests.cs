using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

/// <summary>
/// Contrato de markup por archivo fuente (INC-31): lee los .razor/.cs desde la raíz del repo
/// y afirma invariantes estructurales contiene/no-contiene. El render real se verifica en
/// sdd-verify con `dotnet run` (limitación documentada en el diseño: no prueba el DOM).
/// </summary>
public class WebMarkupContractTests
{
    // Emojis prohibidos en la portada (Decisión 6): cada uno tiene su icono Lucide en el catálogo
    private static readonly string[] EmojisDePortada =
    { "🔍", "🎲", "🎁", "📰", "🎪", "🏆", "⭐", "⏱", "🚀", "🔄", "🆕", "🗓", "📅", "📍", "🌐", "🧩" };

    private static string GetRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Ludeka.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }

    private static string ReadSource(string relativePath)
    {
        var path = Path.Combine(GetRepoRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(path), $"No se encontró el archivo fuente: {relativePath}");
        return File.ReadAllText(path);
    }

    public static TheoryData<string, string, string[], string[]> MarkupContracts => new()
    {
        // GameDetail: diseñador como texto plano, sin <a> al directorio de creadores
        { "GameDetail (diseñador texto plano)", "src/Ludeka.Web/Components/Pages/GameDetail.razor",
          new[] { "Diseñado por", "<span class=\"text-[var(--text-primary)] font-bold\">@Game.Designer</span>" },
          new[] { "creadores/", "Ver ficha del autor" } },

        // CreatorDetail: sin "Obras de", reetiquetado a creador de contenido, alias /autores/{slug} conservado
        { "CreatorDetail (ficha reetiquetada)", "src/Ludeka.Web/Components/Pages/CreatorDetail.razor",
          new[] { "@page \"/autores/{Slug}\"", "Ficha del Creador de Contenido", "Cargando ficha del creador...", "Creador de contenido" },
          new[] { "Obras de", "Autor / Diseñador", "Obras y Ficha de Autor", "autor o creador registrado" } },

        // CreatorsDirectory: PageTitle/badge/h1/párrafo nuevos, sin badge de obras, alias /autores conservado
        { "CreatorsDirectory (directorio reetiquetado)", "src/Ludeka.Web/Components/Pages/CreatorsDirectory.razor",
          new[] { "@page \"/autores\"", "Directorio de Creadores de Contenido — Ludeka", "Creadores de Contenido &bull; Divulgadores del Hobby", "hobby hispanohablante" },
          new[] { "Creadores y Autores", "Autoría Lúdica", "GamesCount", "diseñadores, ilustradores y divulgadores referentes" } },

        // MainLayout: navegación y pie reetiquetados a Creadores
        { "MainLayout (nav y pie)", "src/Ludeka.Web/Components/Layout/MainLayout.razor",
          new[] { "Creadores", "✍️ Creadores" },
          new[] { "Autores" } },

        // CreatorEditModal: formulario reetiquetado
        { "CreatorEditModal (formulario)", "src/Ludeka.Web/Components/Shared/CreatorEditModal.razor",
          new[] { "Nuevo Creador de Contenido", "Nombre del Creador de Contenido", "Ej. Análisis Parálisis, Meepletopía...", "Trayectoria, canales y estilo de divulgación..." },
          new[] { "Creador / Autor", "Nombre Completo del Creador", "Elizabeth Hargrave, Klaus Teuber", "estilo de diseño" } },

        // AuditService: etiqueta de auditoría
        { "AuditService (etiqueta)", "src/Ludeka.Application/Features/Admin/AuditService.cs",
          new[] { "\"Creador de Contenido\"" },
          new[] { "Autor/Creador" } },

        // AuditLogViewer: filtro de auditoría sin "Autor / Diseñador"
        { "AuditLogViewer (filtro)", "src/Ludeka.Web/Components/Pages/AuditLogViewer.razor",
          new[] { "Creador de Contenido" },
          new[] { "Autor / Diseñador" } },

        // UserPermissionsModal: permiso reetiquetado
        { "UserPermissionsModal (permiso)", "src/Ludeka.Web/Components/Shared/UserPermissionsModal.razor",
          new[] { "Gestionar Creadores de Contenido" },
          new[] { "Autores y Diseñadores" } },

        // HomeDashboard: hero minimalista con h1 sr-only, sin badge ni titular, sin enlaces a /radar
        { "HomeDashboard (hero minimalista)", "src/Ludeka.Web/Components/Pages/HomeDashboard.razor",
          new[] { "sr-only", "<h1 class=\"sr-only\">Ludeka — Juegos de mesa en español</h1>" },
          new[] { "PORTADA EDITORIAL", "href=\"/radar\"" } },

        // Radar: sin banner legacy, alias silencioso con ambas rutas @page
        { "Radar (sin banner legacy)", "src/Ludeka.Web/Components/Pages/Radar.razor",
          new[] { "@page \"/sorteos\"", "@page \"/radar\"" },
          new[] { "¡Radar renovado!", "IsLegacyRoute" } },

        // Icon: SVG Lucide inline, currentColor, aria-hidden por defecto; sin <img> ni peticiones de red
        { "Icon (SVG Lucide inline)", "src/Ludeka.Web/Components/Shared/Icon.razor",
          new[] { "viewBox=\"0 0 24 24\"", "stroke=\"currentColor\"", "aria-hidden=\"true\"" },
          new[] { "<img", "http" } },

        // DefaultImage: SVG inline temable por variables de tema; sin <img> roto
        { "DefaultImage (SVG inline temable)", "src/Ludeka.Web/Components/Shared/DefaultImage.razor",
          new[] { "viewBox=\"0 0 400 225\"", "var(--brand-", "var(--bg-", "aria-hidden" },
          new[] { "<img" } },

        // Assets por defecto servibles (variante estática para onerror), uno por dominio
        { "Asset default de eventos", "src/Ludeka.Web/wwwroot/images/defaults/evento-default.svg",
          new[] { "EVENTO LUDEKA", "viewBox=\"0 0 400 225\"" },
          new[] { "game-placeholder" } },
        { "Asset default de sorteos", "src/Ludeka.Web/wwwroot/images/defaults/sorteo-default.svg",
          new[] { "SORTEO LUDEKA", "viewBox=\"0 0 400 225\"" },
          new[] { "game-placeholder" } },
        { "Asset default de novedades", "src/Ludeka.Web/wwwroot/images/defaults/novedad-default.svg",
          new[] { "NOVEDAD", "viewBox=\"0 0 400 225\"" },
          new[] { "game-placeholder" } },
        { "Asset default genérico", "src/Ludeka.Web/wwwroot/images/defaults/generico-default.svg",
          new[] { "LUDEKA", "viewBox=\"0 0 400 225\"" },
          new[] { "game-placeholder" } },

        // Fundación CSS de microinteracciones (Decisión 4): tokens compartidos + .rail-card
        // + tipografía display del hero y de los títulos de carril (Decisiones 7 y 10)
        { "Fundación CSS (tokens, rail-card y hero)", "src/Ludeka.Web/Styles/input.css",
          new[] { "--ease-out-expo", "--ease-out-quad", "--dur-fast", "--dur-base", "--dur-slow", "--rail-lift", "--rail-zoom", "--font-display: 'Fraunces'",
                  ".rail-card:hover, .rail-card:focus-visible", ".rail-cover--square", ".rail-cover--wide", ".rail-cover--banner",
                  ".scrollbar-none", "prefers-reduced-motion: reduce", ".hero-title", ".rail-title", ".hero-scrim" },
          Array.Empty<string>() },

        // HeroEditorial: hero narrativo con <picture> AVIF/WebP/JPG priorizado (patrón INC-07),
        // escena CSS bajo la foto, scrim por tema y titular en serif display (Decisiones 1, 2 y 10)
        { "HeroEditorial (picture, prioridad y escena CSS)", "src/Ludeka.Web/Components/Home/HeroEditorial.razor",
          new[] { "<picture", "<source type=\"image/avif\"", "<source type=\"image/webp\"",
                  "fetchpriority=\"high\"", "width=\"1600\"", "height=\"900\"",
                  "alt=\"@HeroBackgroundAssets.AltText(Background)\"", "hero-scrim", "@switch (Background)",
                  "<h1 class=\"hero-title\">La mesa está servida</h1>" },
          new[] { "PORTADA EDITORIAL", "alt=\"\"" } },

        // RailHeader: cabecera de carril reutilizable con título en serif display, icono Lucide
        // y enlace "Ver todos…" solo cuando hay destino (Decisiones 3 y 7)
        { "RailHeader (cabecera de carril)", "src/Ludeka.Web/Components/Home/RailHeader.razor",
          new[] { "rail-title", "Icon", "href=\"@Href\"" },
          EmojisDePortada },

        // HomeGameCard: carril Top 20 con lenguaje .rail-card y fallback vigente de carátulas
        { "HomeGameCard (Top 20)", "src/Ludeka.Web/Components/Home/HomeGameCard.razor",
          new[] { "rail-card", "rail-cover--square", "game-placeholder.svg", "expansion-placeholder.svg", "loading=\"lazy\"", "onerror" },
          EmojisDePortada },

        // HomeGiveawayCard: carril Sorteos; estrena render de ThumbnailUrl con fallback por dominio
        { "HomeGiveawayCard (Sorteos)", "src/Ludeka.Web/Components/Home/HomeGiveawayCard.razor",
          new[] { "rail-card", "rail-cover--wide", "sorteo-default.svg", "DefaultImage", "loading=\"lazy\"", "onerror" },
          EmojisDePortada },

        // HomeReleaseCard: carril Novedades; estrena render de CoverImageUrl con fallback por dominio
        { "HomeReleaseCard (Novedades)", "src/Ludeka.Web/Components/Home/HomeReleaseCard.razor",
          new[] { "rail-card", "rail-cover--wide", "novedad-default.svg", "DefaultImage", "loading=\"lazy\"", "onerror" },
          EmojisDePortada },

        // HomeEventCard: carril Eventos; añade dimensiones y fallback que hoy no tiene
        { "HomeEventCard (Eventos)", "src/Ludeka.Web/Components/Home/HomeEventCard.razor",
          new[] { "rail-card", "rail-cover--banner", "evento-default.svg", "DefaultImage", "loading=\"lazy\"", "onerror", "width=", "height=" },
          EmojisDePortada },
    };

    [Theory]
    [MemberData(nameof(MarkupContracts))]
    public void Source_FulfillsMarkupContract(string description, string relativePath, string[] mustContain, string[] mustNotContain)
    {
        var source = ReadSource(relativePath);

        foreach (var fragment in mustContain)
        {
            Assert.True(source.Contains(fragment, StringComparison.Ordinal),
                $"{description}: el archivo {relativePath} debe contener '{fragment}'.");
        }

        foreach (var fragment in mustNotContain)
        {
            Assert.False(source.Contains(fragment, StringComparison.Ordinal),
                $"{description}: el archivo {relativePath} no debe contener '{fragment}'.");
        }
    }

    [Fact]
    public void HomeDashboard_Pills_AreExactlyTheFourD4PillsInOrder()
    {
        var source = ReadSource("src/Ludeka.Web/Components/Pages/HomeDashboard.razor");
        var start = source.IndexOf("@* Píldoras de acceso directo *@", StringComparison.Ordinal);
        var end = source.IndexOf("@if (_isLoading)", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "No se encontró el bloque de píldoras del hero.");
        var pillBlock = source[start..end];

        // Exactamente 4 píldoras D4, en orden: Catálogo Completo, Sorteos, Novedades, Eventos
        Assert.Equal(4, System.Text.RegularExpressions.Regex.Matches(pillBlock, "href=\"").Count);

        var positions = new List<int>();
        foreach (var destination in new[] { "/catalogo", "/sorteos", "/novedades", "/eventos" })
        {
            var pos = pillBlock.IndexOf($"href=\"{destination}\"", StringComparison.Ordinal);
            Assert.True(pos >= 0, $"Falta la píldora con destino {destination}.");
            positions.Add(pos);
        }
        Assert.Equal(positions.OrderBy(p => p).ToList(), positions);
    }

    [Fact]
    public void RailHeader_EnlaceVerTodos_SeRenderizaSoloConDestinoYRespetaElHref()
    {
        // Retarget tras la extracción (Decisión 3): el enlace "Ver todas las novedades"
        // pasa a vivir en RailHeader.razor y su destino lo fija el Href del orquestador
        // (paridad protegida en la entrada del orquestador del C3).
        var source = ReadSource("src/Ludeka.Web/Components/Home/RailHeader.razor");

        // El enlace se emite con el Href recibido y solo cuando hay destino (Eventos no tiene)
        Assert.Contains("<a href=\"@Href\"", source);
        Assert.Contains("@if (Href is not null)", source);
    }

    [Fact]
    public void App_razor_CargaFrauncesEnLaMismaPeticionDeFuentesSinNuevoEnlace()
    {
        // Decisión 7 (INC-35): la serif display Fraunces entra en la URL ya existente de
        // Google Fonts (misma petición css2, display=swap intacto, sin preload).
        var source = ReadSource("src/Ludeka.Web/Components/App.razor");

        Assert.Contains("family=Fraunces:opsz,wght@9..144,600..700", source);
        Assert.Contains("display=swap", source);

        // No se añade ningún <link> de fuente nuevo: una única URL css2 y las mismas
        // tres hojas de estilo que había antes del incremento.
        Assert.Equal(1, System.Text.RegularExpressions.Regex.Matches(source, @"fonts\.googleapis\.com/css2").Count);
        Assert.Equal(3, System.Text.RegularExpressions.Regex.Matches(source, "rel=\"stylesheet\"").Count);
    }

    // ===== INC-35 PR-2: hero editorial narrativo (Decisiones 1, 2 y 10) =====

    private static Type? GetHeroBackgroundVariantType() =>
        typeof(Ludeka.Web.Components.Pages.HomeDashboard).Assembly
            .GetType("Ludeka.Web.Components.Home.HeroBackgroundVariant", throwOnError: false);

    private static Type? GetHeroBackgroundAssetsType() =>
        typeof(Ludeka.Web.Components.Pages.HomeDashboard).Assembly
            .GetType("Ludeka.Web.Components.Home.HeroBackgroundAssets", throwOnError: false);

    [Fact]
    public void HeroBackgroundVariant_ExponeLasCincoVariantesDelDiseno()
    {
        // Decisión 2: 3 fotos de ambiente, escena CSS de serie e ilustración futura.
        var variantType = GetHeroBackgroundVariantType();
        Assert.NotNull(variantType);
        Assert.Equal(
            new[] { "FotoEurogame", "FotoMesaAmigos", "FotoPrimerPlano", "CssScene", "Ilustracion" },
            Enum.GetNames(variantType!));
    }

    [Theory]
    [InlineData("FotoEurogame", "hero-ambiente-eurogame")]
    [InlineData("FotoMesaAmigos", "hero-ambiente-mesa-amigos")]
    [InlineData("FotoPrimerPlano", "hero-ambiente-primer-plano")]
    [InlineData("Ilustracion", "hero-ilustracion")]
    public void HeroBackgroundAssets_MapeaCadaVarianteFotoASusTresFormatos(string variantName, string baseName)
    {
        var variantType = GetHeroBackgroundVariantType();
        Assert.NotNull(variantType);
        var assetsType = GetHeroBackgroundAssetsType();
        Assert.NotNull(assetsType);

        var variant = Enum.Parse(variantType!, variantName);
        foreach (var (methodName, extension) in new[] { ("Avif", ".avif"), ("Webp", ".webp"), ("Jpg", ".jpg") })
        {
            var method = assetsType!.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method);
            var resolved = method!.Invoke(null, new[] { variant }) as string;
            Assert.Equal($"/images/home/{baseName}{extension}", resolved);
        }
    }

    [Fact]
    public void HeroBackgroundAssets_AltTextosDeFotoEnCastellanoNoVacios()
    {
        // Decisión 10: alt descriptivo en castellano por variante con imagen; la escena
        // CSS no renderiza <img>, así que su alt queda vacío por diseño.
        var variantType = GetHeroBackgroundVariantType();
        Assert.NotNull(variantType);
        var assetsType = GetHeroBackgroundAssetsType();
        Assert.NotNull(assetsType);
        var altTextMethod = assetsType!.GetMethod("AltText", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(altTextMethod);

        var expectedByVariant = new (string VariantName, string ExpectedAlt)[]
        {
            ("FotoEurogame", "Mesa de juego con un eurogame en marcha sobre el tapete y una estantería lúdica al fondo"),
            ("FotoMesaAmigos", "Grupo de amigos riendo alrededor de una mesa de madera con juegos de mesa"),
            ("FotoPrimerPlano", "Primer plano de manos colocando piezas sobre el tablero de un juego de mesa"),
            ("Ilustracion", "Ilustración editorial de una mesa de juego con estantería al fondo"),
        };

        foreach (var (variantName, expectedAlt) in expectedByVariant)
        {
            var variant = Enum.Parse(variantType!, variantName);
            var alt = altTextMethod!.Invoke(null, new[] { variant }) as string;
            Assert.False(string.IsNullOrWhiteSpace(alt), $"El alt de {variantName} debe ser descriptivo.");
            Assert.Equal(expectedAlt, alt);
        }

        var cssScene = Enum.Parse(variantType!, "CssScene");
        Assert.Equal(string.Empty, altTextMethod!.Invoke(null, new[] { cssScene }));
    }

    [Fact]
    public void HeroEditorial_RamaCssScene_NoRenderizaPicture()
    {
        // Escenario «Escena CSS sin peticiones de imagen»: en la rama del @switch
        // correspondiente a CssScene el <picture> no existe (troceado de fuente).
        var source = ReadSource("src/Ludeka.Web/Components/Home/HeroEditorial.razor");
        var casePos = source.IndexOf("case HeroBackgroundVariant.CssScene", StringComparison.Ordinal);
        Assert.True(casePos >= 0, "El hero debe conmutar el fondo con @switch sobre Background (rama CssScene).");
        var breakPos = source.IndexOf("break;", casePos, StringComparison.Ordinal);
        Assert.True(breakPos > casePos, "La rama CssScene del @switch debe terminar en break;");
        var cssSceneBranch = source[casePos..breakPos];
        Assert.DoesNotContain("<picture", cssSceneBranch, StringComparison.Ordinal);
    }
}
