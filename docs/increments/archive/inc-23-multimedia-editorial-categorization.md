# Incremento 23: Categorización y Gestión Editorial de Vídeos y Multimedia en Fichas de Juego

- **Identificador SDD:** `change-23-multimedia-editorial-categorization`
- **Estado:** ✅ **Archivado / Implementado**
- **Fecha de Cierre:** 2026-09-08
- **Puntos de la Especificación:** Hub Multimedia (INC-04), Moderación Editorial Rápida, Gestión de Fichas de Juego (INC-18), Permisos Granulares (INC-20).
- **Pruebas Automatizadas:** 531 superadas (100% en verde, +59 pruebas agregadas en este ciclo).
- **Objetivo Cumplido:** Dotar al sistema de una clasificación taxonómica formal de contenidos audiovisuales (`QuickOverview` ["Cómo Funciona"], `Tutorial`, `Gameplay`, `ReviewOpinion`) tanto en el panel de moderación de ingesta como directamente en la ficha de cada juego. Los administradores y moderadores con el permiso correspondiente (`CanApproveMedia` o `FoundingTeam`) pueden reasignar la categoría, transferir un vídeo a otro juego del catálogo con buscador asistido con autocompletado, o eliminarlo con confirmación irreversible sin necesidad de abandonar la vista de detalle del juego, todo registrado en la bitácora central de auditoría.

---

## 1. Alcance Funcional y Técnico Entregado

1. **Taxonomía Formal de Contenidos Multimedia (`MediaCategory`):**
   - Incorporación del enum `MediaCategory` en el dominio:
     - `QuickOverview` (0): Vídeos breves de 2–3 minutos ("⚡ Cómo Funciona") que explican el juego por encima y sus mecánicas principales sin ser un tutorial completo.
     - `Tutorial` (1): Explicaciones detalladas de reglas completas, guías paso a paso de cómo jugar.
     - `Gameplay` (2): Partidas completas o demostrativas con distintivo obligatorio de comensales.
     - `ReviewOpinion` (3): Reseñas, análisis de sensaciones, unboxings y veredictos de creadores.
   - Clasificador heurístico inteligente [`MediaClassifier.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Helpers/MediaClassifier.cs) con expresiones regulares semánticas en español e inglés.

2. **Categorización en el Panel de Moderación Multimedia (`/admin/multimedia`):**
   - Selector desplegable obligatorio con valor inicial precalculado por la heurística de `MediaClassifier`.
   - Soporte de aprobación atómica con asignación de categoría en `ApproveMediaAsync(id, category)`.

3. **Gestión Multimedia Directa en la Ficha del Juego (`MultimediaHub.razor` & `GameDetail.razor`):**
   - Para usuarios con roles `FoundingTeam` o con permiso granular `CanApproveMedia`:
     - Cada tarjeta multimedia dispone de un botón `[ ⚙️ Moderar ]`.
     - **Cambio Inmediato de Categoría:** Selector reactivo con actualización instantánea de la UI.
     - **Reasignación Asistida:** Modal con buscador de catálogo en vivo para transferir un vídeo mal catalogado a la ficha correcta.
     - **Eliminación Segura:** Confirmación modal para eliminar enlaces rotos u obsoletos.
   - Subsección destacada en la pestaña social para **Reseñas en Vídeo** panorámicas.

4. **Integración con el Registro de Auditoría y Migración:**
   - Registro con diff detallado en `AuditLogEntry` (`AuditEntityType.Media`) para `Updated` y `Deleted`.
   - Reconciliación automática de esquema SQLite en el paso 15 de `SqliteSchemaMigrator` añadiendo la columna `Category` e índice.

---

## 2. Criterios de Aceptación Verificados

```gherkin
Escenario: Moderador cambia la categoría de un vídeo desde la ficha de juego
  Dado un moderador autenticado con permiso "CanApproveMedia" en la ficha de "Wingspan"
  Y un vídeo clasificado erróneamente en la pestaña "Opiniones" que en realidad explica cómo jugar
  Cuando pulsa en el menú contextual del vídeo y selecciona la categoría "Tutorial"
  Entonces el vídeo desaparece de la pestaña "Opiniones"
  Y pasa a renderizarse automáticamente en la pestaña "Tutoriales"
  Y se genera una entrada de auditoría con la acción "Updated" para la entidad "Media"

Escenario: Moderador reasigna un vídeo a otro juego distinto
  Dado un moderador en la ficha de "Ark Nova" visualizando un vídeo que corresponde a su expansión "Ark Nova: Mundos Marinos"
  Cuando pulsa "Reasignar Juego" y busca "Ark Nova: Mundos Marinos"
  Y confirma el cambio
  Entonces el vídeo se desvincula de "Ark Nova"
  Y queda visible exclusivamente en la ficha de "Ark Nova: Mundos Marinos"

Escenario: Usuario no autorizado no puede ver los controles de moderación multimedia
  Dado un usuario con rol comunitario navegando por la ficha de "Catan"
  Cuando visualiza los vídeos de YouTube
  Entonces puede reproducirlos con normalidad
  Pero no visualiza ningún botón o menú de edición, cambio de categoría ni borrado
```
