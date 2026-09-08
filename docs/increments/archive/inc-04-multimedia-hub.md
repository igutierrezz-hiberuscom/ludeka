# Incremento 4: Hub Multimedia (YouTube e Instagram)

- **Identificador SDD:** `change-04-multimedia-hub`
- **Puntos del MVP cubiertos:** 5.1, 5.2, 5.3 (Hub multimedia segregado e ingesta).
- **Estado:** ✅ **Completado y Archivado** (Commit `eb03473`).

---

## 1. Alcance Funcional Entregado

1. **Pestañas horizontales limpias en la ficha sin mezclar formatos:**
   - *Pestaña 1:* 🎬 Tutoriales de YouTube (16:9 con canal y duración).
   - *Pestaña 2:* 🎲 Partidas completas de YouTube (16:9 con badge obligatorio de número de jugadores, ej. "Partida a 2").
   - *Pestaña 3:* 💬 Opiniones y Redes (posts cuadrados de Instagram y Reels/Shorts 9:16).
2. **Reproducción Embebida Respetuosa:** Modal de visualización accesible `MediaEmbedModal` con `role="dialog"`.
3. **Panel de Moderación Rápida Móvil:**
   - Interfaz en `/admin/moderacion-medios` para aprobar o descartar contenidos en 1 clic.
   - Detección de vídeos huérfanos y asignación a fichas de juego.
4. **Verificador de Enlaces Rotos:** Servicio `IBrokenLinkCheckerService` para salvaguardar la calidad del hub.

---

## 2. Artefactos y Componentes Clave

- **Dominio:** [`MediaItem`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/MediaItem.cs), [`MediaPlatform`](file:///c:/repos/Ludeka/src/Ludeka.Core/Enums/MediaPlatform.cs), [`MediaType`](file:///c:/repos/Ludeka/src/Ludeka.Core/Enums/MediaType.cs), [`ModerationStatus`](file:///c:/repos/Ludeka/src/Ludeka.Core/Enums/ModerationStatus.cs).
- **Aplicación:** [`IMediaService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IMediaService.cs), [`MediaService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Media/MediaService.cs).
- **Infraestructura:** [`SqliteMediaRepository`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteMediaRepository.cs), [`BrokenLinkCheckerService`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Services/BrokenLinkCheckerService.cs).
- **Web UI:** [`MultimediaHub.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/MultimediaHub.razor), [`MediaEmbedModal.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/MediaEmbedModal.razor), [`MediaModeration.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/MediaModeration.razor).

---

## 3. Verificación

- Pruebas unitarias de aprobación, descarte y segregación de pestañas en [`tests/Ludeka.UnitTests`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests).
