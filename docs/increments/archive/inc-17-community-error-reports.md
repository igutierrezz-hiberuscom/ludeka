# Incremento 17: Sistema Comunitario de Reporte de Errores y Bandeja de Moderación de Fichas

- **Identificador SDD:** `change-17-community-error-reports`
- **Estado:** ✅ **Archivado**
- **Puntos del MVP cubiertos:** Calidad de catálogo, Moderación colaborativa y feedback comunitario continuo.
- **Objetivo Principal:** Permitir a cualquier jugador o visitante de la comunidad reportar incidencias o erratas en las fichas de juego de forma intuitiva en 2 clics (imagen incorrecta o de baja calidad, datos erróneos de jugadores/duración/edad, enlaces rotos, etc.), y dotar a los moderadores de una bandeja de entrada centralizada para filtrar, revisar y gestionar el estado de dichos reportes.

---

## 1. Alcance Funcional Completado

1. **Acción de Reporte en la Ficha de Juego (`GameDetail.razor`):**
   - Botón contextual y accesible en la cabecera/barra de acciones: `[ 🚩 Reportar problema ]`.
   - Accesible tanto para usuarios autenticados como para invitados/comunidad abierta.
2. **Modal de Reporte Rápido (`GameReportModal.razor`):**
   - Selector tipificado de motivos de incidencia:
     - `WrongImage`: Imagen incorrecta, desactualizada o perteneciente a otra edición.
     - `BrokenImage`: La imagen no carga o presenta enlace caído.
     - `IncorrectPlayerCount`: Rango de comensales (mínimo/máximo) o semáforo erróneo.
     - `IncorrectDuration`: Tiempo estimado por partida desajustado.
     - `IncorrectAge`: Edad legal o recomendada inconsistente.
     - `ErroneousMetadata`: Título, diseñador, editorial, año o sinopsis con erratas.
     - `BrokenPurchaseLink`: Enlace a tienda o afiliado defectuoso o fuera de stock permanente.
     - `Other`: Otras incidencias con campo de texto libre.
   - Campo opcional de sugerencia de corrección y detalles adicionales (hasta 1.000 caracteres).
   - Feedback inmediato con microtexto de agradecimiento ("*¡Gracias por ayudarnos a mantener el catálogo impecable!*").
3. **Entidad de Dominio y Persistencia (`GameIssueReport`):**
   - Atributos: `Id`, `GameId`, `GameSlug`, `GameTitle`, `IssueType`, `Details`, `ReportedByUserId`, `ReporterNameOrAlias`, `Status` (`Pending`, `InReview`, `Resolved`, `Dismissed`), `ModeratorNotes`, `CreatedAt`, `UpdatedAt`, `ResolvedAt`, `ResolvedByUserId`.
   - Repositorio desacoplado `IGameIssueReportRepository` y servicio de aplicación `IGameIssueReportService`.
4. **Bandeja de Reportes en el Panel de Moderación (`GameReportsModeration.razor`):**
   - Vista en `/moderacion/reportes` y `/admin/reportes`.
   - Filtros rápidos por estado (`Pendientes`, `En Revisión`, `Resueltos`, `Descartados`) y por tipología de fallo.
   - Acciones de moderación:
     - *Marcar en Revisión*: Bloquea o destaca el reporte para que otros moderadores sepan que está siendo atendido.
     - *Descartar*: Cierra el reporte con un motivo justificativo.
     - *Resolver*: Marca como resuelto registrando las notas de la acción efectuada.
     - *Reabrir*: Reactiva reportes cerrados.
     - *Enlace directo*: Acceso en 1 clic a la ficha del juego reportado.

---

## 2. Criterios de Aceptación Cumplidos (Gherkin)

```gherkin
Escenario: Usuario comunitario reporta una imagen incorrecta
  Dado que un usuario está visualizando la ficha de un juego
  Cuando pulsa el botón "🚩 Reportar problema"
  Y selecciona el motivo "Imagen incorrecta o de otra edición"
  Y añade el detalle opcional "La carátula corresponde a la segunda edición en inglés"
  Y envía el formulario
  Entonces el reporte queda guardado en la base de datos en estado "Pending"
  Y la interfaz muestra una confirmación visual de agradecimiento sin recargar la página

Escenario: Moderador visualiza y filtra los reportes pendientes
  Dado un usuario autenticado con rol "Moderator" o "FoundingTeam"
  Cuando accede a la sección de moderación de reportes de catálogo
  Entonces visualiza una lista paginada con los reportes clasificados
  Y puede filtrar específicamente por el tipo de fallo y estado "Pending"

Escenario: Moderador descarta un reporte falso o duplicado
  Dado un reporte en estado "Pending" en la bandeja de moderación
  Cuando el moderador pulsa "Descartar" e indica el motivo "La imagen es la edición canónica oficial"
  Entonces el estado del reporte pasa a "Dismissed"
  Y queda registrado el identificador del moderador y la fecha de resolución
```

---

## 3. Arquitectura y Componentes Clave

- **Dominio (`Ludeka.Core`):**
  - Entidad `GameIssueReport.cs`.
  - Enumerados `GameIssueType.cs` y `GameReportStatus.cs`.
- **Aplicación (`Ludeka.Application`):**
  - Contratos `IGameIssueReportRepository.cs` e `IGameIssueReportService.cs`.
  - DTOs `GameIssueReportDtos.cs` (`GameIssueReportDto`, `CreateGameReportCommand`, `UpdateGameReportStatusCommand`, `GameReportFilter`, `GameIssueReportSummaryDto`).
  - Implementación `GameIssueReportService.cs`.
- **Infraestructura (`Ludeka.Infrastructure`):**
  - Repositorio EF Core `SqliteGameIssueReportRepository.cs`.
  - Configuración de tabla `GameIssueReports` en `LudekaDbContext`.
- **Presentación Web (`Ludeka.Web`):**
  - Componente `GameReportModal.razor` (integrado en `GameDetail.razor`).
  - Página `GameReportsModeration.razor` (`/moderacion/reportes`).
  - Navegación en `MainLayout.razor` y acceso rápido en `MediaModeration.razor`.
- **Pruebas (`Ludeka.UnitTests`):**
  - `GameIssueReportTests.cs` (17 tests).
  - `GameIssueReportServiceTests.cs` (8 tests).
  - `SqliteGameIssueReportRepositoryTests.cs` (4 tests).
  - `GameReportsWebIntegrationTests.cs` (3 tests).

---

## 4. Sinergia con otros Incrementos

- **Incremento 18 (Editor Editorial de Fichas y Carga de Imágenes):** Proporciona la herramienta con la que el moderador subsana físicamente el fallo reportado en INC-17 (por ejemplo, reemplazando la imagen o editando los textos).
