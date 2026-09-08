# Design: Portada minimalista y reorientación Autores → Creadores (`portada-minimalista-creadores`)

> Idioma: español castellano (regla suprema del repo). Fase: `sdd-design`. Modo: hybrid.
> Decisiones confirmadas que este diseño NO reinterpreta: **D1** purga + re-siembra solo creadores de contenido · **D2** alias `/autores` mantenido · **D3** hero = subtítulo + buscador + h1 sr-only · **D4** píldoras Catálogo/Sorteos/Novedades/Eventos · **D5** scrapeo social fuera de alcance.

## Enfoque técnico

Estrategia quirúrgica en 3 frentes, reutilizando ~80 % del código existente (entidad `Creator`, `SocialNetworkLink`, modal de redes, `SocialLinksList`, permiso `CanManageCreators`, `ChannelDirectoryProvider`):

1. **Datos (Infrastructure):** purga idempotente de los 6 diseñadores sembrados + re-siembra desde el padrón estático de `ChannelFocusProvider` (fuente única exigida por spec). Sin migraciones EF: el DDL de `SqliteSchemaMigrator` ya soporta `SocialLinks` JSON.
2. **Contrato (Application):** desacople total de `CreatorService` respecto a `Game.Designer` (constructor, conteos, listado de obras) y recorte honesto de DTOs.
3. **Presentación (Web):** hero minimalista, banner legacy fuera, diseñador como texto plano, ficha sin "Obras", reetiquetado transversal.

Sin rutas nuevas ni retiradas: `/sorteos`+`/radar`, `/creadores`+`/autores` conservan sus `@page` (alias silenciosos, D2).

## Decisiones de arquitectura

### AD-1 · Mecanismo de purga en DB existente (respuesta al punto 1)

**Choice:** lista estática cerrada de **slugs retirados** en `DirectorySeeder` (`RetiredSeedCreatorSlugs` = los 6 slugs exactos que el seed antiguo creaba: `elizabeth-hargrave`, `klaus-teuber`, `uwe-rosenberg`, `bruno-cathala`, `jacob-fryxelius`, `jamey-stegmaier`). `SeedCreatorsAsync` ejecuta: (1) **purga** — `SELECT` de creators cuyo `Slug` ∈ lista retirada → `RemoveRange` + `SaveChanges`; (2) **re-siembra aditiva** — patrón existente (insertar solo si el slug no existe). Corre en cada arranque (Program.cs L232); tras la primera ejecución es no-op.

**Distinción seed vs. creación manual:** el criterio discriminable es la pertenencia del slug a la lista cerrada de semillas retiradas. Un creador creado por un moderador en runtime (`CanManageCreators`) jamás colisiona salvo que re-cree deliberadamente uno de los 6 diseñadores retirados — acción que D1 declara fuera de producto. Los 6 datos son 100 % sembrados (proposal: "datos solo de sembrado"), así que la pérdida es reversible por diseño (git revert del seeder los re-siembra).

**Alternativas rechazadas:**

| Alternativa | Por qué se rechaza |
|---|---|
| Purgar todo creator no presente en el padrón | Destruiría creators creados manualmente en runtime (violación grave de datos). |
| Discriminar por `AvatarUrl` (`/images/creators/*.png`) | No fiable: `jacob-fryxelius` se siembra sin avatar y los usuarios pueden cambiar avatares. |
| Discriminar por `BggPersonId != null` | El modal expone el campo BGG a moderadores: borraría creators manuales legítimos. |
| Columna `CreatorType` (discriminador, D1-B) | Exige migración de esquema, contradice D1 confirmada (purga) y el "sin migraciones EF". |

**Idempotencia y conservación:** la siembra es solo-aditiva y **sin updates**: el creator existente "Sergio (Análisis Parálisis)" (slug `analisis-paralisis`) coincide con el slug del padrón (verificado: `Game.GenerateSlug` normaliza diacríticos, Game.cs L315-341) → no se duplica y **no se sobrescribe** (se respetan ediciones de usuarios). Resultado esperado en DB tras arranque: 12 creators (11 nuevos del padrón + el existente de Análisis Parálisis).

### AD-2 · Padrón fuente única: `ChannelFocusProvider.GetStaticCreators()`

**Choice:** nuevo método estático público `GetStaticCreators()` en `ChannelFocusProvider` que devuelve `Channels.Where(c => c.Category == ChannelCategory.Creator)` (la lista estática L40-52, 12 creadores). El seeder mapea cada entrada: `Name = ChannelName`, `Slug = Game.GenerateSlug(ChannelName)`, `Bio = Description`, `Nationality/AvatarUrl/BggPersonId/WebsiteUrl = null` (fallback de iniciales ya implementado en CreatorsDirectory L87-92), `SocialLinks = [YouTube: url = $"https://youtube.com/{handle}", handle]`.

**Alternativa rechazada:** duplicar los 12 nombres en el seeder — rompe la fuente única que el spec exige ("tomando como padrón los creadores del registro estático de foco de canales"). **Rechazado también** usar `GetReferenceChannels()` desde el seeder: hace merge con canales dinámicos leyendo la tabla `Creators` vía `IServiceScopeFactory` (L111-136) — dependencia circular durante el arranque y lectura de la tabla que se está sembrando.

### AD-3 · Markup final del hero (respuesta al punto 2)

`HomeDashboard.razor` L12-61 queda así (L232 se corrige aparte):

```razor
<div class="relative overflow-hidden rounded-3xl ... p-6 sm:p-10 shadow-xl">  @* contenedor L12 intacto *@
    <div class="max-w-3xl space-y-4">
        <h1 class="sr-only">Ludeka — Juegos de mesa en español</h1>
        <p class="text-sm sm:text-base text-[var(--text-secondary)] leading-relaxed">
            Descubre títulos imprescindibles para tu mesa, ...  @* párrafo actual L22-24 intacto *@
        </p>
        @* Buscador L26-43 INTACTO (form → HandleQuickSearch → /catalogo?q={termino}) *@
        <div class="flex items-center gap-2 pt-2 flex-wrap text-xs font-semibold">
            <a href="/catalogo"  class="...">🎲 Catálogo Completo</a>
            <a href="/sorteos"   class="...">🎁 Sorteos</a>       @* antes "Radar & Sorteos" → /radar *@
            <a href="/novedades" class="...">📰 Novedades</a>     @* nueva *@
            <a href="/eventos"   class="...">🎪 Eventos</a>       @* nueva *@
        </div>
    </div>
</div>
```

- Se **elimina** badge L14-16 y h1 visible L18-20. El h1 `sr-only` es la **única** modificación estructural del buscador/párrafo (D3; texto fijado por spec: `Ludeka — Juegos de mesa en español`). `sr-only` es utilidad core de Tailwind (v3+), sin cambio de config.
- Emojis de las píldoras nuevas: 📰 y 🎪, tomados de los botones existentes del banner legacy ("Ver Novedades"/"Ver Eventos", Radar L26/L29) — coherencia con la identidad actual.
- **Fix L232:** `href="/radar"` → `href="/novedades"` en "Ver todas las novedades".
- Verificado por grep: HomeDashboard contiene un único `<h1>` (L18) → tras el cambio, el `sr-only` es el único h1 del documento de la portada (WCAG 2.2 AA).

### AD-4 · Radar.razor: banner y condición legacy fuera (respuesta al punto 3)

- Eliminar el bloque `@if (IsLegacyRoute)` completo (comentario L14 + banner L15-33).
- Eliminar la propiedad `IsLegacyRoute` (L303).
- Verificado por grep: `Navigation` (`@inject NavigationManager` L9) **solo** se usa en `IsLegacyRoute` → eliminar también el inject (limpieza sin efectos).
- **Se mantienen** `@page "/sorteos"` (L1) y `@page "/radar"` (L2): alias silencioso (spec giveaway-radar), sin aviso ni mensaje. Cero coste; `transparency-manifesto` sigue enlazando `/radar`.

### AD-5 · GameDetail: diseñador texto plano (respuesta al punto 4)

L200-208 queda:

```razor
<p class="text-xs text-[var(--text-secondary)] mt-1">
    Diseñado por
    @if (!string.IsNullOrWhiteSpace(Game.Designer))
    {
        <span class="text-[var(--text-primary)] font-bold">@Game.Designer</span>
    }
    else
    {
        <span class="text-[var(--text-primary)] font-bold">Desconocido</span>
    }
    &bull; Publicado en España por ...
```

- La etiqueta **"Diseñado por" se conserva** (el spec fija el texto "Diseñado por {Designer}"; no es una etiqueta "Autores"). No hay icono en esta línea.
- Se elimina el cálculo `Game.GenerateSlug(Game.Designer)`, el `<a href="/creadores/{slug}">`, sus clases hover y el atributo `title="Ver ficha del autor..."`. El `<span font-bold>` mantiene la jerarquía visual. El fallback "Desconocido" se conserva tal cual.
- El bloque "Publicado en España por … /editoriales" **no se toca** (las editoriales siguen siendo directorio con ficha real).

### AD-6 · CreatorService sin cruce por Game.Designer (respuesta al punto 5)

**Contrato final** (firmas públicas de `ICreatorService` sin cambios):

| Elemento | Antes | Después |
|---|---|---|
| Constructor | `(ICreatorRepository, IGameRepository, ICurrentUserService?, IAuditService?)` | `(ICreatorRepository, ICurrentUserService?, IAuditService?)` — `IGameRepository` eliminado |
| `GetAllAsync` | carga `allGames` (L36) y computa `gamesCount` por `Designer.Contains(name)` (L52-54) | solo filtrado/ordenación; `MapToDto(c)` |
| `GetBySlugAsync`/`GetByIdAsync` | `GetByDesignerAsync(creator.Name)` → `gameSummaries` (L68, L79-80) | ficha solo con `SocialLinks`; `MapToDetailDto(c)` |
| `UpdateAsync` | re-cuenta juegos tras guardar (L180-181) | `MapToDto(creator)` |
| `MapToDto` / `MapToDetailDto` | con `gamesCount` / `games` | sin parámetro de juegos |

**DTOs (recorte honesto, no "siempre 0"):** `CreatorDto` pierde `GamesCount`; `CreatorDetailDto` pierde `Games`. Los DTOs son records internos de Blazor (sin contrato de serialización externo); dejar campos muertos en 0/array vacío induciría a error. Ripple aceptado: tarjeta del directorio (`CreatorsDirectory.razor` L107-109, badge "@creator.GamesCount obras") → se elimina el badge sin reemplazo; sección "Obras" de `CreatorDetail` (L127-156) → eliminada (desaparece el único consumidor de `CreatorDetailDto.Games`).

**DI:** `AddScoped<ICreatorService, CreatorService>()` (Program.cs L183) no cambia — DI resuelve el único constructor público.

**Textos internos del servicio** (auditoría visible en `/admin/auditoria`): L127 `Alta de autor/diseñador '{Name}'` → `Alta de creador de contenido '{Name}'`; L140 `autor/creador` → `creador de contenido`; L175 `Modificación de autor` → `Modificación de creador de contenido`; L202 `Eliminación de autor` → `Eliminación de creador de contenido`; L214 excepción → `...para dar de alta o editar creadores de contenido.`

### AD-7 · Reetiquetado transversal: lista exacta archivo→cadena (respuesta al punto 6)

| Archivo | Línea | Antes | Después |
|---|---|---|---|
| `MainLayout.razor` | L33 | `Autores` | `Creadores` |
| `MainLayout.razor` | L210 | `✍️ Autores` | `✍️ Creadores` |
| `CreatorsDirectory.razor` | L10 (PageTitle) | `Directorio de Creadores y Autores de Juegos de Mesa — Ludeka` | `Directorio de Creadores de Contenido — Ludeka` |
| `CreatorsDirectory.razor` | L17 (badge) | `✍️ Autoría Lúdica • Diseñadores y Divulgadores` | `✍️ Creadores de Contenido &bull; Divulgadores del Hobby` |
| `CreatorsDirectory.razor` | L19-21 (h1) | `Creadores y Autores` | `Creadores de Contenido` |
| `CreatorsDirectory.razor` | L23 (párrafo) | `…diseñadores, ilustradores y divulgadores referentes del hobby.` | `Descubre a los divulgadores y creadores de contenido referentes del hobby hispanohablante.` |
| `CreatorDetail.razor` | L11 (PageTitle) | `…Obras y Ficha de Autor` | `…Redes y Ficha del Creador de Contenido` |
| `CreatorDetail.razor` | L17 | `Cargando ficha del autor...` | `Cargando ficha del creador...` |
| `CreatorDetail.razor` | L26 | `No encontramos ningún autor o creador registrado…` | `No encontramos ningún creador de contenido registrado…` (h1 "Creador no encontrado" ya correcto) |
| `CreatorDetail.razor` | L72 (badge) | `✍️ Autor / Diseñador` | `🎬 Creador de contenido` |
| `CreatorEditModal.razor` | L22 | `➕ Nuevo Creador / Autor` | `➕ Nuevo Creador de Contenido` |
| `CreatorEditModal.razor` | L43 | `Nombre Completo del Creador / Autor *` | `Nombre del Creador de Contenido *` |
| `CreatorEditModal.razor` | L46 | `Ej. Elizabeth Hargrave, Klaus Teuber...` | `Ej. Análisis Parálisis, Meepletopía...` |
| `CreatorEditModal.razor` | L86 | `Trayectoria profesional, reconocimientos y estilo de diseño...` | `Trayectoria, canales y estilo de divulgación...` |
| `AuditService.cs` | L122 | `Autor/Creador` | `Creador de Contenido` |
| `AuditLogViewer.razor` | L95 (extra, grep) | `✍️ Autor / Diseñador` | `✍️ Creador de Contenido` |
| `UserPermissionsModal.razor` | L143 (extra, grep) | `Gestionar Autores y Diseñadores` | `Gestionar Creadores de Contenido` |
| `HomeDashboard.razor` | L57 | píldora `✍️ Autores` | **eliminada** (D4) |

**N/A (fuera de alcance, decidido):** `LibraryStatsDashboard.razor` (L213/269/274/291 "Top Autores/Editoriales") — es estadística de biblioteca derivada de `Game.Designer` del **catálogo**, que persiste intacto (D1 no borra `Game.Designer` del modelo de juego); no pertenece al directorio de creadores.

## Flujo de datos

```
Arranque (Program.cs L232)
   └─> DirectorySeeder.SeedCreatorsAsync(db)
         ├─ 1. PURGA:  db.Creators WHERE Slug ∈ RetiredSeedCreatorSlugs → DELETE   (idempotente)
         └─ 2. SIEMBRA: ChannelFocusProvider.GetStaticCreators()                    (fuente única)
                └─ por cada entrada: slug = GenerateSlug(ChannelName)
                     ├─ slug existe (p.ej. analisis-paralisis) → SKIP (sin update)
                     └─ slug nuevo → INSERT Creator + SocialLink YouTube
   ─── lectores ───
   /creadores  → CreatorService.GetAllAsync  (sin Game.Designer)
   /creadores/{slug} → CreatorService.GetBySlugAsync (solo SocialLinks)
   /creadores (fichas de juego) → GameDetail: Designer como TEXTO PLANO (sin cruce)
   Pipeline YouTube → ChannelDirectoryProvider lee SocialLinks (INTACTO)
```

## Cambios de archivos

| Archivo | Acción | Descripción |
|---|---|---|
| `src/Ludeka.Infrastructure/YouTube/ChannelFocusProvider.cs` | Modificar | Añadir `GetStaticCreators()` (accesor estático del padrón, ~10 líneas) |
| `src/Ludeka.Infrastructure/Seeding/DirectorySeeder.cs` | Modificar | `RetiredSeedCreatorSlugs` + purga idempotente + re-siembra desde padrón (−~80 líneas de diseñadores, +~40 de lógica) |
| `src/Ludeka.Application/Features/Directory/CreatorService.cs` | Modificar | Quitar `IGameRepository`, conteos y obras; textos de auditoría/permiso (AD-6) |
| `src/Ludeka.Application/DTOs/DirectoryDtos.cs` | Modificar | `CreatorDto` sin `GamesCount`; `CreatorDetailDto` sin `Games` |
| `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` | Modificar | Hero minimalista (AD-3) + fix L232 |
| `src/Ludeka.Web/Components/Pages/Radar.razor` | Modificar | Banner L14-33, `IsLegacyRoute` L303 e inject `NavigationManager` fuera (AD-4) |
| `src/Ludeka.Web/Components/Pages/GameDetail.razor` | Modificar | Diseñador texto plano (AD-5) |
| `src/Ludeka.Web/Components/Pages/CreatorDetail.razor` | Modificar | Sin sección "Obras" (L127-156), badge y textos (AD-7) |
| `src/Ludeka.Web/Components/Pages/CreatorsDirectory.razor` | Modificar | Sin badge "obras" (L107-109), etiquetas (AD-7) |
| `src/Ludeka.Web/Components/Shared/CreatorEditModal.razor` | Modificar | Textos del formulario (AD-7) |
| `src/Ludeka.Web/Components/Layout/MainLayout.razor` | Modificar | Etiquetas L33/L210 (AD-7) |
| `src/Ludeka.Application/Features/Admin/AuditService.cs` | Modificar | Etiqueta L122 (AD-7) |
| `src/Ludeka.Web/Components/Pages/AuditLogViewer.razor` | Modificar | Etiqueta filtro L95 (AD-7) |
| `src/Ludeka.Web/Components/Shared/UserPermissionsModal.razor` | Modificar | Etiqueta permiso L143 (AD-7) |
| `tests/Ludeka.UnitTests/Infrastructure/DirectorySeederTests.cs` | Crear | Purga, idempotencia, conservación de creators ajenos (T1-T3) |
| `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` | Crear | Contratos de markup por archivo (T6) |
| `tests/Ludeka.UnitTests/Application/DirectoryServicesTests.cs` | Modificar | Reescritura del test de matching (T5) |
| `tests/Ludeka.UnitTests/Application/GranularPermissionsTests.cs` | Modificar | Constructor de `CreatorService` sin gameRepo |
| `tests/Ludeka.UnitTests/Infrastructure/ChannelFocusProviderTests.cs` | Modificar | Test del nuevo accesor (T4) |
| `docs/increments/ROADMAP.md`, `docs/specs/ROADMAP_MVP_SLICES.md` | Modificar | Registrar INC-31 (ediciones en apply) |

## Interfaces / Contratos

```csharp
// ChannelFocusProvider (nuevo, Infrastructure)
public static IReadOnlyList<ChannelFocusEntry> GetStaticCreators()
    => Channels.Where(c => c.Category == ChannelCategory.Creator).ToList();

// DirectorySeeder (nuevo, Infrastructure) — privado, verificado por comportamiento
private static readonly string[] RetiredSeedCreatorSlugs =
    ["elizabeth-hargrave", "klaus-teuber", "uwe-rosenberg", "bruno-cathala", "jacob-fryxelius", "jamey-stegmaier"];

// DTOs (Application) — contratos recortados
public record CreatorDto(Guid Id, string Name, string Slug, string? Nationality, string? Bio,
    string? AvatarUrl, int? BggPersonId, string? WebsiteUrl,
    IReadOnlyList<SocialNetworkLinkDto> SocialLinks, DateTimeOffset CreatedAt);           // sin GamesCount
public record CreatorDetailDto(Guid Id, string Name, string Slug, string? Nationality, string? Bio,
    string? AvatarUrl, int? BggPersonId, string? WebsiteUrl,
    IReadOnlyList<SocialNetworkLinkDto> SocialLinks, DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);                                                            // sin Games

// CreatorService — constructor final
public CreatorService(ICreatorRepository creatorRepository,
    ICurrentUserService? currentUserService = null, IAuditService? auditService = null);

// ICreatorService — SIN cambios (firmas públicas intactas)
```

## Estrategia de pruebas (strict TDD) — mapeo rojo→verde (respuesta al punto 7)

Marco: solo existe `tests/Ludeka.UnitTests` (xUnit puro, sin bUnit). Comando: `dotnet test Ludeka.sln` (con `--no-build` si el proceso dev corre).

**Decisión de marco para markup — contrato de fuente en vez de bUnit.** *Choice:* tests que leen los `.razor` y afirman invariantes estructurales (contiene/no-contiene) por archivo. *Alternativa rechazada:* bUnit — exige paquete nuevo + fakes de `IHomeDashboardService`/`IGiveawayService`/`ICurrentUserService`/`IUserLocationService`/`ICreatorService` (~150-200 líneas) y rompe el presupuesto de revisión. *Racional:* el contrato de fuente es determinista, rojo→verde real por tarea, y el render real se confirma en `sdd-verify` con `dotnet run`. Limitación documentada: no prueba el DOM renderizado.

| # | Test (rojo) | Cubre (spec) | Verde (tarea) |
|---|---|---|---|
| T1 | `DirectorySeederTests.SeedCreatorsAsync_OnLegacyDbWithDesigners_PurgesAndSeedsOnlyContentCreators` — SQLite en memoria (patrón `CatalogSeederFullDatasetTests`): inserta los 6 diseñadores + Sergio a mano, ejecuta `SeedDirectoryAsync`, afirma: 0 slugs retirados, 12 creators, "Análisis Parálisis" con link YouTube | creators-directory · Sembrado D1 (escenario 1) | AD-1 + AD-2 |
| T2 | `..._IsIdempotent_NoDuplicatesOnSecondRun` — 2ª ejecución: count estable, sin diseñadores | Sembrado D1 (escenario 2) | AD-1 |
| T3 | `..._PreservesCreatorsOutsidePadron` — creator manual con slug ajeno sobrevive a la purga | Purga quirúrgica (no borra datos reales) | AD-1 |
| T4 | `ChannelFocusProviderTests.GetStaticCreators_ReturnsTwelveContentCreators` — 12 entradas `Category.Creator`, incluye "Análisis Parálisis" | Sembrado D1 (padrón) | AD-2 |
| T5 | Reescritura `DirectoryServicesTests.CreatorService_CRUD_Works` (antes L259): CRUD íntegro; **sin** matching — el detalle expone `SocialLinks` y ya no `Games`/conteo | creators-directory · Sin cruce por Designer | AD-6 |
| T6 | `WebMarkupContractTests` — teorías compactas `(archivo, debeContener[], noDebeContener[])`: **hero** (contiene `sr-only` + texto exacto del h1; no contiene "PORTADA EDITORIAL"; los hrefs de píldoras son exactamente `/catalogo,/sorteos,/novedades,/eventos` en orden; no contiene `href="/radar"`); **link novedades** (`Ver todas las novedades` + `href="/novedades"`); **radar** (no contiene "Radar renovado" ni `IsLegacyRoute`; contiene ambas `@page`); **GameDetail** (no contiene `creadores/{creatorSlug}`; contiene span del designer); **CreatorDetail** (no contiene "Obras de"; contiene "Creador de contenido"); **etiquetas** (MainLayout sin "Autores" con "Creadores"; AuditLogViewer; UserPermissionsModal; PageTitle del directorio); **alias** (CreatorsDirectory conserva `@page "/autores"`; CreatorDetail conserva `@page "/autores/{Slug}"`) | home-landing-hero completo · giveaway-radar · creators-directory · Alias D2 | AD-3..AD-7 |

**Tests existentes — qué se toca y qué NO:**

- `DirectoryServicesTests` L259 → **reescrito** (T5). L352-376 (`ChannelDirectoryProvider` "Sergio AP") → **sin cambios**: no usa `CreatorService`, valida el pipeline YouTube que queda intacto.
- `DirectoryDomainTests` L73/L142 → **sin cambios**: la entidad `Creator` no se modifica; creación/validación siguen vigentes.
- `GranularPermissionsTests` L158-181 → **ajuste mecánico**: `new CreatorService(creatorRepo)` (sin gameRepo); los asserts de permiso no cambian.
- `ChannelFocusProviderTests` → existentes intactos (padrón estático no cambia); se añade T4.
- `SqliteDirectoryRepositoriesTests` → sin cambios (repo intacto).

## Matriz de amenazas

`N/A — el cambio no altera enrutado real (las 4 rutas @page existentes se conservan sin añadir/quitar), ni shell, ni subprocessos, ni automatización VCS/PR en código de producto; solo hrefs internos hacia rutas ya existentes.` Filas de la matriz (paths tipo-doc, selección de repo git, estados de commit/push, comandos PR): todas no aplicables — no se ejecutan comandos externos ni se modifica clasificación de ejecutables.

## Migración / Rollout

- **Migración:** ninguna (sin migraciones EF ni cambios de esquema). La "migración de datos" es la purga idempotente del seeder, que corre sola en el primer arranque tras desplegar (borra exactamente 6 filas de semilla; crea 11 creators del padrón; conserva el resto).
- **Rollout:** PR único (`single-pr`). Commits convencionales por unidad de trabajo.
- **Rollback:** `git revert` del PR restaura markup, textos y seeder; el arranque re-siembra los diseñadores al volver el seed antiguo. Los creators de contenido sembrados por la versión nueva quedan como filas válidas (no rompen nada: el seed es aditivo). Alias `/autores` y `/radar` no se tocan → cero URLs rotas durante y después.

## Unidades de trabajo (respuesta al punto 8) — orden, verificación y rollback por unidad

Cada WU = 1 commit convencional independiente, compilable y con suite verde; PR único.

| WU | Contenido | Verificación (verde) | Rollback | Est. líneas |
|---|---|---|---|---|
| WU1 | AD-2 accesor + AD-1 seeder + tests T1-T4 | T1-T4 verdes; suite completa | revert del commit; DB queda con creators válidos | ~120 |
| WU2 | AD-6 CreatorService + DTOs + textos internos; ajustes T5 + GranularPermissions | suite verde | revert; el matching Designer vuelve sin afectar markup | ~90 |
| WU3 | AD-5 GameDetail + AD-7 ficha/directorio/modal/labels (+ parte de T6 de fichas/etiquetas) | T6 (subconjunto) + suite | revert | ~110 |
| WU4 | AD-3 hero + AD-4 radar + fix L232 (+ parte de T6 hero/radar/alias) | T6 (subconjunto) + suite | revert | ~80 |
| WU5 | Docs: INC-31 en `ROADMAP.md` y `ROADMAP_MVP_SLICES.md` (ediciones en apply) | revisión manual | revert | ~15 |

**Orden:** WU1→WU2 (contrato de aplicación antes de markup), WU3 antes de WU4 (el fix del hero es el disparador visible y va último), WU5 cierre. **Estimación honesta:** ~415 líneas tocadas (adiciones+borraciones), por encima de los 300-380 del proposal pero cerca del presupuesto de 400 — el punto flexible son los tests de contrato de markup (T6, ~70 líneas); `sdd-tasks` debe emitir el forecast con guard lines (`Decision needed before apply: No` · `Chained PRs recommended: No` · `400-line budget risk: Low-Medium`).

## Trazabilidad requisito → decisión

| Requisito (spec) | Decisión |
|---|---|
| home-landing-hero · Hero sin badge ni titular | AD-3 |
| home-landing-hero · Buscador conservado | AD-3 (intacto) |
| home-landing-hero · Píldoras D4 (4 exactas, sin Editoriales/Autores, "Sorteos" → /sorteos) | AD-3 |
| home-landing-hero · Link novedades → /novedades | AD-3 (fix L232) |
| home-landing-hero · h1 único sr-only con texto fijo | AD-3 |
| creators-directory · Listado solo creadores de contenido | AD-1 + AD-2 |
| creators-directory · Ficha con SocialLinks reutilizable | AD-6 (ficha solo redes; `SocialLinksList` intacto, CreatorDetail L118-124) |
| creators-directory · Alta/edición con CanManageCreators | Sin cambios estructurales (permiso L208-216 se conserva; solo texto AD-7) |
| creators-directory · Sembrado purgado, padrón, idempotente | AD-1 + AD-2 |
| creators-directory · Alias /autores operativo (D2) | AD-4/AD-7: `@page` intactos; T6 lo afirma |
| creators-directory · Diseñador texto plano en ficha de juego | AD-5 |
| creators-directory · Sin "Obras" ni cruce por Designer | AD-6 |
| creators-directory · Reetiquetado transversal | AD-7 |
| giveaway-radar · Sin banner legacy bajo ninguna ruta | AD-4 |
| giveaway-radar · /radar alias silencioso | AD-4 |

## Open Questions

- Ninguna bloqueante. D1-D5 están confirmadas; los microdetalles pendientes (emojis de píldoras nuevas 📰/🎪, texto exacto de párrafos reetiquetados) quedan fijados en AD-3/AD-7 y son ajustables en review sin impacto estructural.
