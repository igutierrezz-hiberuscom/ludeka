# Exploración: portada minimalista y creadores (`portada-minimalista-creadores`)

> Fase: `sdd-explore` — investigación de solo lectura. No se modificó código de la aplicación.
> Fecha: 2026-09-08. Idioma del artefacto: español castellano (regla suprema del repo).

---

## 1. Estado actual del sistema (lo relevante para este cambio)

La portada (`/`) es un dashboard editorial con un hero prominente, y el directorio "Autores" ya empezó
su migración conceptual hacia "Creadores" (rutas `/creadores` con alias `/autores`, entidad `Creator`
con `SocialLinks`), **pero el modelo de datos y el sembrado están sesgados a diseñadores de juegos**,
no a creadores de contenido. El scrapeo que hoy consume las redes de los creadores es solo el de
YouTube (búsqueda de vídeos por juego); no existe ningún batch nocturno de scrapeo de perfiles
sociales (Instagram/TikTok) — el "batch nocturno" real (`NightlyCatalogingService`) cataloga JUEGOS
de BGG/Gemini y no consume creadores.

## 2. Mapa del código (puntos 1-5 pedidos, con rutas y líneas exactas)

### 2.1 Hero de la portada

Componente: `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` (ruta `@page "/"`, línea 1).

| Elemento | Ubicación exacta |
|---|---|
| `<PageTitle>` "El Letterboxd de los juegos de mesa en español" | Líneas 8 |
| Badge "✨ PORTADA EDITORIAL" | Líneas 14-16 |
| Titular `h1` "El Letterboxd de los juegos de mesa." | Líneas 18-20 |
| Subtítulo descriptivo (párrafo) | Líneas 22-24 |
| Buscador (form `HandleQuickSearch` → `/catalogo?q=...`) | Líneas 26-43 (handler: 382-392) |
| Píldora "🎲 Catálogo Completo" → `/catalogo` | Líneas 47-49 |
| Píldora "🎁 Radar & Sorteos" → `/radar` | Líneas 50-52 |
| Píldora "🏢 Editoriales" → `/editoriales` | Líneas 53-55 |
| Píldora "✍️ Autores" → `/creadores` | Líneas 56-58 |

Nota: la píldora "Autores" ya apunta a `/creadores` (etiqueta desactualizada respecto a la ruta).
El hero completo vive en el bloque HTML de las líneas 12-61.

### 2.2 Link "Ver todas las novedades" — BUG CONFIRMADO

- `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` líneas 231-234:
  `<a href="/radar">Ver todas las novedades →</a>` en la sección "Novedades en Tiendas" (líneas 220-235).
- `/radar` es alias de la página de **Sorteos** (`Radar.razor`, líneas 1-2: `@page "/sorteos"` + `@page "/radar"`).
- Ruta correcta esperada: **`/novedades`** — existe: `src/Ludeka.Web/Components/Pages/News.razor`
  (línea 1: `@page "/novedades"`, "Novedades de los Viernes & Lanzamientos").
- Bug fix: cambiar `href="/radar"` → `href="/novedades"` en la línea 232.

### 2.3 Banner "¡Radar renovado!" en Sorteos

- `src/Ludeka.Web/Components/Pages/Radar.razor` líneas 14-33: banner completo con links a
  `/novedades` y `/eventos`.
- Condición: `IsLegacyRoute` (línea 303) => `Navigation.Uri.Contains("/radar", ...)`.
- Causa del reporte del usuario: **el hero enlaza a `/radar`** (HomeDashboard.razor línea 50), por lo
  que entrar a Sorteos desde la portada muestra el banner. La navegación del menú usa `/sorteos`
  (MainLayout.razor línea 38) y no lo dispara.
- Eliminar: el bloque `@if (IsLegacyRoute)` (líneas 15-33) y la propiedad `IsLegacyRoute` (línea 303).
- Consideración: si el hero pasa a enlazar `/sorteos`, el banner queda inerte igualmente, pero el
  usuario pidió quitarlo explícitamente.

### 2.4 Navegación principal

`src/Ludeka.Web/Components/Layout/MainLayout.razor`:

- Menú superior (desktop): líneas 25-50 —
  Catálogo → `/catalogo` (26-28), Editoriales → `/editoriales` (29-31), **Autores → `/creadores`
  (32-34, etiqueta "Autores")**, Tiendas → `/tiendas` (35-37), Sorteos → `/sorteos` (38-41),
  Novedades → `/novedades` (42-45), Eventos → `/eventos` (46-49).
- Footer: líneas 204-236 — Editoriales (205-207), **Autores → `/creadores` (209-211)**, Tiendas
  (213-215), Transparencia (217-219), Sorteos (221-223), Novedades (225-227), Eventos (229-231),
  Discord (233-235).

Mapa de rutas públicas relevantes (`@page`):
`/` HomeDashboard · `/catalogo` Home · `/editoriales` PublishersDirectory · `/editoriales/{Slug}`
PublisherDetail · `/creadores` + `/autores` CreatorsDirectory · `/creadores/{Slug}` + `/autores/{Slug}`
CreatorDetail · `/tiendas` + `/tiendas/{Slug}` Stores · `/sorteos` + `/radar` Radar ·
`/novedades` News · `/eventos` Events · `/mi-ludoteca` MyLibrary · `/u/{UserId}` PublicProfile ·
`/transparencia` Transparency.

### 2.5 Feature "Autores" completa (mapa de la feature actual)

**Dominio**
- `src/Ludeka.Core/Entities/Creator.cs` — entidad con `Name`, `Slug` (único), `Nationality`, `Bio`,
  `AvatarUrl`, `BggPersonId`, `WebsiteUrl`, `SocialLinks` (línea 22), métodos `SetSocialLinks`
  (83-91), `AddOrUpdateSocialLink` (93-100) y `GetYouTubeLink()` (102-103).
- `src/Ludeka.Core/ValueObjects/SocialNetworkLink.cs` — record `Platform/Url/Handle/Title` con
  iconos y nombres amigables por plataforma. **La entidad de red social YA está modelada y es
  reutilizable** (la comparten Publishers, Creators y Stores).
- `src/Ludeka.Core/Enums/SocialPlatform.cs` — enum: Website, YouTube, Instagram, Twitter, Discord,
  Facebook, BoardGameGeek, Twitch, TikTok, Other.

**Aplicación**
- DTOs: `src/Ludeka.Application/DTOs/DirectoryDtos.cs` líneas 7-14 (`SocialNetworkLinkDto`) y
  67-116 (`CreatorDto`, `CreatorDetailDto`, `CreateCreatorDto`, `UpdateCreatorDto`).
- Contratos: `src/Ludeka.Application/Contracts/ICreatorService.cs`,
  `src/Ludeka.Application/Contracts/ICreatorRepository.cs`.
- Servicio: `src/Ludeka.Application/Features/Directory/CreatorService.cs` — CRUD completo con
  permiso `CanManageCreators` (línea 212) y auditoría (118-129, 166-178, 193-204).
  **Acoplamiento clave**: el "GamesCount" se calcula por coincidencia de texto contra
  `Game.Designer` (líneas 52-54) y la ficha lista juegos vía
  `IGameRepository.GetByDesignerAsync(creator.Name)` (líneas 68, 79-80). Es decir, hoy la feature es
  estructuralmente un **directorio de diseñadores**.
- Directorio → scrapeo YouTube: `src/Ludeka.Application/Features/Directory/ChannelDirectoryProvider.cs`
  (líneas 49-65) convierte el link de YouTube de cada creator en `ChannelFocusEntry`
  (`ChannelCategory.Creator`, PriorityBonus 65).

**Infraestructura**
- Repositorio: `src/Ludeka.Infrastructure/Data/SqliteCreatorRepository.cs` (GetBySlug 36-44,
  GetByName 46-54, Update 63-81).
- EF: `src/Ludeka.Infrastructure/Data/LudekaDbContext.cs` — `DbSet<Creator>` línea 27; config
  Incremento 19 líneas 331-339: tabla `Creators`, índice único en `Slug`, `SocialLinks` como
  `OwnsMany(...).ToJson()` (columna JSON).
- DDL SQLite sin migraciones EF: `src/Ludeka.Infrastructure/Data/SqliteSchemaMigrator.cs`
  líneas 266-289 (columna `SocialLinks TEXT`).
- Sembrado: `src/Ludeka.Infrastructure/Seeding/DirectorySeeder.cs`, `SeedCreatorsAsync`
  líneas 153-253, invocado desde `src/Ludeka.Web/Program.cs` línea 232. Semilla actual: **6
  diseñadores de juegos** (Elizabeth Hargrave, Klaus Teuber, Uwe Rosenberg, Bruno Cathala, Jacob
  Fryxelius, Jamey Stegmaier) + **1 creador de contenido** (Sergio "Análisis Parálisis").
- Padrón estático YouTube: `src/Ludeka.Infrastructure/YouTube/ChannelFocusProvider.cs` líneas
  40-52 — 12 creadores de contenido hispanos (Análisis Parálisis, Meepletopía, El Agujero de
  Hobbit, Mesa de Guerra, Pareja de Ases, etc.) que se fusionan con los canales dinámicos del
  directorio (método `GetReferenceChannels`, líneas 63-84).
- DI: `src/Ludeka.Web/Program.cs` líneas 178-185 (repositorios, servicios y
  `IChannelDirectoryProvider`).

**UI Web**
- `src/Ludeka.Web/Components/Pages/CreatorsDirectory.razor` (216 líneas) — directorio con buscador,
  grid de tarjetas, guard de moderación (línea 28) y modal de alta/edición.
- `src/Ludeka.Web/Components/Pages/CreatorDetail.razor` (193 líneas) — ficha: hero con avatar,
  badge "✍️ Autor / Diseñador" (líneas 71-73), bio, `SocialLinksList` (líneas 118-124) y sección
  "Obras de X en Ludeka" (líneas 127-156, dependiente de `Game.Designer`).
- `src/Ludeka.Web/Components/Shared/CreatorEditModal.razor` (304 líneas) — formulario con campos
  Nombre/Nacionalidad/BggPersonId/WebsiteUrl/AvatarUrl/Bio y editor dinámico de redes
  (selección de plataforma + URL + handle, líneas 91-133).
- `src/Ludeka.Web/Components/Shared/SocialLinksList.razor` (27 líneas) — listado de píldoras de
  redes reutilizable (también usado por Editoriales y Tiendas).

**Pruebas existentes (relacionadas)**
- `tests/Ludeka.UnitTests/Domain/DirectoryDomainTests.cs` — creación/validación de `Creator`
  (líneas 73, 142).
- `tests/Ludeka.UnitTests/Application/DirectoryServicesTests.cs` — CRUD + matching de catálogo
  (línea 259), `ChannelDirectoryProvider` con creator "Sergio AP" (líneas 352-376).
- `tests/Ludeka.UnitTests/Infrastructure/SqliteDirectoryRepositoriesTests.cs` — persistencia
  SQLite del repo de creators (líneas 18, 83).
- `tests/Ludeka.UnitTests/Application/GranularPermissionsTests.cs` — permiso `CanManageCreators`
  (líneas 158-181).
- `tests/Ludeka.UnitTests/Infrastructure/ChannelFocusProviderTests.cs` — padrón de creadores
  (líneas 18, 37-41).
- `tests/Ludeka.UnitTests/Application/UserManagementDomainTests.cs` (líneas 49-108) y
  `UserManagementAndAuditServiceTests.cs` (línea 200) — permisos de creators.

**Cómo se alimenta hoy (respuesta directa)**
- Los creadores alimentan el **foco YouTube**: `ChannelDirectoryProvider` lee `SocialLinks`
  (plataforma YouTube) de la tabla `Creators` y los fusiona con el padrón estático de
  `ChannelFocusProvider` para priorizar canales en `YouTubeSearchService` (búsqueda de vídeos
  para fichas de juego). No hay scrapeo de Instagram: `InstagramApiClient` (INC-28) es el
  publicador de posts, no un scraper. El batch nocturno (`NightlyCatalogingService`,
  `NightlyCatalogingHostedService`) cataloga juegos y **no consume creadores**.

**Tamaño real de la reorientación "Autores → Creadores"**
- **Se reutiliza (no se toca o se reetiqueta):** entidad `Creator` + `SocialNetworkLink` +
  `SocialPlatform` (ya cubre Instagram/TikTok/Twitch), DTOs, repositorio, EF config, DDL, modal de
  edición con redes, `SocialLinksList`, permiso `CanManageCreators`, auditoría, pruebas de
  persistencia/permisos.
- **Se reetiqueta (solo texto español):** píldora del hero, menú superior + footer
  (MainLayout 32-34 y 209-211), títulos/h1/PlaceTitle de `CreatorsDirectory.razor` y
  `CreatorDetail.razor`, badge "Autor / Diseñador", textos del modal, label de auditoría
  (`AuditService.cs` línea 122: "Autor/Creador").
- **Se elimina/borra (según decisión de producto):** 6 diseñadores sembrados en DB (o se conservan
  con discriminador), acoplamiento `Game.Designer` del listado de obras, alias de ruta `/autores`.
- **Es nuevo:** conceptos "creador de contenido" (discriminador si se opta por coexistencia),
  re-siembra con creadores de contenido reales, ficha orientada a redes (sección "Obras" ya no
  aplica), y —si el usuario quiere scrapeo de redes en batches nocturnos— un job/fondo nuevo que
  consuma `SocialLinks` (hoy solo existe el pipeline YouTube vía `ChannelDirectoryProvider`).
  La infraestructura de fondo existe (`NightlyCatalogingHostedService` como patrón a imitar).

### 2.6 Sesgos de datos y entidades de redes sociales

- Semilla sesgada: 6/7 creators son diseñadores de juegos (DirectorySeeder.cs líneas 158-237).
- No hay migraciones EF: el esquema es DDL manual idempotente en `SqliteSchemaMigrator.cs`; los
  datos existen solo en el SQLite local/Docker (`ludeka.db`), sembrados en arranque
  (Program.cs línea 232).
- Entidad de red social ya modelada y compartida por 3 directorios: `SocialNetworkLink` +
  `SocialPlatform` (10 plataformas, incluidas Instagram y TikTok).
- En Sorteos la plataforma se modela aparte con `GiveawayPlatform` (Instagram, TwitterX, YouTube,
  Community) — usado en `Radar.razor` líneas 217-220. No confundir con `SocialPlatform`.
- Avatares sembrados apuntan a `/images/creators/*.png`; nuevos creadores de contenido pueden usar
  el fallback de iniciales ya implementado (CreatorsDirectory líneas 87-92).

### 2.7 Roadmap

- `docs/increments/ROADMAP.md`: 30 incrementos, todos ✅ Archivados. **Este cambio NO figura.**
  Debe registrarse como **INC-31** (fila nueva en la tabla, líneas 14-45).
- `docs/specs/ROADMAP_MVP_SLICES.md`: último incremento listado es el 30 (líneas 269-270,
  `change-30-played-independent-status`). Añadir sección "Incremento 31" con identificador
  SDD `change-31-portada-minimalista-creadores`.
- Antecedente directo: **INC-19** (`change-19-publishers-creators-directory`, ROADMAP.md línea 34)
  introdujo el directorio de Editoriales/Creadores/Tiendas; e **INC-21** (`change-21-home-dashboard`,
  línea 36) introdujo el hero actual. El nuevo incremento es, en parte, una **corrección de
  rumbo de producto** sobre INC-19.

---

## 3. Riesgos

- **CRITICAL — Ruptura de enlaces "Diseñado por" en fichas de juego:**
  `GameDetail.razor` líneas 200-208 genera el slug del diseñador con `Game.GenerateSlug(Game.Designer)`
  y enlaza a `/creadores/{slug}` **sin verificar existencia** en el directorio. Si el directorio se
  purga de diseñadores, TODAS las fichas enlazan a páginas "Creador no encontrado". Hoy ese problema
  ya existe de forma latente (solo 7 creators sembrados frente a cientos de diseñadores del
  catálogo); la reorientación lo agravaría de forma sistemática. Requiere decisión (ver decisión D1).
- **WARNING — Alcance del "quitar Editoriales y Autores" del hero:** la petición afecta solo a las
  píldoras del hero; las features `/editoriales` y `/creadores` (y su entrada en el menú/footer)
  siguen existiendo. No eliminar las features salvo instrucción explícita.
- **WARNING — Renombrado transversal de etiquetas:** si "Autores" pasa a "Creadores" en UI, hay que
  actualizar mínimo: MainLayout (header 32-34 y footer 209-211), PageTitle/h1 de las dos páginas de
  creadores, textos del `CreatorEditModal`, PageTitle del hero (línea 8, si se decide), label de
  auditoría (`AuditService.cs` línea 122) y textos de tests que afirman etiquetas. Es texto en
  español, sin impacto en identificadores de código.
- **WARNING — `/radar` queda como alias huérfano:** al quitar el banner y cambiar el hero a
  `/sorteos`, la ruta `/radar` sigue registrada (Radar.razor línea 2). Decidir si se conserva como
  alias retrocompatible (recomendado, cero coste) o se retira.
- **SUGGESTION — Tests acoplados a la semántica de diseñadores:** `DirectoryServicesTests.cs`
  (línea 259, "CRUD_And_CatalogMatching_Works") y `ChannelDirectoryProviderTests` dependen del
  matching por `Game.Designer`; si la ficha de creador de contenido deja de listar obras, estos
  tests necesitan revisión (no rompen la build, cambian de significado).
- **SUGGESTION — Ambigüedad de `IsLegacyRoute`:** `Navigation.Uri.Contains("/radar")` también
  coincidiría con URLs que contengan "/radar" en cualquier posición; al eliminar el banner desaparece
  la condición, no hay riesgo residual.
- **SUGGESTION — Caché del dashboard:** los datos del carril "Novedades en Tiendas" pasan por
  `CachedHomeDashboardService` (Program.cs líneas 196-199), pero el link bugueado es markup
  estático: el fix es trivial y sin riesgo de caché.

---

## 4. Decisiones de producto abiertas (requieren al usuario)

### D1. ¿Qué pasa con los datos/contenido existentes de Autores (los 6 diseñadores sembrados)?
- **Opción A — Purga y re-siembra:** borrar diseñadores del seed y sembrar solo creadores de
  contenido (Análisis Parálisis + los 12 del padrón: Meepletopía, El Agujero de Hobbit, etc.).
  · Pro: el directorio refleja la visión real del producto ("creadores de contenido").
  · Con: **rompe** el link "Diseñado por" de `GameDetail.razor` (diseñadores sin ficha) y la
  sección "Obras" deja de tener sentido. Hay que decidir qué pasa con ese link (quitarlo,
  apuntar a búsqueda de catálogo por diseñador, o mantenerlo solo si existe ficha).
- **Opción B — Coexistencia con discriminador:** añadir campo `CreatorType`
  (Diseñador | CreadorContenido) a la entidad; el directorio `/creadores` lista creadores de
  contenido; los diseñadores quedan accesibles solo desde la ficha del juego.
  · Pro: no rompe nada existente, reutiliza todo; el listado público muestra lo que el usuario quiere.
  · Con: pequeña migración de datos (re-tipo de los 6 seeds) y un enum nuevo en dominio.
- **Opción C — Entidad nueva separada** (`ContentCreator`) dejando `Creator` intacto.
  · Pro: máxima pureza conceptual.
  · Con: duplica repositorio/servicio/UI/modal/pruebas; esfuerzo alto para valor igual al de B.
- **Recomendación de exploración:** B (coexistencia con discriminador) o A si el usuario confirma
  que los diseñadores no interesan en absoluto; C descartada por coste.

### D2. ¿Se mantiene `/autores` como alias de ruta?
- **Opción A — Mantener alias** (estado actual, 2 rutas por página): cero URLs rotas, bookmarks
  y SEO intactos. Con: perpetúa el concepto "autores" que el usuario desecha.
- **Opción B — Eliminar `/autores`**: dominio semántico limpio; rompe URLs antiguas (404 → hoy
  hay página de "no encontrado" amable). Requiere actualizar referencias (no hay links internos
  a `/autores` salvo las rutas mismas: verificado por grep).
- **Recomendación:** A (mantener alias) con redirección implícita Blazor; coste cero.

### D3. ¿Con qué queda el hero tras quitar badge y titular?
- **Opción A — Minimalismo puro:** hero = buscador + píldoras de acceso, sin titular ni subtítulo
  (mantiene `PageTitle` del navegador). Máxima reducción visual.
- **Opción B — Titular neutro sin marca ajena:** un `h1` corto propio (p. ej. "Tu universo del
  juego de mesa en español.") conservando el subtítulo. Mantener "El Letterboxd..." es marca de
  terceros usada como eslogan; el usuario ya pidió quitarlo.
- **Opción C — Solo subtítulo** (párrafo de la línea 22-24) sin `h1` ni badge: conserva el copy
  explicativo con mínima jerarquía. Riesgo SEO/a11y: la página perdería su único `h1` (hoy es el
  titular). Si se elige A o C, reubicar un `h1` visualmente discreto (o mantenerlo solo para
  lectores de pantalla) para cumplir WCAG 2.2 AA (jerarquía de encabezados).
- **Recomendación:** B, con decisión explícita del usuario sobre el texto.

### D4. Configuración final de las píldoras del hero (interpretación de la petición #2)
La petición literal contiene una ambigüedad ("el de eventos debe decir 'Eventos' (hoy dice 'Radar &
Sorteos')" y "en su lugar poner 'Novedades' y 'Eventos'").
- **Opción A — 4 píldoras:** Catálogo Completo / **Sorteos** (re-etiquetado de "Radar & Sorteos",
  href `/sorteos`) / **Novedades** (`/novedades`) / **Eventos** (`/eventos`); se quitan
  Editoriales y Autores. Refleja la segregación de INC-22 y elimina el disparador del banner.
- **Opción B — Literal estricta:** la píldora actual pasa a llamarse "Eventos" apuntando a
  `/eventos`, y se añade "Novedades"; el resultado son 4 píldoras sin acceso directo a Sorteos
  (Catálogo / Eventos / Novedades / [+1]) — pierde el acceso thumb-friendly a sorteos.
- **Recomendación:** A; el botón "Proponer Sorteo" y el carril "Sorteos en Marcha" de la misma
  portada ya llevan al radar, así que no se pierde descubribilidad.

### D5. Alcance del "scrapeo con las redes de creadores" (límite del incremento)
- **Opción A — Solo modelo + UI (recomendado para este slice):** reorientar el directorio y ficha a
  creadores de contenido con redes asociadas; la integración al pipeline YouTube ya funciona vía
  `ChannelDirectoryProvider`. El scrapeo nocturno de perfiles (Instagram/TikTok) queda como
  incremento futuro propio (requiere APIs/tokens y políticas de rate limit).
- **Opción B — Incluir un job de scrapeo social nocturno en este incremento:** añade un hosted
  service nuevo que consuma `SocialLinks` por plataforma. Coste alto, APIs externas (Instagram no
  ofrece scraping legal sencillo), y mezcla dos problemas en un PR.
- **Recomendación:** A; documentar B como incremento candidato (INC-32).

---

## 5. Enfoques comparados (resumen)

| Enfoque | Pros | Conos | Esfuerzo |
|---|---|---|---|
| 1. Cosmético (solo hero + 2 fixes) | Riesgo mínimo, rápido, 1 PR pequeño | No resuelve la reorientación de Autores | Bajo |
| 2. Cosmético + reetiquetado Creadores (Opciones B/A/D2-A/D4-A/D5-A) | Resuelve los 5 puntos, sin migraciones destructivas, reutiliza ~80% del código existente | Toca ~10 archivos de texto + 2 páginas | Medio |
| 3. Reorientación profunda (entidad nueva o purga + job de scrapeo) | Máxima limpieza conceptual | Rompe enlaces de GameDetail, duplica código, mezcla scrapeo externo | Alto |

**Recomendación de exploración:** Enfoque 2. Los 4 fixes de portada (puntos 1-4) son cambios
quirúrgicos de markup (HomeDashboard.razor y Radar.razor); la reorientación a Creadores (punto 5)
se apoya en que la infraestructura de redes ya existe, y solo exige decisión D1 sobre los datos y
un reetiquetado transversal. Queda listo para `sdd-propose`.

## 6. ¿Listo para la propuesta?

**Sí.** La propuesta debe condicionarse a las decisiones D1-D5 (recomendaciones: D1-B, D2-A,
D3-B, D4-A, D5-A). El alcance estimado cabe holgadamente en el presupuesto de revisión de 400
líneas si D5 se acota a la Opción A.
