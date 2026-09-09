# Tasks: Portada Editorial (INC-35)

> Fase SDD `sdd-tasks`. Idioma: español castellano (regla suprema AGENTS.md). Store: hybrid — este archivo + Engram `sdd/portada-editorial/tasks`.
> Entradas: `proposal.md` (D1–D5 cerradas), las 4 specs delta en `specs/`, `design.md` (10 decisiones), `explore.md`.
> Worktree `C:\repos\ludeka-wt\portada-editorial`. Modo **TDD estricto** activo: cada unidad de código nueva sigue RED (test que falla) → GREEN (implementación mínima) → commit conjunto. Runner: `dotnet test Ludeka.sln`.

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ≈ 950–1.250 autoradas (excluye binarios: fotos del hero y SVG) |
| 400-line budget risk | High — mitigado por auto-chain (3 PRs encadenados decididos por el usuario) |
| Chained PRs recommended | Yes |
| Suggested split | PR-1 Fundación editorial (~300-350) → PR-2 Portada narrativa (~350-420) → PR-3 Lucide global (~300-600; corte condicional PR-3a/PR-3b) |
| Delivery strategy | auto-chain |
| Chain strategy | stacked-to-main |

```text
Decision needed before apply: No
Chained PRs recommended: Yes
Chain strategy: stacked-to-main
400-line budget risk: High
```

### Suggested Work Units

| Unit | Goal | Likely PR | Focused test command | Runtime harness | Rollback boundary |
|------|------|-----------|----------------------|-----------------|-------------------|
| 1 | Fundación editorial: tokens CSS, `.rail-card`, `Icon.razor`, `DefaultImage.razor`, assets por defecto, serif Fraunces | PR-1 | `dotnet test tests/Ludeka.UnitTests/Ludeka.UnitTests.csproj --filter "FullyQualifiedName~WebMarkupContractTests|IconCatalogTests|DefaultImageAssetsTests"` | `dotnet run --project src/Ludeka.Web` → GET `/images/defaults/{evento,sorteo,novedad,generico}-default.svg` = 200 | Revert del PR-1: todo aditivo, sin consumidores en páginas |
| 2 | Conversión de hero: `scripts/convert-hero-images.mjs` + AVIF/WebP/JPG <200 KB | PR-1 | N/A — tooling de desarrollo manual, fuera del límite de confianza (Threat Matrix del diseño) | `node scripts/convert-hero-images.mjs` + verificación de pesos impresa | Revert del PR-1 elimina script y assets generados |
| 3 | Hero editorial con variantes de fondo (D1) | PR-2 | `dotnet test tests/Ludeka.UnitTests/Ludeka.UnitTests.csproj --filter "FullyQualifiedName~HeroEditorial"` | `dotnet run` → `/` renderiza titular serif, subtítulo, buscador, 4 píldoras y fondo según variante | Revert del PR-2 (el hero deja de existir; la portada anterior se restaura con el mismo PR) |
| 4 | Carriles por componentes + microinteracciones + fallbacks (D2/D5 spec) | PR-2 | `dotnet test tests/Ludeka.UnitTests/Ludeka.UnitTests.csproj --filter "FullyQualifiedName~(RailHeader|HomeGameCard|HomeGiveawayCard|HomeReleaseCard|HomeEventCard)"` | `dotnet run` → `/` con seed actual: 4 carriles, hover/foco, sin `<img>` roto | Revert del PR-2 |
| 5 | Fix D5: fallback de imagen en `Events.razor` / `EventsManagement.razor` | PR-3 (inclusión dura, cabeza del PR) | `dotnet test tests/Ludeka.UnitTests/Ludeka.UnitTests.csproj --filter "FullyQualifiedName~WebMarkupContractTests"` | `dotnet run` → `/eventos` con evento sin imagen muestra el default de eventos | Revert de 1 commit (2 archivos) |
| 6 | Migración emojis → Lucide del resto de la web (D3) por fases de la Decisión 6 | PR-3 (± split PR-3a/3b) | `dotnet test tests/Ludeka.UnitTests/Ludeka.UnitTests.csproj --filter "FullyQualifiedName~WebMarkupContractTests"` | `rg -n "🔍|🎲|🎁|📰|🎪|🏆|⭐|⏱|🚀|🔄|🆕|🗓|📅|📍|🌐|🧩" src/Ludeka.Web/Components` sin resultados + smoke visual | Revert por fase (1 commit atómico por fase) |

### Estrategia de cadena (stacked-to-main, regla 1-bis del AGENTS.md)

1. **PR-1**: rama `inc/portada-editorial` (actual) → `main`.
2. **PR-2**: rama `inc/portada-editorial-2` creada desde `inc/portada-editorial` → `main`.
3. **PR-3**: rama `inc/portada-editorial-3` creada desde `inc/portada-editorial-2` → `main`. Si supera el presupuesto, se corta en PR-3a (`inc/portada-editorial-3`) + PR-3b (`inc/portada-editorial-4`) — criterio objetivo en la tarea 3.7.
4. Merges estrictamente en orden (1 → 2 → 3). Cada PR debe mostrar solo su work unit; si un diff hijo arrastra cambios del padre, rebase/retarget antes de revisar.

### Convenciones transversales (obligatorias en los 3 PRs)

- **Commits por unidad de trabajo** (skill `work-unit-commits`): cada tarea con marcador `Commit K#` = 1 commit convencional que incluye test + implementación juntos (los tests viajan con el código que verifican). El ciclo RED→GREEN se ejecuta en local ANTES de commitear: la suite debe estar verde en cada commit (bisectable).
- **Contratos de markup (patrón INC-31)**: extender la TheoryData `MarkupContracts` de `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` o añadir tests dedicados que lean la fuente real desde la raíz del repo. Sin bUnit: comportamiento por instanciación directa + reflexión (patrón `HomeDashboardQuickSearchTests`).
- **Runner y gotcha conocido**: `dotnet test Ludeka.sln` falla con **MSB3027** si `Ludeka.Web` está en ejecución (bin bloqueado). En ese caso: `dotnet build Ludeka.sln; dotnet test Ludeka.sln --no-build`. Baseline INC-31: **705/705** en verde; el recuento crece con los tests nuevos de este incremento — documentar el recuento exacto en cada PR.
- **Presupuesto 400 líneas**: `additions + deletions` autoradas vía `git diff --stat <rama-base>..HEAD`. Nunca comprimir código, comentarios ni tests para caber (skill `chained-pr`); si un corte honesto no cabe, reportar overage y recomendar `size:exception`.
- Las specs delta en `openspec/changes/portada-editorial/specs/` son **entrada de verificación**, no se editan en ningún PR (solo `sdd-archive` las sincroniza).

---

## PR-1 — Fundación editorial (~300-350 líneas autoradas)

**Rama**: `inc/portada-editorial` (base `main`). **Objetivo**: fundación aditiva de identidad editorial — tokens de microinteracción, `.rail-card`, serif Fraunces, `Icon.razor`, `DefaultImage.razor`, assets por defecto y conversión del hero. **Cero toques en `Components/Pages/`** → riesgo de regresión cero.
**Rollback boundary**: revertir el PR-1 completo; nada del resto de la app depende de estos archivos todavía.

- [x] 1.1 Commitear los artefactos SDD del incremento como primer commit de la rama. **Archivos**: `openspec/changes/portada-editorial/**` (proposal, design, explore, tasks, 4 specs delta), `docs/increments/ROADMAP.md` (INC-35 ya en ⏳ En progreso). **Criterio**: `git status` sin artefactos pendientes; el PR-1 abre con el contexto SDD versionado. **Commit K1**: `docs: artefactos SDD del incremento portada-editorial (INC-35)`.

- [x] 1.2 **[RED]** Contratos de `Icon.razor` + `IconCatalog`. **Archivos**: `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` (nueva entrada para `src/Ludeka.Web/Components/Shared/Icon.razor`: debe contener `viewBox="0 0 24 24"`, `stroke="currentColor"`, `aria-hidden="true"`; no debe contener `<img` ni `http`), nuevo `tests/Ludeka.UnitTests/Web/IconCatalogTests.cs` (por reflexión sobre `IconCatalog`: contiene los 16 nombres de portada — search, dices, gift, newspaper, tent, trophy, star, timer, rocket, refresh-cw, sparkles, calendar-days, calendar, map-pin, globe, puzzle —; lookup de nombre desconocido devuelve vacío sin lanzar). **Criterio RED**: compila y falla porque los archivos no existen. **Commit K2** (junto a 1.3).

- [x] 1.3 **[GREEN]** Crear `src/Ludeka.Web/Components/Shared/Icon.razor` y `src/Ludeka.Web/Components/Shared/IconCatalog.cs` (Decisión 6: `Name` `[Parameter, EditorRequired]` kebab-case, `Size`=16, `StrokeWidth`=2, `Title` null → `aria-hidden="true"`; nombre fuera de catálogo → sin salida y sin error; diccionario whitelist nombre→paths Lucide, sin red). **Criterio**: escenarios «Icono decorativo hereda color» e «Icono desconocido» de `openspec/changes/portada-editorial/specs/iconography-lucide/spec.md` (read-only) cubiertos por los tests de 1.2 en verde.

- [x] 1.4 **[RED]** Contratos de `DefaultImage.razor` + `DefaultImageDomain`. **Archivos**: `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` (entrada para `src/Ludeka.Web/Components/Shared/DefaultImage.razor`: contiene `viewBox="0 0 400 225"`, rellenos con `var(--brand-`/`var(--bg-`, `aria-hidden` condicionado a `Alt` null) + nuevo `tests/Ludeka.UnitTests/Web/DefaultImageAssetsTests.cs` (`DefaultImageAssets.Url(Domain)` devuelve `/images/defaults/evento-default.svg`, `sorteo-default.svg`, `novedad-default.svg`, `generico-default.svg` según dominio; nunca null; valor no contemplado → `Generico`). **Criterio RED**: en rojo. **Commit K3** (junto a 1.5).

- [x] 1.5 **[GREEN]** Crear `src/Ludeka.Web/Components/Shared/DefaultImageDomain.cs` (enum `Evento/Sorteo/Novedad/Generico` + estática `DefaultImageAssets.Url()`) y `src/Ludeka.Web/Components/Shared/DefaultImage.razor` (SVG inline temable, motivos por dominio: carrusel/entoldado evento, caja con lazo sorteo, etiqueta con destello novedad, isotipo dado genérico; dominio sin variante → genérico sin fallar ni quedar en blanco). **Criterio**: escenarios «Inline temable en los 5 temas» y «Dominio sin variante soportada» de `openspec/changes/portada-editorial/specs/default-image-fallbacks/spec.md` (read-only).

- [x] 1.6 **[GREEN]** Crear los 4 SVG estáticos `src/Ludeka.Web/wwwroot/images/defaults/{evento,sorteo,novedad,generico}-default.svg` (Decisión 8: viewBox 400×225, gradiente `#1B2228→#242C34`, acento `#E05A38`, texto `#9AB0C2`, Plus Jakarta Sans, microtextos «EVENTO LUDEKA»/«SORTEO LUDEKA»/«NOVEDAD»/«LUDEKA»; paridad visual 1:1 con los inline de 1.5). **Contrato**: test que afirma la existencia de los 4 archivos y sus microtextos (misma técnica `ReadSource` sobre wwwroot). **Criterio**: requisito «Assets por defecto de Ludeka por dominio» + escenario «Assets servidos» de `specs/default-image-fallbacks/spec.md` (read-only). **Commit K4**.

- [x] 1.7 **[RED]** Contrato CSS de la fundación. **Archivos**: `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` (entrada para `src/Ludeka.Web/Styles/input.css`: debe contener `--ease-out-expo`, `--ease-out-quad`, `--dur-fast`, `--dur-base`, `--dur-slow`, `--rail-lift`, `--rail-zoom`, `--font-display: 'Fraunces'`, `.rail-card:hover, .rail-card:focus-visible`, `.rail-cover--square`, `.rail-cover--wide`, `.rail-cover--banner`, `.scrollbar-none` definida, `prefers-reduced-motion: reduce`). **Criterio RED**: en rojo contra el CSS actual. **Commit K5** (junto a 1.8).

- [x] 1.8 **[GREEN]** Editar `src/Ludeka.Web/Styles/input.css`: bloque de tokens en `:root` + `.rail-card` (lift, borde `--brand-primary`, glow `--brand-glow`, outline de foco 2 px, zoom de imagen en `.rail-cover img`) + `.rail-cover(--square/wide/banner)` + `.scrollbar-none` real con `::-webkit-scrollbar` + `@media (prefers-reduced-motion: reduce)` (Decisión 4, literal del diseño). **Criterio**: parte CSS de los escenarios «Hover en tarjeta de carril», «Foco de teclado equivalente», «Movimiento reducido» y «Scroll horizontal sin scrollbar visible» de `specs/home-dashboard-rails/spec.md` (read-only).

- [ ] 1.9 **[GREEN]** Editar `src/Ludeka.Web/Components/App.razor`: añadir `family=Fraunces:opsz,wght@9..144,600..700` a la URL existente de Google Fonts (misma petición, `display=swap` intacto, sin preload — Decisión 7). **Contrato**: entrada `WebMarkupContractTests` (contiene `Fraunces`; no se añade ningún `<link>` de fuente nuevo). **Criterio**: parte de carga del escenario «Títulos de carril en serif display» de `specs/home-dashboard-rails/spec.md` (read-only). **Commit K6**.

- [ ] 1.10 Crear `scripts/convert-hero-images.mjs` (sharp vía `npm i --no-save sharp` en temp; redimensiona a 1600 px; AVIF q≈50, WebP q≈72, JPEG q≈75; imprime el peso de cada salida y avisa si supera 200 KB). **Sin test unitario** (tooling manual; Threat Matrix del diseño lo declara fuera del límite de confianza). **Commit K7** (junto a 1.11).

- [ ] 1.11 Ejecutar `node scripts/convert-hero-images.mjs` sobre las 3 fotos de `src/Ludeka.Web/wwwroot/images/home/` (`hero-ambiente-eurogame`, `hero-ambiente-mesa-amigos`, `hero-ambiente-primer-plano`): genera `.avif`/`.webp` y el JPEG de fallback recomprimido por foto, con la convención de nombres de la Decisión 2. **Verificación de pesos**: salida del script + `Get-ChildItem src/Ludeka.Web/wwwroot/images/home` — cada archivo servible < 200 KB. **Criterio**: requisito «Rendimiento del hero con imagen» (presupuesto de peso) de `specs/home-landing-hero/spec.md` (read-only). Commitear los assets generados.

- [ ] 1.12 Verificación y apertura de PR-1. **Checklist del PR**: (a) `dotnet build Ludeka.sln; dotnet test Ludeka.sln --no-build` en verde (recuento = 705 + tests nuevos de K2/K3/K4/K6/K7, documentarlo); (b) `dotnet run --project src/Ludeka.Web` → los 4 SVG de defaults responden 200; (c) `git diff --stat main..HEAD` ≤ ~400 líneas autoradas; (d) `git diff main..HEAD -- src/Ludeka.Web/Components/Pages` vacío. Push y PR a `main` (título `feat: fundación editorial`). **Commit K8** si la verificación requiere ajustes.

## PR-2 — Portada narrativa (~350-420 líneas autoradas)

**Rama**: `inc/portada-editorial-2` (apilada sobre PR-1). **Objetivo**: hero editorial con variantes de fondo conmutables (D1), 4 carriles extraídos a `Components/Home/` con imagen + fallback + microinteracciones (D2), serif en h1/h2, spinner con `Icon`, cero emojis en la portada y muerte de las clases muertas.
**Rollback boundary**: revertir el PR-2 restaura la portada actual; PR-1 no se ve afectado.
**Orden de commits**: C1 (hero) → C2 (componentes de carril) → C3 (reescritura del orquestador). Cada commit verde.

- [ ] 2.1 **[RED]** Contratos del hero + migración parcial de contratos legacy. **Archivos**: `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` — (a) nuevas entradas para `src/Ludeka.Web/Components/Home/HeroEditorial.razor` (debe contener `<picture>`, `<source type="image/avif">`, `<source type="image/webp">`, `fetchpriority="high"`, `width="1600"`, `height="900"`, `alt="` no vacío, `hero-scrim`, `@switch` sobre `Background`; NO «PORTADA EDITORIAL»; en la rama del `@switch` de `CssScene` no hay `<picture>` — troceado de fuente como el test de píldoras) y `src/Ludeka.Web/Components/Home/HeroBackgroundVariant.cs` (por reflexión: existen las 5 variantes); (b) `src/Ludeka.Web/Styles/input.css` debe contener `.hero-title`, `.rail-title`, `.hero-scrim`. **Criterio RED**: en rojo. **Commit C1** (junto a 2.2).

- [ ] 2.2 **[GREEN]** Crear `src/Ludeka.Web/Components/Home/HeroBackgroundVariant.cs` (enum `FotoEurogame/FotoMesaAmigos/FotoPrimerPlano/CssScene/Ilustracion` + alt castellano por variante según Decisión 10) y `src/Ludeka.Web/Components/Home/HeroEditorial.razor` (Decisión 1: h1 «La mesa está servida» + subtítulo de 2 frases; buscador y 4 píldoras internos vía `NavigationManager`; `<picture>` AVIF/WebP/JPG con `fetchpriority="high"`, dimensiones fijas y alt; capa de escena CSS SIEMPRE bajo el `<picture>`; `onerror` del `<img>` añade `hero-bg-media--failed` al `<picture>`; en `CssScene` el `<picture>` no se renderiza) + añadir `.hero-title`, `.rail-title`, `.hero-scrim` a `input.css` (Decisiones 7 y 10). **Criterio**: escenarios «Portada renderiza hero editorial», «Único h1 visible con serif display», «Hero renderiza la variante configurada», «Escena CSS sin peticiones de imagen», «Cambio de variante sin tocar estructura», «Imagen del hero con <picture> y prioridad» y «Alt descriptivo de la foto de ambiente» de `specs/home-landing-hero/spec.md` (read-only). El componente aún sin consumidor: la portada actual sigue verde.

- [ ] 2.3 **[RED+GREEN]** Crear `src/Ludeka.Web/Components/Home/RailHeader.razor` (props `IconName`, `Title`, `Count`, `CountLabel`, `Href` nullable — Eventos no tiene —, `LinkText`, `AriaLabel`; `<h2>` con `rail-title`; icono vía `Icon`; enlace «Ver todos…» solo si `Href`). **Contrato**: entrada `WebMarkupContractTests` (contiene `rail-title`, `Icon`); retarget del test «Ver todas las novedades» hacia `RailHeader.razor` con `Href="/novedades"`. **Criterio**: paridad de cabeceras del escenario «Paridad de comportamiento tras la extracción» de `specs/home-dashboard-rails/spec.md` (read-only). **Commit C2**.

- [ ] 2.4 **[RED+GREEN]** Crear `src/Ludeka.Web/Components/Home/HomeGameCard.razor` (`[Parameter, EditorRequired] GameSummaryDto Game`; conserva el fallback vigente `game-placeholder.svg`/`expansion-placeholder.svg` con `onerror`; `.rail-card` + `.rail-cover--square`; badges con `Icon` sin emoji). **Contrato**: entrada (contiene `rail-card`, `rail-cover--square`, `game-placeholder.svg`, sin emojis). **Criterio**: paridad Top 20 de «Paridad de comportamiento tras la extracción» de `specs/home-dashboard-rails/spec.md` (read-only). **Commit C2**.

- [ ] 2.5 **[RED+GREEN]** Crear `src/Ludeka.Web/Components/Home/HomeGiveawayCard.razor` (`GiveawayDto Giveaway`; estrena render de `ThumbnailUrl`: presente → `<img loading="lazy" decoding="async" onerror="this.onerror=null; this.src='/images/defaults/sorteo-default.svg'">`; nula/vacía → `DefaultImage` Domain=Sorteo inline; `.rail-cover--wide`). **Contrato**: entrada (contiene `sorteo-default.svg`, `rail-cover--wide`, `loading="lazy"`). **Criterio**: escenarios «Card de sorteo con y sin imagen» de `specs/home-dashboard-rails/spec.md` (read-only) y «Sorteo sin imagen» de `specs/default-image-fallbacks/spec.md` (read-only). **Commit C2**.

- [ ] 2.6 **[RED+GREEN]** Crear `src/Ludeka.Web/Components/Home/HomeReleaseCard.razor` (`WeeklyReleaseDto Release`; estrena render de `CoverImageUrl` con fallback Novedad (mismo patrón que 2.5); badge de reimpresión con `Icon` `refresh-cw` + texto visible; `.rail-cover--wide`). **Contrato**: entrada (contiene `novedad-default.svg`, `rail-cover--wide`). **Criterio**: escenario «Novedad y evento con imagen ausente o caída» (parte novedad) de `specs/home-dashboard-rails/spec.md` (read-only). **Commit C2**.

- [ ] 2.7 **[RED+GREEN]** Crear `src/Ludeka.Web/Components/Home/HomeEventCard.razor` (`BoardGameEventDto Event`; `ImageUrl` vacío → `DefaultImage` Domain=Evento; presente → `<img>` con `width`/`height`, `loading="lazy"`, `decoding="async"` y `onerror` → `evento-default.svg`; `.rail-cover--banner`). **Contrato**: entrada (contiene `evento-default.svg`, `rail-cover--banner`, `onerror`). **Criterio**: escenario «Novedad y evento con imagen ausente o caída» (parte evento) de `specs/home-dashboard-rails/spec.md` (read-only) y «URL externa caída» de `specs/default-image-fallbacks/spec.md` (read-only). **Commit C2**.

- [ ] 2.8 **[RED+GREEN]** Reescribir `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` como orquestador y migrar los contratos legacy restantes en el mismo commit: `<HeroEditorial Background="HeroBackgroundVariant.FotoEurogame" />` (variante activa en 1 línea), 4 secciones con `RailHeader` + bucle de su card, spinner de carga con `Icon` `dices` (muere el emoji 🎲), eliminación de `sm:w-68` y de todo el markup duplicado. **Test**: en `WebMarkupContractTests` reescribir la entrada `HomeDashboard (hero minimalista)` → `HomeDashboard (orquestador editorial)` (debe contener `<HeroEditorial` y los 4 `<Home…Card`; NO emojis de la lista de 16, NO `sm:w-68`, NO markup inline de card — sin `BggRating`/`RemainingTimeText` inline); retarget del test de las 4 píldoras exactas en orden al bloque de píldoras de `HeroEditorial.razor`; sustituir `tests/Ludeka.UnitTests/Web/HomeDashboardQuickSearchTests.cs` por `HeroEditorialQuickSearchTests.cs` (mismo escenario por reflexión: submit con «azul» → `/catalogo?q=azul`; vacío → `/catalogo`). **Criterio**: escenarios «Cada carril usa su componente», «Paridad de comportamiento…», «Hover en tarjeta de carril», «Foco de teclado equivalente», «Anchura definida en tarjetas de Novedades» (sin `sm:w-68`), «Títulos de carril en serif display» de `specs/home-dashboard-rails/spec.md` (read-only) y «Portada sin emojis» de `specs/iconography-lucide/spec.md` (read-only). **Commit C3**.

- [ ] 2.9 Verificación y apertura de PR-2. **Checklist del PR**: (a) `dotnet build Ludeka.sln; dotnet test Ludeka.sln --no-build` en verde (documentar recuento); (b) `dotnet run` → `/` renderiza hero narrativo, 4 carriles por componentes, hover/foco, sin emojis, sin `<img>` roto con el seed actual; (c) revisar visualmente al menos `data-theme="charcoal"` y `data-theme="editorial"` (inline temable + scrim); (d) `git diff --stat inc/portada-editorial..HEAD` ≤ ~420 autoradas. Push y PR a `main` (título `feat: portada narrativa`). **Commit C4** si la verificación requiere ajustes.

## PR-3 — Iconografía Lucide global (~300-600 líneas; corte condicional PR-3a/PR-3b)

**Rama**: `inc/portada-editorial-3` (apilada sobre PR-2). **Objetivo**: fixes D5 como **inclusión dura en cabeza** del PR (no dependen de ningún condicionante de PR-2) y migración emojis→Lucide del resto de la web por las fases de la Decisión 6 del diseño, con 1 commit atómico por fase.
**Rollback boundary**: D5 = revert de 1 commit (2 archivos); la migración = revert por fase (1 commit/fase).

- [ ] 3.1 **[RED+GREEN] Fix D5** (primera unidad del PR, sin condiciones). **Test**: entradas en `WebMarkupContractTests` para `src/Ludeka.Web/Components/Pages/Events.razor` y `src/Ludeka.Web/Components/Pages/EventsManagement.razor` (la zona de imagen de evento debe contener `onerror` con `/images/defaults/evento-default.svg` o `DefaultImage` Domain=Evento; no debe existir `<img>` de evento sin fallback). **Implementación**: aplicar el fix (~30-50 líneas) reutilizando `DefaultImage.razor` + patrón `onerror` vigente. **Criterio**: escenario «Página de eventos con evento sin imagen» de `specs/default-image-fallbacks/spec.md` (read-only). **Commit L1**: `fix: fallback de imagen por defecto en páginas de eventos (D5)`.

- [ ] 3.2 Inventario de emojis (análisis, sin commit de código): `rg -n "🔍|🎲|🎁|📰|🎪|🏆|⭐|⏱|🚀|🔄|🆕|🗓|📅|📍|🌐|🧩" src/Ludeka.Web/Components` + barrido de emojis fuera de esa lista (p. ej. ✍️ en `MainLayout.razor`); producir el mapa emoji → nombre Lucide por componente y la lista de paths nuevos para `src/Ludeka.Web/Components/Shared/IconCatalog.cs`. El mapa se documenta en la descripción del PR-3.

- [ ] 3.3 **Fase 2 — layout global, catálogo y fichas**: migrar los emojis de `src/Ludeka.Web/Components/Layout/MainLayout.razor`, `src/Ludeka.Web/Components/Shared/GameCard.razor`, `src/Ludeka.Web/Components/Pages/GameDetail.razor`, `src/Ludeka.Web/Components/Pages/CreatorsDirectory.razor`, `src/Ludeka.Web/Components/Pages/CreatorDetail.razor`, `src/Ludeka.Web/Components/Pages/Home.razor` (`/catalogo`) y `src/Ludeka.Web/Components/Shared/StoreOffersCard.razor` (según inventario) a `Icon.razor`; añadir sus paths a `IconCatalog.cs`; donde el emoji era señal semántica, el texto visible o `aria-label` ya existente queda junto al icono `aria-hidden`. **Test**: por archivo migrado, entrada `MarkupContracts` con `mustNotContain` de sus emojis (actualizar la entrada MainLayout que hoy exige «✍️ Creadores»). **Criterio**: parte de la fase del escenario «Resto de la web sin emojis» de `specs/iconography-lucide/spec.md` (read-only). **Commit L2**.

- [ ] 3.4 **Fase 3 — ludoteca**: migrar los emojis de `src/Ludeka.Web/Components/Pages/MyLibrary.razor` y componentes de préstamos del inventario; mismo patrón test+`IconCatalog`. **Criterio**: continuidad del escenario «Resto de la web sin emojis» (read-only). **Commit L3**.

- [ ] 3.5 **Fase 4 — eventos, sorteos y novedades**: migrar los emojis restantes de `src/Ludeka.Web/Components/Pages/Events.razor`, `src/Ludeka.Web/Components/Pages/EventsManagement.razor`, `src/Ludeka.Web/Components/Shared/GiveawayCard.razor`, `src/Ludeka.Web/Components/Pages/Radar.razor` y afines del inventario. **Criterio**: ídem. **Commit L4**.

- [ ] 3.6 **Fase 5 — admin y modales de gestión**: migrar los emojis de las páginas admin y modales (`src/Ludeka.Web/Components/Shared/UserPermissionsModal.razor`, `src/Ludeka.Web/Components/Shared/CreatorEditModal.razor`, páginas de admin del inventario). **Criterio**: cierre del escenario «Resto de la web sin emojis» de `specs/iconography-lucide/spec.md` (read-only). **Commit L5**.

- [ ] 3.7 **Criterio de corte PR-3 — evaluar ANTES de abrir el PR**: medir `git diff --stat inc/portada-editorial-2..HEAD` (adiciones+eliminaciones autoradas). Si ≤ 400 → un único PR-3 con L1–L5. Si > 400 → **PR-3a** = L1 (D5) + fases 2 y 3 en `inc/portada-editorial-3`, y **PR-3b** = fases 4 y 5 en `inc/portada-editorial-4` (apilada sobre `-3`); el corte cae siempre en un límite de fase porque cada fase es 1 commit atómico. Si tras este único corte honesto algún PR siguiera > 400, NO comprimir código: reportar el overage y recomendar `size:exception` al usuario (regla `chained-pr`). **Verificación del PR**: (a) `dotnet build Ludeka.sln; dotnet test Ludeka.sln --no-build` en verde; (b) `rg -n "🔍|🎲|🎁|📰|🎪|🏆|⭐|⏱|🚀|🔄|🆕|🗓|📅|📍|🌐|🧩" src/Ludeka.Web/Components` sin resultados; (c) smoke visual de `/catalogo`, `/eventos` y `/mi-biblioteca`. Push y PR(s) a `main` en orden (títulos `refactor: iconografía Lucide global`).

## Cierre del incremento (transversal)

- [ ] 4.1 Suite completa en verde sobre la cabeza de la cadena final: `dotnet build Ludeka.sln; dotnet test Ludeka.sln --no-build` (≥ 705 baseline + todos los tests nuevos). Registrar el recuento exacto en el PR de la cola de la cadena.
- [ ] 4.2 Confirmar que `docs/increments/ROADMAP.md` mantiene INC-35 en ⏳ En progreso durante los PRs (su actualización a ✅ y el volcado a `docs/specs/sistema/` corresponden a `sdd-archive`, no a estos PRs).
- [ ] 4.3 Handoff a `sdd-verify`: verificación de integración contra las 4 specs delta y el checklist de éxito de la propuesta (LCP < 2,5 s y CLS 0 de portada con foto, defaults en los 3 carriles, 0 emojis en portada, `dotnet test Ludeka.sln` en verde).
