# Design: Portada Editorial (INC-35)

> Fase SDD `sdd-design`. Idioma: español castellano (regla suprema AGENTS.md; prevalece sobre el "default to English" de la skill). Worktree `C:\repos\ludeka-wt\portada-editorial`, rama `inc/portada-editorial`.
> Store: hybrid — este documento + Engram `sdd/portada-editorial/design`.
> Entradas: `proposal.md` (D1–D5 cerradas), las 4 specs delta (`home-landing-hero`, `home-dashboard-rails`, `default-image-fallbacks`, `iconography-lucide`) y `explore.md`.

## Technical Approach

Extender el sistema editorial existente, no reemplazarlo. La portada se descompone en componentes dedicados (`Components/Home/`) que consumen los DTOs actuales sin tocar `IHomeDashboardService`, DTOs ni seeders. La identidad carbón+terracota se refuerza con tokens de microinteracción compartidos en `input.css` (generalizando `.game-card-editorial`), serif display Fraunces solo en portada, `Icon.razor` (Lucide inline) y `DefaultImage.razor` (SVG temable + variante estática). El hero pasa de bloque de gradiente a `HeroEditorial.razor` con variantes de fondo conmutables (D1), imagen servida en AVIF/WebP según el patrón INC-07. Tres PRs encadenados desde el mismo worktree: fundación → portada → Lucide global.

## Architecture Decisions

### Decision 1 — Copy definitiva del hero

**Choice**: `h1` = **"La mesa está servida"**. Subtítulo (2 frases): **"Ludeka es el Letterboxd de los juegos de mesa en español: descubre qué títulos merecen tu tapete, participa en los sorteos de la comunidad y sigue las novedades y ferias del sector. Tu próxima partida empieza aquí."**

**Alternatives considered**: "Tu próxima partida empieza aquí" como titular (redundante con el cierre del subtítulo); "El Letterboxd de los juegos de mesa en español" como titular (es posicionamiento de marca, frío para un hero hogareño; ya vive en el `<PageTitle>`).

**Rationale**: el delta exige un h1 castellano que describa la invitación a la mesa; "La mesa está servida" evoca tapete y sobremesa con registro editorial cálido. El subtítulo conserva las 4 promesas actuales (catálogo, sorteos, novedades, ferias) y transporta la marca. Las 4 píldoras y el buscador quedan intactos (spec congelada).

### Decision 2 — Conmutación de variantes del fondo del hero (D1)

**Choice**: enum `HeroBackgroundVariant` (`Components/Home/HeroBackgroundVariant.cs`) + componente `Components/Home/HeroEditorial.razor` con `[Parameter] public HeroBackgroundVariant Background { get; set; } = HeroBackgroundVariant.FotoEurogame;`. La llamada en `HomeDashboard.razor` fija la variante activa en UNA línea. Mapa variante→asset:

| Variante | Archivos (`wwwroot/images/home/`) | Origen |
|---|---|---|
| `FotoEurogame` (activa) | `hero-ambiente-eurogame.{avif,webp,jpg}` | foto existente 571 KB |
| `FotoMesaAmigos` | `hero-ambiente-mesa-amigos.{avif,webp,jpg}` | foto existente 393 KB |
| `FotoPrimerPlano` | `hero-ambiente-primer-plano.{avif,webp,jpg}` | foto existente 1,4 MB |
| `CssScene` | — (solo CSS + variables de tema) | escena de serie |
| `Ilustracion` | `hero-ilustracion.{avif,webp,jpg}` | futura (nano banana) |

Cómo añadir la ilustración: exportar el asset (jpg/png) a `wwwroot/images/home/`, ejecutar `node scripts/convert-hero-images.mjs` (genera AVIF/WebP) y cambiar la constante a `HeroBackgroundVariant.Ilustracion`. Convención de nombre reservada: `hero-ilustracion.*`.

Fallback si el asset falta: en variantes con imagen, la capa de escena CSS SIEMPRE se renderiza debajo del `<picture>` (son gradientes, 0 peticiones). El `onerror` del `<img>` añade la clase `hero-bg-media--failed` al `<picture>` (`display:none`) y el fondo cae a la escena CSS. El escenario "escena CSS sin peticiones de imagen" se cumple porque en `CssScene` el `<picture>` no se renderiza (`@switch`).

**Alternatives considered**: constante `static readonly` dentro de `HomeDashboard.razor` (menos archivos, pero mezcla lógica de hero con orquestación y dificulta reutilizar el hero); config por `appsettings` (innecesario: es decisión de diseño, no de entorno).

**Rationale**: el parámetro con default es el mecanismo más simple que cumple el escenario "cambio de variante sin tocar estructura": un valor, cero markup condicional fuera del `@switch` del fondo.

### Decision 3 — Estructura de `Components/Home/` (D5, extracción)

**Choice**: 7 archivos nuevos:

| Archivo | Props / DTO consumido |
|---|---|
| `HeroEditorial.razor` | `HeroBackgroundVariant Background`; buscador y píldoras internos (usa `NavigationManager`) |
| `RailHeader.razor` | `IconName`, `Title`, `Count`, `CountLabel`, `Href` (nullable: Eventos no tiene), `LinkText`, `AriaLabel` |
| `HomeGameCard.razor` | `GameSummaryDto Game` (CoverImageUrl, IsExpansion, SpanishTitle, Slug, BggRating, YearPublished, IdealPlayerCountText, Designer). Conserva fallback actual (`game-placeholder.svg`/`expansion-placeholder.svg`) |
| `HomeGiveawayCard.razor` | `GiveawayDto Giveaway` (ThumbnailUrl→**estrena render**, Title, Url, IsPromoted, Platform, RemainingTimeText, FormattedOrganizer) |
| `HomeReleaseCard.razor` | `WeeklyReleaseDto Release` (CoverImageUrl→**estrena render**, Title, Publisher, IsReprint, ReleaseDate, EstimatedPvp) |
| `HomeEventCard.razor` | `BoardGameEventDto Event` (ImageUrl, Title, FormattedDates, RemainingDaysText, Location, Description, WebsiteUrl). Añade `width`/`height` y fallback que hoy no tiene |
| `HeroBackgroundVariant.cs` | enum |

`HomeDashboard.razor` queda como orquestador: carga de datos, `@if (_isLoading)` (spinner con `Icon` `dices`, sin streaming), 4 secciones con `RailHeader` + bucle de su card. Paridad garantizada: mismo destino, `aria-label` y contenido por tarjeta (spec home-dashboard-rails).

**Alternatives considered**: reusar `GameCard.razor`/`GiveawayCard.razor` de `Shared` (acoplan anchos, badges y estilos de otras páginas; la extracción con lenguaje `.rail-card` común es más limpia); refactor mínimo sin componentes (perpetúa la duplicación, viola la spec nueva).

**Rationale**: el markup duplicado (≈240 líneas) se centraliza y el hover/fallback viven en un único punto por dominio. `RailHeader` elimina además la cabecera de sección repetida 4 veces (incluido el enlace "Ver todos…").

### Decision 4 — Tokens de microinteracción en `input.css`

**Choice**: bloque de tokens en `:root` (no varían por tema) + clase `.rail-card` que generaliza `.game-card-editorial`:

```css
:root {
  --ease-out-expo: cubic-bezier(0.16, 1, 0.3, 1);   /* el que ya usa .game-card-editorial */
  --ease-out-quad: cubic-bezier(0.25, 0.46, 0.45, 0.94);
  --dur-fast: 150ms; --dur-base: 220ms; --dur-slow: 350ms;
  --rail-lift: -4px; --rail-zoom: 1.03;
  --font-display: 'Fraunces', 'Plus Jakarta Sans', Georgia, serif;
}
.rail-card { background: var(--bg-card); border: 1px solid var(--border-subtle);
  border-radius: 0.95rem; overflow: hidden; display: flex; flex-direction: column;
  transition: transform var(--dur-base) var(--ease-out-expo),
              border-color var(--dur-base) ease, box-shadow var(--dur-base) ease; }
.rail-card:hover, .rail-card:focus-visible {
  transform: translateY(var(--rail-lift)); border-color: var(--brand-primary);
  box-shadow: 0 16px 36px -6px rgba(0,0,0,.25), 0 0 0 1px var(--brand-glow), 0 0 24px var(--brand-glow); }
.rail-card:focus-visible { outline: 2px solid var(--brand-primary); outline-offset: 2px; }
.rail-card:hover .rail-cover img, .rail-card:focus-visible .rail-cover img { transform: scale(var(--rail-zoom)); }
.rail-cover { position: relative; overflow: hidden; background: var(--bg-surface-elevated); }
.rail-cover--square { aspect-ratio: 1 / 1; }        /* Top 20 */
.rail-cover--wide   { aspect-ratio: 16 / 9; }       /* Sorteos y Novedades (imagen nueva) */
.rail-cover--banner { height: 9rem; }               /* Eventos (sustituye h-36) */
.rail-cover img { width: 100%; height: 100%; object-fit: cover;
  transition: transform var(--dur-slow) var(--ease-out-quad); }
.scrollbar-none { scrollbar-width: none; -ms-overflow-style: none; }
.scrollbar-none::-webkit-scrollbar { display: none; }
@media (prefers-reduced-motion: reduce) {
  .rail-card, .rail-card .rail-cover img { transition: none; }
  .rail-card:hover, .rail-card:focus-visible, .rail-card:hover .rail-cover img { transform: none; }
}
```

**Alternatives considered**: utilidades Tailwind inline `hover:-translate-y-1 hover:scale-105` (rápido pero perpetúa la inconsistencia, sin reduced-motion ni tokens); librería JS de animación (coste de hidratación absurdo en SSR/InteractiveServer).

**Rationale**: `.game-card-editorial` ya usa exactamente `220ms cubic-bezier(0.16,1,0.3,1)` y zoom 1.03 → los tokens son la extracción de lo que ya funciona. `.scrollbar-none` deja de ser clase muerta (definición real) y `sm:w-68` se elimina al reescribir las cards (anchos ya definidos por componente: `w-40 sm:w-48`, `w-64 sm:w-72`, `w-60 sm:w-72`, `w-72 sm:w-80`).

### Decision 5 — `DefaultImage.razor` (SVG temable + estático)

**Choice**: `Components/Shared/DefaultImage.razor` + enum `DefaultImageDomain { Evento, Sorteo, Novedad, Generico }`:

```csharp
[Parameter, EditorRequired] public DefaultImageDomain Domain { get; set; }
[Parameter] public string? Alt { get; set; }      // null => aria-hidden="true" (decorativo)
[Parameter] public string? CssClass { get; set; } // p. ej. "w-full h-full"
```

Renderiza `<svg viewBox="0 0 400 225">` inline con rellenos por variables CSS (`--bg-surface-elevated`, `--brand-primary`, `--brand-glow`, `--border-subtle`, `--text-secondary`) → hereda los 5 temas. Dominio sin variante → `Generico` (isotipo Ludeka), nunca blanco (spec). Clase estática `DefaultImageAssets.Url(Domain)` → `/images/defaults/{evento|sorteo|novedad|generico}-default.svg` para la ruta estática. Regla de uso en cards:

- **URL nula/vacía** → `DefaultImage.razor` inline en el contenedor `.rail-cover` (sin `<img>`).
- **URL presente** → `<img loading="lazy" decoding="async" onerror="this.onerror=null; this.src='/images/defaults/{dominio}-default.svg';">` (patrón vigente de placeholders).

**Alternatives considered**: solo SVG estático (un `<img>` no lee variables CSS: mismo look en temas claros/oscuros — descartado como única vía); fotos AVIF por dominio (3 assets que producir, peso alto, misma rigidez temática).

**Rationale**: el inline cubre el caso estructural (dato sin imagen, themable) y el estático cubre el caso runtime donde Blazor no existe (URL externa caída). Un solo componente, reusable en `Events.razor`/`EventsManagement.razor` (D5) y cualquier página futura.

### Decision 6 — `Icon.razor` y estrategia de migración

**Choice**: `Components/Shared/Icon.razor` + catálogo en clase parcial `Components/Shared/IconCatalog.cs` (`static readonly Dictionary<string, string>` nombre→paths Lucide):

```csharp
[Parameter, EditorRequired] public string Name { get; set; } = null!; // kebab-case Lucide
[Parameter] public int Size { get; set; } = 16;        // px
[Parameter] public int StrokeWidth { get; set; } = 2;
[Parameter] public string? Title { get; set; }         // null => aria-hidden="true"
```

SVG envolvente: `viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round"`. Nombre desconocido → sin salida, sin error (spec). **Sin** petición de red. Estrategia de include: **inline partial** (diccionario whitelisted + `MarkupString` interno controlado) — no sprite: el sprite exige hoja global inyectada y coordinación SSR por lo que gana poco con ~20-40 iconos; no `<img src>` (pierde `currentColor`); no NuGet (dependencia por algo que son paths).

Mapa de migración de la portada (contrato semántico PR-2): 🔍→`search`, 🎲→`dices` (píldora Catálogo y spinner), 🎁→`gift`, 📰→`newspaper`, 🎪→`tent`, 🏆→`trophy`, ⭐→`star`, ⏱️→`timer`, 🚀→`rocket`, 🔄→`refresh-cw`, 🆕→`sparkles`, 🗓️→`calendar-days`, 📅→`calendar`, 📍→`map-pin`, 🌐→`globe`, 🧩→`puzzle`. Donde el emoji era señal semántica (badge "Expansión", "Promocionado") el texto visible ya existe y el icono queda `aria-hidden`.

Plan de migración global (PR-3, por orden): 1) portada (PR-2), 2) catálogo y fichas (`GameCard`, `GameDetail`, `CreatorsDirectory`, `CreatorDetail`…), 3) ludoteca (`MyLibrary`, préstamos), 4) eventos/sorteos/novedades (`Events*`, `GiveawayCard`, radar), 5) admin y modales de gestión. Cada página añade sus paths nuevos a `IconCatalog.cs`.

**Alternatives considered**: migrar todo en PR-2 (rompe presupuesto 400); dejar el resto para otro incremento (contradice D3 cerrada).

**Rationale**: el diccionario whitelist es auditable en tests de contrato, crece por página y mantiene cero dependencias (AGENTS.md pide Lucide).

### Decision 7 — Serif Fraunces (D4)

**Choice**: ampliar la URL existente de Google Fonts en `App.razor:13` con `family=Fraunces:opsz,wght@9..144,600..700` (variable, eje óptico completo + rango 600–700; misma petición, un woff2 latin extra ≈ 15-40 KB, `display=swap` ya presente). `--font-display` en `:root` (ver Decision 4). Aplicación: `.hero-title` (h1 del hero) y `.rail-title` (los 4 `<h2>`), exclusivamente en portada. **NO** global.

**Alternatives considered**: `rel=preload` del woff2 (la URL con hash cambia en cada release de Google → preload frágil y riesgo de doble descarga; los preconnect ya existentes cubren el handshake); Playfair Display/Newsreader (válido pero Fraunces aporta más carácter editorial-cálido y soporta eje óptico).

**Rationale**: cero peticiones nuevas (spec home-dashboard-rails), fallback a Jakarta Sans/Georgia sin FOUT grave, y alcance tipográfico acotado a lo aprobado en D4.

### Decision 8 — Assets por defecto (estilo y contenido)

**Choice**: 4 SVG estáticos en `wwwroot/images/defaults/`, `viewBox 400×225` (formato wide de los carriles), paleta fija carbón+terracota legible en temas claros y oscuros (gradiente `#1B2228→#242C34`, acento `#E05A38`, texto `#9AB0C2`, tipografía Plus Jakarta Sans como `game-placeholder.svg`):

| Asset | Motivo central | Microtexto |
|---|---|---|
| `evento-default.svg` | carrusel/entoldado de feria (motivo `tent`) | "EVENTO LUDEKA" |
| `sorteo-default.svg` | caja de regalo con lazo (motivo `gift`) | "SORTEO LUDEKA" |
| `novedad-default.svg` | etiqueta/roseta con destello (motivo `sparkles`) | "NOVEDAD" |
| `generico-default.svg` | isotipo dado de Ludeka (herencia del placeholder de juego) | "LUDEKA" |

Las variantes inline de `DefaultImage.razor` replican los mismos motivos con variables CSS. El placeholder de juego NO se reutiliza para estos dominios (spec).

**Alternatives considered**: fotos por dominio (coste de producción, peso); un único SVG genérico (pierde la identidad por dominio que pide la spec).

**Rationale**: composición plana coherente con la identidad existente, peso ínfimo, cacheable, y paridad visual 1:1 entre la variante inline y la estática (misma geometría, distinto sistema de color).

### Decision 9 — Plan de PRs encadenados (auto-chain)

**Choice**: desde el mismo worktree, ramas apiladas `inc/portada-editorial` → `inc/portada-editorial-2` → `inc/portada-editorial-3`. Criterio de inclusión por archivo y estimación:

| PR | Título | Incluye (criterio duro) | Estimación |
|---|---|---|---|
| PR-1 | `feat: fundación editorial` | `Styles/input.css` (tokens, `.rail-card`, `.rail-cover`, `.scrollbar-none`, reduced-motion, `--font-display`), `App.razor` (1 línea Fraunces), `Shared/Icon.razor` + `IconCatalog.cs`, `Shared/DefaultImage.razor` + `DefaultImageDomain.cs`, `wwwroot/images/defaults/*.svg` (4), `scripts/convert-hero-images.mjs`, assets `images/home/*.{avif,webp}`. **Cero toques en `Pages/*.razor`** | ~300-350 líneas |
| PR-2 | `feat: portada narrativa` | `Components/Home/*` (7), reescritura `HomeDashboard.razor` (hero + carriles por componentes; aquí muere `sm:w-68` y el emoji del spinner). **Criterio**: si excede 400 líneas, los fixes D5 migran a la cabeza de PR-3 | ~350-420 líneas |
| PR-3 | `refactor: iconografía Lucide global` | fixes D5 (`Events.razor:132`, `EventsManagement.razor:81` con `DefaultImage.razor`/`onerror`) + migración emojis por el orden de la Decision 6, añadiendo paths a `IconCatalog.cs` | ~300-600 líneas |

**Alternatives considered**: PR único (~1.100 líneas, inviable para revisión); dividir por capas en vez de por slices (PR-1 no entrega valor visible solo).

**Rationale**: cada PR es independiente y reversible (plan de rollback de la propuesta), PR-1 es puramente aditivo (no toca páginas → riesgo cero), y el presupuesto de 400 líneas por PR se respeta con un solo criterio objetivo de reasignación (D5).

### Decision 10 — Rendimiento y accesibilidad del hero y carriles

**Choice**:
- **Conversión de assets**: no existen `cwebp`/`avifenc`/`ffmpeg`/`magick` en el entorno; sí `node`/`npx`. Se crea `scripts/convert-hero-images.mjs` con **sharp** (`npm i --no-save sharp` en temp): redimensiona a 1600 px de ancho y genera AVIF (q≈50) y WebP (q≈72) + JPEG recomprimido (q≈75). Presupuesto: cada fuente < 200 KB (verificado con script; las 3 fotos actuales pesan 393 KB-1,4 MB). `squoosh-cli` descartado (sin mantenimiento).
- **Markup del hero**: `<picture>` con `<source type="image/avif">` + `<source type="image/webp">` + `<img src="…jpg" fetchpriority="high" decoding="async" width="1600" height="900" alt="…">`. **Sin `preload`**: el `<picture>` se resuelve en el propio documento y el hero es el primer bloque del DOM; un preload desde `App.razor` dispararía la carga en todas las páginas (la portada es la única con hero) y un preload del formato equivocado duplica la descarga. `fetchpriority="high"` + posición temprana cubren el LCP (< 2,5 s objetivo, patrón INC-07). CLS 0: contenedor con `aspect-ratio` fijo + atributos dimensionales.
- **Scrim por tema**: capa overlay `linear-gradient(90deg, var(--bg-main) 0%, transparent 60%), linear-gradient(180deg, transparent 40%, var(--bg-main) 100%)` → el cuadrante inferior-izquierdo (donde vive el texto) queda casi opaco con `--bg-main`; el texto usa `var(--text-primary)`, por lo que el contraste ≥ 4,5:1 se conserva por construcción en los 5 temas.
- **Alt en castellano**: `FotoEurogame` → "Mesa de juego con un eurogame en marcha sobre el tapete y una estantería lúdica al fondo"; `FotoMesaAmigos` → "Grupo de amigos riendo alrededor de una mesa de madera con juegos de mesa"; `FotoPrimerPlano` → "Primer plano de manos colocando piezas sobre el tablero de un juego de mesa"; `Ilustracion` → "Ilustración editorial de una mesa de juego con estantería al fondo".
- **Carriles**: `loading="lazy"` + `decoding="async"` + contenedores de aspecto fijo (`.rail-cover--*`), targets ≥ 24×24 (píldoras actuales ≈ 29 px), `aria-hidden` en iconos decorativos, foco visible 2 px (`.rail-card:focus-visible`).

**Alternatives considered**: srcset 800w/1600w (6 archivos por foto; diferible: el AVIF 1600w ya cabe en presupuesto); preload del hero (razón arriba).

**Rationale**: cumple literalmente el requirement de rendimiento del delta (`<picture>`, `fetchpriority`, dimensiones, < 200 KB) y el de contraste/alt, con las decisiones que evitan trampas conocidas (preload frágil, doble descarga).

## Data Flow

```
HomeDashboard.razor (orquestador, @rendermode InteractiveServer)
   │  IHomeDashboardService.GetDashboardDataAsync()  [sin cambios, cacheado]
   ├── HeroEditorial.razor ── HeroBackgroundVariant ──> <picture> AVIF/WebP/JPG  o  escena CSS
   │        └── Icon (search/dices/gift/newspaper/tent)  +  píldoras 4x
   ├── RailHeader.razor  (icono + h2 .rail-title + contador + enlace)
   ├── HomeGameCard.razor    (GameSummaryDto)  ── fallback actual game-placeholder
   ├── HomeGiveawayCard.razor(GiveawayDto)     ── estrena imagen: DefaultImage inline si falta
   ├── HomeReleaseCard.razor (WeeklyReleaseDto)── estrena imagen: DefaultImage inline si falta
   └── HomeEventCard.razor   (BoardGameEventDto)── onerror -> /images/defaults/evento-default.svg
                    │
                    └── Shared/Icon.razor (catálogo Lucide)  ·  Shared/DefaultImage.razor (SVG temable)
```

## File Changes

| Archivo | Acción | Descripción |
|---|---|---|
| `src/Ludeka.Web/Styles/input.css` | Modificar | Tokens (`--ease-*`, `--dur-*`, `--rail-*`, `--font-display`), `.rail-card`, `.rail-cover(--square/wide/banner)`, `.hero-title`, `.rail-title`, `.hero-scrim`, `.scrollbar-none` real, `prefers-reduced-motion` |
| `src/Ludeka.Web/Components/App.razor` | Modificar | Fraunces añadida a la URL existente de Google Fonts (misma petición) |
| `src/Ludeka.Web/Components/Shared/Icon.razor` | Crear | SVG Lucide inline, `currentColor`, `aria-hidden` por defecto |
| `src/Ludeka.Web/Components/Shared/IconCatalog.cs` | Crear | Diccionario whitelist nombre→paths |
| `src/Ludeka.Web/Components/Shared/DefaultImage.razor` | Crear | SVG inline temable por dominio |
| `src/Ludeka.Web/Components/Shared/DefaultImageDomain.cs` | Crear | Enum `Evento/Sorteo/Novedad/Generico` + `DefaultImageAssets.Url()` |
| `src/Ludeka.Web/Components/Home/HeroEditorial.razor` | Crear | Hero narrativo con variantes de fondo |
| `src/Ludeka.Web/Components/Home/HeroBackgroundVariant.cs` | Crear | Enum de variantes + mapa a archivos |
| `src/Ludeka.Web/Components/Home/RailHeader.razor` | Crear | Cabecera de carril reutilizable |
| `src/Ludeka.Web/Components/Home/HomeGameCard.razor` | Crear | Card Top 20 (fallback actual conservado) |
| `src/Ludeka.Web/Components/Home/HomeGiveawayCard.razor` | Crear | Card Sorteos (estrena imagen + fallback) |
| `src/Ludeka.Web/Components/Home/HomeReleaseCard.razor` | Crear | Card Novedades (estrena imagen + fallback) |
| `src/Ludeka.Web/Components/Home/HomeEventCard.razor` | Crear | Card Eventos (añade dimensiones + fallback) |
| `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` | Modificar | Orquestador: hero por componente, carriles por componentes, sin markup duplicado ni emojis |
| `src/Ludeka.Web/Components/Pages/Events.razor` | Modificar | Fix D5: imagen de evento con fallback (`~30-50` líneas) |
| `src/Ludeka.Web/Components/Pages/EventsManagement.razor` | Modificar | Fix D5: ídem |
| `src/Ludeka.Web/wwwroot/images/defaults/*.svg` | Crear | 4 assets por dominio (Decision 8) |
| `src/Ludeka.Web/wwwroot/images/home/hero-*.{avif,webp}` | Crear | Conversión sharp de las 3 fotos (+ jpg recomprimido) |
| `scripts/convert-hero-images.mjs` | Crear | Conversión reproducible AVIF/WebP/JPEG con sharp |
| PR-3: páginas del resto de la web | Modificar | Migración emojis→`Icon.razor` por orden de la Decision 6 |

## Interfaces / Contracts

Ver bloques de parámetros en Decisiones 2, 5 y 6. Contratos clave:
- `HeroBackgroundVariant { FotoEurogame, FotoMesaAmigos, FotoPrimerPlano, CssScene, Ilustracion }`.
- `DefaultImageDomain { Evento, Sorteo, Novedad, Generico }`; `DefaultImageAssets.Url(Domain)` nunca devuelve null.
- `Icon.razor`: nombre fuera de catálogo → render vacío (nunca emoji ni icono sustituto).
- Los componentes de carril aceptan su DTO con `[Parameter, EditorRequired]` y no acceden a servicios (datos puros desde el orquestador).

## Testing Strategy

| Layer | What to Test | Approach |
|---|---|---|
| Unit (markup contract, patrón INC-31 `WebMarkupContractTests`) | `HomeDashboard.razor` sin markup de card inline, sin emojis; `HeroEditorial` contiene `<picture>` con `fetchpriority="high"`, `width`/`height`, alt castellano y sin badge "PORTADA EDITORIAL"; `Icon.razor` sin emojis como fallback; `input.css` define `.rail-card`, `.scrollbar-none`, `--font-display` y `prefers-reduced-motion`; `Events.razor`/`EventsManagement.razor` contienen `onerror`+defaults | Extensiones de `MarkupContracts` leyendo fuentes reales |
| Unit (comportamiento, patrón `HomeDashboardQuickSearchTests`) | `HandleQuickSearch` del hero (navegación a `/catalogo?q=`) intacto tras extraer `HeroEditorial`; `DefaultImageAssets.Url()` por dominio; `Icon` render vacío ante nombre desconocido (lógica de lookup) | Instanciación directa + reflexión, sin bUnit (decisión INC-31 vigente) |
| Integración (sdd-verify) | Render real en los 5 `data-theme` (inline temable), contraste del scrim, LCP/CLS en móvil, peso < 200 KB por asset, assets servidos 200 | `dotnet run` + DevTools/Lighthouse; `dotnet test Ludeka.sln` en verde |

## Threat Matrix

N/A — sin routing nuevo, shell, subprocesses en runtime, automatización VCS/PR más allá del flujo de worktree vigente, clasificación de ejecutables ni integración de procesos. `scripts/convert-hero-images.mjs` es tooling de desarrollo manual (convierte assets locales), no un límite de confianza del producto.

## Migration / Rollout

No hay migración de datos ni cambios de contrato de servicios. Rollback: revertir el PR del slice restaura el estado anterior (PR-1 aditivo; PR-2 reversible a la portada actual; PR-3 revertible por página). La variante activa del hero es una constante: si la foto elegida no convence, cambiar a otra variante es 1 línea, sin redeploy de assets nuevos.

## Open Questions

- [ ] Foto por defecto (`FotoEurogame`) sujeta a verificación visual del usuario; las 3 se convierten y el cambio es de 1 línea (Decision 2).
- [ ] La ilustración nano banana no existe aún: la variante `Ilustracion` queda definida pero inactiva; el prompt de generación queda fuera de este diseño.
- [ ] Streaming SSR/esqueletos del estado de carga: fuera de alcance en este diseño (spinner migrado a `Icon`); se evaluará en sdd-tasks solo si no dispara el presupuesto de PR-2.
