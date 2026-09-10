```yaml
schema: gentle-ai.verify-result/v1
evidence_revision: sha256:19aa14ff7340bc9af6f00da14923c45100d39aaa9ac8e277a12f5ab7938c8d8f
verdict: pass
blockers: 0
critical_findings: 0
requirements: 13/13
scenarios: 34/34
test_command: dotnet test Ludeka.sln
test_exit_code: 0
test_output_hash: sha256:3e294d43e6d7f34e08616099f6a6019732f8719b19b673a93b6595f9c3fd397c
build_command: dotnet build Ludeka.sln --no-restore
build_exit_code: 0
build_output_hash: sha256:fff78a634d09e935deb1b21eeb40e6cb41a34d481f7e2d8b9918dc0e72ddaaa5
```
# Informe de verificación SDD

**Cambio**: `rediseno-paginas-editoriales` (INC-36)  
**Modo**: Strict TDD  
**Worktree**: `C:\repos\ludeka-wt\rediseno-paginas-editoriales`  
**Rama/base**: `inc/rediseno-paginas-editoriales-5` sobre `inc/rediseno-paginas-editoriales-4` (PR #13)  
**HEAD verificado**: `d7b1f43` (remediación `310c464` modal F4.2, `379c074` hero DD-01, `d7b1f43` docs)  
**Store**: OpenSpec  
**Herramienta interactiva**: Chrome real vía Chrome DevTools MCP (`chrome-devtools_*`)

> `evidence_revision` = SHA-256 del cuerpo de este informe (desde `# Informe de verificación SDD` inclusive), codificado en UTF-8 sin BOM con finales de línea LF; es recomputable de forma independiente. Este informe es FRESCO y se liga DESPUÉS del asentamiento nativo del intento adquirido por el orquestador (token `sha256:1ab1eecd…`); la evidencia fallida `d12c2eff…` del informe anterior quedó remediada por `310c464`/`379c074`.

## Veredicto ejecutivo

**PASS — 13/13 requisitos y 34/34 escenarios conformes, sin blockers ni hallazgos críticos.** La remediación `310c464` (shell del modal siempre montado + contrato `mustNotContain`) y `379c074` (`width: 100%` en `.hero-editorial`, `app.css` regenerado y contrato acotado al bloque/regla) resolvió el FAIL anterior: **F4.2** queda cerrado con evidencia funcional de navegador (al cerrar con Escape o con la X, `ludekaModal.close` SÍ se invoca y el foco vuelve al botón disparador — mismo nodo) y la **desviación DD-01 del ancho** quedó corregida (hero = ancho del contenedor en 360/640/1240, sin desborde propio). Build y suite verdes: **855/855**, contratos **103/103**, rendimiento/a11y **5/5**. `sdd-archive` puede proceder.

## Reconciliación del delta del candidato

Delta auditado desde la revisión que falló (`c08cd9b`):

- `Radar.razor` y `News.razor`: −3 líneas cada uno (retiro del envoltorio `@if (_isCreateModalOpen)`; contenido interno sin reindentado).
- `Styles/input.css`: +1 línea (`width: 100%` en el bloque `.hero-editorial`).
- `wwwroot/app.css`: 1 línea minificada cambiada; regla servida `.hero-editorial{width:100%;aspect-ratio:16/9;min-height:200px;max-height:clamp(200px,36vw,460px)}` (248402→248413 bytes).
- `WebMarkupContractTests.cs`: +31/−4 (dos aserciones `mustNotContain` nuevas en las filas `Radar`/`News` y un Fact nuevo acotado al bloque CSS).
- `PerformanceAndAccessibilityTests.cs`: +9 (aserción acotada a la regla compilada `.hero-editorial{…}`).
- `apply-progress.md`: documentación de la remediación (+63).

## Completitud

| Métrica | Resultado |
|---|---:|
| Requisitos recuperados de las tres specs | 13 |
| Requisitos completamente verificados | 13/13 |
| Escenarios recuperados | 34 |
| Escenarios completamente verificados | 34/34 |
| Escenarios fallidos en runtime | 0 |
| Tareas | 22/22 |
| Blockers / hallazgos críticos | 0 / 0 |

**Recuento de pruebas (justificación del +1, verificada de forma independiente)**: la suite pasó de 854 a **855** porque `379c074` añade el Fact `HeroEditorial_AnchoDelContenedorDeclaradoEnElBloqueCss` (+1 test) y una aserción acotada dentro de `AppCss_FundacionInc36_Regenerada` (no añade test nuevo). El contrato se acotó al bloque `.hero-editorial` y a su regla compilada porque `width: 100%` ya existía en otros bloques de `input.css` (y `width:100%` en 9 puntos de `app.css`, incluida `.w-full`): un fragmento global habría sido tautológico y no habría producido ROJO real. Comprobado: el Fact nuevo existe, falla sin la propiedad (ciclo declarado) y pasa hoy **1/1** en aislamiento; el fact de `app.css` pasa **1/1** en aislamiento; `WebMarkupContractTests` suma **103** (antes 102) y la suite **855** (antes 854). La cifra histórica `847` de `tasks.md` sigue siendo una discrepancia documental preexistente, no un fallo.

## Herramientas y comandos ejecutados (esta verificación)

| Comando / herramienta | Resultado exacto |
|---|---|
| `dotnet build Ludeka.sln --no-restore` | exit 0; 0 advertencias, 0 errores; `build_output_hash=sha256:fff78a634d09e935deb1b21eeb40e6cb41a34d481f7e2d8b9918dc0e72ddaaa5` (sha256 del volcado completo stdout+stderr) |
| `dotnet test Ludeka.sln` | exit 0; **855 correctos, 0 fallidos, 0 omitidos**; `test_output_hash=sha256:3e294d43e6d7f34e08616099f6a6019732f8719b19b673a93b6595f9c3fd397c` |
| `dotnet test Ludeka.sln --filter FullyQualifiedName~WebMarkupContractTests` | exit 0; **103/103**; hash `sha256:c28d4787be7d03bf83b8878fb7e8296cac97d5a5b82419aeee62a94d3063ec1e` |
| `dotnet test Ludeka.sln --filter FullyQualifiedName~PerformanceAndAccessibilityTests` | exit 0; **5/5**; hash `sha256:05090ae46a42b6b15c55c43772234930a11580e214a179aa32b0b0b56dcb6354` |
| `dotnet test Ludeka.sln --filter FullyQualifiedName~HeroEditorial_AnchoDelContenedorDeclaradoEnElBloqueCss` | exit 0; **1/1** (Fact de la remediación, aislado) |
| `dotnet test Ludeka.sln --filter FullyQualifiedName~AppCss_FundacionInc36_Regenerada` | exit 0; **1/1** (fact compilado acotado, aislado) |
| `gh pr view 13 --json state,mergeable,…` | `{"state":"OPEN","mergeable":"MERGEABLE","headRefName":"inc/rediseno-paginas-editoriales-5","baseRefName":"inc/rediseno-paginas-editoriales-4"}` |
| `dotnet run --project src/Ludeka.Web --urls http://localhost:5199` | Servidor dev con sesión de Mesa Fundadora; detenido al final (`PORT_5199_FINAL_LISTENERS=0`) |
| Chrome DevTools MCP (`navigate_page`, `take_snapshot`, `evaluate_script`, `click`, `press_key`, `emulate`, `performance_start_trace`, `list_console_messages`) | Ciclo de modal ×2 rutas con instrumentación previa a la primera invocación, hero ×3 viewports, traza de rendimiento, observador LCP/CLS independiente, refresco de 7 rutas y consola; sin errores atribuibles al delta |

Nota de entorno: `resize_page` no baja de ~500 px reales; los viewports 360/640/1240 se aplicaron con `emulate` + recarga (Chrome real, no simulados). A 360 el cliente mide 345 px por la barra de scroll de layout (padding 20/20 → contenedor útil 305), igual que en el informe anterior.

## Evidencia interactiva

### F4.2 — Modal editorial: foco, Escape y restauración (✅ resuelto)

Instrumentación independiente: en cada página se parchearon `ludekaModal.open`/`close` **antes de la primera invocación** (el registro `open-enter` de texto crudo confirma que el parche ya estaba activo) y el disparador se marcó en DOM (`data-smoke-trigger`) para comparar identidad de nodo (`el === trigger`).

| Página | Foco antes | Registro de `open` | Cierre real | Diálogo tras cierre | `activeElement` tras cierre | `ludekaModal.close` | Identidad |
|---|---|---|---|---|---|---|---|
| `/sorteos` | `BUTTON "Proponer Sorteo"` | `open-enter\|BUTTON\|Proponer Sorteo` → `open-after\|BUTTON\|Cerrar Registrar o Proponer Sorteo` | **Escape real** (CDP `press_key`) | ausente | `BUTTON "Proponer Sorteo"` | **invocado**: `close-enter` → `close-after\|BUTTON\|Proponer Sorteo` | `activeEsDisparador=true`, `mismoNodoDisparador=true` |
| `/novedades` | `BUTTON "Añadir Novedad"` | `open-enter\|BUTTON\|Añadir Novedad` → `open-after\|BUTTON\|Cerrar Añadir Novedad Editorial` | **clic real en la X** | ausente | `BUTTON "Añadir Novedad"` | **invocado**: `close-enter` → `close-after\|BUTTON\|Añadir Novedad` | `activeEsDisparador=true`, `mismoNodoDisparador=true` |

Valores crudos con el diálogo abierto: `role="dialog"`, `aria-modal="true"`, `aria-label="Registrar o Proponer Sorteo"` / `"Añadir Novedad Editorial"`, botón `[data-editorial-cierre]` enfocado (`focusEnCierre=true`).

El contrato A es **estructural** (`mustNotContain "@if (_isCreateModalOpen)"` en las filas `Radar`/`News`): detecta la regresión del desmontaje, pero no puede probar la restauración de foco. La evidencia funcional de F4.2 es este smoke de navegador real.

### H1.1 / H1.2 / DD-01 — Ancho y alto del hero (✅ resuelto)

Regla compilada servida (extraída de `app.css`): `.hero-editorial{width:100%;aspect-ratio:16/9;min-height:200px;max-height:clamp(200px,36vw,460px)}`.

| Viewport (emulado) | `clientWidth` | Contenedor útil | Hero medido (x / ancho × alto) | `right` vs viewport | DD-01 esperaba | Resultado |
|---:|---:|---:|---|---:|---|---|
| 360×740 | 345 | 305 | x=20; **305 × 200** | 325 ≤ 345; sin desborde propio | 280×200 (suelo); ancho = contenedor | ✅ coincide |
| 640×800 | 625 | 585 | x=20; **585 × 230,39** | 605 ≤ 625 | 600×230 | ✅ coincide (36vw = 230,4) |
| 1240×900 | 1225 | 1185 | x=20; **1185 × 446,39** | 1205 ≤ 1225 | 1200×446 | ✅ coincide (36vw = 446,4) |

- `anchoIgualAlContenedor=true` en los tres casos (diferencia < 0,5 px) y `desbordePropioHero=false`. Desaparece la transferencia del ratio desde el alto clampado que producía 355,55 / 409,58 / 793,58 px de ancho y el borde derecho en 375,5 a 360 (informe anterior).
- Comparación directa con la medición previa: 355,55→**305**; 409,58→**585**; 793,58→**1185**. La tabla de DD-01 queda reproducida (antes: desviación WARNING).
- Matiz honesto a 360: la caja mide 305×200 (ratio 1,525) porque manda el suelo `min-height:200px`; es exactamente el comportamiento que tabula DD-01 para el suelo (280×200) y el recorte lateral residual lo absorben los focos autorales X. Sin desborde.
- La traza de rendimiento se re-ejecutó porque el delta toca el bloque CSS del hero.

### H1.3 — CLS y LCP bajo el modelo corregido (✅ re-ejecutado)

`performance_start_trace(reload: true, autoStop: true)` a 360×740, CPU 1×, sin throttling de red (servidor local):

| Métrica | Valor medido |
|---|---|
| LCP | **115 ms** (< 2500 ms) |
| Elemento LCP | `IMG` → `http://localhost:5199/images/home/hero-ilustracion.avif` (AVIF servido) |
| Desglose LCP | TTFB 11 ms · load delay 9 ms · load duration 5 ms · render delay 91 ms |
| CLS | **0,00** |
| Observador independiente | LCP 116 ms (misma URL), CLS 0, **0** entradas `layout-shift` |

### Refresco de navegación (7 rutas a 360)

Todas cargan (`readyState=complete`) con exactamente un `<h1>`:

| Ruta | Título | `<h1>` | Marcadores observados |
|---|---|---:|---|
| `/` | Ludeka — El Letterboxd de los juegos de mesa en español | 1 | hero 305×200, `hero-scene`, h1 `sr-only` «La mesa está servida» |
| `/catalogo` | Catálogo de Juegos de Mesa | 1 | `.page-header-title`, `scrollbar-none` presente / `no-scrollbar` ausente, 41 `.game-card-editorial` |
| `/juegos/wingspan` | Wingspan — Ficha Inteligente | 1 | `flex-wrap` presente; errata «Ludeca» ausente; `detail-hero-backdrop` presente |
| `/eventos` | Grandes Citas y Ferias Lúdicas | 1 | 6 `.rail-card`, 2 `aria-controls="panel-…"`, 1 `role="tabpanel"` |
| `/radar` | Radar de Sorteos | 1 | 5 `.rail-card`, 6 referencias a `sorteo-default.svg` |
| `/novedades` | Novedades y Estrenos Editoriales | 1 | 4 `.rail-card`, `novedad-default.svg`, 0 `<img>` con `src` vacío |

Consola del navegador: solo el warning preexistente del manifiesto (`icon-192.png`) y 5× HTTP 400 de imágenes externas `cf.geekdo-images.com` (fallback I2.2 preexistente, fuera de alcance). Sin errores atribuibles al delta.

## Matriz de requisitos y escenarios

`COMPLIANT` = evidencia ejecutada y pasada. La columna «Evidencia en esta ronda» indica si el escenario se re-ejecutó en esta verificación fresca o se referencia del informe anterior porque el delta no lo toca (sin fabricar PASS nuevos donde no hubo re-ejecución).

| # | Escenario | Evidencia | Resultado | Evidencia en esta ronda |
|---:|---|---|---|---|
| F1.1 | `--on-brand` definido y accesible en cinco temas | Fact CSS + cálculo WCAG | ✅ COMPLIANT | heredado (el delta no toca tokens) |
| F1.2 | Botones de marca usan `--on-brand` | contratos de markup + smoke de temas | ✅ COMPLIANT | contratos re-ejecutados (103/103); smoke heredado |
| F2.1 | Tokens de estado en cinco temas | Fact CSS por bloques | ✅ COMPLIANT | heredado (tokens intactos) |
| F2.2 | Sin hardcodes de estado en páginas contratadas | 103 contratos + `git grep` acotado | ✅ COMPLIANT | contratos re-ejecutados |
| F2.3 | Sin `dark:` inerte ni `darkMode` por tema | contratos + `tailwind.config.js` | ✅ COMPLIANT | contratos re-ejecutados |
| F3.1 | Cabecera compartida en cuatro listados | contratos + navegación de las 4 rutas | ✅ COMPLIANT | rutas re-ejecutadas |
| F3.2 | Ficha no usa `PageHeaderEditorial` | fuente re-verificada (0 coincidencias; `detail-hero-backdrop` presente) + ruta (h1 «Wingspan») | ✅ COMPLIANT | re-ejecutado |
| F3.3 | Un único `h1` por listado | navegador: 1 h1 en las 7 rutas | ✅ COMPLIANT | re-ejecutado |
| F4.1 | Radar y News adoptan `EditorialModal` | contratos + inspección de shells | ✅ COMPLIANT | re-ejecutado (contratos + smoke) |
| F4.2 | Etiqueta, modal, cierre, foco y restauración | navegador: ARIA y entrada OK; `close` invocado; foco al disparador (mismo nodo) | ✅ **COMPLIANT** (antes FAILING) | re-ejecutado |
| F5.1 | Tarjetas de página usan `.rail-card` | contratos + CSS + navegador (6 eventos, 5 sorteos, 4 novedades) | ✅ COMPLIANT | rutas re-ejecutadas |
| F5.2 | Catálogo mantiene `.game-card-editorial` | contrato + 41 tarjetas en navegador | ✅ COMPLIANT | re-ejecutado |
| F5.3 | `focus-visible` y reduced motion | `input.css` + contrato CSS | ✅ COMPLIANT | heredado (CSS de rail intacto) |
| F6.1 | Back-bar envuelve y agrupa moderación | fuente + navegación de ficha (`flex-wrap` presente) | ✅ COMPLIANT | ruta re-ejecutada |
| F6.2 | Filtros usan `scrollbar-none` | navegador `/catalogo`: presente, `no-scrollbar` ausente | ✅ COMPLIANT | re-ejecutado |
| F6.3 | Tabs enlazan `aria-controls`/`tabpanel` | navegador `/eventos`: `panel-upcoming`/`panel-past` | ✅ COMPLIANT | re-ejecutado |
| F6.4 | Título Ludeka y estado inexistente legible | fuente + ficha existente/inexistente | ✅ COMPLIANT | heredado (delta no lo toca) |
| F7.1 | `app.css` contiene fundación regenerada | facts + regla `.hero-editorial{…}` + hashes | ✅ COMPLIANT | re-ejecutado (5/5 y regla servida) |
| H1.1 | Alto proporcional a 360 px | navegador: 200 px de alto, ancho = contenedor (305), sin desborde | ✅ COMPLIANT | re-ejecutado |
| H1.2 | Cap/suelo a 640 y 1240 px | navegador: 230,39 y 446,39 px, sin media queries | ✅ COMPLIANT | re-ejecutado |
| H1.3 | CLS 0 | traza + observador: CLS 0,00, 0 `layout-shift` | ✅ COMPLIANT | re-ejecutado |
| H1.4 | Contrato CSS sin alturas antiguas | contrato + CSS compilado | ✅ COMPLIANT | contratos re-ejecutados |
| H2.1 | Buscar AA en cinco temas | cálculo WCAG + markup SSR | ✅ COMPLIANT | heredado (el delta no toca el botón) |
| H3.1 | Hero protagonista sin shells prohibidos | smoke + contrato hero | ✅ COMPLIANT | contrato re-ejecutado |
| H3.2 | Foco autoral por variante | CSS + cuatro `?hero=` | ✅ COMPLIANT | heredado (CSS de focos intacto) |
| H3.3 | Prohibiciones INC-35 conservadas | contrato hero + smoke | ✅ COMPLIANT | contrato re-ejecutado |
| H4.1 | Picture, formatos, prioridad, dimensiones y alt | contrato + navegador (AVIF `hero-ilustracion.avif` servido en LCP) | ✅ COMPLIANT | re-ejecutado |
| H4.2 | Peso, LCP y CLS | navegador: LCP 115 ms, CLS 0 | ✅ COMPLIANT | re-ejecutado |
| H4.3 | Fallback del hero ante error de imagen | navegador: 404 real, `hero-bg-media--failed`, escena visible | ✅ COMPLIANT | heredado (delta no toca `onerror`) |
| H4.4 | Buscador rápido conserva navegación | test 1/1, término y vacío | ✅ COMPLIANT | heredado (contrato intacto) |
| I1.1 | Novedad sin imagen muestra default | navegador `/novedades`: 4 tarjetas con `novedad-default.svg`, 0 `src` vacíos | ✅ COMPLIANT | re-ejecutado |
| I1.2 | Novedad externa tiene dimensiones/fallback | contrato y markup (sin muestra externa en datos servidos) | ✅ COMPLIANT | contrato re-ejecutado |
| I2.1 | Sorteo sin imagen muestra default | navegador `/radar` y `/sorteos`: `sorteo-default.svg` | ✅ COMPLIANT | rutas re-ejecutadas |
| I2.2 | Falla externa sustituye por default | fallo real geekdo + prueba determinista | ✅ COMPLIANT | heredado (fallo 400 visible en consola) |

**Resumen de cumplimiento: 34/34 escenarios conformes. 13/13 requisitos.**

## Tabla de requisitos

| Requisito | Estado | Motivo |
|---|---|---|
| Fundaciones `--on-brand` | ✅ | Completo |
| Tokens de estado | ✅ | Completo |
| `PageHeaderEditorial` | ✅ | Completo (h1 único en las 4 rutas) |
| `EditorialModal` | ✅ | F4.2 resuelto y verificado con navegador (cierre + restauración de foco) |
| `.rail-card` | ✅ | Completo |
| Fixes transversales | ✅ | Completo |
| `app.css` regenerado | ✅ | Regla `.hero-editorial{width:100%;…}` servida; facts verdes |
| Hero responsivo | ✅ | Ancho = contenedor y alturas DD-01 a 360/640/1240 |
| Contraste del hero | ✅ | Completo |
| Hero protagonista/focos | ✅ | SSR y contratos completos |
| Rendimiento/fallback hero | ✅ | LCP 115 ms, CLS 0, fallback con 404 real |
| Fallback de News | ✅ | Runtime de `DefaultImage` en `/novedades` |
| Fallback de GiveawayCard | ✅ | Fallo real y prueba determinista |

## Coherencia de diseño

| Decisión | Resultado |
|---|---|
| DD-01 ratio/suelo/cap | ✅ Reproducido tras la remediación: ancho = contenedor (305/585/1185) y alturas 200/230,39/446,39; sin transferencia del ratio ni desborde a 360 |
| DD-02 focos | ✅ Cuatro `object-position` autorales y rutas verificadas |
| DD-03/04 tokens | ✅ Valores, estados y reduced motion presentes |
| DD-06 cabecera | ✅ API compartida y cuatro adopciones |
| DD-07 modal | ✅ Shell/ARIA/JS y restauración de foco funcional en ambas páginas (componente siempre montado) |
| DD-08 back-bar | ✅ Estructura SSR verificada |
| DD-09 contratos | ✅ 103/103 y facts verdes (incluido el Fact acotado de la remediación) |
| DD-10 compilación | ✅ Salida regenerada; regla compilada verificada por dos aserciones acotadas |
| DD-11 PR chain | ✅ PR #13 `OPEN`/`MERGEABLE` sobre la rama `-4` (comprobado con `gh` en esta ronda) |

## TDD estricto

Existe `TDD Cycle Evidence` para PR-1 a PR-5 y para la remediación F4.2 (filas A y B). Las filas declaradas se cruzaron con la ejecución real de esta ronda.

| Comprobación | Resultado |
|---|---|
| Evidencia TDD reportada | ✅ Tabla presente (PR-1..PR-5 + remediación A/B) |
| Todas las tareas tienen pruebas | ✅ 22/22 + 2 unidades de remediación |
| RED confirmado | ✅ Archivos y aserciones existen; los `mustNotContain` y el Fact acotado fallaron en su ciclo declarado y hoy pasan |
| GREEN confirmado | ✅ 855/855 suite; 103/103 y 5/5 focales; los dos facts de remediación pasan aislados (1/1 cada uno) |
| Triangulación | ✅ A: navegador ×2 rutas; B: navegador ×3 viewports + aserciones acotadas |
| Safety net | ✅ 102/102 y 854/854 previos declarados (coherentes con los 103/855 actuales) |

**Cumplimiento TDD: 6/6.**

### Capas de prueba y aserciones

| Capa | Casos | Archivos |
|---|---:|---|
| Unitaria/contrato | 115 | 4 (`WebMarkup` 103, `Performance` 5, `QuickSearch` 1, `DefaultImage` 6) |
| Integración | 0 | No hay bUnit |
| E2E navegador real | 21 comprobaciones | Chrome DevTools MCP (modal ×2 ciclos completos, hero ×3 viewports + traza + observador independiente, rutas ×7, consola ×1) |
| SSR/HTTP manual | 17 comprobaciones | Harness PowerShell del informe previo (vigente; el delta no toca SSR) |

Auditoría de aserciones del delta (Step 5f): el Fact nuevo parsea el bloque CSS real y falla si desaparece `width: 100%` (ROJO demostrado en su ciclo); la aserción de `app.css` se acota a la regla `.hero-editorial{…}` y comprueba `width:100%` dentro (no hay `width:100%` global que la haga vacua); las dos aserciones `mustNotContain "@if (_isCreateModalOpen)"` fallan si vuelve el envoltorio. **0 CRITICAL, 0 WARNING** en el delta; la auditoría de tautologías/ghost loops del resto se mantiene del informe previo (mismos archivos, sin cambios materiales). Nota: los contratos estructurales no pueden probar restauración de foco; esa evidencia es el smoke de navegador de F4.2.

## Cobertura de archivos modificados

Se hereda la cobertura Coverlet del informe previo (854/854, exit 0): los componentes Razor no se renderizan desde tests unitarios y el delta de remediación es CSS + 2 líneas de Razor + contratos; el `width: 100%` y el `app.css` no son instrumentables por Coverlet. No se repite la corrida de cobertura por proporcionalidad y sin cambio material.

| Archivo | Líneas | Ramas | Cobertura |
|---|---:|---:|---|
| `HeroBackgroundVariant.cs` | 18/27 (66,7%) | 10/18 (55,6%) | ⚠️ |
| `HeroEditorial.razor` | 20/50 (40,0%) | 2/24 (8,3%) | ⚠️ |
| `Events.razor`, `GameDetail.razor`, `Home.razor`, `News.razor`, `Radar.razor`, `EditorialModal.razor`, `GiveawayCard.razor`, `PageHeaderEditorial.razor` | 0% | 0% | ⚠️ sin capa de render |
| `App.razor`, CSS y `editorial-modal.js` | N/A | N/A | no instrumentables por Coverlet |

## Warnings y riesgos

**CRITICAL**: Ninguno.

**WARNING** (ninguno bloquea archive):
1. Desborde horizontal preexistente del nav (`scrollWidth` 501 a 360 px; `MainLayout.razor` sin cambios en el incremento): fuera de alcance, documentado. El hero ya no contribuye (sin desborde propio).
2. La `safelist` de `tailwind.config.js` conserva utilidades cromáticas genéricas antiguas en el CSS global; los seis archivos contratados no las usan.
3. Browserslist/caniuse-lite desactualizado durante Tailwind, sin efecto en la salida.
4. `apply-progress.md` atribuye las filas News/Giveaway de 5.3 a `dd7f42c`; `git blame` de los contratos las sitúa en `2f67036`. Observación documental, sin reescritura post-settlement.
5. Cobertura 0% de componentes Razor por ausencia de capa de render/browser.
6. La restauración de foco del modal sigue sin cobertura automatizada (el contrato es estructural); su evidencia es el smoke de navegador de esta verificación.

**SUGGESTION**: incorporar en el futuro (a) prueba automatizada de restauración de foco del modal (capa de render/browser) y (b) valorar contención/`scroll-margin` del desborde preexistente del nav en un incremento propio.

## Veredicto final

**PASS — 13/13 requisitos y 34/34 escenarios conformes; build y suite verdes (855/855, 103/103, 5/5); sin blockers ni hallazgos críticos.** La remediación `310c464`/`379c074` resolvió el FAIL anterior (F4.2) y la desviación DD-01, ambos confirmados con navegador real en esta verificación fresca ligada después del asentamiento nativo. `sdd-archive` puede proceder.

## Artefactos

- `openspec/changes/rediseno-paginas-editoriales/verify-report.md` (este informe; único archivo no rastreado intencionado del worktree).
- No se modificó código de aplicación, contratos, `tasks.md`, `apply-progress.md` ni `app.css`.
- No se ejecutó `gentle-ai sdd-attempt acquire|settle` ni `sdd-archive`.
- Servidor detenido y puerto 5199 libre (`PORT_5199_FINAL_LISTENERS=0`).
