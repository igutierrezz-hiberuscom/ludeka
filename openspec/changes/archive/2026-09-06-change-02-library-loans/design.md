# Diseño Técnico: Incremento 2 — Ludoteca Personal, Colección en 4 Estados y Préstamos (`change-02-library-loans`)

## Enfoque Técnico
Implementación de la gestión de colección personal, cuaderno de préstamos y formulario modular de valoración en 45 segundos siguiendo Clean Architecture y Domain-Driven Design (DDD). Se añaden entidades ricas en `Ludeca.Core`, orquestación en `Ludeca.Application`, persistencia SQLite con EF Core 10 (aprovechando `.ToJson()` para votos dinámicos) en `Ludeca.Infrastructure`, y componentes Blazor Server interactivos con Tailwind CSS en `Ludeca.Web`.

## Decisiones de Arquitectura

| Decisión | Opción Elegida | Alternativas Evaluadas | Justificación |
|---|---|---|---|
| **Modelo de Dominio** | Entidades independientes (`UserCollectionItem`, `GameLoan`, `UserGameReview`) | Embeber colecciones dentro de `Game` | Mantiene el catálogo público desacoplado de los datos privados de usuarios; previene problemas de concurrencia y volumen. |
| **Identidad de Usuario** | Abstracción `ICurrentUserService` con implementación local predeterminada | Bloquear desarrollo hasta integrar OAuth completo | Permite probar interactivamente la UI y los flujos sin retrasar el incremento; transición transparente a OAuth en Incremento 3/4. |
| **Persistencia de Votos de Comensales** | Mapeo nativo JSON en EF Core 10 (`OwnsMany().ToJson()`) | Tablas relacionales normalizadas para cada voto | Menor sobrecarga de joins para lecturas de 1J a 7J+; esquema ágil que refleja el patrón ya establecido en `ScalabilityEntry`. |
| **Micro-interacciones en UI** | Componentes Razor interactivos con Tailwind CSS (`InteractiveServer`) | Formularios con recarga completa de página | Respuesta instantánea al pulsar botones de colección, feedback háptico/visual táctil y actualización en vivo del contador de 280 caracteres. |

## Flujo de Datos

```
[Usuario / Navegador]
       │ (1. Clic en estado / préstamo / valoración)
       ▼
[CollectionActionBar / ReviewBottomSheet / LoanModal] (Blazor Server)
       │ (2. Invoca UserLibraryService con DTOs)
       ▼
[UserLibraryService] (Ludeca.Application)
       │ (3. Verifica invariantes de dominio: GameLoan / UserGameReview)
       ▼
[LudecaDbContext & Repositorios SQLite] (Ludeca.Infrastructure)
       │ (4. Persiste en sqlite con mapeo JSON e índices)
       ▼
[Game.UpdateLudistRating()] (Recalcula consenso comunitario si aplica)
```

## Cambios en Archivos

| Archivo | Acción | Descripción |
|---|---|---|
| `src/Ludeca.Core/Enums/CollectionStatus.cs` | Crear | Enum de 4 estados: `InCollection`, `Played`, `Wishlist`, `WantToBuy` |
| `src/Ludeca.Core/Enums/PlayContextType.cs` | Crear | Enum de contexto: `Owned`, `ClubOrAssociation`, `Friends`, `BoardGameBar`, `Bga` |
| `src/Ludeca.Core/ValueObjects/UserPlayerCountVote.cs` | Crear | Voto de comensal (1 a 7+) y semáforo personal |
| `src/Ludeca.Core/ValueObjects/UserFamilyExperienceVote.cs` | Crear | Experiencia infantil (edad sugerida y reglas adaptadas) |
| `src/Ludeca.Core/Entities/UserCollectionItem.cs` | Crear | Entidad de vínculo usuario-juego con estado lúdico |
| `src/Ludeca.Core/Entities/GameLoan.cs` | Crear | Entidad de préstamo privado con invariantes de pertenencia |
| `src/Ludeca.Core/Entities/UserGameReview.cs` | Crear | Entidad de micro-valoración (1–10, máx. 280 chars) |
| `src/Ludeca.Core/Entities/Game.cs` | Modificar | Método `UpdateLudistRating(double newAverage)` |
| `src/Ludeca.Application/Contracts/IUserCollectionRepository.cs` | Crear | Consultas y operaciones de colección |
| `src/Ludeca.Application/Contracts/IGameLoanRepository.cs` | Crear | Consultas de préstamos activos e históricos y devolución |
| `src/Ludeca.Application/Contracts/IUserReviewRepository.cs` | Crear | Consultas de reseñas y agregados comunitarios |
| `src/Ludeca.Application/Contracts/ICurrentUserService.cs` | Crear | Abstracción del usuario autenticado |
| `src/Ludeca.Application/Contracts/IUserLibraryService.cs` | Crear | Orquestador de aplicación de ludoteca |
| `src/Ludeca.Application/DTOs/UserLibraryDtos.cs` | Crear | DTOs de colección, préstamo, reseña y peticiones |
| `src/Ludeca.Application/Features/Library/UserLibraryService.cs` | Crear | Lógica de negocio de la biblioteca de usuario |
| `src/Ludeca.Infrastructure/Data/LudecaDbContext.cs` | Modificar | DbSets e índices para colección, préstamos y reseñas |
| `src/Ludeca.Infrastructure/Data/SqliteUserCollectionRepository.cs` | Crear | Implementación EF Core SQLite |
| `src/Ludeca.Infrastructure/Data/SqliteGameLoanRepository.cs` | Crear | Implementación EF Core SQLite |
| `src/Ludeca.Infrastructure/Data/SqliteUserReviewRepository.cs` | Crear | Implementación EF Core SQLite |
| `src/Ludeca.Infrastructure/Services/DefaultCurrentUserService.cs` | Crear | Proveedor de usuario fundador local |
| `src/Ludeca.Web/Components/Shared/CollectionActionBar.razor` | Crear | Barra ergonómica de 4 estados para ficha |
| `src/Ludeca.Web/Components/Shared/LoanModal.razor` | Crear | Modal de préstamo y devolución en 1 toque |
| `src/Ludeca.Web/Components/Shared/ReviewBottomSheet.razor` | Crear | Formulario modular en 45 s |
| `src/Ludeca.Web/Components/Shared/UserReviewCard.razor` | Crear | Tarjeta destacada de valoración propia |
| `src/Ludeca.Web/Components/Pages/MyLibrary.razor` | Crear | Vista `/mi-ludoteca` organizada en pestañas |
| `src/Ludeca.Web/Components/Pages/GameDetail.razor` | Modificar | Integración de barra, modales y tarjeta de opinión |
| `src/Ludeca.Web/Components/Layout/MainLayout.razor` | Modificar | Enlace en cabecera hacia "Mi Ludoteca" |
| `src/Ludeca.Web/Program.cs` | Modificar | Registro de servicios y repositorios en DI |
| `tests/Ludeca.UnitTests/Domain/UserCollectionItemTests.cs` | Crear | Tests de estados de colección |
| `tests/Ludeca.UnitTests/Domain/GameLoanTests.cs` | Crear | Tests de invariantes de préstamo y devolución |
| `tests/Ludeca.UnitTests/Domain/UserGameReviewTests.cs` | Crear | Tests de límites (nota 1–10, 280 chars) |
| `tests/Ludeca.UnitTests/Application/UserLibraryServiceTests.cs` | Crear | Tests de orquestación de aplicación |
| `tests/Ludeca.UnitTests/Infrastructure/SqliteLibraryPersistenceTests.cs` | Crear | Tests de persistencia e índices con SQLite en memoria |

## Contratos e Interfaces Principales

```csharp
public interface IUserLibraryService
{
    Task<UserCollectionItemDto?> GetCollectionStateAsync(Guid gameId, CancellationToken ct = default);
    Task<UserCollectionItemDto?> SetCollectionStateAsync(Guid gameId, CollectionStatus? status, CancellationToken ct = default);
    Task<GameLoanDto?> GetActiveLoanAsync(Guid gameId, CancellationToken ct = default);
    Task<GameLoanDto> CreateLoanAsync(Guid gameId, string borrowerName, DateTimeOffset loanDate, string? notes, CancellationToken ct = default);
    Task<GameLoanDto> ReturnLoanAsync(Guid loanId, CancellationToken ct = default);
    Task<UserReviewDto?> GetUserReviewAsync(Guid gameId, CancellationToken ct = default);
    Task<UserReviewDto> SubmitReviewAsync(Guid gameId, double score, string? microReview, List<UserPlayerCountVote> playerCountVotes, UserFamilyExperienceVote? familyExp, PlayContextType? context, CancellationToken ct = default);
    Task<UserLibrarySummaryDto> GetLibrarySummaryAsync(CancellationToken ct = default);
}
```

## Estrategia de Pruebas

| Capa | Qué se Prueba | Enfoque |
|---|---|---|
| **Dominio** | Invariantes de entidades (`GameLoan`, `UserGameReview`, `UserCollectionItem`) | Pruebas unitarias puras con xUnit (sin I/O) |
| **Aplicación** | `UserLibraryService`: transiciones de estado, prohibición de prestar si no es propio, recálculo de consenso | Pruebas unitarias con mocks/in-memory fakes |
| **Infraestructura** | Serialización JSON de comensales, índices únicos `(UserId, GameId)`, persistencia en SQLite | Pruebas de integración con SQLite in-memory |

## Threat Matrix
N/A — no routing, shell, subprocess, VCS/PR automation, executable-file classification, or process-integration boundary.

## Migración y Despliegue
`LudecaDbContext.Database.EnsureCreatedAsync()` se ejecuta al inicio en desarrollo; las nuevas tablas `UserCollectionItems`, `GameLoans` y `UserGameReviews` se añaden automáticamente sin romper la base existente.
