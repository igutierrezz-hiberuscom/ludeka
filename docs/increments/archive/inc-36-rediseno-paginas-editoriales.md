# Incremento 36: Rediseño Editorial del Resto de Páginas + Fix Responsive del Hero

- **Identificador SDD:** `rediseno-paginas-editoriales`
- **Estado:** ✅ **Completado y Archivado** (suite **855/855** en verde al 100%; baseline 854 al inicio del incremento + el Fact acotado del ancho del hero; build con 0 errores)
- **Entrega:** cadena de **6 PRs apilados** (`inc/rediseno-paginas-editoriales-1a` → `-1b` → `-2` → `-3` → `-4` → `-5`): PR-1a «Fundación» (#8), PR-1b «Componentes compartidos» (#9), PR-2 «Catálogo» (#10), PR-3 «Ficha» (#11), PR-4 «Eventos» (#12) y PR-5 «Sorteos + Novedades» con la remediación F4.2/DD-01 (#13); **pendiente del merge ordenado por el maintainer**.
- **Verificación SDD:** **PASS** — 13/13 requerimientos y 34/34 escenarios COMPLIANT, 0 blockers, 0 hallazgos críticos (evidencia ligada al asentamiento nativo `sha256:19aa14ff…`).
- **Artefactos SDD archivados:** [`openspec/changes/archive/2026-09-10-rediseno-paginas-editoriales/`](file:///c:/repos/Ludeka/openspec/changes/archive/2026-09-10-rediseno-paginas-editoriales/proposal.md) (proposal, 3 deltas de specs, design, tasks 22/22, apply-progress, verify-report y archive-report)
- **Módulos de la Especificación Viva:** [`24-fundaciones-editoriales-y-componentes.md`](file:///c:/repos/Ludeka/docs/specs/sistema/24-fundaciones-editoriales-y-componentes.md) (nuevo) + módulos [23](file:///c:/repos/Ludeka/docs/specs/sistema/23-portada-editorial.md), [15](file:///c:/repos/Ludeka/docs/specs/sistema/15-dashboard-inicio-editorial.md), [01](file:///c:/repos/Ludeka/docs/specs/sistema/01-catalogo-y-fichas.md) y [16](file:///c:/repos/Ludeka/docs/specs/sistema/16-sorteos-novedades-y-eventos.md)
- **Worktree / Rama:** `C:\repos\ludeka-wt\rediseno-paginas-editoriales` / `inc/rediseno-paginas-editoriales-5`
- **Origen:** registrado desde el fuera-de-alcance de INC-35 + bug responsive del hero reportado por el maintainer con captura («la imagen se aplasta por los bordes y ocupa mucho; el alto debería reducirse proporcionalmente y quedar bien»).

---

## 1. Alcance Técnico

Extender el sistema editorial de INC-35 (tokens de tema, `.rail-card`, `Icon.razor`, `DefaultImage.razor`, serif Fraunces) a las 5 páginas restantes de la web y corregir el bug responsive del hero. Todo es presentación: cero cambios de DTOs, servicios, dominio o datos.

1. **Fix responsive del hero (D1 = estrategia A):** sustituir `min-h-[360px]/[460px]` por `aspect-ratio` responsiva + `object-position` focal por variante + tapa `max-height` vía `clamp()` + suelo acotado (~200px). Causa raíz corregida: altura fija independiente del ancho, hijos todos `position: absolute` y recorte destructivo del asset 16:9 en caja casi cuadrada de móvil (cover escala ×0,40 y muestra solo el 50% central). Se conservan `<picture>` AVIF/WebP/JPEG, `fetchpriority="high"`, `width`/`height`, `onerror`, alt castellano y el h1 `sr-only` (contratos INC-35 intactos). La estrategia C (`srcset`/`sizes` con recortes autoriales) queda descartada a segunda ola.
2. **Token `--on-brand` (D3):** nuevo token por tema para el texto sobre botones de marca (oscuro en charcoal/tabletop/midnight, blanco en editorial/wood), resolviendo el fallo WCAG 2.2 AA del botón «Buscar» del hero (3,69:1) y de todos los botones primarios `bg-[var(--brand-primary)] text-white` de la app. ⚠️ Cambio de identidad visual que requiere validación del maintainer.
3. **`PageHeaderEditorial` compartida (D4/D5):** cabecera editorial (badge píldora + h1 serif `--font-display` + subtítulo + acción) para Catálogo, Eventos, Sorteos y Novedades; elimina ~75 líneas duplicadas. GameDetail no la usa (su cabecera es el hero de ficha).
4. **Rediseño editorial de las 5 páginas** con tokens/clases existentes:
   - **Catálogo (`Home.razor`):** cabecera editorial, fix de la clase muerta `no-scrollbar` → `scrollbar-none`, píldoras de filtro alineadas con `.badge-pill`.
   - **Fichas (`GameDetail.razor`):** sustitución de colores hardcodeados (`text-white` invisible en temas claros, slate/amber/indigo/purple/rose/sky) por tokens, back-bar con `flex-wrap` + agrupación de moderación, typo `<PageTitle>` «Ludeca» → «Ludeka».
   - **Eventos (`Events.razor`):** tarjetas con lenguaje `.rail-card`, badges de urgencia con tokens semánticos, accesibilidad de pestañas (`tabpanel`/`aria-controls`).
   - **Sorteos (`Radar.razor`):** `GiveawayCard` con `.rail-card`, `DefaultImage` + `onerror` + `width`/`height` (hoy sin dimensiones ni fallback), badges con tokens, modal sobre el shell compartido.
   - **Novedades (`News.razor`):** zona de imagen siempre renderizada con `DefaultImage` (paridad con `HomeReleaseCard`), `onerror` + dimensiones, tokens semánticos, modal sobre el shell compartido.
5. **`EditorialModal` shell (D11):** shell compartido para los modales duplicados de Radar y News (overlay + card + cierre + aria + foco), ~100-140 líneas ahorradas.
6. **Tokens semánticos de estado (D8) y limpieza de `dark:` (D9):** estados (error/warning/info/highlight) por tema en las 5 páginas; eliminación de las variantes `dark:*` inertes (`tailwind.config.js` usa `darkMode: 'class'`, nunca activo con la estrategia real `data-theme`).
7. **Contratos de markup (patrón INC-31, TDD estricto):** contratos nuevos/ajustados en `WebMarkupContractTests.cs` en el mismo commit que cada cambio (ratio del hero en `input.css`, `--on-brand` en los 5 temas, `PageHeaderEditorial` en las 4 cabeceras, fallback de News/GiveawayCard, shell `EditorialModal`); regeneración de `wwwroot/app.css` con el pipeline vigente.

**Plan de entrega:** forecast > 400 líneas con certeza → PRs apilados desde el mismo worktree (auto-chain): PR-1 «Fundación» (tokens + componentes compartidos + fix hero + contratos CSS) → PR-2..n páginas agrupadas. Los cortes exactos y el forecast de líneas los fija `sdd-tasks`.

---

## 2. Decisiones Clave (D1–D13 cerradas)

| # | Decisión | Resolución |
|---|---|---|
| D1 | Estrategia del fix del hero | **A** (ratio responsiva + foco + cap clamp + suelo). C (srcset) a segunda ola; B (clamp puro) recrearía el bug. |
| D2 | Forma móvil del hero | Móvil 16/9 fiel con suelo ~200px (buscador ~110-130px); escritorio cap ~460px con foco autoral. Fórmula exacta en `sdd-design`. |
| D3 | Contraste AA de marca | Token global `--on-brand` en los 5 temas + adopción en las 5 páginas. **Requiere validación visual del maintainer.** |
| D4 | Cabecera de página | `PageHeaderEditorial` para las 4 páginas de listado; GameDetail no (hero de ficha propio). |
| D5 | Serif display | Serif en cabeceras de listado; título de ficha en sans bold (la ficha ya tiene jerarquía densa). |
| D6 | Unificación de tarjetas | Events/News/GiveawayCard → `.rail-card`; `.game-card-editorial` se mantiene en Catálogo. |
| D7 | Back-bar de GameDetail | `flex-wrap` + agrupación de acciones de moderación (no desbordar en móvil es el mínimo). |
| D8 | Tokens de estado | Sustituir hardcodes solo en las 5 páginas (sin barrido global). |
| D9 | Variantes `dark:` inertes | Eliminar `dark:*` de las 5 páginas y sustituir por tokens; NO habilitar `darkMode` por `data-theme` global. |
| D10 | Carátula en News | Zona de imagen siempre con `DefaultImage` + `onerror` + dimensiones (paridad con carriles). |
| D11 | Modales | Shell `EditorialModal` compartido para Radar/News. |
| D12 | Filtros | Fix `no-scrollbar` → `scrollbar-none` sí; unificación de píldoras donde toque; sticky fuera. |
| D13 | Alcance | MyLibrary, directorios, admin, skeletons/streaming, srcset, Unsplash, darkMode global, DTOs/servicios → FUERA. |

---

## 3. Criterios de Aceptación

- En viewport ~360px el hero reduce el alto proporcionalmente (ratio, sin `min-height:360px`) y la mesa/composición queda en cuadro (punto focal por variante).
- Contraste AA del botón primario de marca en los 5 `data-theme` (token `--on-brand`, ≥ 4,5:1), tras validar visualmente el maintainer.
- Cabecera editorial compartida en las 4 páginas de listado (Catálogo, Eventos, Sorteos, Novedades).
- News/GiveawayCard con `DefaultImage` + `onerror` + `width`/`height` (paridad con carriles de portada).
- Tarjetas de Events/News/GiveawayCard con lenguaje `.rail-card` (lift/glow/focus-visible/reduced-motion).
- Sin `text-white` invisible en temas claros, sin `dark:` inertes en las 5 páginas, back-bar sin desborde en móvil, pestañas de Eventos con `tabpanel`/`aria-controls`.
- Fixes triviales: `scrollbar-none` real y typo «Ludeka» corregido.
- `dotnet test Ludeka.sln` en verde (847 previos + contratos nuevos) y `app.css` regenerado; verificación runtime de los 5 temas en `sdd-verify`.

## 4. Fuera de Alcance

MyLibrary, directorios (editoriales/creadores/tiendas), páginas admin y el resto de la web; skeletons/streaming SSR; `srcset`/`sizes` del hero (estrategia C, segunda ola); reemplazo de assets hotlink de Unsplash en eventos; `darkMode` por `data-theme` global en `tailwind.config.js`; cambios de DTOs, servicios, dominio o datos.
