# Exploración: change-04-multimedia-hub (Incremento 4: Hub Multimedia)

## 1. Estado Actual del Sistema

### 1.1 Solución y Arquitectura Base (.NET 10 y C# 13)
- **Estructura limpia:** 4 capas activas (`Ludeka.Core`, `Ludeka.Application`, `Ludeka.Infrastructure`, `Ludeka.Web`) con 71 pruebas unitarias y de integración pasando al 100% en `tests/Ludeka.UnitTests`.
- **Incremento 1 (`change-01-core-catalog`):** Catálogo de juegos de mesa con ADN lúdico, semáforo dinámico de escalabilidad, doble rating (BGG vs Ludist), guía de fundas y almacenamiento relacional SQLite con colecciones ToJson().
- **Incremento 2 (`change-02-library-loans`):** Ludoteca personal en 4 estados (`InCollection`, `Played`, `Wishlist`, `WantToBuy`), cuaderno de préstamos activos con devolución en 1 clic y sistema modular de reseñas comunitarias.
- **Incremento 3 (`change-03-founding-verdict`):** Ciclo de vida del veredicto fundador en 3 fases (resumen inicial de IA y sustitución prioritaria por el veredicto oficial), galería de fotos reales de mesa y gestión de roles (`FoundingTeam`, `Moderator`, `User`) con conmutador de prueba.
- **Rebranding oficial:** Consolidado a marca **Ludeka** en todo el monorepo y UI.

### 1.2 Situación Actual de Multimedia en la Ficha
- Actualmente la ficha `GameDetail.razor` no dispone de ningún módulo multimedia ni visualización de vídeos o redes.
- No existen entidades de dominio para representar vídeos, tutoriales, partidas, posts de Instagram ni contenidos huérfanos.
- No existe infraestructura de ingesta de YouTube/Instagram ni panel de moderación para contenidos multimedia.

---

## 2. Requerimientos del Incremento 4 (según ROADMAP_MVP_SLICES y Especificación Maestra)

El Incremento 4 aborda los puntos **5.1**, **5.2** y **5.3** de `LUDIST_SPEC_FUNCIONAL_MVP.md`:

### 2.1 Estructura Segregada por Pestañas (Punto 5.1 de la Spec)
Evitar a toda costa la mezcla caótica de formatos visuales:
1. **Pestaña 1: 🎬 Tutoriales (YouTube):**
   - Miniaturas horizontales 16:9 con canal/autor, duración formateada y título.
   - Enfoque: Guías explicativas de "cómo se juega" en 10–20 minutos.
   - Modal limpio de reproducción embebida o enlace externo directo.
2. **Pestaña 2: 🎲 Partidas Completas (YouTube):**
   - Miniaturas 16:9 con **badge identificativo obligatorio del número de jugadores** (ej. `"Partida a 2"`, `"Partida a 4"`).
   - Datos de duración real y canal especializado.
3. **Pestaña 3: 💬 Opiniones y Redes (Instagram + Shorts/Reels):**
   - **Posts de Instagram (Foto + Texto):** Tarjetas cuadradas limpias (1:1) con fotografía principal del post, autor (`@canal`), contador de likes y extracto de las primeras líneas de la reseña. Al tocarlo abre modal o redirige al post original.
   - **Vídeos Verticales (Reels / Shorts):** Tarjetas en formato 9:16 con icono de reproducción rápida y duración (ej. `0:55`).

### 2.2 Estrategia de Ingesta y Cuotas (Punto 5.2 de la Spec)
- **Cold Start (Carga Inicial Curada):**
  - Enfoque offline-first con catálogo semillado enriquecido: contenidos multimedia reales de referencia hispanohablante (Análisis-Parálisis, Zacatrus TV, El Troquel, Meepletopia, Rincón Legacy, Juegos de Mesa 221B, etc.) para los 15 juegos del catálogo base.
  - Regla de negocio estricta: máximo 2 mejores vídeos/partidas por juego en cold start para no sobrecargar la vista ni saturar cuotas.
- **Modo Crucero y Extensibilidad de Ingesta:**
  - Interfaces y servicios extensibles (`IMediaIngestionService`, `IYouTubeClient`, `IInstagramClient`) preparados para llamadas API y recolección controlada.

### 2.3 Panel de Moderación Rápido Móvil (Punto 5.3 de la Spec)
- **Ruta dedicada:** `/moderacion/multimedia`, accesible exclusivamente para roles `FoundingTeam` y `Moderator`.
- **Acciones en 1 Clic (Táctil móvil):**
  - `[ ✅ Aprobar ]`: Publica el contenido en la ficha del juego correspondiente.
  - `[ ❌ Descartar ]`: Rechaza y oculta el contenido.
- **Bandeja de Vídeos Huérfanos:**
  - Contenidos multimedia recogidos que no pudieron emparejarse automáticamente con un juego del catálogo (`GameId == null`).
  - Acción `[ 🔗 Asignar Juego ]` con selector/autocompletado para asociarlo de inmediato a un juego existente.
- **Detector de Enlaces Rotos:**
  - Servicio ligero de verificación de enlaces (comprobación HTTP de disponibilidad) con marcado de elementos rotos (HTTP 404) y despublicación en 1 clic.

---

## 3. Análisis de Impacto y Diseño Técnico Preliminar

### 3.1 Capa de Dominio (`src/Ludeka.Core`)
- **Nueva Entidad `MediaItem`:**
  - `Id` (Guid)
  - `GameId` (Guid?, nullable para huérfanos)
  - `Type` (`MediaType`: `Tutorial`, `Playthrough`, `InstagramPost`, `ShortReel`)
  - `Platform` (`MediaPlatform`: `YouTube`, `Instagram`)
  - `Title` (string)
  - `Url` (string)
  - `EmbedUrl` (string?)
  - `ThumbnailUrl` (string)
  - `AuthorChannel` (string, ej. "@analisisparalisis")
  - `DurationSeconds` (int?, duración en segundos)
  - `PlayerCountBadge` (string?, ej. "Partida a 2", obligatorio para `Playthrough`)
  - `LikesCount` (int?, para posts de Instagram)
  - `Excerpt` (string?, extracto de texto de reseña)
  - `Status` (`ModerationStatus`: `PendingApproval`, `Approved`, `Rejected`)
  - `IsBroken` (bool)
  - `PublishedAt` (DateTimeOffset)
  - `CreatedAt` y `UpdatedAt` (DateTimeOffset)
  - Métodos de negocio: `Approve()`, `Reject()`, `AssignToGame(Guid gameId)`, `MarkAsBroken(bool isBroken)`.
- **Nuevos Enums:**
  - `MediaType`: `Tutorial`, `Playthrough`, `InstagramPost`, `ShortReel`.
  - `MediaPlatform`: `YouTube`, `Instagram`.
  - `ModerationStatus`: `PendingApproval`, `Approved`, `Rejected`.

### 3.2 Capa de Aplicación (`src/Ludeka.Application`)
- **Contrato de Repositorio `IMediaRepository`:**
  - `GetApprovedByGameIdAsync(Guid gameId, CancellationToken ct)`
  - `GetPendingModerationAsync(CancellationToken ct)`
  - `GetOrphansAsync(CancellationToken ct)`
  - `GetByIdAsync(Guid id, CancellationToken ct)`
  - `AddAsync(MediaItem item, CancellationToken ct)`
  - `UpdateAsync(MediaItem item, CancellationToken ct)`
  - `DeleteAsync(Guid id, CancellationToken ct)`
- **Contrato de Servicio `IMediaService`:**
  - `GetGameMediaAsync(Guid gameId, CancellationToken ct)`: Devuelve colecciones segregadas para las 3 pestañas.
  - `GetModerationQueueAsync(ModerationFilterCriteria criteria, CancellationToken ct)`
  - `ApproveMediaAsync(Guid id, CancellationToken ct)`
  - `RejectMediaAsync(Guid id, CancellationToken ct)`
  - `AssignOrphanMediaAsync(Guid id, Guid gameId, CancellationToken ct)`
  - `CheckBrokenLinksAsync(CancellationToken ct)`: Comprobación de enlaces y reporte.
- **DTOs:**
  - `MediaItemDto`, `GameMediaHubDto` (con listas específicas `Tutorials`, `Playthroughs`, `SocialOpinions`), `CreateMediaItemRequest`, `ModerateMediaItemRequest`, `AssignMediaGameRequest`, `BrokenLinkReportDto`.

### 3.3 Capa de Infraestructura (`src/Ludeka.Infrastructure`)
- **Configuración EF Core 10 en `LudekaDbContext`:**
  - `DbSet<MediaItem> MediaItems` con índices sobre `GameId`, `Status`, `Type`, `Platform`.
  - Relación opcional con `Game` (clave foránea nullable con restricción `SetNull` o `Cascade`).
- **Implementación `SqliteMediaRepository`:**
  - Consultas optimizadas con EF Core 10 y LINQ asíncrono.
- **Semillado Multimedia (`CatalogSeeder`):**
  - Incorporar datos iniciales aprobados de tutoriales, partidas a 2/4 jugadores y posts de Instagram para los 15 juegos base, más 3-5 contenidos en cola de moderación y contenidos huérfanos para probar el panel.
- **Verificador de Enlaces `BrokenLinkChecker`:**
  - Servicio basado en `HttpClient` (o simulación determinista para pruebas) que detecta estados 404 y despublica elementos.

### 3.4 Capa de Presentación Blazor (`src/Ludeka.Web`)
- **Componente `MultimediaHub.razor`:**
  - Pestañas horizontales fluidas:
    - 🎬 *Tutoriales* (16:9 con duración y canal)
    - 🎲 *Partidas Completas* (16:9 con badge identificativo de número de jugadores)
    - 💬 *Opiniones y Redes* (1:1 fotos cuadradas y 9:16 reels/shorts)
  - Microtextos con identidad propia cuando no haya contenido (ej. *"¿Conoces un gran tutorial en español para este juego? ¡Avísanos!"*).
  - Modal reproductor embebido `MediaEmbedModal.razor` para reproducir vídeos de YouTube sin abandonar la página o abrir links de Instagram.
- **Página de Moderación Móvil `/moderacion/multimedia` (`MediaModeration.razor`):**
  - Protección con `CurrentUserService.IsFoundingTeam || CurrentUserService.IsInRole("Moderator")`.
  - Pestañas de moderación: `Pendientes de Aprobación`, `Bandeja de Huérfanos`, `Aprobados`.
  - Tarjetas compactas táctiles para revisión rápida en móvil con acciones en 1 clic.
  - Modal o selector rápido de asignación de juego para huérfanos.
  - Botón de ejecución del detector de enlaces rotos.
- **Navegación en `MainLayout.razor`:**
  - Acceso directo al Hub de Moderación en la barra superior para usuarios con rol moderador o equipo fundador.

---

## 4. Estrategia de Pruebas (TDD Riguroso)
- **Tests Unitarios de Dominio:** Invariantes de `MediaItem` (URLs válidas, requerimiento de badge en partidas, transiciones de estado de moderación, asignación de huérfanos).
- **Tests Unitarios de Aplicación:** Casos de uso de `MediaService` (segregación por pestañas, filtros de moderación, aprobación, descarte, asignación a juego).
- **Tests de Integración de Persistencia:** Consultas en SQLite EF Core 10 con índices y operaciones de moderación.
- **Tests de Componentes:** Renderizado de pestañas sin mezclar formatos, insignias de comensales en partidas y visibilidad de controles de moderación según rol.
