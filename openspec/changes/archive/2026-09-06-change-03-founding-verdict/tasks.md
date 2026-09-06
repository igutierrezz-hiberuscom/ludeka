# Tasks: Incremento 3 — Panel y Veredicto de la Mesa Fundadora (`change-03-founding-verdict`)

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
| 2 | Componentes Blazor y Ficha Editorial | PR 2 | `dotnet test` | `dotnet run --project src/Ludeca.Web` | `src/Ludeca.Web/Components` |

---

## Phase 1: Dominio Lúdico y Reglas de Negocio (`src/Ludeca.Core`)

- [x] 1.1 Crear enum `FoundingRecommendation.cs` (`MustPlay`, `RecommendedWithAdaptations`, `Skippable`) en `src/Ludeca.Core/Enums/`.
- [x] 1.2 Crear Value Object inmutable `FoundingPhoto.cs` con `PhotoUrl` y `Caption` en `src/Ludeca.Core/ValueObjects/`.
- [x] 1.3 Implementar entidad/agregado `FoundingVerdict.cs` en `src/Ludeca.Core/Entities/` con invariantes de autoría, textos estructurados (general, 2 jugadores, familias) y límite de 3 fotos.

## Phase 2: Contratos, DTOs y Orquestación Editorial (`src/Ludeca.Application`)

- [x] 2.1 Actualizar contrato `ICurrentUserService.cs` en `src/Ludeca.Application/Contracts/` añadiendo roles (`Roles`, `IsFoundingTeam`, `IsInRole`, `SwitchRole`).
- [x] 2.2 Definir interfaces `IFoundingVerdictRepository.cs` e `IFoundingVerdictService.cs` en `src/Ludeca.Application/Contracts/`.
- [x] 2.3 Crear DTOs de veredicto, fotos, petición de guardado y resumen de IA en `src/Ludeca.Application/DTOs/FoundingVerdictDtos.cs`.
- [x] 2.4 Implementar `FoundingVerdictService.cs` en `src/Ludeca.Application/Features/Founding/` con lógica de sustitución de IA, autorización por rol y recálculo de `LudistRating`.

## Phase 3: Persistencia SQLite EF Core 10 y Semillas (`src/Ludeca.Infrastructure`)

- [x] 3.1 Actualizar `LudecaDbContext.cs` en `src/Ludeca.Infrastructure/Data/` incorporando `DbSet<FoundingVerdict>` con índice único en `GameId` y serialización JSON de fotos (`ToJson()`).
- [x] 3.2 Implementar `SqliteFoundingVerdictRepository.cs` en `src/Ludeca.Infrastructure/Data/`.
- [x] 3.3 Actualizar `DefaultCurrentUserService.cs` en `src/Ludeca.Infrastructure/Services/` con soporte mutable de roles en memoria y conmutación rápida.
- [x] 3.4 Actualizar `CatalogSeeder.cs` en `src/Ludeca.Infrastructure/Seeding/` con veredictos fundadores precargados y fotos reales para *Ark Nova*, *Wingspan* y *Carcassonne*, manteniendo otros títulos en Fase 1 (IA).

## Phase 4: Pruebas Unitarias e Integración (`tests/Ludeca.UnitTests`)

- [x] 4.1 Crear pruebas de dominio `FoundingVerdictTests.cs` en `tests/Ludeca.UnitTests/Domain/`.
- [x] 4.2 Crear pruebas de persistencia `SqliteFoundingVerdictRepositoryTests.cs` en `tests/Ludeca.UnitTests/Infrastructure/`.
- [x] 4.3 Crear pruebas de servicio y autorización `FoundingVerdictServiceTests.cs` en `tests/Ludeca.UnitTests/Application/`.
- [x] 4.4 Ejecutar suite completa con `dotnet test src/Ludeca.slnx` y confirmar 100% verde sin regresiones.

## Phase 5: Componentes UI Blazor y Experiencia Editorial (`src/Ludeca.Web`)

- [x] 5.1 Crear componente `FoundingVerdictCard.razor` en `src/Ludeca.Web/Components/Shared/` con insignia de sello, secciones de análisis y carrusel/galería de fotos de mesa real con visor modal.
- [x] 5.2 Crear componente `AiSummaryCard.razor` en `src/Ludeca.Web/Components/Shared/` con badge identificativo `🤖 Resumen generado por IA` y síntesis objetiva de escalabilidad, edad y huella.
- [x] 5.3 Crear modal `FoundingVerdictModal.razor` en `src/Ludeca.Web/Components/Shared/` con selector táctil de sello, campos de análisis y gestor de fotos en tiempo real.
- [x] 5.4 Modificar `GameDetail.razor` en `src/Ludeca.Web/Components/Pages/` integrando la sustitución visual condicional (IA vs Veredicto) y el botón `[ 🛡️ Gestionar Veredicto Fundador ]` para moderadores.
- [x] 5.5 Modificar `MainLayout.razor` en `src/Ludeca.Web/Components/Layout/` incorporando conmutador visual de rol en cabecera (`[ 🛡️ Modo Fundador ] / [ 👤 Modo Usuario ]`).
- [x] 5.6 Registrar servicios y repositorios en `Program.cs` de `Ludeca.Web`.

## Phase 6: Verificación en Vivo y Cierre del Incremento

- [x] 6.1 Compilación de la solución sin errores ni advertencias de código.
- [x] 6.2 Ejecución de la suite completa de pruebas unitarias asegurando el paso de las 54 existentes + nuevas pruebas.
- [x] 6.3 Verificación de arranque del servidor web y validación de sustitución visual en caliente.
- [x] 6.4 Generación del informe de verificación `verify-report.md` y sincronización en memoria persistente Engram.
