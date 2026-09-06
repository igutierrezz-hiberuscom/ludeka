# Tasks: Incremento 5 — Importador BGG en 1 Clic y Auto-Catalogación (`change-05-bgg-importer`)

## Review Workload Forecast

| Field | Value |
|---|---|
| Estimated changed lines | ~800-1100 líneas |
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
| 1 | Dominio, DTOs, Infraestructura BGG y Persistencia SQLite | PR 1 | `dotnet test --filter Category=Unit` | N/A (bibliotecas de clases y tests) | `src/Ludeka.Core`, `Ludeka.Application`, `Ludeka.Infrastructure` |
| 2 | Componentes Blazor, Modales de Importación/Búsqueda y Panel de Cola | PR 2 | `dotnet test` | `dotnet run --project src/Ludeka.Web` | `src/Ludeka.Web/Components` |

---

## Phase 1: Dominio Lúdico y Modelos de Entidad (`src/Ludeka.Core`)

- [x] 1.1 Crear enum `CatalogQueueStatus.cs` (`Pending`, `Processing`, `Completed`, `Failed`) en `src/Ludeka.Core/Enums/`.
- [x] 1.2 Crear entidad `PendingBggImport.cs` en `src/Ludeka.Core/Entities/` con invariantes de demanda comunitaria (`RequestedCount`), estados de ciclo de vida (`MarkAsProcessing`, `MarkAsCompleted`, `MarkAsFailed`) y marcas temporales.
- [x] 1.3 Modificar `UserCollectionItem.cs` en `src/Ludeka.Core/Entities/` permitiendo `GameId` opcional (`Guid?`), propiedades `BggId`, `PendingTitle`, `PendingThumbnailUrl`, `PendingYearPublished`, propiedad computada `IsPendingCataloging` y método `PromoteToCataloged(Guid gameId)`.

## Phase 2: Contratos, DTOs y Servicios de Aplicación (`src/Ludeka.Application`)

- [x] 2.1 Actualizar contrato `IBggClient.cs` en `src/Ludeka.Application/Contracts/` añadiendo `FetchUserCollectionAsync(string username, CancellationToken ct)` y `SearchGamesAsync(string query, CancellationToken ct)`.
- [x] 2.2 Crear DTOs en `src/Ludeka.Application/DTOs/BggImportDtos.cs` (`BggCollectionItemDto`, `BggSearchResultDto`, `BggImportRequest`, `BggImportResultDto`, `CatalogQueueItemDto`, `ProcessQueueResultDto`).
- [x] 2.3 Crear interfaz de repositorio `IPendingBggImportRepository.cs` en `src/Ludeka.Application/Contracts/`.
- [x] 2.4 Actualizar `IUserCollectionRepository.cs` para soportar consultas de ítems pendientes por `BggId` y promoción atómica.
- [x] 2.5 Crear interfaz `IBggImportService.cs` e implementar `BggImportService.cs` en `src/Ludeka.Application/Features/Bgg/` con cruce bidireccional local vs BGG y encolado.
- [x] 2.6 Crear interfaz `IBggCatalogQueueService.cs` e implementar `BggCatalogQueueService.cs` en `src/Ludeka.Application/Features/Bgg/` para procesamiento por lotes de los títulos más demandados y promoción masiva de usuarios.
- [x] 2.7 Crear interfaz `IBggSearchAssistedService.cs` e implementar `BggSearchAssistedService.cs` en `src/Ludeka.Application/Features/Bgg/` para búsqueda asistida y catalogación instantánea al vuelo sin duplicados.

## Phase 3: Infraestructura BGG, Persistencia SQLite y Seeder (`src/Ludeka.Infrastructure`)

- [x] 3.1 Actualizar `BggXmlApiClient.cs` y `BggXmlParser.cs` en `src/Ludeka.Infrastructure/Bgg/` implementando consultas a `/xmlapi2/collection` con manejo de HTTP 202 y `/xmlapi2/search` con parsers LINQ-to-XML.
- [x] 3.2 Actualizar `LudekaDbContext.cs` en `src/Ludeka.Infrastructure/Data/` incorporando `DbSet<PendingBggImport>`, índices sobre `BggId` y `(Status, RequestedCount)`, y clave foránea opcional para `UserCollectionItem`.
- [x] 3.3 Implementar `SqlitePendingBggImportRepository.cs` en `src/Ludeka.Infrastructure/Data/`.
- [x] 3.4 Actualizar `SqliteUserCollectionRepository.cs` para soportar ítems pendientes y promoción a catalogado.
- [x] 3.5 Actualizar `CatalogSeeder.cs` en `src/Ludeka.Infrastructure/Seeding/` precargando títulos populares en la cola `PendingBggImports` para permitir pruebas offline de auto-catalogación inmediata.

## Phase 4: Pruebas Unitarias e Integración (Strict TDD) (`tests/Ludeka.UnitTests`)

- [x] 4.1 Crear pruebas de dominio `PendingBggImportTests.cs` en `tests/Ludeka.UnitTests/Domain/`.
- [x] 4.2 Actualizar pruebas de dominio `UserCollectionItemTests.cs` verificando soporte de juegos en cola y promoción.
- [x] 4.3 Crear pruebas de infraestructura `BggXmlCollectionParserTests.cs` y `BggXmlSearchParserTests.cs` en `tests/Ludeka.UnitTests/Infrastructure/` con fixtures XML reales.
- [x] 4.4 Crear pruebas de servicio `BggImportServiceTests.cs` en `tests/Ludeka.UnitTests/Application/` (cruce local/remoto, vinculación y encolado).
- [x] 4.5 Crear pruebas de servicio `BggCatalogQueueServiceTests.cs` en `tests/Ludeka.UnitTests/Application/` (procesamiento por lotes y promoción atómica).
- [x] 4.6 Crear pruebas de servicio `BggSearchAssistedServiceTests.cs` en `tests/Ludeka.UnitTests/Application/` (prevención de duplicados e inserción al vuelo).
- [x] 4.7 Crear pruebas de persistencia `SqlitePendingBggImportRepositoryTests.cs` en `tests/Ludeka.UnitTests/Infrastructure/`.
- [x] 4.8 Ejecutar suite completa con `dotnet test` y confirmar 100% de tests en verde.

## Phase 5: Componentes UI Blazor y Experiencia Editorial (`src/Ludeka.Web`)

- [x] 5.1 Crear modal `BggImportModal.razor` en `src/Ludeka.Web/Components/Shared/` con estados reactivos, animación de carga y tarjeta de resumen lúdica.
- [x] 5.2 Crear modal `BggSearchModal.razor` en `src/Ludeka.Web/Components/Shared/` para búsqueda asistida y catalogación rápida en 1 clic.
- [x] 5.3 Crear componente `CatalogQueuePanel.razor` en `src/Ludeka.Web/Components/Shared/` con lista de títulos en cola ordenada por popularidad y botón `[ ⚡ Ejecutar Auto-Catalogación Ahora ]`.
- [x] 5.4 Modificar `MyLibrary.razor` en `src/Ludeka.Web/Components/Pages/`:
  - Incorporar botones `[ 📥 Importar BGG ]` y `[ + Añadir por BGG ]`.
  - Renderizar badge editorial `⏳ En cola de catalogación` en las tarjetas correspondientes.
  - Integrar pestaña / acceso al panel de cola comunitaria para la Mesa Fundadora y moderadores.
- [x] 5.5 Registrar nuevos repositorios y servicios BGG en `Program.cs` de `Ludeka.Web`.

## Phase 6: Verificación en Vivo y Cierre del Incremento

- [x] 6.1 Compilación de la solución completa sin errores ni advertencias de código.
- [x] 6.2 Ejecución de suite de tests automatizados confirmando todos los tests en verde sin regresiones.
- [x] 6.3 Verificación manual de importación, auto-catalogación y búsqueda asistida.
- [x] 6.4 Generación del informe de verificación `verify-report.md` y sincronización en memoria persistente Engram.
