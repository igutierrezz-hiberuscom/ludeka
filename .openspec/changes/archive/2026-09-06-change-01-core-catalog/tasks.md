# Tasks: Incremento 1 — Catálogo Base, Ficha Inteligente y Semáforo de Escalabilidad

## Review Workload Forecast
| Field | Value |
|---|---|
| Líneas Totales Estimadas | ~1200-1500 |
| Proyectos Afectados | `Ludeca.Core`, `Ludeca.Application`, `Ludeca.Infrastructure`, `Ludeca.Web`, `Ludeca.UnitTests` |

Decision needed before apply: No
Chained PRs recommended: Yes
Chain strategy: size-exception
400-line budget risk: High

### Suggested Work Units
| Unit | Descripción | Estimación |
|---|---|---|
| 1 | Dominio y Reglas del Semáforo (`Ludeca.Core` + tests) | ~300 líneas |
| 2 | Contratos de Aplicación y DTOs (`Ludeca.Application`) | ~180 líneas |
| 3 | Infraestructura SQLite, Seeder y Cliente BGG (`Ludeca.Infrastructure` + tests) | ~450 líneas |
| 4 | UI Blazor, Componentes Editoriales y Ficha Inteligente (`Ludeca.Web`) | ~400 líneas |
| 5 | Verificación e Integración End-to-End | ~120 líneas |

## Phase 1: Dominio `Ludeca.Core`
- [x] 1.1 **RED**: Tests para `Slug` determinista y VOs (`AgeRating`, `GameDuration`, `SleeveItem`, `ScalabilityEntry`).
- [x] 1.2 **GREEN**: Enums (`ConfrontationType`, `GameStyle`, `ScalabilityStatus`, `LanguageDependence`, `TableFootprint`) y VOs inmutables.
- [x] 1.3 **RED**: Tests de reglas del agregado `Game`: estados del semáforo (`MustPlay`, `Recommended`, `NotRecommended`), `IdealPlayerCountText` y `IsAccessibleEarlier`.
- [x] 1.4 **GREEN**: Entidad `Game` con lógica de semáforo, invariantes y fábrica de agregados.
- [x] 1.5 **REFACTOR**: Refactorizar modelo de dominio eliminando duplicidad e impulsando inmutabilidad.

## Phase 2: Capa de Aplicación `Ludeca.Application`
- [x] 2.1 Contratos `IGameRepository` (búsqueda y CRUD) e `IBggClient`.
- [x] 2.2 DTOs: `GameSummaryDto`, `GameDetailDto` y filtros `GameFilterCriteria`.
- [x] 2.3 Casos de uso `GetCatalogGamesQuery` y `GetGameDetailBySlugQuery` con mapeo a DTOs.

## Phase 3: Infraestructura `Ludeca.Infrastructure`
- [x] 3.1 `LudecaDbContext` con SQLite y mapeo `.ToJson()` para `Scalability` y `Sleeves`.
- [x] 3.2 `SqliteGameRepository` con consultas compiladas y búsqueda multi-criterio.
- [x] 3.3 Catálogo base curado `seed-games.json` y servicio `CatalogSeeder` idempotente.
- [x] 3.4 **RED**: Tests con fixtures XML de BGG simulando encuestas de escalabilidad y nombres en español.
- [x] 3.5 **GREEN**: `BggXmlApiClient` con parser `XDocument`, rate limiting (2 req/s) y reintentos ante 429/202.
- [x] 3.6 **REFACTOR**: Modularizar extractores LINQ-to-XML con navegación segura contra nulos.

## Phase 4: Componentes Web Blazor y Tailwind en `Ludeca.Web`
- [x] 4.1 Componentes atómicos: `ScalabilityTrafficLight` (🟢/🟡/🔴), `QuickBadges` y `SleeveGuideCard`.
- [x] 4.2 Componentes de catálogo: `GameCard` (lectura en 3s) y `CatalogSearchBar` reactivo con debounce.
- [x] 4.3 Página `CatalogPage` (`/` y `/catalogo`) con filtros ("Especial Parejas", "Mesa Familiar", "Solo Top").
- [x] 4.4 Página `GameDetailPage` (`/juegos/{slug}`) con hero visual, semáforo, ADN y guía de fundas.
- [x] 4.5 Configuración de DI, migración SQLite y siembra en `Program.cs`.

## Phase 5: Verificación y Pruebas de Integración
- [x] 5.1 Suite completa `dotnet test` validando dominio, parser BGG y persistencia.
- [x] 5.2 Verificación de arranque en frío, inicialización de `ludeca.db` y catálogo offline.
