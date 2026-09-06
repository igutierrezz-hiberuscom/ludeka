# Tasks: Incremento 4 — Hub Multimedia (YouTube e Instagram) (`change-04-multimedia-hub`)

## Review Workload Forecast

| Field | Value |
|---|---|
| Estimated changed lines | ~700-900 líneas |
| 400-line budget risk | Medium |
| Chained PRs recommended | No |
| Suggested split | single-pr |
| Delivery strategy | exception-ok |
| Chain strategy | size-exception |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: size-exception
400-line budget risk: Medium

### Suggested Work Units

| Unit | Goal | Likely PR | Focused test command | Runtime harness | Rollback boundary |
|---|---|---|---|---|---|
| 1 | Dominio, Aplicación y Persistencia SQLite | PR 1 | `dotnet test --filter Category=Unit` | N/A (bibliotecas de clases y tests) | `src/Ludeka.Core`, `Ludeka.Application`, `Ludeka.Infrastructure` |
| 2 | Componentes Blazor, Hub y Panel de Moderación | PR 2 | `dotnet test` | `dotnet run --project src/Ludeka.Web` | `src/Ludeka.Web/Components` |

---

## Phase 1: Dominio Lúdico y Reglas de Negocio (`src/Ludeka.Core`)

- [x] 1.1 Crear enum `MediaType.cs` (`Tutorial`, `Playthrough`, `InstagramPost`, `ShortReel`) en `src/Ludeka.Core/Enums/`.
- [x] 1.2 Crear enum `MediaPlatform.cs` (`YouTube`, `Instagram`) en `src/Ludeka.Core/Enums/`.
- [x] 1.3 Crear enum `ModerationStatus.cs` (`PendingApproval`, `Approved`, `Rejected`) en `src/Ludeka.Core/Enums/`.
- [x] 1.4 Crear entidad `MediaItem.cs` en `src/Ludeka.Core/Entities/` con invariantes (URLs válidas, `PlayerCountBadge` obligatorio para `Playthrough`, `GameId` nullable para huérfanos, métodos `Approve()`, `Reject()`, `AssignToGame()`, `MarkAsBroken()`).

## Phase 2: Contratos, DTOs y Orquestación Multimedia (`src/Ludeka.Application`)

- [x] 2.1 Crear interfaces `IMediaRepository.cs`, `IMediaService.cs` e `IBrokenLinkCheckerService.cs` en `src/Ludeka.Application/Contracts/`.
- [x] 2.2 Crear DTOs en `src/Ludeka.Application/DTOs/MediaDtos.cs` (`MediaItemDto`, `GameMediaHubDto`, `ModerateMediaItemRequest`, `AssignMediaGameRequest`, `BrokenLinkReportDto`).
- [x] 2.3 Implementar `MediaService.cs` en `src/Ludeka.Application/Features/Media/` con lógica de segregación por pestañas, filtros de moderación, aprobación, descarte y asignación de huérfanos.

## Phase 3: Persistencia SQLite EF Core 10, Seeder y Detector de Enlaces (`src/Ludeka.Infrastructure`)

- [x] 3.1 Actualizar `LudekaDbContext.cs` en `src/Ludeka.Infrastructure/Data/` incorporando `DbSet<MediaItem>` con índices en `GameId`, `Status`, `Type`, `Platform` y relación opcional con `Game`.
- [x] 3.2 Implementar `SqliteMediaRepository.cs` en `src/Ludeka.Infrastructure/Data/`.
- [x] 3.3 Implementar `BrokenLinkCheckerService.cs` en `src/Ludeka.Infrastructure/Services/` para comprobación HTTP de enlaces.
- [x] 3.4 Actualizar `CatalogSeeder.cs` en `src/Ludeka.Infrastructure/Seeding/` con contenidos de referencia hispanos (Análisis-Parálisis, Zacatrus TV, El Troquel, Meepletopia, Rincón Legacy, 221B, etc.) para los 15 juegos base, más elementos en cola de moderación y contenidos huérfanos.

## Phase 4: Pruebas Unitarias e Integración (`tests/Ludeka.UnitTests`)

- [x] 4.1 Crear pruebas de dominio `MediaItemTests.cs` en `tests/Ludeka.UnitTests/Domain/` (validación de URLs, badge obligatorio en partidas, transiciones de estado y huérfanos).
- [x] 4.2 Crear pruebas de persistencia `SqliteMediaRepositoryTests.cs` en `tests/Ludeka.UnitTests/Infrastructure/` (consultas por juego, estado, huérfanos y concurrencia).
- [x] 4.3 Crear pruebas de servicio `MediaServiceTests.cs` en `tests/Ludeka.UnitTests/Application/` (segregación de 3 pestañas, aprobación en 1 clic y asignación de huérfanos).
- [x] 4.4 Ejecutar suite completa con `dotnet test` y confirmar 100% de tests en verde sin regresiones.

## Phase 5: Componentes UI Blazor y Hub Multimedia (`src/Ludeka.Web`)

- [x] 5.1 Crear modal reproductor `MediaEmbedModal.razor` en `src/Ludeka.Web/Components/Shared/` con iframe seguro responsive y botón de enlace externo.
- [x] 5.2 Crear componente `MultimediaHub.razor` en `src/Ludeka.Web/Components/Shared/` con 3 pestañas horizontales segregadas (16:9 tutoriales, 16:9 partidas con badge de jugadores, 1:1 posts y 9:16 reels) y microtextos editoriales.
- [x] 5.3 Crear página táctil móvil `MediaModeration.razor` en `src/Ludeka.Web/Components/Pages/` con ruta `/moderacion/multimedia`, tabs de moderación (Pendientes, Huérfanos, Aprobados), acciones en 1 clic, asignador de juegos y detector de enlaces rotos.
- [x] 5.4 Modificar `GameDetail.razor` en `src/Ludeka.Web/Components/Pages/` integrando `MultimediaHub.razor` y cargando datos multimedia.
- [x] 5.5 Modificar `MainLayout.razor` en `src/Ludeka.Web/Components/Layout/` incorporando el enlace directo `[ 🎬 Moderar Medios ]` visible para roles `FoundingTeam` y `Moderator`.
- [x] 5.6 Registrar repositorios y servicios multimedia en `Program.cs` de `Ludeka.Web`.

## Phase 6: Verificación en Vivo y Cierre del Incremento

- [x] 6.1 Compilación de la solución completa sin errores ni advertencias de código.
- [x] 6.2 Ejecución de suite de tests automatizados (30 nuevos tests, 101 tests en total superados al 100%).
- [x] 6.3 Verificación de navegación y renderizado visual en navegador.
- [x] 6.4 Generación del informe de verificación `verify-report.md` y sincronización en memoria persistente Engram.
