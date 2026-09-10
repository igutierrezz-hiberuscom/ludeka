# Tareas: Rediseño Editorial del Resto de Páginas + Fix Responsive del Hero (INC-36)

> Fase SDD `sdd-tasks`. Idioma: español castellano (regla suprema AGENTS.md). Store: hybrid (este archivo + Engram `sdd/rediseno-paginas-editoriales/tasks`).
> Entradas: `proposal.md` (aprobada, commit 0f13f41), `specs/editorial-page-foundations/spec.md`, `specs/home-landing-hero/spec.md`, `specs/default-image-fallbacks/spec.md`, `design.md` (DD-01..DD-11).
> Worktree único: `C:\repos\ludeka-wt\rediseno-paginas-editoriales`, rama `inc/rediseno-paginas-editoriales`. Runner: `dotnet test Ludeka.sln` (baseline 847 en verde). TDD estricto (patrón INC-31, sin bUnit): cada tarea empieza por el contrato (rojo) y termina en verde; contrato + implementación en el MISMO commit convencional, sin atribuciones de IA. Las líneas de referencia («línea N») son las verificadas en `design.md`.

## Review Workload Forecast

| PR | Contenido (DD-11) | Líneas estimadas |
|---|---|---|
| PR-1 «Fundación» | tokens + fix hero + 2 componentes compartidos + app.css + contratos | ~380-450 |
| PR-2 «Catálogo» | `Home.razor` + contratos | ~90-120 |
| PR-3 «Ficha» | `GameDetail.razor` + contratos | ~120-160 |
| PR-4 «Eventos» | `Events.razor` + contratos | ~140-180 |
| PR-5 «Sorteos + Novedades» | `Radar.razor` + `News.razor` + `GiveawayCard.razor` + contratos | ~250-350 |
| **Total** | | **~980-1260** |

Decision needed before apply: No
Chained PRs recommended: Yes
Chain strategy: feature-branch-chain
400-line budget risk: High

- Estrategia ya fijada (auto-chain, sin reabras): PR-1 base = `main`; base del PR-n (n≥2) = rama del PR-(n-1), cada PR encima del anterior desde el MISMO worktree (patrón INC-35). `scripts/sdd-worktree.ps1` (read-only) se usa tal cual, sin modificarlo.
- Guarda de presupuesto: si un PR supera 400 líneas al aplicar, se parte automáticamente (AGENTS.md 1-bis.7). Corte natural de PR-1: tokens+hero vs componentes compartidos.
- El `app.css` compilado cuenta como 1-2 líneas de diff minificado.

### Unidades de trabajo

| Unidad | Objetivo | PR | Test focal | Runtime harness | Rollback |
|---|---|---|---|---|---|
| U1 | Fundación tokens+hero+componentes+app.css | PR-1 | `dotnet test Ludeka.sln --filter DisplayName~"Fundación CSS"` + `--filter HeroEditorialQuickSearchTests` | `dotnet run` + smoke `/` × 5 temas `?theme=` y variantes `?hero=` | revert = tokens aditivos fuera, hero a `min-height` fijo, componentes sin referencias |
| U2 | Catálogo editorial | PR-2 | `--filter DisplayName~"Home catalogo"` | smoke `/catalogo` | revert aislable (cabecera) |
| U3 | Ficha tokenizada | PR-3 | `--filter DisplayName~"GameDetail"` | smoke ficha ejemplo ± moderador | idem |
| U4 | Eventos editoriales | PR-4 | `--filter DisplayName~"Events"` | smoke `/eventos` (tabs + tarjetas) | idem |
| U5 | Sorteos + Novedades | PR-5 | `--filter DisplayName~"Radar"` / `"News"` / `"GiveawayCard"` | smoke `/sorteos`, `/novedades`, modales | idem |

## PR-1 «Fundación» (DD-01..DD-07, DD-09, DD-10)

- [x] 1.1 [ROJO] En `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs`: crear Fact `FundacionCss_OnBrand_DeclaradoEnLosCincoDataTheme` y `FundacionCss_TokensEstado_DeclaradosEnLosCincoDataTheme` (parseo de los 5 bloques `[data-theme=…]`; assertion de `--on-brand` y de los 4 tokens base por tema) + ajustar fila `Fundación CSS (tokens, rail-card y hero)`: mustContain + `--on-brand`, `--state-error`, `--state-warning`, `--state-info`, `--state-highlight`, `aspect-ratio: 16 / 9`, `max-height: clamp(200px, 36vw, 460px)`, `.page-header-title`, `hero-focal--eurogame`, `hero-focal--mesa-amigos`, `hero-focal--primer-plano`, `hero-focal--ilustracion`, `object-position`; mustNotContain + `min-height: 360px`, `min-height: 460px`. Rojo: los 3 contratos fallan.
- [x] 1.2 [VERDE] En `src/Ludeka.Web/Styles/input.css`: `--on-brand` ×5 con valores DD-03; 12 tokens de estado ×5 con valores DD-04; bloque `.hero-editorial` (DD-01); 4 `.hero-focal--*` con `object-position` (DD-02); `.page-header-title` agrupado con `.rail-title` + comentario actualizado (DD-06); `.filter-btn.active` → `var(--on-brand)` (línea 597); `.animate-pulse { animation: none; }` dentro del `@media (prefers-reduced-motion: reduce)` de `.rail-card` (DD-04). Verde: 1.1. Commit: `feat: tokens on-brand y de estado en los cinco temas`.
- [x] 1.3 [ROJO→VERDE] Fact `AppCss_FundacionInc36_Regenerada` en `tests/Ludeka.UnitTests/Web/PerformanceAndAccessibilityTests.cs` (app.css contiene `--on-brand`, `aspect-ratio:16/9`, `hero-focal--eurogame`, `page-header-title`; NO contiene `min-height:360px` minificado) → regenerar con pipeline DD-10 desde el directorio `src/Ludeka.Web` (read-only): `npx.cmd -y tailwindcss@3.4.17 -i ./Styles/input.css -o ./wwwroot/app.css --minify`. Verde: fact nueva + `ProductionAssets_AppCss_DebeExistir_YContenerClasesEditorialesDeTailwind` vigente. Commit: `feat: app.css regenerado con la fundacion inc-36`.
- [x] 1.4 [ROJO→VERDE] Hero (DD-01/DD-02/DD-03): ajustar fila `HeroEditorial (picture, prioridad y escena CSS)`: mustContain + `hero-focal--`, `text-[var(--on-brand)]`; mustNotContain + `min-h-[360px]`, `sm:min-h-[460px]`, `text-white` (el chip de pruebas pasa a `text-[#F1F5F9]`: par autocontenido sobre su fondo literal) → en `src/Ludeka.Web/Components/Home/HeroEditorial.razor:11` quitar `min-h-[360px] sm:min-h-[460px]` conservando el resto, añadir clase `hero-focal--{clave}` a la `<section>`, línea 70 Buscar → `text-[var(--on-brand)]`; en `src/Ludeka.Web/Components/Home/HeroBackgroundVariant.cs` añadir `FocalClass(variant)` (vacío en `CssScene`). `HeroEditorialQuickSearchTests` (read-only) intacto. Verde: fila ajustada + buscador. Commit: `feat: hero responsivo con foco por variante y boton accesible`.
- [x] 1.5 [ROJO→VERDE] Contrato nuevo `PageHeaderEditorial (cabecera compartida)` sobre `src/Ludeka.Web/Components/Shared/PageHeaderEditorial.razor`: mustContain `page-header-title`, `<h1`, `badge-pill`, `Actions` → crear el componente con API DD-06 (Badge, BadgeIcon, Title RenderFragment, Subtitle, Actions). Verde: contrato + fila fundación. Commit: `feat: PageHeaderEditorial compartida`.
- [x] 1.6 [ROJO→VERDE] Contrato nuevo `EditorialModal (shell compartido)` sobre `src/Ludeka.Web/Components/Shared/EditorialModal.razor`: mustContain `role="dialog"`, `aria-modal="true"`, `aria-label="@Title"`, `OnClose`, `ChildContent`, `Footer`, `ludekaModal.open` → crear el shell DD-07 (overlay, tarjeta `max-w-lg`, cierre con `aria-label="Cerrar {Title}"`), `src/Ludeka.Web/wwwroot/js/editorial-modal.js` (≤35 líneas, objeto global `ludekaModal`: guarda foco, enfoca cierre al abrir, restaura al cerrar, Escape) y `<script src="/js/editorial-modal.js" defer>` junto a los scripts de `src/Ludeka.Web/Components/App.razor:76-77`. Verde: contrato. Commit: `feat: EditorialModal compartido con foco accesible`.
- [x] 1.7 [Boundary PR-1] `dotnet test Ludeka.sln` verde (847 + 3 Fact nuevos + filas); smoke runtime: `/` con los 5 `data-theme` (`?theme=`) — contraste del botón Buscar y hero sin saltos — y variantes con `?hero=` (foco por variante). Pushear y abrir PR-1 (base `main`).

Riesgos PR-1 (del diseño): regresión de contratos INC-35 (mitigación: TDD en el mismo commit); cambio de identidad del botón primario — ya validado visualmente por el maintainer; presupuesto ~380-450 con riesgo de corte; ratios reales ≠ `width`/`height` declarados (sin impacto runtime, deuda ola C).
Dependencias: 1.1 → 1.2 → 1.3 → 1.4 → 1.5 → 1.6 → 1.7. PR-2..5 dependen de PR-1 completo (tokens y componentes).

## PR-2 «Catálogo» (DD-06, D12)

- [x] 2.1 [ROJO] Ajustar fila `Home catalogo (sin emojis)` sobre `src/Ludeka.Web/Components/Pages/Home.razor`: mustContain + `<PageHeaderEditorial`, `scrollbar-none`; mustNotContain + `<h1`, `no-scrollbar`. Rojo: falla.
- [x] 2.2 [VERDE] En `src/Ludeka.Web/Components/Pages/Home.razor`: adoptar `PageHeaderEditorial` (badge «Catálogo Colaborativo» + `dices`; h1 «Descubre tu próxima partida.» conservando el punto terracota vía RenderFragment; sin acción); `CatalogSearchBar` a bloque propio debajo (`max-w-xl`, handler intacto); `no-scrollbar` → `scrollbar-none`; abandonar la variante centrada (`max-w-3xl mx-auto text-center`). Verde: 2.1 + un único `<h1>` en la página. Commit: `feat: cabecera editorial y tira de filtros real en el catalogo`.
- [x] 2.3 [Boundary PR-2] `dotnet test Ludeka.sln` verde; smoke `/catalogo` (un solo h1 serif, buscador funcional, tira de filtros sin banda de scroll). Pushear y abrir PR-2 (base = rama del PR-1).

Riesgo PR-2 (del diseño): bajo — PR más pequeño; la cabecera duplicada se elimina en la misma edición del rediseño.
Dependencias: PR-1; 2.1 → 2.2 → 2.3.

## PR-3 «Ficha» (DD-03, DD-04, DD-08)

- [ ] 3.1 [ROJO] Ajustar fila `GameDetail (ficha sin emojis)` sobre `src/Ludeka.Web/Components/Pages/GameDetail.razor`: mustContain + `flex-wrap`, `text-[var(--on-brand)]`; mustNotContain + `Ludeca`, `text-white`, `text-slate-400`, `bg-amber-500`, `bg-indigo-500`, `bg-purple-950`, `bg-rose-500`, `text-orange-400`. La fila `GameDetail (diseñador texto plano)` (read-only) no cambia: se protege en verde. Rojo: falla.
- [ ] 3.2 [VERDE] En `src/Ludeka.Web/Components/Pages/GameDetail.razor`: `<PageTitle>` «Ludeka» (corrige errata «Ludeca»); estado «juego no encontrado» tokenizado (líneas 16/25/26/27: mapa DD-04 — `text-white` invisible → `--text-primary`); back-bar DD-08 (líneas 38-108: `flex flex-wrap`, moderación agrupada en subcontenedor con borde, «Ir a Mi Ludoteca» al grupo izquierdo de navegación); hardcodes → tokens (56 hover rose → `--state-error-*`, 76 → `--state-warning-*`, 83/294 → `--state-info-*`, 131 chip sólido → `bg-[var(--state-highlight)] text-[var(--on-brand)]`, 299 → `--text-muted`/`--text-primary`); botón «Generar Síntesis con IA Ahora» (323) → `text-[var(--on-brand)]`. Verde: 3.1 + diseñador intacto. Commit: `feat: ficha editorial tokenizada con back-bar envolvente`.
- [ ] 3.3 [Boundary PR-3] `dotnet test Ludeka.sln` verde (incluye diseñador texto plano); smoke ficha de ejemplo: con y sin moderador (la fila envuelve en móvil sin desbordar) y estado no-encontrado legible en los 5 temas. Pushear y abrir PR-3 (base = rama del PR-2).

Riesgos PR-3 (del diseño): densidad de la ficha (verificar que el rediseño de cabecera no rompe el span del diseñador); el agrupado de moderación debe envolver como unidad.
Dependencias: PR-2; 3.1 → 3.2 → 3.3.

## PR-4 «Eventos» (DD-03, DD-04, DD-06, DD-09)

- [ ] 4.1 [ROJO] Ajustar fila `Events (sin emojis)` sobre `src/Ludeka.Web/Components/Pages/Events.razor`: mustContain + `<PageHeaderEditorial`, `text-[var(--on-brand)]`, `rail-card`, `role="tabpanel"`, `aria-controls="panel-`; mustNotContain + `hover:scale-105`, `bg-rose-500/90`, `bg-amber-500/90`, `text-zinc-300`, `dark:`, y `text-white` acotado al patrón de botón de marca (el par overlay `bg-black/60` + `text-white` de las líneas 152/281 es la excepción contratada de DD-04: el mustNotContain debe formularse sin capturarlo, igual que el chip del hero). Rojo: falla.
- [ ] 4.2 [VERDE] En `src/Ludeka.Web/Components/Pages/Events.razor`: cabecera vía `PageHeaderEditorial` (badge «Calendario Oficial del Sector» + `tent`; h1 «Grandes Citas, Ferias & Festivales»; acción «Gestionar Eventos» moderador); pestañas con `role="tabpanel"`/`aria-controls`; tarjetas a `.rail-card` con imagen `rail-cover h-48` (sin `hover:scale-105` suelto); badges de urgencia (273/276) → `bg-[var(--state-error)] text-[var(--on-brand)] border-[var(--state-error-border)]` / warning equivalente; overlay «Finalizado» (279) → `text-white/80 border-white/10`; `text-white` → `text-[var(--on-brand)]` en botones de marca (30/66/73/117/208, incluido «Web Oficial»). Verde: 4.1. Commit: `feat: eventos editoriales con pestañas accesibles y badges tokenizados`.
- [ ] 4.3 [Boundary PR-4] `dotnet test Ludeka.sln` verde (incluye `PaginasEventos_TodaImagenDeEventoTieneFallbackPorDominio` reprotegida en verde); smoke `/eventos`: tablist con `tabpanel`/`aria-controls`, tarjetas rail-card (lift/focus-visible), chips de marca con contraste en los 5 temas. Pushear y abrir PR-4 (base = rama del PR-3).

Riesgos PR-4 (del diseño): mayor densidad de sustituciones (mapa DD-04 de Events); excepción de overlay exige mustNotContain fino; assets hotlink de Unsplash quedan fuera (segunda ola, no tocar).
Dependencias: PR-3; 4.1 → 4.2 → 4.3.

## PR-5 «Sorteos + Novedades» (DD-03, DD-04, DD-06, DD-07, DD-09, DD-10)

- [ ] 5.1 [ROJO] Ajustar fila `Radar (sin emojis)` sobre `src/Ludeka.Web/Components/Pages/Radar.razor`: mustContain + `<PageHeaderEditorial`, `<EditorialModal`; mustNotContain + `dark:`, `text-amber-500`, `text-rose-600`, `fixed inset-0 z-50` (la aserción `rail-card` va en la fila `GiveawayCard`, donde vive la clase). Rojo: falla.
- [ ] 5.2 [VERDE] En `src/Ludeka.Web/Components/Pages/Radar.razor`: cabecera compartida (badge «Radar de Sorteos Comunitarios» + `gift`; h1 «Sorteos de Juegos de Mesa»; acción «Proponer Sorteo»); adoptar `EditorialModal` (elimina el shell inline ~140 líneas, Radar.razor:120-135; el checkbox de moderación → `--state-warning-*` y el div de error → `--state-error-*` quedan como contenido del shell); filtros territoriales (55/62) y botones (30/99/253) → `text-[var(--on-brand)]`; chips (226-231) → `--state-warning-*` y error (240) → `bg-[var(--state-error-bg)] border-[var(--state-error-border)] text-[var(--state-error)]` sin `dark:`. Verde: 5.1. Commit: `feat: sorteos con modal editorial y tokens de estado`.
- [ ] 5.3 [ROJO] Ajustar filas `News (sin emojis)` sobre `src/Ludeka.Web/Components/Pages/News.razor` (mustContain + `<PageHeaderEditorial`, `<EditorialModal`, `DefaultImage`, `DefaultImageDomain.Novedad`, `novedad-default.svg`, `onerror`, `this.onerror=null`, `width=`, `height=`, `rail-card`; mustNotContain + `dark:`, `text-pink-500`, `text-rose-600`, `hover:scale-105`, `fixed inset-0 z-50`) y `GiveawayCard (sin emojis)` sobre `src/Ludeka.Web/Components/Shared/GiveawayCard.razor` (mustContain + `rail-card`, `rail-cover`, `DefaultImage`, `DefaultImageDomain.Sorteo`, `sorteo-default.svg`, `onerror`, `this.onerror=null`, `width=`, `height=`, `text-[var(--on-brand)]`; mustNotContain + `dark:`, `hover:scale-105`, `text-amber-500`, `text-pink-500`, `text-sky-500`, `text-rose-600`). Rojo: fallan.
- [ ] 5.4 [VERDE] En `src/Ludeka.Web/Components/Pages/News.razor`: cabecera compartida (badge «Calendario de Estrenos» + `newspaper`; h1 «Novedades de los Viernes & Lanzamientos»; acción «Añadir Novedad» moderador); adoptar `EditorialModal` (elimina shell inline ~100, News.razor:185-200; pies de acciones al fragment `Footer` sin cambiar handlers); zona de imagen SIEMPRE renderizada: sin URL → `DefaultImage.razor` dominio `Novedad` en el mismo contenedor y aspecto (paridad `HomeReleaseCard`); URL externa → `<img>` con `width`/`height` y `onerror` (`this.onerror=null`) hacia `novedad-default.svg`; tokens (53 × de chip → `--state-error`, 164 pink → `--state-highlight` + `hover:opacity-80`, 171 sky → `--state-info`, 265 error de formulario → triple `--state-error` eliminando el fallo AA 3,24:1 y el `dark:` muerto); tarjetas a `.rail-card`; botones (28/82/278) → `text-[var(--on-brand)]`. Verde: fila News de 5.3.
- [ ] 5.5 [VERDE] En `src/Ludeka.Web/Components/Shared/GiveawayCard.razor`: `.rail-card` + `rail-cover h-44`; ante `ThumbnailUrl` ausente → `DefaultImage.razor` dominio `Sorteo` con el mismo aspecto; URL externa → `width`/`height` y `onerror` (`this.onerror=null`) hacia `sorteo-default.svg`; tokens (13 badge → `bg-[var(--state-warning)] text-[var(--on-brand)]`, 25/87 → triple `--state-warning`, 96/103 → `--state-highlight`/`--state-info` + `hover:opacity-80`, 146 → triple `--state-error` sin `dark:` con `animate-pulse` bajo guarda de movimiento reducido); «Participar» (112) → `text-[var(--on-brand)]`. Verde: fila GiveawayCard de 5.3. Commit: `feat: novedades y GiveawayCard con imagen por defecto y rail-card`.
- [ ] 5.6 [Boundary PR-5 y del incremento] `dotnet test Ludeka.sln` verde con barrido completo (847 + todos los contratos nuevos/ajustados; `RestoDeLaWeb_SinEmojis…` reprotegida); regeneración final de `app.css` (DD-10, purga utilidades muertas) manteniendo `AppCss_FundacionInc36_Regenerada` en verde; smoke: `/sorteos`, `/radar`, `/novedades`, ficha de ejemplo, modales de Radar/News (foco al abrir, restaurar al cerrar, Escape) y tarjeta de sorteo/novedad sin imagen con default del dominio. Pushear y abrir PR-5 (base = rama del PR-4).

Riesgos PR-5 (del diseño): mayor corte (~250-350; los shells se RESTAN ~240 líneas); interop de foco como patrón nuevo (precedente `LocationSelectorModal`, degradación a markup puro sin JS); `safelist` de Tailwind puede reimplantar hardcodes eliminados (lo detectan la fact de app.css y los mustNotContain).
Dependencias: PR-4; 5.1 → 5.2 → 5.3 → 5.4 → 5.5 → 5.6.

## Orden global y verificación

1. Orden estricto PR-1 → PR-2 → PR-3 → PR-4 → PR-5 (base de cada PR = rama del anterior); nunca reabrir decisiones INC-35/INC-36 (D1..D13 y DD-01..DD-11).
2. Boundary de cada PR: `dotnet test Ludeka.sln` verde (baseline 847 + contratos acumulados) + smoke runtime de su ruta + regeneración DD-10 si tocó `input.css` (solo PR-1 y barrido final del PR-5).
3. Con PR-5 en verde, los 5 PRs apilados quedan listos para merge en orden; `sdd-verify` añade la capa runtime completa (5 temas, LCP/CLS del hero, modales, back-bar móvil).
