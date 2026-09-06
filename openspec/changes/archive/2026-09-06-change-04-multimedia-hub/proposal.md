# Propuesta: Incremento 4 — Hub Multimedia (YouTube e Instagram)

## Intención
Diseñar e implementar el **Hub Multimedia** de Ludeka, organizando de forma limpia los contenidos de YouTube e Instagram en la ficha del juego mediante **tres pestañas horizontales segregadas que evitan la mezcla caótica de formatos** (Tutoriales 16:9, Partidas completas 16:9 con badge obligatorio de número de comensales, y Opiniones/Redes con fotos 1:1 y vídeos 9:16), junto con una estrategia de ingesta acotada offline-first curada con canales hispanos de referencia y un panel móvil de moderación rápida con bandeja de vídeos huérfanos y detector de enlaces rotos.

---

## Alcance

### Dentro del Alcance
- **Pestañas Horizontales Segregadas en la Ficha de Juego (Punto 5.1 de la Spec):**
  - *Pestaña 1: 🎬 Tutoriales (YouTube):* Miniaturas 16:9 con duración, canal y modal de reproducción embebido.
  - *Pestaña 2: 🎲 Partidas Completas (YouTube):* Miniaturas 16:9 con **badge identificativo obligatorio de comensales** (ej. *"Partida a 2"*), canal y duración.
  - *Pestaña 3: 💬 Opiniones y Redes (Instagram + Shorts/Reels):* Posts cuadrados 1:1 con autor `@canal`, likes y extracto de texto, junto a vídeos verticales 9:16 con duración e icono de reproducción rápida.
- **Pipeline de Ingesta Acotada y Cold Start Curado (Punto 5.2 de la Spec):**
  - Seeder curado enriquecido con contenidos multimedia reales del ecosistema de divulgación hispanohablante (Análisis-Parálisis, Zacatrus TV, El Troquel, Meepletopia, Rincón Legacy, Juegos de Mesa 221B, etc.) para los 15 juegos del catálogo base.
  - Regla de corte: máximo 2 mejores vídeos/partidas por juego en la ingesta inicial para evitar saturación y respetar cuotas.
- **Panel Móvil de Moderación Rápida (Punto 5.3 de la Spec):**
  - Página `/moderacion/multimedia` con acceso restringido para roles `FoundingTeam` y `Moderator`.
  - Revisión táctil en 1 clic: `[ ✅ Aprobar ]` y `[ ❌ Descartar ]`.
  - **Bandeja de Huérfanos:** Listado de contenidos no vinculados automáticamente (`GameId == null`) y selector ergonómico `[ 🔗 Asignar Juego ]` para asociarlos de inmediato.
  - **Detector de Enlaces Rotos:** Proceso de verificación para identificar enlaces caídos (HTTP 404) y despublicarlos o reactivarlos en 1 clic.
- **Persistencia en SQLite EF Core 10:**
  - Nueva entidad `MediaItem` mapeada en `LudekaDbContext` con índices sobre `GameId`, `Status`, `Type` y `Platform`.
- **Suite de Pruebas Automatizadas:**
  - Tests unitarios de dominio para invariantes de `MediaItem`.
  - Tests de aplicación para orquestación de pestañas y moderación.
  - Tests de integración de repositorio y persistencia en SQLite.

### Fuera del Alcance
- Importador masivo y sincronización de colecciones con BGG XMLAPI2 (Incremento 5: `change-05-bgg-importer`).
- Publicación desatendida hacia Meta Graph API / Instagram oficial de Ludeka y radar de sorteos (Incremento 6: `change-06-automation-community`).
- Consultorio de dudas de reglas Q&A (Incremento 6: `change-06-automation-community`).

---

## Capacidades

### Nuevas Capacidades
- `multimedia-hub-tabs`: Visualización segregada por pestañas en la ficha del juego con miniaturas 16:9, badges de número de jugadores en partidas, tarjetas 1:1 de Instagram, vídeos 9:16 y modal reproductor embebido sin mezclar formatos.
- `media-ingestion-curated`: Ingesta inicial acotada (cold start) y contratos de servicios multimedia para enriquecer juegos del catálogo con máximo 2 mejores vídeos por título.
- `media-moderation-panel`: Panel táctil móvil `/moderacion/multimedia` con acciones de aprobación/descarte en 1 clic, bandeja de contenidos huérfanos y detector de enlaces rotos.

### Capacidades Modificadas
- `core-catalog`: Ficha de juego `GameDetail.razor` ampliada con la sección multimedia mediante `MultimediaHub.razor`.
- `editorial-role-management`: Integración de accesos al panel de moderación en la barra de navegación `MainLayout.razor` para moderadores y equipo fundador.

---

## Enfoque Arquitectónico

- **Dominio (`Ludeka.Core`)**:
  - Entidad `MediaItem` con validación de URL, invariante obligatoria de `PlayerCountBadge` para partidas completas (`Playthrough`), estado de moderación y trazabilidad temporal.
  - Enums `MediaType`, `MediaPlatform` y `ModerationStatus`.
- **Aplicación (`Ludeka.Application`)**:
  - Contratos `IMediaRepository` y `IMediaService`.
  - DTOs `MediaItemDto`, `GameMediaHubDto` (listas segregadas por pestañas) y DTOs de moderación (`ModerateMediaItemRequest`, `AssignMediaGameRequest`, `BrokenLinkReportDto`).
  - Casos de uso de filtrado, aprobación, asignación de huérfanos y verificación de enlaces.
- **Infraestructura (`Ludeka.Infrastructure`)**:
  - `DbSet<MediaItem>` en `LudekaDbContext` con relaciones e índices.
  - Repositorio `SqliteMediaRepository`.
  - Seeder multimedia enriquecido con datos reales hispanohablantes para los 15 juegos.
  - Servicio detector de enlaces rotos.
- **Presentación Blazor (`Ludeka.Web`)**:
  - Componente `MultimediaHub.razor` con pestañas reactivas, microtextos de marca y reproductor `MediaEmbedModal.razor`.
  - Página `/moderacion/multimedia` (`MediaModeration.razor`) con pestañas de moderación y botones ergonómicos táctiles.

---

## Áreas Afectadas

| Área | Impacto | Descripción |
|---|---|---|
| `Ludeka.Core` | Nuevo | Entidad `MediaItem`, enums `MediaType`, `MediaPlatform`, `ModerationStatus` |
| `Ludeka.Application` | Nuevo | Contratos `IMediaRepository`, `IMediaService`, DTOs y lógica de segregación multimedia |
| `Ludeka.Infrastructure` | Modificado | Tabla `MediaItems` en `LudekaDbContext`, `SqliteMediaRepository`, seeder curado y verificador de enlaces |
| `Ludeka.Web` | Nuevo / Modificado | Componente `MultimediaHub`, modal embebido, página de moderación `/moderacion/multimedia` y enlace en `MainLayout` |
| `tests/Ludeka.UnitTests` | Nuevo | Tests unitarios de dominio, servicios de moderación, repositorio SQLite y presentación |

---

## Riesgos y Mitigaciones

| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Incompatibilidad de iframes embebidos por restricciones de YouTube | Media | Se proporciona reproductor embebido estándar (`youtube.com/embed/...`) con botón de enlace directo externo de respaldo |
| Deformación visual al mezclar formatos de imagen | Alta | Segregación estricta por pestañas: la pestaña 1 y 2 fuerzan aspecto 16:9; la pestaña 3 separa fotos cuadradas 1:1 de reels 9:16 |
| Caída de enlaces externos a lo largo del tiempo | Media | Inclusión de la herramienta de detección de enlaces rotos en el panel de moderación para despublicar 404 en 1 clic |
| Ruptura de cuotas de APIs externas | Baja | Enfoque offline-first con catálogo curado y regla de ingesta acotada a máximo 2 vídeos por juego |

---

## Plan de Rollback
- Revertir los cambios del incremento 4 en git.
- Eliminar la tabla `MediaItems` en `LudekaDbContext` y regenerar la base de datos `ludeka.db`.

## Dependencias
- Catálogo base y ficha inteligente (`change-01-core-catalog`).
- Gestión de roles de moderación y equipo fundador (`change-03-founding-verdict`).
