# 21. Generador y Publicador Directo de Posts para Instagram en Moderación

> **Incremento Asociado:** INC-28 (`change-28-instagram-direct-publisher`)  
> **Estado:** Implementado, Verificado y Documentado  
> **Módulo:** Moderación, Comunidad y Difusión Social  

---

## 1. Visión General y Propósito

El módulo de **Publicador Directo de Instagram** dota al equipo de moderación y a la Mesa Fundadora de Ludeka de una herramienta editorial integrada para generar, maquetar, previsualizar y publicar directamente en la cuenta oficial de Instagram de la plataforma sin salir de la aplicación web.

Permite transformar con un solo clic cualquier sorteo activo de la comunidad (`Giveaway`), novedad semanal de las editoriales (`WeeklyRelease`) o ficha destacada del catálogo (`Game`) en un post de Instagram de alto impacto visual (formato 1:1, 1080x1080 píxeles) acompañado de un texto formateado con menciones sociales, fechas límite, enlaces a la bio y hashtags temáticos lúdicos.

---

## 2. Modelo de Dominio (`Ludeka.Core`)

### 2.1. Entidad `InstagramPostDraft`
Representa el ciclo de vida editorial de una publicación para Instagram.

- **Identificadores y Origen:**
  - `Id`: Identificador único (GUID).
  - `SourceType`: Tipología de contenido origen (`InstagramPostSourceType`: `Giveaway`, `WeeklyRelease`, `Game`, `Manual`).
  - `SourceId`: Identificador de la entidad de origen vinculada (clave foránea lógica tipo string).
- **Contenido Visual y Textual:**
  - `Title`: Título editorial del post.
  - `Caption`: Texto enriquecido (copy) para Instagram (hasta 2.200 caracteres según límites de la plataforma).
  - `ImageUrl`: URL pública de la imagen de fondo o carátula de alta resolución.
  - `SvgContent`: Código vectorial SVG generado para la tarjeta cuadrada de marca 1080x1080.
  - `Theme`: Variante visual aplicada (`"Dark"` u `"Light"`).
- **Estado y Trazabilidad:**
  - `Status`: Estado del ciclo (`InstagramPostDraftStatus`: `Draft`, `Publishing`, `Published`, `Failed`).
  - `InstagramMediaId`: Identificador del medio asignado por la API de Meta tras publicar.
  - `InstagramPermalink`: Enlace permanente directo a la publicación en Instagram.
  - `ErrorMessage`: Detalle del fallo en caso de error de red o rechazo de la API de Meta.
  - `CreatedByUserId`, `CreatedByUserName`: Moderador responsable de la composición.
  - `CreatedAt`, `PublishedAt`, `UpdatedAt`: Marcas temporales UTC.

### 2.2. Enriquecimiento de Entidades Existentes
- **`Giveaway` (Sorteos):**
  - Campos `InstagramMediaId` y `InstagramPermalink`.
  - Propiedad calculada `IsPublishedOnInstagram => !string.IsNullOrWhiteSpace(InstagramPermalink)`.
  - Método de dominio `MarkPublishedOnInstagram(mediaId, permalink)`.
- **`WeeklyRelease` (Novedades Semanales):**
  - Campos `InstagramMediaId` y `InstagramPermalink`.
  - Propiedad calculada `IsPublishedOnInstagram => !string.IsNullOrWhiteSpace(InstagramPermalink)`.
  - Método de dominio `MarkPublishedOnInstagram(mediaId, permalink)`.
- **`ModeratorPermission`:**
  - Nuevo flag bitwise granular `CanPublishInstagram = 1 << 7` (128).
  - Actualización de la máscara acumulativa `All = 255`.
- **`AuditEntityType` y `AuditAction`:**
  - `AuditEntityType.InstagramPost` para la bitácora inmutable.
  - `AuditAction.Published` para registrar el lanzamiento oficial en redes sociales.

---

## 3. Arquitectura de Casos de Uso y Contratos (`Ludeka.Application`)

### 3.1. Compositor Visual y Textual (`IInstagramComposerService` y `InstagramComposerService`)
Genera el diseño vectorial y el copy optimizado sin dependencias de canvas o librerías pesadas en servidor:
- **`ComposeSvg(sourceType, sourceEntity, theme)`:** Retorna el documento SVG 1080x1080 con jerarquía visual:
  - Cabecera de marca (`Ludeka.` con punto naranja de marca y badge de sección).
  - Marco de carátula 800x450 con esquinas redondeadas (`rx="24"`) y sombras.
  - Píldoras flotantes dinámicas (ej. `🎁 Sorteo Activo`, `📰 Novedad Editorial`, `🔄 Reimpresión`, bandera del país o ámbito internacional).
  - Bloque tipográfico con título truncado elegantemente, organizador o editorial, PVP estimado y fecha límite.
  - Pie de marca con llamada a la acción hacia `ludeka.app`.
  - Soporte de temas: fondo oscuro profundo (`#0B0F17` a `#131B2A`) o editorial claro (`#FFFFFF` a `#F8FAFC`).
- **`GenerateCaption(sourceType, sourceEntity)`:** Compone el texto de la publicación estructurado con emojis, llamadas a la acción en la bio, mención social normalizada con prefijo `@` (`@malditogames`, `@asmodee_es`) y etiquetas temáticas (`#sorteojuegos`, `#novedadesludicas`, `#juegosdemesa`, `#ludeka`).

### 3.2. Cliente de Publicación de Instagram (`IInstagramApiClient`)
Encapsula la comunicación HTTP con la **Meta Graph API v19.0**:
- `CreateMediaContainerAsync(imageUrl, caption)`: POST a `/{ig-user-id}/media` para generar el contenedor `creation_id`.
- `PublishMediaAsync(creationId)`: POST a `/{ig-user-id}/media_publish` para publicar el contenedor en el feed oficial.
- `GetPermalinkAsync(mediaId)`: GET a `/{media-id}?fields=permalink` para obtener la URL pública final.
- **Modo Simulado (`Simulate = true`):** Permite ejecutar pruebas completas en entornos de desarrollo o integración sin credenciales reales de Meta ni túneles HTTPS externos, generando identificadores deterministas `sim_media_...` y permalinks funcionales.

### 3.3. Orquestador de Publicación (`IInstagramPublisherService` e `InstagramPublisherService`)
Coordina todo el flujo editorial:
1. Valida los permisos del moderador o pertenencia a la Mesa Fundadora.
2. Si se crea desde un sorteo o novedad, verifica si ya existe un borrador previo o crea uno nuevo a partir del compositor.
3. Al publicar (`PublishDraftAsync`):
   - Cambia el estado a `Publishing`.
   - Obtiene o compone la URL pública de la imagen (apuntando al endpoint vectorial `/api/instagram/card/{draftId}.svg` o imagen de carátula).
   - Ejecuta las dos fases contra `IInstagramApiClient`.
   - Actualiza el borrador a `Published` con su `mediaId` y `permalink`.
   - Sincroniza la entidad de origen (`Giveaway` o `WeeklyRelease`) invocando `MarkPublishedOnInstagram`.
   - Registra una entrada inmutable en `IAuditService` (`AuditEntityType.InstagramPost`, `AuditAction.Published`) con el diff de cambios.
   - En caso de excepción, captura el fallo, lo persiste en `draft.ErrorMessage`, marca el estado como `Failed` y no audita como publicado.

---

## 4. Persistencia e Infraestructura (`Ludeka.Infrastructure`)

- **Entity Framework Core (`LudekaDbContext`):**
  - Mapeo de la tabla `InstagramPostDrafts` con índices en `(SourceType, SourceId)` y `Status`.
  - Mapeo de nuevas columnas `InstagramMediaId` e `InstagramPermalink` en las tablas `Giveaways` y `WeeklyReleases`.
- **Reconciliación Automática de Base de Datos (`SqliteSchemaMigrator`):**
  - Detección automática en arranque de SQLite y ejecución idempotente de `CREATE TABLE IF NOT EXISTS InstagramPostDrafts` y `ALTER TABLE ADD COLUMN` para bases de datos existentes.
- **Repositorio Específico (`SqliteInstagramPostDraftRepository`):**
  - Consultas filtradas por estado, búsqueda por fuente origen y operaciones CRUD asíncronas con transaccionalidad.

---

## 5. Experiencia de Usuario y Capa Web (`Ludeka.Web`)

### 5.1. Panel de Moderación de Instagram (`/admin/instagram`)
- **Acceso Restringido:** Solo accesible por usuarios con rol `FoundingTeam` o moderadores con permiso explícito `CanPublishInstagram`. Si no está autorizado, presenta un estado de acceso denegado elegante.
- **Bandeja de Borradores:** Barra lateral con filtros por estado (`Todos`, `Borradores`, `Publicados`, `Fallidos`), ordenados cronológicamente con distintivo visual de la fuente.
- **Simulador Fidedigno de Feed de Instagram:**
  - Cabecera con avatar oficial de Ludeka y anillo con gradiente oficial de Instagram.
  - Visor 1:1 con pestañas de previsualización: diseño vectorial SVG renderizado o carátula fotográfica.
  - Barra de interacciones de Instagram (corazón de me gusta con animación local, globo de comentarios, botón de compartir y guardar).
  - Previsualización dinámica del copy con expansión "más" si excede las dos líneas.
- **Editor en Vivo:**
  - Selector de tema en 1 clic (🌙 Oscuro / ☀️ Claro) con actualización reactiva en el visor.
  - Textarea con contador de caracteres (0 / 2.200) y alerta visual de límite.
  - Nube de hashtags sugeridos rápidos para insertar con un clic (`#juegosdemesa`, `#ludeka`, `#sorteojuegos`, etc.).
  - Botón de guardado manual del borrador.
  - Botón de copiado al portapapeles (`navigator.clipboard.writeText`) para soporte de publicación manual alternativa.
  - Botón de despacho directo a la API de Meta (`Despachar a Instagram API`), con estado de carga, deshabilitación de seguridad y enlace directo al post publicado.
- **Modales de Creación Rápida:**
  - Permite abrir un selector de sorteos activos o novedades editoriales para crear un nuevo borrador en 2 clics.
  - Soporte de deep-linking con query parameters: `/admin/instagram?SourceType=Giveaway&SourceId={id}` para saltar directamente desde las tarjetas públicas de sorteos y novedades.

### 5.2. Puntos de Entrada Contextuales
- **Tarjeta de Sorteo (`GiveawayCard.razor`):** Si el usuario es moderador con permiso, muestra el botón de acceso directo `[ 📸 Crear Post Instagram ]` o el badge informativo con enlace `[ 📸 Publicado en Instagram ↗ ]`.
- **Vista de Novedades (`News.razor`):** Botón `[ 📸 Instagram ]` y badge verde con enlace permanente si ya fue emitido a la red.
- **Barra de Navegación de Moderación (`MainLayout.razor`):** Acceso directo `Instagram` en la barra superior para moderadores.
- **Modal de Permisos de Usuario (`UserPermissionsModal.razor`):** Casilla de verificación para otorgar o revocar `CanPublishInstagram` por la Mesa Fundadora.

### 5.3. Endpoint Minimal API de Tarjetas Vectoriales
- `GET /api/instagram/card/{draftId:guid}.svg`: Devuelve el SVG del borrador como `image/svg+xml` con cabeceras de caché controladas para que pueda ser consumido por bots, previsualizaciones o descargas directas.

---

## 6. Verificación Automatizada

La funcionalidad completa se encuentra validada mediante **681 pruebas unitarias e integración en verde** (0 fallos, 0 omitidos), entre las cuales destacan:

1. **`InstagramPostDraftTests`:**
   - Creación de borrador con validación de cadenas no vacías y trimming.
   - Modificación de contenido mediante `UpdateDraft`.
   - Transiciones de estado: `MarkPublishing`, `MarkPublished`, `MarkFailed`.
   - Integración con `Giveaway.MarkPublishedOnInstagram` y `WeeklyRelease.MarkPublishedOnInstagram`.
2. **`InstagramComposerServiceTests`:**
   - Generación de SVG de sorteos en tema Oscuro y Claro.
   - Generación de SVG de novedades editoriales con badges de reimpresión.
   - Generación de SVG para juegos del catálogo con nota e identidad de Ludeka.
   - Formateo de handles y hashtags temáticos.
3. **`InstagramApiClientTests`:**
   - Generación de identificadores simulados en modo `Simulate = true`.
   - Manejo de excepciones ante URLs o identificadores vacíos.
4. **`InstagramPublisherServiceTests`:**
   - Creación y persistencia de borradores a partir de entidades existentes.
   - Flujo exitoso de publicación con actualización bidireccional en repositorios.
   - Registro obligatorio en la bitácora inmutable de auditoría (`AuditAction.Published`).
   - Manejo de fallos de API y marcado como `Failed` sin generar auditoría de publicación ficticia.
