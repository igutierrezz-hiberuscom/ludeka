# Exploración: rediseno-paginas-editoriales (INC-36)

> Fase SDD `sdd-explore` — incremento `rediseno-paginas-editoriales` (INC-36 en roadmap). Worktree `C:\repos\ludeka-wt\rediseno-paginas-editoriales`, rama `inc/rediseno-paginas-editoriales`.
> Idioma: español castellano (regla suprema AGENTS.md; prevalece sobre el "default to English" de la skill).
> Store: hybrid — este documento + Engram `sdd/rediseno-paginas-editoriales/explore`.
> Entradas: AGENTS.md del worktree, código fuente real de las 5 páginas + hero + `Styles/input.css` + `wwwroot/app.css`, `WebMarkupContractTests.cs` (patrón INC-31), `HeroEditorialQuickSearchTests.cs`, `PerformanceAndAccessibilityTests.cs`, y los artefactos archivados de INC-35 (`openspec/changes/archive/2026-09-10-portada-editorial/`).

---

## 1. Hallazgos del hero — bug responsive (prioridad del maintainer)

### 1.1 Cómo se renderiza la imagen hoy

La imagen del hero es un `<img>` real (NO un `background-image` CSS) dentro de un `<picture>` con AVIF/WebP/JPG (`HeroEditorial.razor:24-34`), con `width="1600" height="900"` (16:9), `fetchpriority="high"`, `decoding="async"`, alt descriptivo por variante y `onerror` que oculta el `<picture>` (`hero-bg-media--failed`) para caer a la escena CSS de debajo. El posicionamiento lo hace el CSS (`input.css:265-305`):

```css
.hero-scene     { position: absolute; inset: 0; }              /* gradiente, fallback */
.hero-bg-media  { position: absolute; inset: 0; }
.hero-bg-media img { width: 100%; height: 100%; object-fit: cover; }
.hero-actions   { position: absolute; inset-inline: 0; top: 0; }
```

Y la sección contenedora (`HeroEditorial.razor:11`): `class="hero-editorial relative min-h-[360px] sm:min-h-[460px] overflow-hidden rounded-3xl …"`.

### 1.2 Causa raíz técnica exacta

1. **Altura fija en px independiente del ancho**: `min-h-[360px]` (móvil) / `sm:min-h-[460px]` (≥640px). Todos los hijos del hero son `position: absolute` (`hero-scene`, `hero-bg-media`, `hero-actions`, el `<h1 class="sr-only">` y la tira de pruebas), de modo que **no hay contenido en flujo** y la altura efectiva de la sección es exactamente el suelo fijo: 360px en cualquier viewport móvil, salte a 460px al cruzar `sm` (640px).
2. **Recorte accidental del 16:9 en caja casi cuadrada**: con `object-fit: cover` y `object-position` por defecto (`50% 50%`), en un teléfono (~360px de ancho de viewport, ~320px de ancho de contenido tras el padding de `.container-ludeka`) la caja es ~320×360 (proporción ≈ 0,9:1) mientras el asset es 16:9. El factor de escala de `cover` es `max(360/900, 320/1600) = 0,40`: se ven solo ~800 de los 1600px de ancho del original (el 50% central) y la mesa/estantería de la composición queda **cortada por ambos bordes**. Técnicamente NO hay deformación (cover jamás deforma: recorta); lo que el maintainer percibe como "imagen aplastada/deformada por los bordes" es recorte destructivo sin punto focal diseñado.
3. **Alto vertical desproporcionado**: 360px fijos sobre un viewport de ~640-700px ≈ el 55% de la pantalla para una imagen ambiental que solo porta un buscador. El diseño de INC-35 (Decisión 10) preveía "contenedor con aspect-ratio fijo", pero la implementación lo degradó a `min-height` fija: no hay `aspect-ratio`, ni `clamp()`, ni breakpoints intermedios que reduzcan el alto proporcionalmente al ancho — que es exactamente lo que pide el maintainer ("el alto debería reducirse proporcionalmente y quedar bien compuesto").
4. **Sin `srcset`/`sizes`**: el móvil descarga el asset de 1600px aunque muestre ~50% del mismo (presupuesto de ~50-100 KB evitables; el LCP móvil carga un asset sobredimensionado). El AVIF ya está optimizado (<200 KB, Decisión 10), pero no hay variantes de ancho.
5. **Descartado que sea problema de build**: `wwwroot/app.css` compilado (245 KB) contiene `.min-h-\[360px\]{min-height:360px}`, `sm:min-h-[460px]`, `.hero-bg-media img` con `object-fit:cover` y todas las reglas del hero (verificado). El `tailwind.config.js` escanea `./Components/**/*.razor`, así que las utilidades arbitrarias sí se generan.
6. **Auditoría WCAG 2.2 AA del buscador sobre la imagen (móvil)**:
   - El `<input>` va sobre tarjeta opaca `bg-[var(--bg-card)]` con borde y sombra: contraste del texto/placeholder garantizado por fondo propio, sin depender del scrim → **OK**.
   - El botón "Buscar" usa `text-white` sobre `--brand-primary` (`#E05A38` en tema charcoal): contraste **3,69:1 → FALLA AA** (texto `text-xs`/`text-sm` bold = texto normal, exige 4,5:1). En temas de marca más oscura (editorial `#B8432F`, wood `#B85323`) el blanco sí pasa (≈5,4:1 y ≈4,9:1); el fallo es específico de los temas de marca cálida-clara (charcoal, tabletop, midnight). Texto oscuro (`#14181C`) sobre `#E05A38` = 4,84:1 → pasaría.
   - Placeholder del input: `--text-muted` (`#677B8C`) sobre `--bg-card` (`#1F262D`) ≈ 3,5:1 → borderline, no AA (mejora deseable, menor prioridad).
   - El `<img>` del hero es ambiental con alt descriptivo castellano (Decisión 10 INC-35 vigente, correcto); los iconos van `aria-hidden` por defecto en `Icon.razor` (correcto).

### 1.3 Estrategias de corrección (con tradeoffs)

| # | Estrategia | Descripción | Pros | Contras | Esfuerzo |
|---|------------|-------------|------|---------|----------|
| A | **Ratio responsiva + tapa con clamp + `object-position` focal** (recomendada) | Sustituir `min-h-[360px]/[460px]` por `aspect-ratio: 16/9` (coincide con la fuente → recorte ~0 en móvil) con `max-height: clamp()` (p. ej. `max-height: clamp(240px, 34vw, 460px)`) para acotar el alto en escritorio, suelo `min-height` bajo (~200px) y `object-position` por variante para el recorte residual | Alto proporcional al ancho (lo pedido); sin saltos bruscos en breakpoints; CLS 0 se conserva (el alto deriva del ancho, no del contenido; los atributos width/height del `<img>` siguen vigentes); 1-3 líneas de CSS + 1 propiedad de foco | En 320px el hero queda bajo (~180px): decidir si basta o se sube el suelo; hay que calibrar el `object-position` por variante (la mesa debe quedar en cuadro en el recorte de escritorio) | Bajo |
| B | **`clamp()` fluido puro** | `height: clamp(220px, 38vw, 480px)` — una sola declaración | Mínimo esfuerzo, transición suave sin media queries | El suelo re-domina en pantallas estrechas y recrea la caja casi cuadrada (recorte masivo del 16:9) que motivó el bug; no aporta foco autoral | Bajo |
| C | **`srcset`/`sizes` + recortes autoriales por breakpoint** | Generar variante móvil (p. ej. recorte 4/3 de 1200×900) y desktop (16:9 1600×900) en `scripts/convert-hero-images.mjs`, servirlas con `sizes="(max-width: 640px) 100vw, 1240px"` | La composición móvil se **diseña** (no es accidental); menos KB en móvil (mejora LCP real); cada variante se puede componer con el buscador en mente | Exige ampliar el script de conversión y generar 2-3 assets por variante; más coste de build/mantenimiento y de contratos de assets; rompe la paridad "1 foto = 3 formatos" vigente | Medio-Alto |

**Recomendación**: A como núcleo del fix (ratio responsiva + `object-position` + tapa `clamp()` + suelo acotado), manteniendo intactos `<picture>`, `fetchpriority`, dimensiones y `onerror` (contratos INC-35). La estrategia C queda como segunda ola opcional si el maintainer quiere composición móvil autoral; B como mínimo si se prioriza tocar una sola línea. En cualquier caso, añadir el fix de contraste del botón (D3: token `--on-brand`).

---

## 2. Inventario y estado por página

Iconografía: las 5 páginas ya usan `Icon.razor` (Lucide) — la migración de emojis de INC-35 PR-3 está completa y el barrido `RestoDeLaWeb_SinEmojisDeLaListaSpecEnNingunComponenteRazor` protege el resultado. Los glifos tipográficos (★, →, ↗, ←) están permitidos por decisión documentada de INC-35.

### 2.1 Catálogo — `src/Ludeka.Web/Components/Pages/Home.razor` (`/catalogo`, 163 líneas)

- **Estructura**: cabecera centrada `max-w-3xl` (h1 sans `font-extrabold` con punto terracota "Descubre tu próxima partida.", subtítulo, `CatalogSearchBar`) → tira scrollable de 7 `.filter-btn` (presets con Icon) → grid `2/3/4/5` cols de `GameCard` → empty state con reset → spinner bloqueante.
- **Estilos**: casi todo con tokens de tema (`--text-*`, `--bg-card`, `--border-subtle`) + clases `.filter-btn`, `.game-card-editorial` (en `GameCard.razor`, ya editorial). **Sin serif display** (el h1 no usa `--font-display`: INC-35 acotó Fraunces a la portada).
- **Hallazgo concreto**: la tira de filtros usa `no-scrollbar` (línea 25) — **clase muerta** (no existe ni en `input.css` ni en `app.css`; la real es `.scrollbar-none`, definida en INC-35): la banda de scroll del carril de filtros se ve en escritorio. Es el mismo bug de clase muerta que INC-35 arregló en la portada.
- **Componentes**: `CatalogSearchBar`, `GameCard` (Shared), `Icon`. Grid de `GameCard` con `.game-card-editorial` — ya habla el lenguaje editorial de hover/border/glow.
- **Patrones INC-35 aplicables**: h1 en `--font-display` (o `PageHeaderEditorial`), píldoras de filtro unificadas con `.badge-pill`/estados, `scrollbar-none` real, esqueletos (opcional), microtextos en vacíos.
- **Decisiones de diseño abiertas**: ver D4, D5, D12.

### 2.2 Fichas — `src/Ludeka.Web/Components/Pages/GameDetail.razor` (`/juegos/{Slug}`, 719 líneas)

- **Estructura**: barra superior de acciones (hasta 8-9 botones con moderador) → `detail-hero-backdrop` (gradiente radial con `--brand-glow`, patrón editorial existente) con carátula `aspect-square` + identidad + doble rating BGG/Ludeka (glifo ★, permitido) + `QuickBadges` (ADN Lúdico) → `CollectionActionBar` (barra mobile-first ya existente) → bloques editoriales (Veredicto/IA, expansión, semáforo, tiendas, fundas, multimedia, Q&A, sinopsis) → 7 modales.
- **Estilos**: mezcla de tokens y **colores Tailwind hardcodeados** que rompen los 5 temas: `text-white` (línea 25, en el estado "Juego no encontrado" — **invisible en temas claros** editorial/wood), `text-slate-400` (línea 16), `bg-amber-500/10`, `bg-indigo-500/10`, `bg-purple-950/85`, `hover:bg-rose-500/10`, `text-sky-400`.
- **Hallazgos concretos**:
  - La fila de acciones usa `flex items-center gap-3` **sin `flex-wrap`** (línea 42): con moderador hay 8+ botones y en móvil desbordan el ancho → scroll horizontal de página.
  - **Typo en `<PageTitle>`**: "Ludeca" en vez de "Ludeka" (2 ocurrencias, línea 12).
  - Los bloques repetidos `p-6 rounded-2xl bg-[var(--bg-card)] border …` son candidatos a clase editorial compartida.
- **Componentes**: `CollectionActionBar`, `QuickBadges`, `ScalabilityTrafficLight`, `ParentGameBanner`, `ExpansionAporteCard/EcosystemSection/SisterList`, `FoundingVerdictCard`, `AiSummaryCard`, `UserReviewCard`, `StoreOffersCard`, `SleeveGuideCard`, `MultimediaHub`, `RuleQuestionsSection` + 7 modales.
- **Patrones INC-35 aplicables**: h1 con `--font-display`, tokens de estado semánticos, barra de acciones mobile-first (ya existe: `CollectionActionBar`; falta domar la barra de moderación), `--on-brand` en botones primarios.
- **Decisiones abiertas**: ver D7, D8.

### 2.3 Eventos — `src/Ludeka.Web/Components/Pages/Events.razor` (`/eventos`, 283 líneas)

- **Estructura**: cabecera de página (badge píldora + h1 `font-black` + subtítulo) → `role="tablist"` con 2 pestañas (aria-selected correcto, pero sin `tabpanel`/`aria-controls` asociados — a11y incompleta) → filtros territoriales (botones + `select` con `CountryCatalog.GetFlag`, excepción data-driven documentada) → grid `1/2/3` de tarjetas inline (imagen `h-48`, badges superpuestos `bg-black/60`, metadatos, pie con "Web Oficial") → empty states por pestaña.
- **Estilos**: mayormente tokens; badges de urgencia con `GetRemainingBadgeClass` devuelven colores hardcodeados (`bg-rose-500/90`, `bg-amber-500/90 text-black`, `bg-black/60`) — sobre imagen es legible, pero rompe la consistencia de tokens; `animate-pulse` sin equivalente `prefers-reduced-motion` explícito para ese caso; `hover:scale-105` ad-hoc en la imagen (fuera del lenguaje `.rail-card`).
- **Fortalezas ya conquistadas (INC-35 D5)**: `DefaultImage` inline + `onerror` → `evento-default.svg` + `width/height` en toda `<img>` — protegido por contrato `PaginasEventos_TodaImagenDeEventoTieneFallbackPorDominio`.
- **Patrones INC-35 aplicables**: adoptar `.rail-card` (lift/glow/focus/reduced-motion) en las tarjetas, `.rail-title`/serif para el h1, badge de urgencia con tokens semánticos, targets ≥24px.
- **Decisiones abiertas**: ver D4, D5, D6, D8.

### 2.4 Sorteos — `src/Ludeka.Web/Components/Pages/Radar.razor` (`/sorteos` + `/radar`, 380 líneas)

- **Estructura**: cabecera de página idéntica en patrón a Events/News (badge + h1 + subtítulo + acción) → filtros vigentes/finalizados + territoriales → grid de `GiveawayCard` (Shared) → empty state → **modal de creación inline (~140 líneas de formulario)**.
- **Hallazgos concretos**:
  - `GiveawayCard`: `<img>` de `ThumbnailUrl` **sin `width`/`height` (CLS) y sin `onerror`** (imagen rota si la URL externa cae) — incumple el patrón de fallback que la propia página de Eventos ya cumple.
  - Colores hardcodeados `bg-amber-500*`, y variantes **`dark:text-*` inertes** (ver 2.7): p. ej. el error del formulario `text-rose-600 dark:text-rose-300` aplica `#E11D48` también en temas oscuros → **3,24:1 sobre `--bg-card` → FALLA AA** en 12px.
  - Modal con `dark:` idem.
- **Patrones INC-35 aplicables**: `.rail-card` + `.rail-cover--wide` en `GiveawayCard` (paridad con el carril `HomeGiveawayCard` de portada), `DefaultImage` + `onerror` + dimensiones, shell de modal editorial compartido, form inputs ya con tokens.
- **Decisiones abiertas**: ver D6, D8, D9, D11.

### 2.5 Novedades — `src/Ludeka.Web/Components/Pages/News.razor` (`/novedades`, 381 líneas)

- **Estructura**: cabecera idéntica a Radar/Events → buscador inline + chip de editorial removible + contador → grid `1/2/3` de **tarjetas inline** (carátula `h-44` opcional, badge Novedad/Reimpresión con icono, título, editorial clicable, notas) → pie con PVP + Instagram → empty state → **modal de creación inline (casi idéntico al de Radar, ~100 líneas duplicadas)**.
- **Hallazgos concretos**:
  - Carátula de tarjeta: `<img>` **sin `width`/`height` (CLS en el grid) y sin `onerror`**; además si falta la URL la zona de imagen no se renderiza (en portada, `HomeReleaseCard` sí pinta `DefaultImage` de dominio) → paridad rota entre carril y página.
  - `text-pink-500 dark:text-pink-400` (variante dark inerte) y `dark:text-rose-300` en el error del modal (mismo fallo AA que Radar).
- **Patrones INC-35 aplicables**: mismo tratamiento de imagen que `HomeReleaseCard` (`DefaultImage` + onerror + dimensiones), `.rail-card`, cabecera compartida, modal compartido.
- **Decisiones abiertas**: ver D6, D10, D11.

### 2.6 Resumen de incidencias transversales detectadas

| Incidencia | Dónde | Severidad |
|---|---|---|
| Hero responsive: alto fijo + recorte 16:9 sin foco | `HeroEditorial.razor` + `input.css` | Alta (motiva el incremento) |
| Botones `bg-brand + text-white` fallan AA (3,69:1) en temas de marca cálida | Hero "Buscar" + botones primarios de las 5 páginas | Alta (sistémica, WCAG 1.4.3) |
| `text-white` hardcodeado invisible en temas claros | `GameDetail.razor:25` | Alta |
| Variantes `dark:*` inertes (la app usa `data-theme`, nunca clase `.dark`) → color base en temas oscuros | Radar/News/GameDetail/otros (18 matches) | Media |
| Back-bar de GameDetail desborda en móvil (sin `flex-wrap`) | `GameDetail.razor:42` | Media |
| `<img>` sin `width/height/onerror` en páginas (paridad con carriles rota) | `News.razor`, `GiveawayCard.razor` | Media |
| Clase muerta `no-scrollbar` (banda de scroll visible) | `Home.razor:25` | Baja |
| Typo PageTitle "Ludeca" | `GameDetail.razor:12` | Baja |
| Badges de urgencia/estado con colores hardcodeados fuera de tokens | `Events.razor`, `GiveawayCard.razor` | Media |

---

## 3. Patrones editoriales reutilizables (INC-35 → INC-36)

### 3.1 Tokens y clases ya disponibles (`Styles/input.css`, 608 líneas)

- **Tokens de microinteracción** (`:root`, no temáticos): `--ease-out-expo`, `--ease-out-quad`, `--dur-fast/base/slow`, `--rail-lift`, `--rail-zoom`, `--font-display: 'Fraunces'`.
- **Tokens por tema** (5 `data-theme`): `--bg-main/surface/surface-elevated/card/nav`, `--border-subtle/highlight`, `--text-primary/secondary/muted`, `--brand-primary/hover/accent/glow`, `--color-mustplay/recommended/notrec` (+fondo/borde).
- **Clases editoriales**: `.rail-card` (lift+glow+focus-visible+`prefers-reduced-motion`), `.rail-cover--square/wide/banner` + `.rail-cover img` (object-fit + zoom), `.rail-title` (Fraunces, solo portada hoy), `.scrollbar-none` (real), `.game-card-editorial` + `.cover-wrapper` + `.rating-badge-float`, `.badge-pill` (+`.active-pill`), `.traffic-chip` (semáforo), `.search-input`, `.filter-btn`, `.detail-hero-backdrop`, `.skip-link`.

### 3.2 Componentes reutilizables ya construidos (INC-35)

| Componente | Rol | Reutilizable en INC-36 |
|---|---|---|
| `Icon.razor` + `IconCatalog.cs` | Lucide inline, `currentColor`, `aria-hidden` por defecto, nombre desconocido → render vacío | Sí (ya usado en las 5 páginas) |
| `DefaultImage.razor` + `DefaultImageDomain` | SVG inline temable por dominio + estático para `onerror` | Sí: ampliar a News/GiveawayCard (paridad) |
| `RailHeader.razor` | Cabecera de carril (icono + h2 serif + contador + enlace) | Directo en portada; inspiración para cabeceras de página |
| `HeroEditorial.razor` + `HeroBackgroundVariant` | Hero con variantes conmutables | Solo portada (fix responsive en sitio) |
| `HomeGameCard/GiveawayCard/ReleaseCard/EventCard` | Tarjetas de carril con lenguaje `.rail-card` + fallbacks | Referencia de lenguaje para tarjetas de página |
| `CollectionActionBar` | Barra de acciones mobile-first | Ya usada en GameDetail |

### 3.3 Huecos de diseño que INC-36 debería cerrar (candidatos a componente/clase nueva)

1. **`PageHeaderEditorial`**: la cabecera "badge píldora + h1 + subtítulo + acción" está duplicada casi 1:1 en Events, Radar y News (y Catálogo tiene una variante centrada). Extraerla homogeneiza serif/espaciados y baja ~75 líneas duplicadas.
2. **`EditorialModal` (shell)**: Radar y News duplican el mismo modal (overlay `fixed inset-0` + `bg-black/60 backdrop-blur-sm` + card `max-w-lg` + cabecera con cierre + pie de acciones). Extraer el shell y dejar el cuerpo por parámetro/ChildContent.
3. **Botón primario de marca accesible**: token `--on-brand` (texto oscuro en charcoal/tabletop/midnight, blanco en editorial/wood) o ajuste de la paleta `--brand-primary` por tema; resuelve de raíz el fallo AA 3,69:1 en TODOS los botones `bg-[var(--brand-primary)] text-white` de la app.
4. **Clase `.editorial-panel`**: unifica los bloques `p-6 rounded-2xl bg-card border …` repetidos en GameDetail (semáforo, sinopsis) y vacíos de las otras páginas.
5. **Badge de urgencia/estado con tokens**: variante semántica de `.badge-pill` (urgente/activo/finalizado) para `GetRemainingBadgeClass`/countdown, con `prefers-reduced-motion` cubierto si lleva pulso.

---

## 4. Arquitectura de estilos y contratos de test (TDD estricto, sin bUnit)

### 4.1 Sistema de estilos

- `Styles/input.css` (608 líneas): `@tailwind` + 5 temas + tokens de microinteracción + clases editoriales. **Un único punto de verdad de tokens**; el rediseño debe añadir aquí cualquier token/clase nueva (`.rail-*` ya es el estándar demostrado).
- `wwwroot/app.css` (245 KB, compilado): `PerformanceAndAccessibilityTests` ya exige que contenga `container-ludeka`, `badge-pill`, `aspect-square`; un rediseño que añada clases en `input.css` exigirá regenerarlo (pipeline de build vigente) — recordar en `sdd-verify`.
- `tailwind.config.js`: `darkMode: 'class'` **incompatible con la estrategia real de temas (`data-theme` sin clase `.dark` en `<html>`, verificado en `App.razor`)** → todas las variantes `dark:` del repo son reglas muertas y el color base se aplica también en temas oscuros (causa técnica de los fallos AA de Radar/News). Safelist vigente con patrones de color.

### 4.2 Contratos de markup existentes (patrón INC-31)

- `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` (810 líneas): `TheoryData MarkupContracts` (entrada: descripción, ruta relativa, `mustContain`, `mustNotContain`) leída desde la raíz del repo con `Ludeka.sln` como ancla; método `Source_FulfillsMarkupContract` afirma contiene/no-contiene con `StringComparison.Ordinal`. **No prueba DOM**: el render real se verifica en `sdd-verify` con `dotnet run` (limitación documentada).
- Contratos relevantes que INC-36 puede topar: `HomeDashboard (orquestador editorial)`, `HeroEditorial (picture, prioridad y escena CSS)` — exige `<picture>`, AVIF/WebP, `fetchpriority="high"`, `width/height`, alt por variante, `hero-actions`, h1 sr-only "La mesa está servida" y PROHIBE `hero-text-chip|hero-scrim|hero-panel|hero-title|Catálogo Completo`; `Fundación CSS (tokens, rail-card y hero)` — congela nombres de tokens y clases en `input.css` (cualquier token nuevo debe añadirse al mustContain del contrato); `RailHeader`, `Home*Card`, `Events (sin emojis)`, `Radar (sin emojis)`, `News (sin emojis)`, `Home catalogo (sin emojis)`, `GameDetail (ficha sin emojis)` + `GameDetail (diseñador texto plano)`; fact `HeroEditorial_NoRenderizaPildorasRedundantes`; fact `PaginasEventos_TodaImagenDeEventoTieneFallbackPorDominio` (toda `<img>` de evento con `onerror` + `width`/`height`); barrido global `RestoDeLaWeb_SinEmojis…` sobre los 16 emojis.
- Comportamiento (sin bUnit, reflexión): `HeroEditorialQuickSearchTests` (FakeNavigationManager + instanciación directa) — el handler `HandleQuickSearch` no debe romperse al retocar el hero; `DefaultImageAssetsTests`, `IconCatalogTests` (lookup/URL por dominio).
- Cifra de referencia: **847 tests en verde** al cierre de INC-35 (`dotnet test Ludeka.sln`); los contratos de INC-36 deben mantener ese estado y añadir los suyos.

### 4.3 Patrón de trabajo para el rediseño

- Por cada cambio de markup: TDD estricto — primero extender/ajustar `MarkupContracts` (p. ej. `Home catalogo` pasa a exigir `.rail-title` o `PageHeaderEditorial` y prohibir `no-scrollbar`), verlo rojo, implementar, verde.
- Los invariantes vigentes que el rediseño NO debe romper: fallback de eventos (onerror+dimensiones), sin emojis, `Icon` con `aria-hidden`, alt castellano del hero, Fraunces en la misma petición de fuentes (fact de `App.razor`), `h1` sr-only del hero, enlaces "Ver todos…" vía `RailHeader`.
- Nueva cobertura sugerida para la propuesta: contrato de alturas del hero (p. ej. `input.css` debe contener `aspect-ratio` en `.hero-editorial` y dejar de contener `min-height:360px`), contrato `--on-brand` en los 5 temas, contrato de `PageHeaderEditorial` aplicado a las 3 cabeceras, contrato de imagen de News (`DefaultImageDomain.Novedad` + `onerror` + `width/height`), contrato del shell `EditorialModal`.

---

## 5. Decisiones abiertas para la propuesta (D1..Dn)

- **D1 (hero, estrategia)**: A (ratio responsiva + `object-position` + tapa `clamp`, recomendada) vs B (clamp puro) vs C (srcset + recortes autoriales). Recomendado: A ahora, C como opción evaluable.
- **D2 (hero, forma móvil)**: ratio concreta en móvil (16/9 fiel a la fuente ≈180px en 320px vs 4/3 ≈240px con algo de recorte), suelo `min-height` y cap máximo en escritorio (~460px). Interacción con el buscador superpuesto (el input necesita ~110-130px con padding).
- **D3 (a11y transversal)**: ¿entra en INC-36 el token `--on-brand` para botones de marca (arregla AA en toda la app) o solo en el botón del hero? Recomendado: token global + adopción en las 5 páginas.
- **D4 (cabecera editorial de página)**: extraer `PageHeaderEditorial` (badge + h1 + subtítulo + acción) y aplicarla a Catálogo/Eventos/Sorteos/Novedades (¿también GameDetail?).
- **D5 (serif display)**: ¿extender `--font-display` a los h1 de página del listado (hoy solo portada)? En GameDetail, ¿el título del juego va en serif o en sans black? Recomendado: serif en cabeceras de listado; decidir ficha.
- **D6 (unificación de tarjetas)**: ¿migrar tarjetas de Events/News/GiveawayCard al lenguaje `.rail-card` (tokens + focus-visible + reduced-motion) manteniendo `.game-card-editorial` en Catálogo (o migrar también)?
- **D7 (back-bar de GameDetail)**: tratamiento móvil de las 8-9 acciones de moderador (desbordan sin `flex-wrap`): `flex-wrap` simple, scroll horizontal oculto, o menú "más acciones". Recomendado: wrap + agrupación de acciones de moderación.
- **D8 (tokens semánticos de estado)**: ¿sustituir los hardcodeados (amber/indigo/purple/rose/sky/slate) por tokens semánticos por tema (error/warning/info/highlight) en las 5 páginas, o solo donde falla el contraste? Recomendado: solo las 5 páginas de este incremento.
- **D9 (variantes `dark:` inertes)**: ¿eliminar `dark:*` de las 5 páginas y sustituir por tokens (recomendado), o habilitar `darkMode` por `data-theme` (cambio de config global, más riesgo)? Recomendado: primera opción, acotada.
- **D10 (carátula en News)**: ¿renderizar siempre la zona de imagen con `DefaultImage` (paridad con `HomeReleaseCard`), añadir `onerror` y `width/height`? Recomendado: sí.
- **D11 (modales)**: ¿extraer shell `EditorialModal` compartido para Radar/News? Recomendado: sí (ahorra ~100-140 líneas y unifica cierre/aria/foco).
- **D12 (filtros)**: ¿unificar píldoras (`.filter-btn` vs `.badge-pill`), sticky en móvil y fix de la clase muerta `no-scrollbar` → `scrollbar-none`? Recomendado: fix de clase muerta sí (trivial); sticky y unificación, decidiendo por página.
- **D13 (alcance)**: MyLibrary, directorios (editoriales/creadores/tiendas), admin y el resto de páginas quedan FUERA de INC-36 (recomendado); skeletons/streaming fuera; reemplazo de assets de Unsplash en eventos fuera.

---

## 6. Riesgos y sugerencias de alcance

### 6.1 Riesgos

- **[Alta] Regresión de contratos INC-35**: tocar `HeroEditorial.razor`/`input.css`/las 5 páginas puede romper contratos vigentes (`Fundación CSS`, `HeroEditorial (picture…)`, `PaginasEventos_TodaImagen…`, barrido anti-emoji). Mitigación: actualizar los contratos en el MISMO commit que el cambio (TDD estricto) y mantener los invariantes de comportamiento (buscador del hero, alt castellano, Fraunces en 1 petición).
- **[Alta] Presupuesto de revisión (400 líneas/PR)**: 5 páginas + hero + tokens + componentes nuevos superará el presupuesto casi con certeza → plan de PRs apilados desde el mismo worktree (patrón INC-35: PR-1 fundación de tokens/componentes compartidos → PR-2..n páginas en pares). `sdd-tasks` debe predecir el presupuesto explícitamente.
- **[Media] Cambio de identidad visual del botón primario** (texto oscuro sobre terracota): altera el look general — requiere validación visual del maintainer antes de aplicar (por eso D3 va a propuesta).
- **[Media] A11y pestañas de Eventos** (sin `tabpanel`/`aria-controls`/gestión de foco): el rediseño puede arreglarlo de paso o ampliarlo a un problema separado; no dejarlo a medias en el contrato.
- **[Media] Estrategia C del hero** introduce dependencia de build (recortes por variante): solo si D1 la elige, ampliar `scripts/convert-hero-images.mjs` y los contratos de assets.
- **[Baja] Correcciones triviales con riesgo de conflicto de merge**: `no-scrollbar`→`scrollbar-none` (Home.razor), typo "Ludeca" (GameDetail), `dark:*` inertes — tocarlas en las mismas líneas que el rediseño para no duplicar ediciones.
- **[Baja] 5 temas × nuevos tokens**: cualquier token nuevo (estado, `--on-brand`) debe probarse en los 5 `data-theme` (verificación runtime en `sdd-verify`).

### 6.2 Sugerencia de alcance

**DENTRO de INC-36** (recomendado): fix responsive del hero (estrategia A + foco por variante) y contraste AA del buscador (token `--on-brand`); rediseño editorial de las 5 páginas con los tokens/componentes existentes; `PageHeaderEditorial` compartido; unificación `.rail-card` en tarjetas de Events/News/GiveawayCard; fix de clases muertas (`no-scrollbar`) y del typo PageTitle; invariantes a11y (targets ≥24px, `aria-hidden` en iconos, focus-visible); contratos de markup nuevos/actualizados por página.

**FUERA de INC-36** (proponer explícitamente como fuera de alcance): MyLibrary, directorios y páginas admin; streaming SSR/esqueletos de carga; `srcset` responsive del hero (salvo que D1 elija C); reemplazo de assets hotlink de Unsplash; habilitar `darkMode` por `data-theme` global; cambios de datos/DTOs/servicios (ningún cambio de dominio es necesario: todo es presentación).

## Ready for Proposal

**Sí.** La propuesta debe cerrar D1–D13, fijar el plan de PRs apilados y ajustar los contratos INC-31/35 que el rediseño toca. El fix del hero tiene causa raíz técnica clara y una estrategia recomendada de esfuerzo bajo: listo para `sdd-propose`.
