# Propuesta: Rediseño Editorial del Resto de Páginas + Fix Responsive del Hero — INC-36

> Fase SDD `sdd-propose`. Idioma: español castellano (regla suprema AGENTS.md). Store: hybrid (este archivo + Engram `sdd/rediseno-paginas-editoriales/proposal`).
> Entradas: `openspec/changes/rediseno-paginas-editoriales/explore.md` (fase `sdd-explore` completada), contexto confirmado del maintainer y artefactos archivados de INC-35.

## Intención

**Por qué ahora:** el fuera-de-alcance de INC-35 («rediseño editorial del resto de páginas») quedó registrado como INC-36 en el roadmap, y el maintainer ha reportado además un **bug responsive del hero** con captura: «la imagen se aplasta por los bordes y ocupa mucho; el alto debería reducirse proporcionalmente y quedar bien». La exploración confirmó la causa raíz: `min-height` fijo (360px/460px) + hijos todos `position: absolute` (sin contenido en flujo) + recorte destructivo del asset 16:9 en caja casi cuadrada de móvil (cover escala a `max(360/900, 320/1600) = 0,40` y muestra solo el 50% central). Se suma un fallo sistémico WCAG 2.2 AA: el botón «Buscar» usa `text-white` sobre `--brand-primary` (3,69:1 en charcoal, tabletop y midnight — falla AA) y el mismo patrón se repite en los botones primarios de las 5 páginas. El sistema editorial de INC-35 (tokens, `.rail-card`, `Icon.razor`, `DefaultImage.razor`) está maduro: es el momento de extenderlo al resto de la web y cerrar la deuda visual heredada.

## Alcance

### Dentro del alcance

- **Fix responsive del hero (D1=A):** sustituir `min-h-[360px]/[460px]` por ratio responsiva (`aspect-ratio`) + `object-position` focal por variante + tapa `clamp()` + suelo acotado. Sin tocar `<picture>`, `fetchpriority`, `width`/`height`, `onerror`, alt castellano ni el h1 `sr-only` (contratos INC-35 intactos).
- **Token `--on-brand` (D3):** nuevo token en los 5 `data-theme` para texto sobre botones de marca (oscuro en charcoal/tabletop/midnight, blanco en editorial/wood) + adopción en todos los botones `bg-[var(--brand-primary)]` de las 5 páginas. ⚠️ **REQUIERE VALIDACIÓN VISUAL DEL MAINTAINER AL APROBAR ESTA PROPUESTA** (cambia la identidad visual del botón primario: texto oscuro sobre terracota en 3 temas).
- **Rediseño editorial de las 5 páginas** con tokens/clases existentes: Catálogo (`Home.razor`), Fichas (`GameDetail.razor`), Eventos (`Events.razor`), Sorteos (`Radar.razor`), Novedades (`News.razor`).
- **`PageHeaderEditorial` compartida (D4):** componente de cabecera (badge píldora + h1 serif + subtítulo + acción) para las 4 páginas de listado; elimina ~75 líneas duplicadas.
- **Unificación `.rail-card` (D6):** migrar tarjetas de Events/News/GiveawayCard al lenguaje de lift/glow/focus-visible/reduced-motion de portada.
- **`EditorialModal` shell (D11):** extraer el shell duplicado de los modales de Radar y News (~100-140 líneas) con cierre/aria/foco unificados.
- **Token `--on-brand` + tokens semánticos de estado (D8):** sustituir hardcodes (amber/indigo/purple/rose/sky/slate) por tokens por tema, solo en las 5 páginas.
- **Paridad de imagen (D10):** `News.razor` y `GiveawayCard.razor` con `DefaultImage` + `onerror` + `width`/`height` (hoy sin dimensiones ni fallback: CLS e imagen rota).
- **Fixes transversales:** fila de acciones de GameDetail con `flex-wrap` + agrupación de moderación (D7), accesibilidad de pestañas de Eventos (`tabpanel`/`aria-controls`), `text-white` invisible en temas claros (`GameDetail.razor:25`), variantes `dark:` inertes eliminadas (D9), clase muerta `no-scrollbar` → `scrollbar-none`, typo `<PageTitle>` «Ludeca» → «Ludeka».
- **Contratos de markup nuevos/ajustados** (patrón INC-31) en el mismo commit que cada cambio (TDD estricto) + regeneración de `wwwroot/app.css`.

### Fuera del alcance (cierre de D13)

- MyLibrary, directorios (editoriales/creadores/tiendas), páginas admin y el resto de la web.
- Skeletons / streaming SSR.
- `srcset`/`sizes` del hero (**estrategia C descartada a segunda ola**, D1=A).
- Reemplazo de assets hotlink de Unsplash en eventos.
- Habilitar `darkMode` por `data-theme` global en `tailwind.config.js` (se eliminan las variantes `dark:` muertas, D9).
- Cualquier cambio de DTOs, servicios, dominio o datos (todo el incremento es presentación).

## Capacidades (contrato con sdd-spec)

### Nuevas capacidades

- `editorial-page-foundations`: tokens `--on-brand` y semánticos de estado en los 5 temas, componente `PageHeaderEditorial`, shell `EditorialModal`, contraste AA de botones de marca y adopción del lenguaje `.rail-card` en tarjetas de página.

### Capacidades modificadas

- `home-landing-hero`: el modelo de altura pasa de `min-height` fija a ratio responsiva con cap `clamp()`, suelo acotado y `object-position` focal; se conservan `<picture>`, prioridad, dimensiones, alt, h1 `sr-only` y buscador.
- `default-image-fallbacks`: extensión del patrón (`DefaultImage` + `onerror` + dimensiones) a las tarjetas de Novedades (`News.razor`) y de sorteos (`GiveawayCard.razor`), paridad con los carriles de portada.

## Decisiones — CERRADAS (D1–D13, con base en las recomendaciones de explore.md)

| # | Decisión | Resolución y justificación |
|---|---|---|
| D1 | Estrategia del fix del hero | **A**: ratio responsiva + `object-position` focal + cap `clamp()` + suelo. Es la única que reduce el alto proporcionalmente al ancho (lo pedido), mantiene CLS 0 (el alto deriva del ancho) y tiene esfuerzo bajo. C (srcset + recortes autoriales) queda descartada a segunda ola; B (clamp puro) recrearía la caja casi cuadrada que motivó el bug. |
| D2 | Forma móvil del hero | Móvil: `aspect-ratio: 16/9` fiel a la fuente (recorte ~0) con suelo `min-height` ~200px para alojar el buscador (~110-130px). Escritorio: cap `max-height` ~460px vía `clamp()` y foco autoral por variante. Fórmula exacta del clamp y punto focal por variante: `sdd-design`. |
| D3 | Alcance del contraste AA | **Token global `--on-brand` en los 5 temas + adopción en las 5 páginas** (no solo el botón Buscar): resuelve de raíz el fallo 3,69:1 en todos los botones de marca de la app. Texto oscuro (#14181C) sobre #E05A38 = 4,84:1 ✓. **Requiere validación visual del maintainer al aprobar la propuesta.** |
| D4 | Cabecera editorial de página | **Sí, extraer `PageHeaderEditorial`** (badge + h1 + subtítulo + acción) para Catálogo/Eventos/Sorteos/Novedades. **GameDetail NO**: su cabecera es el hero de ficha con backdrop, estructura distinta. |
| D5 | Serif display | **Serif en las 4 cabeceras de listado** (heredada de `PageHeaderEditorial` vía `--font-display`). En la ficha, el título del juego **se queda en sans bold**: la ficha ya tiene jerarquía densa (backdrop + doble rating + badges) y la serif debe reservarse a portada y listados. |
| D6 | Unificación de tarjetas | **Migrar Events/News/GiveawayCard a `.rail-card`** (tokens + focus-visible + reduced-motion). `.game-card-editorial` se mantiene en Catálogo (lenguaje ya conquistado; no se duplica el diff). |
| D7 | Back-bar de GameDetail | **`flex-wrap` + agrupación de acciones de moderación**: el mínimo exigible es que no desborde en móvil; el agrupado reduce de 8-9 botones a 1 bloque. Menú «más acciones» queda para diseño si el wrap no basta. |
| D8 | Tokens semánticos de estado | **Sustituir hardcodes solo en las 5 páginas** (no barrido global): amber/indigo/purple/rose/sky/slate pasan a tokens por tema (error/warning/info/highlight). El resto de la web espera a un barrido futuro. |
| D9 | Variantes `dark:` inertes | **Eliminar `dark:*` de las 5 páginas y sustituir por tokens temáticos**; NO habilitar `darkMode` por `data-theme` global (cambio de config global, más riesgo, fuera de alcance). Causa técnica: `darkMode: 'class'` nunca activa porque la app usa `data-theme`. |
| D10 | Carátula en News | **Sí**: zona de imagen siempre renderizada con `DefaultImage` (paridad con `HomeReleaseCard`), `onerror` a variante estática y `width`/`height` (hoy: CLS y sin fallback). |
| D11 | Modales | **Sí, shell `EditorialModal` compartido** para Radar/News: ahorra ~100-140 líneas y unifica overlay, cierre, aria y foco. |
| D12 | Filtros | **Fix de clase muerta `no-scrollbar` → `scrollbar-none`: sí** (trivial, mismo bug arreglado en INC-35). Unificación de píldoras `.filter-btn`/`.badge-pill` solo donde el rediseño toque la tira (Catálogo). **Sticky en móvil: fuera** (no aporta al rediseño y añade riesgo). |
| D13 | Alcance | **Cerrado conforme a «Fuera del alcance»** (MyLibrary, directorios, admin, skeletons/streaming, srcset C, Unsplash, darkMode global, DTOs/servicios/dominio). |

## Enfoque

Extender el sistema de INC-35, no reemplazarlo: `input.css` sigue siendo el punto único de verdad de tokens; las tarjetas de página hablan el lenguaje `.rail-card` ya probado; el fix del hero se hace en sitio (`input.css` + clase de `HeroEditorial.razor`) sin tocar el mecanismo de variantes. Cada cambio de markup entra por TDD: contrato en `WebMarkupContractTests.cs` primero (rojo), implementación después (verde), actualizando los contratos INC-31/35 que el rediseño toca **en el mismo commit**. `app.css` se regenera con el pipeline vigente tras cada cambio de `input.css` (recordatorio para `sdd-verify`).

## Plan de PRs apilados previsto (forecast cualitativo — auto-chain)

El forecast superará las 400 líneas con certeza (5 páginas + tokens + 2 componentes nuevos), así que se aplican **PRs apilados desde el mismo worktree** (patrón INC-35: 4 PRs encadenados), sin preguntar:

1. **PR-1 «Fundación»**: tokens `--on-brand` en los 5 temas + tokens semánticos de estado + `PageHeaderEditorial` + shell `EditorialModal` + fix responsive del hero + contratos CSS nuevos (~250-350 líneas).
2. **PR-2..n «Páginas agrupadas»**: Catálogo / Fichas / Eventos / Sorteos+Novedades, en pares o grupos según el corte exacto de líneas.

`sdd-tasks` fijará los cortes exactos, el forecast de líneas por PR y las guardas formales (`Decision needed before apply`, riesgo de presupuesto).

## Áreas afectadas

| Área | Impacto |
|---|---|
| `src/Ludeka.Web/Styles/input.css` | Modificado — tokens `--on-brand`/semánticos, ratio del hero, clases nuevas |
| `src/Ludeka.Web/Components/Shared/HeroEditorial.razor` | Modificado — clases de altura responsive |
| `src/Ludeka.Web/Components/Shared/PageHeaderEditorial.razor` | Nuevo (ubicación exacta en sdd-design) |
| `src/Ludeka.Web/Components/Shared/EditorialModal.razor` | Nuevo (idem) |
| `src/Ludeka.Web/Components/Pages/Home.razor` | Modificado — cabecera editorial, filtros, `scrollbar-none` |
| `src/Ludeka.Web/Components/Pages/GameDetail.razor` | Modificado — tokens, back-bar, typo, `text-white` |
| `src/Ludeka.Web/Components/Pages/Events.razor` | Modificado — `.rail-card`, badges, tabs a11y |
| `src/Ludeka.Web/Components/Pages/Radar.razor` | Modificado — `.rail-card`, tokens, modal shell |
| `src/Ludeka.Web/Components/Pages/News.razor` | Modificado — imagen con fallback, tokens, modal shell |
| `src/Ludeka.Web/Components/Shared/GiveawayCard.razor` | Modificado — fallback + dimensiones + `.rail-card` |
| `src/Ludeka.Web/wwwroot/app.css` | Regenerado — pipeline vigente |
| `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` | Modificado — contratos nuevos/ajustados (mismo commit TDD) |

## Riesgos

| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Regresión de contratos INC-35 al tocar hero/`input.css`/5 páginas | Alta | TDD estricto: contratos actualizados en el mismo commit; invariantes vigentes (buscador, alt castellano, Fraunces en 1 petición, sin emojis) blindados |
| Presupuesto de revisión 400 líneas | Alta | Auto-chain de PRs apilados desde el mismo worktree (plan arriba) |
| Cambio de identidad visual del botón primario (`--on-brand`) | Media | **Validación visual del maintainer al aprobar la propuesta** (marcado en D3) |
| Regresión de LCP/CLS del hero al cambiar el modelo de altura | Media | Mantener `<picture>`, `fetchpriority`, `width`/`height` y CLS 0; verificación runtime en `sdd-verify` |
| A11y de pestañas de Eventos a medias | Media | Arreglar `tabpanel`/`aria-controls` de paso con el rediseño, en el mismo PR |
| 5 temas × tokens nuevos sin probar | Media | Verificación runtime de los 5 `data-theme` en `sdd-verify` |
| Fixes triviales con conflicto de merge | Baja | Tocarlos en las mismas líneas que el rediseño (una sola edición) |

## Plan de rollback

Revertir el PR del slice afectado restaura el estado previo: los PRs son independientes y PR-1 es aditivo en tokens (nuevas variables no rompen temas existentes). El fix del hero queda aislado en `input.css` + clase del componente (revert = volver a `min-height` fijo). Sin migraciones de datos, sin cambios de contrato de servicios, assets existentes intactos.

## Dependencias

- Aprobación de esta propuesta por el maintainer (incluye la validación visual del cambio de botón primario, D3).
- Exploración cerrada con causa raíz y estrategias documentadas (`explore.md`).
- Sin dependencias externas nuevas (cero paquetes, cero assets por generar).

## Criterios de éxito

- [ ] En viewport ~360px el hero reduce el alto proporcionalmente (ratio, sin `min-height:360px`) y la mesa/composición queda en cuadro (punto focal por variante).
- [ ] Contraste AA del botón primario de marca en los 5 `data-theme` (token `--on-brand`, ≥4,5:1) — tras validar visualmente el maintainer.
- [ ] Cabecera editorial compartida (`PageHeaderEditorial`) en las 4 páginas de listado (Catálogo, Eventos, Sorteos, Novedades).
- [ ] News/GiveawayCard con `DefaultImage` + `onerror` + `width`/`height` (paridad con carriles de portada).
- [ ] Tarjetas de Events/News/GiveawayCard con lenguaje `.rail-card` (lift/glow/focus-visible/reduced-motion).
- [ ] Sin `text-white` invisible en temas claros, sin variantes `dark:` inertes en las 5 páginas, back-bar de ficha sin desborde en móvil, pestañas de Eventos con `tabpanel`/`aria-controls`.
- [ ] Fixes triviales en verde: `scrollbar-none` real y typo «Ludeka» corregido.
- [ ] `dotnet test Ludeka.sln` en verde (847 previos + contratos nuevos) y `app.css` regenerado.
