# 16. Módulo de Sorteos, Novedades y Grandes Eventos Lúdicos

> **Estado:** Implementado y Verificado  
> **Incremento SDD:** `change-22-draws-news-events-split` (Incremento 22)  
> **Rutas:** `/sorteos` (alias `/radar`), `/novedades`, `/eventos`, `/admin/eventos`  
> **Tests:** 472 pruebas pasando al 100% en verde.

---

## 1. Propósito y Visión del Módulo

Este módulo segrega el concepto anteriormente unificado de "Radar" en tres verticales independientes y especializadas dentro del ecosistema de Ludeka:
1. **Radar de Sorteos (`/sorteos`):** Centraliza sorteos de juegos de mesa en redes sociales (Instagram, Twitter/X, YouTube, Comunidad), control de expiración automática y gobernanza de sorteos destacados/patrocinados (`IsPromoted`).
2. **Calendario de Estrenos y Novedades (`/novedades`):** Línea temporal de anuncios, preventas y lanzamientos de editoriales y tiendas especializadas (`WeeklyRelease`), con buscador y alta manual para moderadores.
3. **Grandes Citas y Convenciones Lúdicas (`/eventos` y `/admin/eventos`):** Directorio oficial de ferias, festivales y macro-eventos del sector de los juegos de mesa en España y el circuito internacional (Festival de Córdoba, InterOcio Madrid, Essen SPIEL, Gen Con, DAU Barcelona), con panel editorial CRUD.

---

## 2. Modelo de Dominio (`Ludeka.Core`)

### 2.1 Entidad `BoardGameEvent`
Ubicación: `src/Ludeka.Core/Entities/BoardGameEvent.cs`
- **Atributos:**
  - `Guid Id`: Identificador único del evento.
  - `string Title`: Título oficial de la feria o festival (obligatorio).
  - `string Description`: Descripción y aspectos destacados de la edición.
  - `string ImageUrl`: Cartel promocional u oficial (obligatorio).
  - `DateOnly StartDate` y `DateOnly EndDate`: Fechas de celebración con validación invariante `EndDate >= StartDate`.
  - `string Location`: Ciudad y recinto (ej. "Córdoba — Palacio de la Merced").
  - `string? WebsiteUrl`: Enlace a la web oficial de venta de entradas o programa.
  - `string Organizer`: Entidad organizadora (ej. "Jugamos Tod@s", "IFEMA").
  - `bool IsOfficial`: Distintivo de gran cita oficial del calendario.
- **Métodos de Dominio:**
  - `IsOngoing(DateOnly today)`: Determina si el evento está en curso.
  - `IsPast(DateOnly today)`: Determina si el evento ya concluyó.
  - `DaysUntilStart(DateOnly today)`: Calcula los días exactos hasta el inicio.
  - `GetFormattedDates()`: Genera formato editorial en español (ej. "11-13 Oct 2026").
  - `Update(...)`: Mutación controlada con validación y marca temporal `UpdatedAt`.

### 2.2 Entidad `Giveaway`
Ubicación: `src/Ludeka.Core/Entities/Giveaway.cs`
- **Atributos Clave:** `IsPromoted` (booleano), `ThumbnailUrl`, `DeadlineAt`, `Platform`, `IsCommunityExclusive`.
- **Método:** `SetPromoted(bool isPromoted)` para conmutar estado de patrocinio con marca `UpdatedAt`.

### 2.3 Entidad `WeeklyRelease`
Ubicación: `src/Ludeka.Core/Entities/WeeklyRelease.cs`
- **Atributos Clave:** `Title`, `Publisher`, `ReleaseDate`, `EstimatedPvp`, `IsReprint`, `Notes`, `CoverImageUrl`.

---

## 3. Capa de Aplicación (`Ludeka.Application`)

### 3.1 Contratos y Servicios
- **`IBoardGameEventService` / `BoardGameEventService`:**
  - `GetUpcomingEventsAsync(int limit = 50)`: Retorna eventos donde `EndDate >= Hoy` ordenados por `StartDate` ascendente.
  - `GetPastEventsAsync(int limit = 50)`: Retorna eventos concluidos (`EndDate < Hoy`) ordenados por `EndDate` descendente.
  - `CreateEventAsync(CreateBoardGameEventRequest request)`: Alta de eventos con invariantes.
  - `UpdateEventAsync(Guid id, UpdateBoardGameEventRequest request)`: Actualización de datos.
  - `DeleteEventAsync(Guid id)`: Eliminación del evento.
- **`IGiveawayService` / `GiveawayService`:**
  - `GetGiveawaysAsync(bool includeExpired)`: Orden prioritario estricto: `IsPromoted DESC`, seguido de `DeadlineAt ASC`.
  - `SetPromotedAsync(Guid id, bool isPromoted)`: Mutación inmediata del flag de patrocinio.
- **`IWeeklyReleaseService` / `WeeklyReleaseService`:**
  - `CreateReleaseAsync(CreateWeeklyReleaseRequest request)`: Alta manual por moderación.
- **`IImageStorageService`:**
  - `SaveEventPosterAsync(...)`: Guarda carteles en `wwwroot/images/events/`.
  - `SaveCommunityImageAsync(...)`: Guarda imágenes en subcarpetas de comunidad (`draws`, `releases`).

---

## 4. Componentes y Vistas Blazor (`Ludeka.Web`)

1. **`MainLayout.razor`:**
   - Barra de navegación y pie de página con accesos limpios a `/sorteos`, `/novedades` y `/eventos`.
   - Botón directo de administración `/admin/eventos` para usuarios con roles `FoundingTeam` o `Moderator`.
2. **`Radar.razor` (`/sorteos` y alias `/radar`):**
   - Especializado exclusivamente en sorteos. Si el usuario accede por `/radar`, muestra banner informativo hacia novedades y eventos.
   - Conmutación en 1 clic de `IsPromoted` para moderadores y badge destacado "⭐ Promocionado".
3. **`News.razor` (`/novedades`):**
   - Cronología editorial de lanzamientos de viernes, buscador por texto, filtro por editorial y modal para alta manual de novedades.
4. **`Events.razor` (`/eventos`):**
   - Calendario con dos pestañas ("Próximas Citas" e "Histórico de Ediciones"), tarjetas con carteles, fecha formateada, cuenta atrás y enlace directo a la web oficial (`rel="noopener noreferrer"`).
5. **`EventsManagement.razor` (`/admin/eventos`):**
   - Panel de control CRUD protegido por roles para la Mesa Fundadora y moderadores, con subida directa de carteles locales (`InputFile`) y vista previa.
