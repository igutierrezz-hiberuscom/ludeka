# 11. Reportes Comunitarios de Errores y Moderación de Fichas

## 1. Visión General y Propósito
El módulo de reporte comunitario permite a cualquier usuario registrado o visitante anónimo reportar erratas e inconsistencias observadas en las fichas del catálogo en 2 clics (imágenes incorrectas, rangos de jugadores o semáforos desfasados, enlaces rotos, etc.). Los moderadores y el equipo fundador disponen de una bandeja de entrada unificada para clasificar, revisar, resolver o descartar estas incidencias con notas de auditoría.

---

## 2. Tipologías de Incidencia (`GameIssueType`)

| Código | Etiqueta | Icono | Descripción |
|---|---|---|---|
| `WrongImage` | Imagen incorrecta o de otra edición | 🖼️ | La carátula no corresponde al juego, es de baja calidad o es de otra edición lingüística. |
| `BrokenImage` | Imagen no carga o enlace roto | 🚫 | La imagen muestra error 404 o no se visualiza. |
| `IncorrectPlayerCount` | Nº de jugadores o semáforo erróneo | 👥 | Rango de comensales mínimo/máximo desajustado o semáforo defectuoso. |
| `IncorrectDuration` | Duración de partida desajustada | ⏱️ | Tiempo estimado por partida incongruente. |
| `IncorrectAge` | Edad recomendada inconsistente | 🎂 | Edad mínima recomendada o legal desfasada. |
| `ErroneousMetadata` | Erratas en metadatos | 📝 | Título, diseñador, editorial, año o sinopsis con erratas tipográficas. |
| `BrokenPurchaseLink` | Enlace de compra o tienda defectuoso | 🛒 | Enlace a tienda o afiliado roto o sin stock permanente. |
| `Other` | Otro problema o sugerencia libre | 💬 | Otra sugerencia o errata no tipificada con detalle obligatorio. |

---

## 3. Modelo de Dominio y Estados (`Ludeka.Core`)

### 3.1 Entidad `GameIssueReport`
Ubicación: [`src/Ludeka.Core/Entities/GameIssueReport.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/GameIssueReport.cs)

- `Id` (Guid), `GameId` (Guid), `GameSlug` (string), `GameTitle` (string).
- `IssueType`: Enumerado `GameIssueType`.
- `Details`: Texto explicativo aportado por el usuario (hasta 1.000 caracteres).
- `ReportedByUserId`: Identificador del usuario si estaba autenticado.
- `ReporterNameOrAlias`: Nombre/alias ("Comunidad anónima" por defecto).
- `Status`: Enumerado `GameReportStatus`:
  - `Pending`: Recibido y a la espera de triaje.
  - `InReview`: Tomado en revisión por un moderador.
  - `Resolved`: Subsanado y cerrado con notas.
  - `Dismissed`: Descartado con motivo explicativo.
- `ModeratorNotes`: Justificación o acción efectuada por el moderador.
- `ResolvedByUserId`: Identificador del moderador actuante.
- `CreatedAt`, `UpdatedAt`, `ResolvedAt`: Marcas temporales UTC.

---

## 4. Servicios y Persistencia (`Ludeka.Application` & `Ludeka.Infrastructure`)

- **Contratos:**
  - [`IGameIssueReportRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IGameIssueReportRepository.cs): Consultas asíncronas, filtrado combinado y cálculo de métricas de resumen.
  - [`IGameIssueReportService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IGameIssueReportService.cs): Lógica de creación con validaciones defensivas y transiciones de estado.
- **Implementaciones:**
  - [`GameIssueReportService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Reports/GameIssueReportService.cs): Servicio de aplicación.
  - [`SqliteGameIssueReportRepository.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Repositories/SqliteGameIssueReportRepository.cs): Repositorio sobre SQLite y EF Core.
  - [`LudekaDbContext.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/LudekaDbContext.cs): Tabla `GameIssueReports` con índices compuestos por `Status`, `GameId`, `IssueType` y `CreatedAt`.

---

## 5. Componentes Web (`Ludeka.Web`)

- [`GameReportModal.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/GameReportModal.razor): Modal accesible (`role="dialog"`, `aria-modal="true"`) para reporte en 2 clics integrado en la cabecera de [`GameDetail.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/GameDetail.razor).
- [`GameReportsModeration.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/GameReportsModeration.razor): Bandeja centralizada en `/moderacion/reportes` y `/admin/reportes` con resumen de KPI, filtros por estado y tipología, buscador y acciones rápidas de moderador (revisar, resolver, descartar, reabrir).
