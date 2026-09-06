# Tasks: Incremento 2 — Ludoteca Personal, Colección en 4 Estados y Préstamos (`change-02-library-loans`)

## Review Workload Forecast

| Field | Value |
|---|---|
| Estimated changed lines | ~600-800 líneas |
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
| 1 | Dominio, Aplicación y Persistencia SQLite | PR 1 | `dotnet test --filter Category=Unit` | N/A (bibliotecas de clases y tests) | `src/Ludeca.Core`, `Ludeca.Application`, `Ludeca.Infrastructure` |
| 2 | Componentes Blazor y Ficha Interactiva | PR 2 | `dotnet test` | `dotnet run --project src/Ludeca.Web` | `src/Ludeca.Web/Components` |

---

## Phase 1: Dominio Lúdico y Reglas de Negocio (`src/Ludeca.Core`)

- [x] 1.1 Crear enums de dominio `CollectionStatus.cs` y `PlayContextType.cs` en `src/Ludeca.Core/Enums/`.
- [x] 1.2 Crear Value Objects inmutables `UserPlayerCountVote.cs` y `UserFamilyExperienceVote.cs` en `src/Ludeca.Core/ValueObjects/`.
- [x] 1.3 Implementar entidad `UserCollectionItem.cs` en `src/Ludeca.Core/Entities/` con invariantes de estado y fechas.
- [x] 1.4 Implementar entidad `GameLoan.cs` en `src/Ludeca.Core/Entities/` con validación de pertenencia, datos de prestatario y método `MarkAsReturned()`.
- [x] 1.5 Implementar entidad `UserGameReview.cs` en `src/Ludeca.Core/Entities/` con validación de nota `[1.0, 10.0]` y límite de 280 caracteres.
- [x] 1.6 Añadir método `UpdateLudistRating(double newAverage)` en `src/Ludeca.Core/Entities/Game.cs`.

## Phase 2: Contratos, DTOs y Servicios de Aplicación (`src/Ludeca.Application`)

- [x] 2.1 Definir interfaces `IUserCollectionRepository.cs`, `IGameLoanRepository.cs`, `IUserReviewRepository.cs` e `ICurrentUserService.cs` en `src/Ludeca.Application/Contracts/`.
- [x] 2.2 Crear DTOs de lectura y peticiones (`UserCollectionItemDto`, `GameLoanDto`, `UserReviewDto`, `UserLibrarySummaryDto`) en `src/Ludeca.Application/DTOs/UserLibraryDtos.cs`.
- [x] 2.3 Definir contrato `IUserLibraryService.cs` en `src/Ludeca.Application/Contracts/`.
- [x] 2.4 Implementar `UserLibraryService.cs` en `src/Ludeca.Application/Features/Library/` con orquestación de colección, préstamos, reseñas y recálculo de consensos.

## Phase 3: Persistencia SQLite EF Core 10 (`src/Ludeca.Infrastructure`)

- [x] 3.1 Actualizar `LudecaDbContext.cs` registrando `DbSet<UserCollectionItem>`, `DbSet<GameLoan>`, `DbSet<UserGameReview>` con índices y `.ToJson()`.
- [x] 3.2 Implementar `SqliteUserCollectionRepository.cs` en `src/Ludeca.Infrastructure/Data/`.
- [x] 3.3 Implementar `SqliteGameLoanRepository.cs` en `src/Ludeca.Infrastructure/Data/`.
- [x] 3.4 Implementar `SqliteUserReviewRepository.cs` en `src/Ludeca.Infrastructure/Data/`.
- [x] 3.5 Implementar `DefaultCurrentUserService.cs` en `src/Ludeca.Infrastructure/Services/`.

## Phase 4: Pruebas Unitarias y Persistencia (`tests/Ludeca.UnitTests`)

- [x] 4.1 Crear pruebas de dominio `UserCollectionItemTests.cs`, `GameLoanTests.cs` y `UserGameReviewTests.cs` en `tests/Ludeca.UnitTests/Domain/`.
- [x] 4.2 Crear pruebas de casos de uso `UserLibraryServiceTests.cs` en `tests/Ludeca.UnitTests/Application/`.
- [x] 4.3 Crear pruebas de persistencia SQLite en memoria `SqliteLibraryPersistenceTests.cs` en `tests/Ludeca.UnitTests/Infrastructure/`.
- [x] 4.4 Ejecutar suite completa con `dotnet test src/Ludeca.slnx` y confirmar 100% verde.

## Phase 5: Interfaz Web Blazor y Experiencia Mobile-First (`src/Ludeca.Web`)

- [x] 5.1 Crear componente `CollectionActionBar.razor` en `src/Ludeca.Web/Components/Shared/` con 4 estados e interacción táctil.
- [x] 5.2 Crear componente `LoanModal.razor` en `src/Ludeca.Web/Components/Shared/` con formulario de préstamo y devolución en 1 clic.
- [x] 5.3 Crear componente `ReviewBottomSheet.razor` en `src/Ludeca.Web/Components/Shared/` para valoración en 45 segundos.
- [x] 5.4 Crear componente `UserReviewCard.razor` en `src/Ludeca.Web/Components/Shared/` con tarjeta propia y edición.
- [x] 5.5 Integrar barra de colección, aviso de préstamo activo y tarjeta de opinión en `src/Ludeca.Web/Components/Pages/GameDetail.razor`.
- [x] 5.6 Implementar página `/mi-ludoteca` en `src/Ludeca.Web/Components/Pages/MyLibrary.razor` con 5 pestañas reactivas.
- [x] 5.7 Añadir enlace de navegación a "Mi Ludoteca" en `src/Ludeca.Web/Components/Layout/MainLayout.razor`.
- [x] 5.8 Registrar dependencias en `src/Ludeca.Web/Program.cs`.

## Phase 6: Verificación Final y Documentación (`sdd-verify`)

- [x] 6.1 Ejecutar `dotnet test` y verificar cero regresiones y 100% de tests pasando.
- [x] 6.2 Verificar navegación y micro-interacciones interactivas.
- [x] 6.3 Generar `verify-report.md` formal en español.
