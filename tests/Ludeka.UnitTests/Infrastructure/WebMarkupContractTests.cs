using System;
using System.IO;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

/// <summary>
/// Contrato de markup por archivo fuente (INC-31): lee los .razor/.cs desde la raíz del repo
/// y afirma invariantes estructurales contiene/no-contiene. El render real se verifica en
/// sdd-verify con `dotnet run` (limitación documentada en el diseño: no prueba el DOM).
/// </summary>
public class WebMarkupContractTests
{
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
}
