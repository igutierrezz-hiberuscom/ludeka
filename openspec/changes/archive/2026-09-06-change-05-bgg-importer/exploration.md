# Exploración: change-05-bgg-importer (Incremento 5: Importador BGG en 1 Clic y Auto-Catalogación)

## 1. Estado Actual del Sistema

### 1.1 Solución y Arquitectura Base (.NET 10 y C# 13)
- **4 Capas en Clean Architecture:** `Ludeka.Core`, `Ludeka.Application`, `Ludeka.Infrastructure`, `Ludeka.Web` con 101 pruebas unitarias e integración pasando al 100% en `tests/Ludeka.UnitTests`.
- **Incremento 1 (`change-01-core-catalog`):** Catálogo de juegos de mesa, ADN lúdico, semáforo dinámico de escalabilidad, doble rating (BGG vs Ludist), guía de fundas y almacenamiento relacional SQLite con colecciones ToJson().
- **Incremento 2 (`change-02-library-loans`):** Ludoteca personal en 4 estados (`InCollection`, `Played`, `Wishlist`, `WantToBuy`), cuaderno de préstamos activos con devolución en 1 clic y sistema modular de reseñas comunitarias.
- **Incremento 3 (`change-03-founding-verdict`):** Ciclo de vida del veredicto fundador en 3 fases (resumen inicial de IA y sustitución prioritaria por veredicto oficial), galería de fotos reales de mesa y gestión de roles (`FoundingTeam`, `Moderator`, `User`).
- **Incremento 4 (`change-04-multimedia-hub`):** Hub multimedia segregado por pestañas en la ficha de juego (tutoriales 16:9, partidas 16:9 con badge obligatorio de comensales, opiniones/redes 1:1 y 9:16), ingesta acotada con catálogo curado hispanohablante y panel táctil móvil de moderación con detector de enlaces rotos.

### 1.2 Situación Actual de la Integración con BGG
- Existe `IBggClient` en `Ludeka.Application.Contracts` con un único método: `Task<Game?> FetchGameByBggIdAsync(int bggId, CancellationToken ct)`.
- Existe `BggXmlApiClient` en `Ludeka.Infrastructure.Bgg` que consume `/xmlapi2/thing?id={bggId}&stats=1` con control de cortesía mediante `TokenBucketRateLimiter` (2 peticiones/segundo) y soporte de reintentos exponenciales para respuestas HTTP 202 y 429.
- Existe `BggXmlParser` en `Ludeka.Infrastructure.Bgg` que parsea metadatos del juego, títulos en español, encuestas de edad comunitaria, escalabilidad y ADN lúdico.
- **Carencias actuales:**
  1. No existe método ni parser para consultar la colección de un usuario en BGG (`/xmlapi2/collection?username={username}&stats=1`).
  2. No existe método ni parser para el buscador en vivo de BGG (`/xmlapi2/search?query={query}&type=boardgame`).
  3. No existe la entidad ni la tabla de persistencia `PendingBggImports` para gestionar los juegos pendientes de catalogación y su cola nocturna de popularidad.
  4. La entidad `UserCollectionItem` requiere actualmente de forma obligatoria un `GameId` que debe existir en la tabla `Games`, impidiendo registrar juegos en la ludoteca personal con estado `⏳ En cola de catalogación`.
  5. La vista `MyLibrary.razor` no dispone de interfaz para disparar la importación en 1 clic desde BGG, ni de buscador asistido para añadir juegos por BGG, ni de visualización de juegos pendientes de catalogación.

---

## 2. Requerimientos del Incremento 5 (según ROADMAP_MVP_SLICES y Especificación Maestra)

El Incremento 5 aborda los puntos **8.1**, **8.2** y **8.3** de `LUDIST_SPEC_FUNCIONAL_MVP.md`:

### 2.1 Importador en 1 Clic desde BGG (Punto 8.1 de la Spec)
- El usuario introduce su nombre de usuario de BoardGameGeek en su perfil o directamente en su ludoteca.
- La plataforma consulta la API de colección de BGG: `/xmlapi2/collection?username={username}&stats=1`.
- Manejo de particularidades de BGG:
  - BGG devuelve HTTP 202 (Accepted) si la colección no está precacheada. El cliente debe reintentar con backoff exponencial. Si persiste el 202, informar amigablemente al usuario para que reintente en unos segundos.
  - Parseo de colecciones con filtrado por subtipo `boardgame` y extracción de banderas de estado:
    - `own="1"` -> Mapeo directo a `CollectionStatus.InCollection`.
    - `wishlist="1"` -> Mapeo directo a `CollectionStatus.Wishlist`.
    - `wanttobuy="1"` -> Mapeo directo a `CollectionStatus.WantToBuy`.
    - `numplays > 0` o `played="1"` -> Mapeo a `CollectionStatus.Played` (si no está en propiedad).
- **Cruce bidireccional con el catálogo local:**
  - **Títulos existentes:** Si el `BggId` ya existe en `Games`, se crea o actualiza de inmediato el `UserCollectionItem` correspondiente del usuario.
  - **Títulos no existentes en Ludeka:** Se añaden al perfil del usuario bajo el distintivo `⏳ En cola de catalogación` y se dan de alta en la cola `PendingBggImports`. Si el juego ya estaba en la cola solicitado por otros usuarios, se incrementa su contador de demanda (`RequestedCount`).

### 2.2 Cola Nocturna de Auto-Catalogación (Punto 8.2 de la Spec)
- **Persistencia de la Cola:** Tabla `PendingBggImports` ordenada por demanda comunitaria (`RequestedCount` DESC) y fecha de solicitud.
- **Worker / Servicio de Auto-Catalogación:**
  - Procesa un lote configurable de los $N$ juegos más demandados (ej. 20 a 50 juegos).
  - Por cada título:
    1. Descarga la ficha completa de BGG mediante `IBggClient.FetchGameByBggIdAsync(bggId)`.
    2. Guarda el nuevo `Game` en el catálogo local (`Games`).
    3. Promueve de forma atómica todos los `UserCollectionItem` que estaban esperando ese `BggId`: asigna `GameId = game.Id`, desactiva el estado de cola y enlaza la ficha oficial.
    4. Actualiza préstamos huérfanos si existieran.
    5. Marca la entrada en `PendingBggImports` como `Completed` con marca temporal de procesamiento.
- **Disparador Manual para Mesa Fundadora / Moderación:**
  - Botón táctil `[ ⚡ Ejecutar Auto-Catalogación Ahora ]` para probar el proceso nocturno bajo demanda sin esperar al cron nocturno.

### 2.3 Añadir Título a Mano Asistido por BGG (Punto 8.3 de la Spec)
- Consulta en vivo a la API `/xmlapi2/search?query={query}&type=boardgame` de BGG.
- Muestra resultados inmediatos con `BggId`, título principal y año de publicación.
- Al seleccionar un juego:
  - Si ya existe en Ludeka: lo vincula a la colección del usuario en el estado seleccionado y abre su ficha.
  - Si NO existe en Ludeka: ejecuta la catalogación en vivo al vuelo (`FetchGameByBggIdAsync`), crea la ficha oficial en `Games`, lo añade a la colección del usuario y marca completada su entrada en la cola si existía.
  - Garantiza **cero duplicados** apoyándose en el índice único de `BggId`.

---

## 3. Análisis de Impacto y Diseño Técnico Preliminar

### 3.1 Capa de Dominio (`src/Ludeka.Core`)
- **Nueva Entidad `PendingBggImport`:**
  - `Id` (Guid)
  - `BggId` (int, único)
  - `Title` (string)
  - `YearPublished` (int?)
  - `ThumbnailUrl` (string?)
  - `RequestedCount` (int, contador de usuarios)
  - `Status` (`CatalogQueueStatus`: `Pending`, `Processing`, `Completed`, `Failed`)
  - `ErrorMessage` (string?)
  - `CreatedAt` (DateTimeOffset)
  - `ProcessedAt` (DateTimeOffset?)
  - Métodos de negocio: `IncrementRequestCount()`, `MarkAsProcessing()`, `MarkAsCompleted()`, `MarkAsFailed(string error)`.
- **Nuevo Enum `CatalogQueueStatus`:**
  - `Pending = 1`, `Processing = 2`, `Completed = 3`, `Failed = 4`.
- **Evolución de `UserCollectionItem`:**
  - Permitir `GameId` opcional/nullable (`Guid? GameId`) para soportar juegos no catalogados aún.
  - Añadir propiedades para el estado en cola:
    - `int? BggId`
    - `string? PendingTitle`
    - `string? PendingThumbnailUrl`
    - `int? PendingYearPublished`
    - `bool IsPendingCataloging => GameId == null;`
  - Método de negocio: `PromoteToCataloged(Guid gameId)`.

### 3.2 Capa de Aplicación (`src/Ludeka.Application`)
- **Ampliación de `IBggClient`:**
  - `Task<IReadOnlyList<BggCollectionItemDto>> FetchUserCollectionAsync(string username, CancellationToken ct = default);`
  - `Task<IReadOnlyList<BggSearchResultDto>> SearchGamesAsync(string query, CancellationToken ct = default);`
- **Nuevos DTOs:**
  - `BggCollectionItemDto`: `BggId`, `Title`, `YearPublished`, `ThumbnailUrl`, `CoverImageUrl`, `IsOwned`, `IsWishlist`, `IsWantToBuy`, `NumPlays`.
  - `BggSearchResultDto`: `BggId`, `Title`, `YearPublished`, `IsAlreadyCataloged`, `ExistingGameSlug`.
  - `BggImportResultDto`: `TotalProcessed`, `ImportedToCollection`, `EnqueuedForCataloging`, `Errors`.
  - `BggImportRequest`: `Username`, `ImportOwned`, `ImportWishlist`.
  - `CatalogQueueItemDto`: `Id`, `BggId`, `Title`, `YearPublished`, `ThumbnailUrl`, `RequestedCount`, `Status`, `CreatedAt`.
  - `ProcessQueueResultDto`: `ProcessedCount`, `SuccessCount`, `FailedCount`, `CatalogedGames`.
- **Nuevos Contratos de Repositorio:**
  - `IPendingBggImportRepository`:
    - `GetByBggIdAsync(int bggId, CancellationToken ct)`
    - `GetTopPendingAsync(int limit, CancellationToken ct)`
    - `GetTotalPendingCountAsync(CancellationToken ct)`
    - `AddAsync(PendingBggImport item, CancellationToken ct)`
    - `UpdateAsync(PendingBggImport item, CancellationToken ct)`
- **Nuevos Servicios de Aplicación:**
  - `IBggImportService`: Orquesta la importación desde BGG, el cruce con el catálogo local y la actualización de `UserCollectionItem` y `PendingBggImports`.
  - `IBggCatalogQueueService`: Orquesta el procesamiento por lotes de la cola nocturna y manual, y la resolución de ítems pendientes.
  - `IBggSearchAssistedService`: Búsqueda en vivo y catalogación instantánea al vuelo.

### 3.3 Capa de Infraestructura (`src/Ludeka.Infrastructure`)
- **Evolución de `BggXmlApiClient` y `BggXmlParser`:**
  - Implementar consulta y parseo LINQ-to-XML de `/xmlapi2/collection`.
  - Implementar consulta y parseo LINQ-to-XML de `/xmlapi2/search`.
  - Manejo de reintentos exponenciales para 202 en colecciones.
- **Persistencia en `LudekaDbContext`:**
  - `DbSet<PendingBggImport> PendingBggImports => Set<PendingBggImport>();`
  - Configuración fluida con índice único sobre `BggId` e índice sobre `(Status, RequestedCount)`.
  - Configuración fluida de `UserCollectionItem`: `GameId` opcional, índices sobre `(UserId, GameId)` y `(UserId, BggId)`.
- **Implementación de Repositorios SQLite:**
  - `SqlitePendingBggImportRepository`.
  - Actualización de `SqliteUserCollectionRepository` para soportar consultas de ítems pendientes por `BggId`.

### 3.4 Capa Web Blazor (`src/Ludeka.Web`)
- **Componentes Editoriales Modernos:**
  - `BggImportModal.razor`: Modal interactivo accesible, con selector de listas, barra de progreso y resumen claro con conteo en 2 bloques (Asociados al instante / En cola de catalogación).
  - `BggSearchModal.razor`: Modal de búsqueda asistida en vivo con autocompletado contra BGG, indicación de estado y botón de catalogación rápida en 1 toque.
  - `CatalogQueuePanel.razor`: Panel o pestaña en la biblioteca para ver los juegos en cola comunitaria y disparar la auto-catalogación.
  - Actualización de `MyLibrary.razor`: Badges `⏳ En cola de catalogación` en las tarjetas, botón `[ 📥 Importar BGG ]` y botón `[ + Añadir por BGG ]`.

---

## 4. Estrategia de Pruebas (Strict TDD)
- **Tests de Dominio:**
  - Invariantes de `PendingBggImport` (incremento de demanda, cambios de estado válidos).
  - Comportamiento de `UserCollectionItem` en estado pendiente y tras promoción a juego catalogado.
- **Tests de Infraestructura BGG:**
  - Tests unitarios con respuestas XML simuladas (fixtures de BGG collection y search).
  - Manejo de reintentos para código HTTP 202.
  - Tests de repositorios SQLite sobre SQLite en memoria.
- **Tests de Servicios de Aplicación:**
  - `BggImportServiceTests`: Cruce local vs BGG, vinculación inmediata y encolado.
  - `BggCatalogQueueServiceTests`: Procesamiento por lotes y promoción de usuarios.
  - `BggSearchAssistedServiceTests`: Añadido manual con prevención de duplicados.
