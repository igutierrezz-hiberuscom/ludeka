# Design: Incremento 1: Catálogo Base, Ficha Inteligente y Semáforo de Escalabilidad

## Technical Approach
Implementación orientada al dominio (DDD) y Clean Architecture dividida en 4 capas desacopladas:
1. **Dominio (`Ludeca.Core`)**: Agregado raíz `Game` con identidad (`Guid Id`), `BggId`, `Slug` normalizado y títulos bilingües (`OriginalTitle`, `SpanishTitle`). Encapsula la lógica pura para el cálculo del semáforo de escalabilidad (`ScalabilityEntry` con estados `MustPlay`, `Recommended`, `NotRecommended`), resumen textual (`IdealPlayerCountText`), badges de ADN lúdico (`ConfrontationType`, `GameStyle`, `IsOfficialSolo`), accesibilidad (`IsAccessibleEarlier`, `LanguageDependence`, `TableFootprint`) y guía de fundas (`SleeveItem`).
2. **Aplicación (`Ludeca.Application`)**: Abstracciones `IGameRepository` y `IBggClient`, modelos DTO (`GameSummaryDto`, `GameDetailDto`), criterios de filtrado `GameFilterCriteria` ("Especial Parejas", "Mesa Familiar", rangos y texto) y casos de uso del catálogo.
3. **Infraestructura (`Ludeca.Infrastructure`)**: Persistencia con EF Core 10 y SQLite (`LudecaDbContext`), serializando colecciones de Value Objects con `.ToJson()`. Cliente `BggXmlApiClient` usando `System.Xml.Linq.XDocument` con rate limiting (2 req/s) y backoff para HTTP 202/429. Seeder offline `CatalogSeeder` que inicializa 25-50 títulos hispanos desde `seed-games.json` embebido.
4. **Presentación (`Ludeca.Web`)**: Blazor Interactive Server y Tailwind CSS mobile-first. Rutas `/` (Catálogo interactivo reactivo con debounce) y `/juegos/{slug}` (Ficha inteligente de lectura en 3s). Componentes modulares reutilizables.

## Architecture Decisions
| Decisión | Opciones | Tradeoffs | Decisión Seleccionada |
|---|---|---|---|
| **Modelo de Dominio** | A) Anémico con primitivas<br>B) Rico DDD con Value Objects | A dispersa reglas de semáforo en la UI.<br>B garantiza invariantes y testeo unitario puro. | **B (Rico DDD)**: `Game` encapsula cálculo de semáforo, slug y accesibilidad sin dependencias externas. |
| **Parser BGG XMLAPI2** | A) `XmlSerializer`<br>B) `System.Xml.Linq.XDocument`<br>C) `XmlReader` | A falla ante esquemas polimórficos de BGG.<br>B aporta máxima tolerancia a fallos y navegación segura contra nulos. | **B (`XDocument`)**: Extracción flexible y tipada de encuestas anidadas (`poll`) y títulos alternativos en español. |
| **Persistencia Local** | A) Repositorio en memoria<br>B) SQLite con EF Core 10<br>C) LiteDB | A es volátil y no valida LINQ.<br>B ofrece persistencia real, soporte `.ToJson()` en EF Core 10 y portabilidad a PostgreSQL. | **B (SQLite EF Core 10)**: Archivo `ludeca.db` con índices sobre slug y títulos. |
| **Operación Offline-First** | A) Dependencia 100% online BGG<br>B) Seeder JSON local curado | A genera bloqueos HTTP 429 y latencia en primer arranque.<br>B garantiza catálogo instantáneo sin conexión. | **B (`CatalogSeeder`)**: Carga automática de 25-50 juegos top en español desde JSON si la BD está vacía. |
| **Renderizado Blazor** | A) Blazor WebAssembly<br>B) Blazor Interactive Server | A requiere descarga inicial pesada de runtime WASM.<br>B ofrece SSR instantáneo y acceso directo a base de datos. | **B (Interactive Server)**: Respuesta rápida móvil y sincronización reactiva de filtros. |

## Data Flow
```
[Navegador / Blazor UI]
    | (1) Búsqueda reactiva / Navegación slug (/juegos/{slug})
    v
[Blazor Pages: CatalogPage / GameDetailPage]
    | (2) Invoca Query / Caso de Uso
    v
[Ludeca.Application (IGameRepository)]
    | (3) Consulta LINQ / SearchAsync / GetBySlugAsync
    v
[Ludeca.Infrastructure (SqliteGameRepository & LudecaDbContext)]
    | (4) Lectura relacional + JSON Value Objects
    v
[(Base de Datos SQLite: ludeca.db)]
    ^
    | (Carga inicial si tabla Games está vacía)
[CatalogSeeder (seed-games.json)]
    ^
    | (Enriquecimiento opcional de catálogo)
[BggXmlApiClient (Rate Limiter 2 req/s + XDocument Parser)]
    ^
    | (Llamadas HTTP seguras a XMLAPI2)
[BoardGameGeek API]
```

## File Changes
| Archivo | Acción | Descripción |
|---|---|---|
| `src/Ludeca.Core/Enums/*.cs` | Crear | Enumeraciones: `ConfrontationType`, `GameStyle`, `ScalabilityStatus`, `LanguageDependence`, `TableFootprint`. |
| `src/Ludeca.Core/ValueObjects/*.cs` | Crear | Value Objects: `ScalabilityEntry`, `SleeveItem`, `GameDuration`, `AgeRating`. |
| `src/Ludeca.Core/Entities/Game.cs` | Crear | Entidad agregada raíz con lógica de slug, semáforo, ADN y accesibilidad. |
| `src/Ludeca.Application/Contracts/IGameRepository.cs` | Crear | Abstracción de persistencia y consultas multi-criterio. |
| `src/Ludeca.Application/Contracts/IBggClient.cs` | Crear | Abstracción del cliente de integración con BGG XMLAPI2. |
| `src/Ludeca.Application/DTOs/*.cs` | Crear | Modelos DTO: `GameSummaryDto`, `GameDetailDto`, `GameFilterCriteria`. |
| `src/Ludeca.Application/Queries/*.cs` | Crear | Casos de uso `GetCatalogGamesQuery`, `GetGameDetailBySlugQuery`. |
| `src/Ludeca.Infrastructure/Data/LudecaDbContext.cs` | Crear | Contexto EF Core 10 con SQLite y mapeo de Value Objects vía `.ToJson()`. |
| `src/Ludeca.Infrastructure/Data/SqliteGameRepository.cs` | Crear | Repositorio SQLite con consultas compiladas y búsqueda bilingüe. |
| `src/Ludeca.Infrastructure/Bgg/BggXmlApiClient.cs` | Crear | Cliente HTTP con rate limiter (2 req/s) y parser LINQ-to-XML. |
| `src/Ludeca.Infrastructure/Seeding/CatalogSeeder.cs` | Crear | Servicio de siembra automática desde archivo JSON embebido. |
| `src/Ludeca.Infrastructure/Seeding/seed-games.json` | Crear | Catálogo curado de 25-50 títulos populares en español. |
| `src/Ludeca.Web/Components/*.razor` | Crear | Componentes: `GameCard`, `ScalabilityTrafficLight`, `QuickBadges`, `SleeveGuideCard`, `CatalogSearchBar`. |
| `src/Ludeca.Web/Pages/CatalogPage.razor` | Crear | Catálogo `/` y `/catalogo` con buscador y filtros predefinidos. |
| `src/Ludeca.Web/Pages/GameDetailPage.razor` | Crear | Ficha `/juegos/{slug}` con lectura en 3s y guía de fundas. |
| `src/Ludeca.Web/wwwroot/app.css` | Modificar | Integración de utilidades Tailwind para semáforos y ADN. |
| `src/Ludeca.Web/Program.cs` | Modificar | Inyección de dependencias, migración automática SQLite y seeder. |
| `tests/Ludeca.UnitTests/Domain/*.cs` | Crear | Tests unitarios de reglas de semáforo, slug, edad y VOs. |
| `tests/Ludeca.UnitTests/Bgg/*.cs` | Crear | Tests unitarios de parsing XML BGG con fixtures. |

## Interfaces / Contracts
```csharp
namespace Ludeca.Core.Entities;

public enum ConfrontationType { Cooperative, Competitive, HiddenRolesOrTeams, SemiCooperative }
public enum GameStyle { Eurogame, Ameritrash, PartyGame, FillerAbstract, NarrativeCampaign }
public enum ScalabilityStatus { MustPlay, Recommended, NotRecommended }
public enum LanguageDependence { None, Low, High }
public enum TableFootprint { SmallTable, StandardTable, TableMonster }

public record ScalabilityEntry(int PlayerCount, string DisplayCount, ScalabilityStatus Status, int BestVotes, int RecommendedVotes, int NotRecommendedVotes);
public record SleeveItem(string FormatName, double WidthMm, double HeightMm, int CardCount, string? AffiliateUrl);
public record AgeRating(int BoxAge, int CommunityAge) { public bool IsAccessibleEarlier => CommunityAge < BoxAge; }
public record GameDuration(int MinMinutes, int MaxMinutes, int EstimatedPerPlayerMinutes);

public class Game
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int BggId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string OriginalTitle { get; private set; } = string.Empty;
    public string SpanishTitle { get; private set; } = string.Empty;
    public int YearPublished { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public string? Description { get; private set; }
    public double BggRating { get; private set; }
    public int? BggRank { get; private set; }
    public double LudistRating { get; private set; }
    public ConfrontationType Confrontation { get; private set; }
    public GameStyle Style { get; private set; }
    public bool IsOfficialSolo { get; private set; }
    public AgeRating Age { get; private set; } = null!;
    public LanguageDependence Language { get; private set; }
    public TableFootprint Footprint { get; private set; }
    public GameDuration Duration { get; private set; } = null!;
    public List<ScalabilityEntry> Scalability { get; private set; } = [];
    public List<SleeveItem> Sleeves { get; private set; } = [];
    public string IdealPlayerCountText => CalculateIdealPlayerCountText();

    public static string GenerateSlug(string title) => /* Normalización en minúsculas sin tildes */ string.Empty;
    private string CalculateIdealPlayerCountText() => /* Lógica 'Ideal: X jugadores' o 'Ideal: X-Y jugadores' */ string.Empty;
}
```

```csharp
namespace Ludeca.Application.Contracts;

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

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Game?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Game?> GetByBggIdAsync(int bggId, CancellationToken ct = default);
    Task<(IReadOnlyList<Game> Items, int TotalCount)> SearchAsync(GameFilterCriteria criteria, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Game> games, CancellationToken ct = default);
    Task<bool> HasAnyAsync(CancellationToken ct = default);
}

public interface IBggClient
{
    Task<Game?> FetchGameByBggIdAsync(int bggId, CancellationToken ct = default);
}
```

```csharp
namespace Ludeca.Application.DTOs;

public record GameSummaryDto(
    Guid Id,
    int BggId,
    string Slug,
    string SpanishTitle,
    string OriginalTitle,
    int YearPublished,
    string? CoverImageUrl,
    double BggRating,
    int? BggRank,
    string IdealPlayerCountText,
    ConfrontationType Confrontation,
    GameStyle Style,
    bool IsOfficialSolo,
    bool IsAccessibleEarlier,
    int CommunityAge,
    int EstimatedPerPlayerMinutes
);

public record GameDetailDto(
    Guid Id,
    int BggId,
    string Slug,
    string SpanishTitle,
    string OriginalTitle,
    int YearPublished,
    string? CoverImageUrl,
    string? Description,
    double BggRating,
    int? BggRank,
    double LudistRating,
    string IdealPlayerCountText,
    ConfrontationType Confrontation,
    GameStyle Style,
    bool IsOfficialSolo,
    AgeRating Age,
    LanguageDependence Language,
    TableFootprint Footprint,
    GameDuration Duration,
    IReadOnlyList<ScalabilityEntry> Scalability,
    IReadOnlyList<SleeveItem> Sleeves
);
```

## Testing Strategy
| Nivel | Componente / Suite | Estrategia de Prueba |
|---|---|---|
| **Unit** | `Ludeca.Core` (Dominio) | - Generación determinista de slugs (tildes, diacríticos y caracteres especiales).<br>- Cálculo del semáforo: asignación de estados `MustPlay`, `Recommended`, `NotRecommended`.<br>- Cálculo de etiqueta `IdealPlayerCountText` (conteo único o rango).<br>- Regla `IsAccessibleEarlier` y estimación de tiempos. |
| **Unit / Integration** | `BggXmlApiClient` (Infraestructura) | - Pruebas con fixtures XML BGG (nombres traducidos en `<name type="alternate">`, encuestas `<poll>`).<br>- Simulación de rate limiting y reintentos ante HTTP 202/429 con `HttpMessageHandler`. |
| **Integration** | `SqliteGameRepository` + `LudecaDbContext` | - Base de datos SQLite temporal en memoria.<br>- Persistencia y lectura de listas serializadas en `.ToJson()` (`Scalability`, `Sleeves`).<br>- Búsqueda multi-criterio (`SearchAsync`) en títulos bilingües y filtros ("Especial Parejas", "Mesa Familiar"). |
| **Integration** | `CatalogSeeder` | - Idempotencia de siembra (no duplica si la tabla contiene datos).<br>- Validación de carga íntegra de 25-50 títulos curados. |
| **UI Component** | Componentes Blazor | - Renderizado visual del semáforo de escalabilidad con estilos de contraste (🟢/🟡/🔴).<br>- Renderizado accesible de badges de ADN lúdico y guía de fundas. |

## Threat Matrix
| Amenaza / Riesgo | Nivel | Mitigación |
|---|---|---|
| **Bloqueo HTTP 429 / 202 por BGG XMLAPI2** | Medio | Operación *Offline-First* con `CatalogSeeder` local; rate limiter (2 req/s) y reintentos con espera exponencial en `BggXmlApiClient`. |
| **Inyección SQL en búsquedas del catálogo** | Bajo | Consultas parametrizadas mediante Entity Framework Core 10 y LINQ sobre SQLite. |
| **XSS vía títulos/descripciones importados** | Bajo | Escapado HTML nativo de Blazor Razor en el renderizado; saneamiento de marcado enriquecido. |
| **Bloqueo de base de datos SQLite por concurrencia** | Bajo | Activación de modo Write-Ahead Logging (`PRAGMA journal_mode=WAL;`) en SQLite para lecturas paralelas no bloqueantes. |

## Migration / Rollout
1. **Inicialización**: Ejecución de `EnsureCreatedAsync()` en `Program.cs` para generar `ludeca.db` si no existe.
2. **Siembra**: Ejecución de `CatalogSeeder.SeedAsync()` si la tabla `Games` está vacía.
3. **Despliegue**: Autónomo y autocontenido; no requiere servidores de base de datos externos.
4. **Plan de Reversión**: Revertir binarios y eliminar `ludeca.db` si se precisa un reinicio limpio de datos.

## Open Questions
- Ninguna pregunta bloqueante abierta. Las votaciones comunitarias por usuario de Ludist quedan desacopladas para el Incremento 3.
