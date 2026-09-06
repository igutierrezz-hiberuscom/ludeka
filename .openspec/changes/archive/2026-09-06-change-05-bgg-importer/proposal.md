# Propuesta: Incremento 5 — Importador BGG en 1 Clic y Auto-Catalogación

## Intención
Implementar la integración avanzada con BoardGameGeek (BGG XMLAPI2) para permitir la **importación en 1 clic de colecciones personales** (`Owned`, `Wishlist`), el **cruce inteligente bidireccional con el catálogo local de Ludeka**, la **cola de auto-catalogación comunitaria priorizada por demanda** (`PendingBggImports`) con procesamiento por lotes, y el **buscador asistido en vivo** para registrar nuevos títulos en el catálogo oficial al vuelo con cero duplicados garantizados por `BggId`.

---

## Alcance

### Dentro del Alcance
- **Importador de Colección en 1 Clic (Punto 8.1 de la Spec):**
  - Consumo de `/xmlapi2/collection?username={username}&stats=1` con control de cortesía, reintentos con backoff exponencial para HTTP 202 y parseo de estados (`own="1"`, `wishlist="1"`, `wanttobuy="1"`, etc.).
  - Cruce de títulos contra el catálogo de Ludeka:
    - **Juegos existentes:** Vinculación instantánea a la ludoteca personal del usuario.
    - **Juegos no existentes:** Alta en el perfil del usuario con distintivo `⏳ En cola de catalogación` e inserción en la tabla de persistencia `PendingBggImports` (o incremento de `RequestedCount` si ya existía en cola).
- **Cola de Auto-Catalogación Priorizada por Demanda (Punto 8.2 de la Spec):**
  - Entidad `PendingBggImport` en `Ludeka.Core` con ordenación por número de solicitudes (`RequestedCount` DESC) y trazabilidad de estado (`Pending`, `Processing`, `Completed`, `Failed`).
  - Servicio de procesamiento por lotes `IBggCatalogQueueService` para auto-catalogar los $N$ títulos más demandados (descarga de ficha completa vía `FetchGameByBggIdAsync`, creación de `Game` y promoción atómica de los `UserCollectionItem` asociados).
  - Disparador manual para administradores y mesa fundadora `[ ⚡ Ejecutar Auto-Catalogación Ahora ]`.
- **Buscador Asistido en Vivo contra BGG (Punto 8.3 de la Spec):**
  - Consumo de `/xmlapi2/search?query={query}&type=boardgame` en `BggXmlApiClient`.
  - Modal interactivo de búsqueda asistida: muestra resultados con `BggId`, título y año.
  - Al seleccionar un título: vinculación directa si ya está en catálogo o catalogación al vuelo en vivo creando la ficha oficial y asignándola al usuario sin duplicados.
- **Evolución del Modelo de Colección (`UserCollectionItem`):**
  - Soporte de `GameId` opcional/nullable para albergar ítems en cola de catalogación antes de su publicación en el catálogo general.
- **Componentes UI Editoriales en Blazor (`Ludeka.Web`):**
  - Modal `BggImportModal.razor` con animación temática, estados de carga y reporte detallado de resultados.
  - Modal `BggSearchModal.razor` para búsqueda y adición manual asistida por BGG.
  - Panel `CatalogQueuePanel.razor` para monitorizar la cola comunitaria y disparar la catalogación.
  - Badges editoriales `⏳ En cola de catalogación` en `MyLibrary.razor`.
- **Suite Completa de Pruebas Automatizadas (Strict TDD):**
  - Pruebas unitarias de dominio, pruebas de parsing XML con fixtures reales, pruebas de servicios de importación y cola, y pruebas de repositorio SQLite.

### Fuera del Alcance
- Publicación desatendida hacia Meta Graph API / Instagram oficial de Ludeka y radar de sorteos (Incremento 6: `change-06-automation-community`).
- Consultorio de dudas de reglas Q&A estilo StackOverflow (Incremento 6: `change-06-automation-community`).
- Edición colaborativa comunitaria de fichas de catálogo (fuera de MVP).

---

## Capacidades

### Nuevas Capacidades
- `bgg-collection-import`: Importación de colecciones desde BGG XMLAPI2, discriminación de estados de posesión/deseo y cruce automático con el catálogo local de Ludeka.
- `bgg-auto-catalog-queue`: Gestión persistente de la cola de juegos pendientes de catalogación, ordenación por demanda comunitaria, procesamiento por lotes y promoción atómica de usuarios vinculados.
- `bgg-live-search`: Búsqueda asistida en vivo contra el catálogo global de BGG y catalogación instantánea al vuelo sin duplicados.

### Capacidades Modificadas
- `bgg-xmlapi-client`: Extensión de métodos en `IBggClient` y `BggXmlApiClient` para soportar colecciones y búsquedas por texto.
- `personal-collection`: Extensión de `UserCollectionItem` para permitir juegos pendientes de catalogación con metadatos provisionales (`BggId`, título, carátula) y promoción transparente a ficha catalogada.
- `user-library-view`: Incorporación en `MyLibrary.razor` de los disparadores de importación, búsqueda asistida, visualización de badges de cola y panel de control de auto-catalogación.

---

## Enfoque Arquitectónico

- **Dominio (`Ludeka.Core`)**:
  - Entidad `PendingBggImport` con invariantes de demanda comunitaria (`IncrementRequestCount`), control de ciclo de vida (`Pending` -> `Processing` -> `Completed`/`Failed`) y timestamps.
  - Enum `CatalogQueueStatus` (`Pending`, `Processing`, `Completed`, `Failed`).
  - Entidad `UserCollectionItem` modificada: `GameId` opcional (`Guid?`), `int? BggId`, `string? PendingTitle`, `string? PendingThumbnailUrl`, propiedad computada `IsPendingCataloging` y método `PromoteToCataloged(Guid gameId)`.
- **Aplicación (`Ludeka.Application`)**:
  - Contrato `IBggClient` enriquecido con `FetchUserCollectionAsync` y `SearchGamesAsync`.
  - Contratos `IPendingBggImportRepository`, `IBggImportService`, `IBggCatalogQueueService` y `IBggSearchAssistedService`.
  - DTOs específicos de importación, búsqueda asistida, ítems de cola y reportes de procesamiento.
- **Infraestructura (`Ludeka.Infrastructure`)**:
  - Implementación de consultas LINQ-to-XML para colecciones y búsqueda en `BggXmlApiClient` y `BggXmlParser`.
  - `DbSet<PendingBggImport>` en `LudekaDbContext` con índice único sobre `BggId` e índice compuesto sobre `(Status, RequestedCount)`.
  - Repositorio `SqlitePendingBggImportRepository` y adaptación de `SqliteUserCollectionRepository`.
- **Presentación Blazor (`Ludeka.Web`)**:
  - Modales interactivos `BggImportModal.razor` y `BggSearchModal.razor` respetando la guía editorial de Ludeka (cero "AI slop", contraste nítido, microtextos lúdicos).
  - Pestaña / Panel de gestión de cola comunitaria con botón de ejecución inmediata para fundadores.

---

## Áreas Afectadas

| Área | Impacto | Descripción |
|---|---|---|
| `Ludeka.Core` | Modificado / Nuevo | Nueva entidad `PendingBggImport`, enum `CatalogQueueStatus`, evolución de `UserCollectionItem` |
| `Ludeka.Application` | Modificado / Nuevo | Métodos en `IBggClient`, repositorios `IPendingBggImportRepository`, servicios de importación, cola y búsqueda asistida |
| `Ludeka.Infrastructure` | Modificado / Nuevo | Métodos en `BggXmlApiClient` / `BggXmlParser`, tabla `PendingBggImports` en `LudekaDbContext`, repositorios SQLite |
| `Ludeka.Web` | Modificado / Nuevo | Modales `BggImportModal`, `BggSearchModal`, integración en `MyLibrary.razor` y panel de cola |
| `tests/Ludeka.UnitTests` | Nuevo | Tests unitarios de dominio, parsing XML BGG, servicios de aplicación y repositorios SQLite |

---

## Riesgos y Mitigaciones

| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Respuestas HTTP 202 recurrentes en colecciones grandes de BGG | Alta | Implementar política de reintentos con backoff exponencial progresivo (2s, 4s, 6s) y mensaje editorial explicativo amigable si BGG tarda en compilar la caché |
| Rate limiting o bloqueos por llamadas masivas a BGG | Media | Utilizar el `TokenBucketRateLimiter` ya integrado para no superar 2 peticiones/segundo y procesar la auto-catalogación en lotes controlados |
| Duplicación de títulos al importar colecciones | Baja | Clave e índice único estricto sobre `BggId` tanto en `Games` como en `PendingBggImports` |
| Juegos no catalogados rompen claves foráneas de BD | Baja | Configurar `GameId` como nullable en `UserCollectionItem` con clave foránea opcional `IsRequired(false)` y cascade delete condicional |

---

## Plan de Rollback
- Revertir las modificaciones del incremento 5 en git.
- Eliminar la tabla `PendingBggImports` en `LudekaDbContext` y restablecer el esquema previo.

## Dependencias
- Catálogo base y cliente BGG inicial (`change-01-core-catalog`).
- Ludoteca personal y estados de colección (`change-02-library-loans`).
