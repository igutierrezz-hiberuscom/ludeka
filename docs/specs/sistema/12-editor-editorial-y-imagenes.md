# 12. Editor Editorial de Fichas de Catálogo y Carga de Imágenes para Moderadores

## 1. Visión General y Propósito
El módulo de edición editorial proporciona al equipo fundador y moderadores de Ludeka la facultad de corregir, enriquecer y actualizar en tiempo real cualquier ficha de catálogo (títulos en español y original, autor, editorial, año, sinopsis, parámetros de mesa, duraciones, edades, modo solitario y ADN lúdico) y cargar/reemplazar imágenes de carátula (subida directa de ficheros físicos `.jpg`, `.jpeg`, `.png`, `.webp` $\le 5\text{ MB}$ con almacenamiento en disco local o URL remota con previsualización en vivo).

Asimismo, establece el circuito cerrado con el módulo de reportes comunitarios (INC-17): los moderadores pueden corregir la ficha directamente desde la bandeja de reportes mediante el botón `[ ✏️ Corregir Ficha y Resolver ]`, persistiendo los cambios en el catálogo, actualizando el reporte a estado `Resolved` con notas de moderador y registrando la auditoría completa en `GameEditLog`.

---

## 2. Modelo de Dominio e Invariantes (`Ludeka.Core`)

### 2.1 Métodos en `Game.cs`
Ubicación: [`src/Ludeka.Core/Entities/Game.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/Game.cs)

- `UpdateCatalogInformation(...)`: Actualiza títulos, autor, editorial, año de publicación (1900–2100), descripción/sinopsis, confrontación, estilo, modo solitario, edad, dependencia de idioma, footprint, duración y rango de jugadores con ajuste de `Scalability`.
- `AdjustScalability(int minPlayers, int maxPlayers)`: Ajusta dinámicamente las entradas del semáforo para el nuevo intervalo de jugadores, preservando votos y recomendaciones previas e inicializando los nuevos puestos como `Recommended`.
- `UpdateImages(string? coverImageUrl, string? thumbnailUrl = null)`: Actualiza las rutas relativas o URLs de portada y miniatura.

### 2.2 Entidad de Auditoría `GameEditLog`
Ubicación: [`src/Ludeka.Core/Entities/GameEditLog.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/GameEditLog.cs)

- `Id`: `Guid` único identificador.
- `GameId`: `Guid` del juego modificado.
- `EditorUserId`: Identificador del usuario que realizó la edición.
- `EditorName`: Nombre/alias legible del moderador.
- `SummaryOfChanges`: Resumen en lenguaje natural de los atributos modificados.
- `AssociatedReportId`: `Guid?` del reporte comunitario resuelto en la misma operación (si aplica).
- `EditedAt`: `DateTimeOffset` UTC.

---

## 3. Servicios de Aplicación (`Ludeka.Application`)

- [`IGameEditorService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IGameEditorService.cs) & [`GameEditorService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Catalog/GameEditorService.cs):
  - Verifica autorización (`ICurrentUserService.IsFoundingTeam || ICurrentUserService.IsInRole("Moderator")`).
  - Obtiene y actualiza la entidad `Game` mediante sus métodos de dominio.
  - Persiste los cambios a través de `IGameRepository.UpdateAsync`.
  - Registra el asiento de auditoría en `IGameEditLogRepository`.
  - Si `AssociatedReportId` está presente, invoca automáticamente `IGameIssueReportService.ChangeStatusAsync` pasando el reporte a `Resolved`.
  - Invalida la caché L1 en memoria llamando a `CachedCatalogService.Invalidate(slug)`.
  - Devuelve el `GameDetailDto` actualizado.
- [`IImageStorageService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IImageStorageService.cs):
  - Contrato para guardado seguro de carátulas y validación de URLs remotas.

---

## 4. Persistencia e Infraestructura (`Ludeka.Infrastructure`)

- [`PhysicalFileImageStorageService`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Services/PhysicalFileImageStorageService.cs):
  - Almacena las imágenes en `wwwroot/images/games/`.
  - Nombres canónicos higienizados: `{slug}-cover-{timestamp}.{ext}`.
  - Valida extensiones admitidas (`.jpg`, `.jpeg`, `.png`, `.webp`) y tope de seguridad de 5 MB.
- [`SqliteGameRepository.UpdateAsync`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteGameRepository.cs):
  - Reconcilia y persiste en SQLite todos los campos actualizados de `Game`.
- [`SqliteGameEditLogRepository`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Repositories/SqliteGameEditLogRepository.cs):
  - Repositorio de auditoría sobre la tabla `GameEditLogs`.
- [`SqliteSchemaMigrator`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteSchemaMigrator.cs):
  - Crea defensivamente la tabla `GameEditLogs` con índices en `GameId` y `EditedAt` en bases existentes.

---

## 5. Componentes Web y Experiencia de Usuario (`Ludeka.Web`)

- [`GameEditorModal.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/GameEditorModal.razor):
  - Modal interactivo con diseño editorial oscuro y 4 pestañas (`Metadatos`, `Mesa y ADN`, `Sinopsis`, `Carátula`).
  - Cumplimiento de accesibilidad WCAG 2.2 AA (`role="dialog"`, `aria-modal="true"`, foco y soporte de tecla Escape).
  - Componente `InputFile` para subida reactiva y campo para URL externa con previsualización 1:1.
  - Banner contextual para incidencias comunitarias y campo de notas de resolución.
- [`GameDetail.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/GameDetail.razor):
  - Botón contextual `[ ✏️ Editar Ficha ]` visible solo para moderadores/fundadores.
  - Actualización inmediata en caliente Zero-FOUC tras guardar.
- [`GameReportsModeration.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/GameReportsModeration.razor):
  - Botón de acción cruzada `[ ✏️ Corregir Ficha y Resolver ]` en cada tarjeta de reporte pendiente o en revisión.
  - Cierra la incidencia y refresca la bandeja en tiempo real al confirmar la corrección.
