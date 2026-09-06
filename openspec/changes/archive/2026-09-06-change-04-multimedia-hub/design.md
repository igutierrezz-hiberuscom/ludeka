# Diseño Técnico: Incremento 4 — Hub Multimedia (YouTube e Instagram) (`change-04-multimedia-hub`)

## Enfoque Técnico
Implementación del Hub Multimedia para Ludeka bajo Clean Architecture en .NET 10 (C# 13) y Blazor Web App con Tailwind CSS. Se garantiza la segregación visual estricta de formatos (16:9, 1:1 y 9:16), un modelo de dominio rico con invariantes de comensales y contenidos huérfanos, ingesta acotada con seeder curado de divulgadores hispanos, y un panel táctil de moderación móvil en 1 clic con detector de enlaces rotos.

---

## Decisiones de Arquitectura

| Decisión | Opción Elegida | Alternativas Evaluadas | Justificación |
|---|---|---|---|
| **Modelo de Datos Multimedia** | Entidad `MediaItem` con clave foránea nullable `GameId` | Colección JSON embebida dentro de `Game` | Permite gestionar elementos huérfanos sin juego asociado, filtrar por plataforma/estado de moderación global y actualizar estados sin bloquear el agregado de juego. |
| **Segregación Visual de Formatos** | 3 pestañas horizontales independientes (`Tutoriales`, `Partidas Completas`, `Opiniones y Redes`) | Feed único mezclado tipo muro | Evita la deformación estética ("AI slop") al coexistir vídeos horizontales 16:9, posts cuadrados 1:1 y vídeos verticales 9:16. Cumple el punto 5.1 de la Spec Maestra. |
| **Invariante de Comensales en Partidas** | Obligatoriedad estricta de `PlayerCountBadge` (ej. `"Partida a 2"`) en `Playthrough` | Campo opcional | El valor diferencial para el usuario de Ludeka es saber al instante a cuántos jugadores se grabó la partida antes de reproducirla. |
| **Reproductor Embebido Híbrido** | Modal `MediaEmbedModal.razor` con iframe responsive `youtube-nocookie.com` + enlace externo oficial | Abrir siempre en ventana nueva o iframe en línea | Ofrece reproducción inmediata sin abandonar el contexto de la ficha ni desmaquetar la página en pantallas móviles, con salida limpia al canal original. |
| **Moderación Rápida Móvil** | Ruta `/moderacion/multimedia` con diseño táctil para pulgar en 1 clic | Panel de administración de escritorio pesado | El equipo fundador y los moderadores gestionan la plataforma desde el móvil en ferias o ratos libres; requiere ergonomía de 1 toque (`Aprobar` / `Descartar`). |
| **Detector de Enlaces Rotos** | Servicio `IBrokenLinkCheckerService` con marcado `IsBroken` | Borrado físico automático inmediato | Permite revisar antes de purgar o corregir URLs que hayan cambiado de privacidad temporalmente. |

---

## Flujo de Datos

```
[Usuario en Ficha de Juego]
       │
       ▼
[GameDetail.razor] ───────► Inyecta IMediaService.GetGameMediaAsync(gameId)
       │
       ▼
[MultimediaHub.razor] ────► 3 Pestañas Horizontales
       ├─► Pestaña 1: [Tutoriales 16:9] ─────► Click ──► [MediaEmbedModal] (Reproductor iframe)
       ├─► Pestaña 2: [Partidas 16:9 + Badge] ─► Click ──► [MediaEmbedModal] ("Partida a 2")
       └─► Pestaña 3: [Opiniones & Redes]
                       ├─► [Posts Instagram 1:1] (Foto, @autor, likes, extracto)
                       └─► [Shorts/Reels 9:16] (Duración, icono de reproducción)

[Moderador / FoundingTeam]
       │
       ▼
[/moderacion/multimedia] (MediaModeration.razor)
       │
       ├─► Pestaña "Pendientes" ──► [ ✅ Aprobar en 1 Clic ] / [ ❌ Descartar ]
       ├─► Pestaña "Huérfanos" ───► [ 🔗 Asignar Juego ] (Selector de títulos)
       ├─► Pestaña "Aprobados" ───► Listado público activo
       └─► Acción [ 🔍 Comprobar Enlaces ] ──► Marca enlaces rotos HTTP 404
```

---

## Cambios en Archivos y Componentes

| Archivo | Acción | Capa | Descripción |
|---|---|---|---|
| `src/Ludeka.Core/Enums/MediaType.cs` | Crear | `Core` | Enum con tipos: `Tutorial`, `Playthrough`, `InstagramPost`, `ShortReel` |
| `src/Ludeka.Core/Enums/MediaPlatform.cs` | Crear | `Core` | Enum con plataformas: `YouTube`, `Instagram` |
| `src/Ludeka.Core/Enums/ModerationStatus.cs` | Crear | `Core` | Enum de estados: `PendingApproval`, `Approved`, `Rejected` |
| `src/Ludeka.Core/Entities/MediaItem.cs` | Crear | `Core` | Entidad con invariantes de URL, `PlayerCountBadge` obligatorio para `Playthrough`, transiciones de estado y gestión de huérfanos |
| `src/Ludeka.Application/Contracts/IMediaRepository.cs` | Crear | `Application` | Contrato de persistencia con filtros por juego, estado y huérfanos |
| `src/Ludeka.Application/Contracts/IMediaService.cs` | Crear | `Application` | Contrato del servicio de orquestación multimedia y moderación |
| `src/Ludeka.Application/Contracts/IBrokenLinkCheckerService.cs` | Crear | `Application` | Contrato de verificación de enlaces rotos |
| `src/Ludeka.Application/DTOs/MediaDtos.cs` | Crear | `Application` | DTOs `MediaItemDto`, `GameMediaHubDto`, `ModerateMediaItemRequest`, `AssignMediaGameRequest`, `BrokenLinkReportDto` |
| `src/Ludeka.Application/Features/Media/MediaService.cs` | Crear | `Application` | Implementación de lógica de negocio, segregación por pestañas y moderación |
| `src/Ludeka.Infrastructure/Data/LudekaDbContext.cs` | Modificar | `Infrastructure` | Adición de `DbSet<MediaItem>` con índices en `GameId`, `Status`, `Type`, `Platform` |
| `src/Ludeka.Infrastructure/Data/SqliteMediaRepository.cs` | Crear | `Infrastructure` | Implementación del repositorio con EF Core 10 y SQLite |
| `src/Ludeka.Infrastructure/Services/BrokenLinkCheckerService.cs` | Crear | `Infrastructure` | Implementación de verificación de disponibilidad de enlaces con `HttpClient` y fallback |
| `src/Ludeka.Infrastructure/Seeding/CatalogSeeder.cs` | Modificar | `Infrastructure` | Precarga de tutoriales, partidas a 2J y posts de Instagram para los 15 juegos base + cola de moderación y huérfanos |
| `src/Ludeka.Web/Components/Shared/MultimediaHub.razor` | Crear | `Web` | Componente de 3 pestañas con formato 16:9, 1:1 y 9:16 |
| `src/Ludeka.Web/Components/Shared/MediaEmbedModal.razor` | Crear | `Web` | Modal de reproducción de vídeo responsive |
| `src/Ludeka.Web/Components/Pages/MediaModeration.razor` | Crear | `Web` | Vista táctil móvil de moderación rápida en `/moderacion/multimedia` |
| `src/Ludeka.Web/Components/Pages/GameDetail.razor` | Modificar | `Web` | Integración de `MultimediaHub.razor` en la ficha lúdica |
| `src/Ludeka.Web/Components/Layout/MainLayout.razor` | Modificar | `Web` | Enlace contextual `[ 🎬 Moderar Medios ]` para moderadores y equipo fundador |
| `src/Ludeka.Web/Program.cs` | Modificar | `Web` | Registro de servicios multimedia en el contenedor de dependencias |
| `tests/Ludeka.UnitTests/Domain/MediaItemTests.cs` | Crear | `Tests` | Tests de invariantes de dominio y transiciones de estado |
| `tests/Ludeka.UnitTests/Application/MediaServiceTests.cs` | Crear | `Tests` | Tests de segregación por pestañas, aprobación, rechazo y huérfanos |
| `tests/Ludeka.UnitTests/Infrastructure/SqliteMediaRepositoryTests.cs` | Crear | `Tests` | Tests de integración con SQLite en memoria |

---

## Modelo de Dominio Detallado (`MediaItem.cs`)

```csharp
namespace Ludeka.Core.Entities;

public class MediaItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? GameId { get; private set; }
    public MediaType Type { get; private set; }
    public MediaPlatform Platform { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string? EmbedUrl { get; private set; }
    public string ThumbnailUrl { get; private set; } = string.Empty;
    public string AuthorChannel { get; private set; } = string.Empty;
    public int? DurationSeconds { get; private set; }
    public string? PlayerCountBadge { get; private set; }
    public int? LikesCount { get; private set; }
    public string? Excerpt { get; private set; }
    public ModerationStatus Status { get; private set; } = ModerationStatus.PendingApproval;
    public bool IsBroken { get; private set; }
    public DateTimeOffset PublishedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public bool IsOrphan => !GameId.HasValue;
    public string FormattedDuration => FormatDuration(DurationSeconds);

    // Métodos de negocio: Approve(), Reject(), AssignToGame(Guid), MarkAsBroken(bool)
}
```

---

## Estrategia de Pruebas

1. **Dominio:**
   - Validar que URLs vacías o inválidas lancen `ArgumentException`.
   - Validar que si `Type == Playthrough`, `PlayerCountBadge` sea estrictamente obligatorio.
   - Validar transiciones de `Approve()`, `Reject()`, `AssignToGame()` y `MarkAsBroken()`.
2. **Aplicación / Servicios:**
   - Validar que `GetGameMediaAsync()` devuelva colecciones segregadas exclusivamente de elementos `Approved` y no rotos.
   - Validar que `GetModerationQueueAsync()` pagine y filtre adecuadamente pendientes, huérfanos y aprobados.
   - Validar la asignación de un huérfano a un juego válido.
3. **Persistencia (SQLite):**
   - Validar inserción, relaciones opcionales con `Game`, consultas por índice y actualizaciones atómicas.
4. **Verificación Visual:**
   - Comprobación de navegación entre pestañas en `GameDetail.razor`.
   - Comprobación de reproducción embebida en `MediaEmbedModal.razor`.
   - Comprobación del flujo de moderación móvil en `/moderacion/multimedia`.
