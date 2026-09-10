# Reporte de Archivo — `rediseno-paginas-editoriales` (INC-36)

> Fase: `sdd-archive` · Modo: hybrid (espejo en Engram `sdd/rediseno-paginas-editoriales/archive-report`) · Fecha: 2026-09-10 · Idioma: español castellano (regla suprema del repo).
> Este documento es el registro terminal del ciclo SDD: describe el estado del cambio AL CIERRE, no instantáneas intermedias.

## 1. Cierre

El cambio `rediseno-paginas-editoriales` (INC-36) queda **archivado** tras completar las fases explore → propose → spec → design → tasks → apply → verify. Verificación final: **PASS** — **13/13 requerimientos y 34/34 escenarios COMPLIANT**, 0 blockers y 0 hallazgos críticos, con evidencia ligada al asentamiento nativo (`evidence_revision: sha256:19aa14ff7340bc9af6f00da14923c45100d39aaa9ac8e277a12f5ab7938c8d8f`). La carpeta del cambio se traslada a `openspec/changes/archive/2026-09-10-rediseno-paginas-editoriales/`.

**Task Completion Gate:** `tasks.md` archivado con **22/22 tareas `[x]`**; no hubo reconciliación de casillas (no quedó ninguna pendiente).

## 2. Estado Final del Cambio

Jerarquía de autoridad aplicada (Final-State Authority): los hechos de estado final del prompt de lanzamiento del orquestador son la cuenta más reciente y prevalecen sobre instantáneas intermedias (`verify-report`, `apply-progress`).

| Métrica | Estado final | Fuente |
|---|---|---|
| Suite de pruebas | **855/855** superadas, exit 0 (baseline 854 + el Fact acotado `HeroEditorial_AnchoDelContenedorDeclaradoEnElBloqueCss`; justificación del +1 verificada) | Prompt del orquestador (corroborado por `verify-report` y por el ciclo RED/GREEN declarado) |
| Contratos de markup | **103/103** (`WebMarkupContractTests`); `PerformanceAndAccessibilityTests` **5/5** | Prompt del orquestador + `verify-report` |
| Build | `dotnet build Ludeka.sln --no-restore` exit 0; 0 advertencias / 0 errores | `verify-report` |
| Verificación SDD | **PASS** nativo; 13/13 requerimientos, 34/34 escenarios; 0 CRITICAL; evidencia `sha256:19aa14ff…` | Prompt del orquestador + `verify-report` |
| Tareas | **22/22** marcadas `[x]` (sin casillas pendientes) | `tasks.md` |
| Entrega | **6 PRs apilados abiertos** (`#8` → `#13`), base del primero `main`; **pendiente del merge ordenado por el maintainer** | Prompt del orquestador + `gh pr list` (comprobado en la fase) |
| Código de aplicación en esta fase | 0 líneas (archivo puro) | Diff del commit de archivo |

## 3. Sincronización de Delta Specs (delta → fuente de verdad)

Punto de partida: `openspec/specs/` con 32 dominios. Resultado: **33 dominios** (1 nuevo + 2 actualizados).

| Dominio | Acción | Detalle |
|---|---|---|
| `editorial-page-foundations` | **Creado (FULL)** | No existía spec canónica: promoción mecánica del delta completo (copia por shell + `diff` vacío + SHA256 idéntico `9D217599271CC756…`) |
| `home-landing-hero` | **Actualizado (2 ADDED + 2 MODIFIED)** | Composición nativa: 8 → **10 requisitos**; `git diff --numstat` **+73 / −4** |
| `default-image-fallbacks` | **Actualizado (2 ADDED)** | Composición nativa: 5 → **7 requisitos**; `git diff --numstat` **+34 / −0** (aditiva pura) |

### 3.1 Composición nativa y desviación operativa documentada

Composición ejecutada con el comando nativo obligatorio `gentle-ai sdd-archive-compose` (v2.7.0). **Primer intento rechazado** con el mensaje exacto:

```
Error: sdd-archive-compose: DELTA: delta spec declares no ADDED, MODIFIED, REMOVED, or RENAMED requirements
```

**Causa raíz diagnosticada (no es un delta malformado):** el checkout de Git for Windows de este worktree deja los archivos en **CRLF** en el árbol de trabajo (`git ls-files --eol` → `w/crlf`) mientras el índice almacena **LF** (`i/lf`); el parser del compositor no reconoce las cabeceras `## ADDED/MODIFIED Requirements` con terminación `\r`. El delta sí declara secciones (verificado: `## ADDED Requirements` en L7 y `## MODIFIED Requirements` en L52 de `home-landing-hero`).

**Resolución mecánica (sin merge manual Read/Edit, prohibido por el contrato):** se ejecutó el compositor nativo sobre **copias normalizadas a LF** de canónica y delta, byte-equivalentes a los blobs del índice git (`i/lf`). Evidencia por dominio:

- Comando ejecutado (por dominio, con rutas temporales): `gentle-ai sdd-archive-compose --canonical "<tmp>/canonical.lf.md" --delta "<tmp>/delta.lf.md" --output "<tmp>/out.lf.md"` → **exit 0** en ambos.
- Readback de la salida del compositor contra el archivo instalado (temporalmente en CRLF): `diff --strip-trailing-cr` → **salida vacía, exit 0** en ambos dominios.
- `git diff --numstat` de la composición: `+73/−4` (`home-landing-hero`) y `+34/−0` (`default-image-fallbacks`) — sin pérdida de requisitos (8→10 y 5→7, los nombres previos preservados).
- El archivo canónico se escribe en CRLF (convención del árbol de trabajo) y Git lo normaliza al comparar: sin ruido en `git status`.

La misma normalización LF se aplicó a la única spec nueva (§4). No se modificó ningún artefacto de fase (los deltas archivados conservan sus bytes originales).

## 4. Spec Nueva: Promoción Mecánica (`editorial-page-foundations`)

```
READBACK 1: diff -r origen vs copia temporal   → vacío, exit 0
READBACK 2: diff -r origen vs destino final    → vacío, exit 0
SHA256 origen : 9D217599271CC756C8961DBBED435ECF406C883FCAE5B9E9FB9840352A5B3899
SHA256 destino: 9D217599271CC756C8961DBBED435ECF406C883FCAE5B9E9FB9840352A5B3899
```

Copia `cp`-equivalente (`Copy-Item`) + `Move-Item`, sin que el contenido pasara por el modelo.

## 5. Movimiento a Archivo

- Origen: `openspec/changes/rediseno-paginas-editoriales/`
- Destino: `openspec/changes/archive/2026-09-10-rediseno-paginas-editoriales/`
- Método: snapshot recursivo previo (`9` archivos) → `git mv` (exit 0) → verificación de origen ausente y destino presente → **readback `diff -r` del snapshot contra el destino: salida vacía, exit 0**.
- `git status` confirmó **8 renombres `R`** de artefactos rastreados + `verify-report.md` (no rastreado intencionado) viajando dentro de la carpeta, ya staged en el commit de archivo.
- Contenido archivado: `proposal.md` ✅ · `explore.md` ✅ · `specs/` (3 deltas) ✅ · `design.md` ✅ · `tasks.md` ✅ (22/22) · `apply-progress.md` ✅ · `verify-report.md` ✅ · `archive-report.md` (este documento) ✅.
- El directorio de cambios activos ya **no** contiene `rediseno-paginas-editoriales/`.

**Readback verbatim (ambos bloques):**

```
== READBACK diff -r snapshot vs destino ==
DIFF_EXIT=0

== READBACK diff -r origen vs destino final (spec nueva) ==
DIFF_FINAL_EXIT=0
```

## 6. Volcado a la Especificación Viva del Sistema

- **Creado:** `docs/specs/sistema/24-fundaciones-editoriales-y-componentes.md` — tokens `--on-brand` y de estado ×5 temas, `PageHeaderEditorial`, `EditorialModal` + `editorial-modal.js`, lenguaje `.rail-card` en tarjetas de página, fixes transversales, pipeline de `app.css`, arquitectura tocada por capa, estrategia de pruebas, fuera de alcance y referencias.
- **Actualizados:**
  - `23-portada-editorial.md` — estado vigente del hero (sin titular visible ni píldoras; h1 `sr-only`), nueva §2.1 con el fix responsive (DD-01/DD-02) y el botón Buscar AA, LCP 115 ms / CLS 0 medidos, pruebas 855 y specs vivas.
  - `15-dashboard-inicio-editorial.md` — nota INC-36 y detalle de `Home.razor` (cabecera compartida, búsqueda propia, `scrollbar-none`).
  - `01-catalogo-y-fichas.md` — `GameDetail.razor` tokenizado, back-bar envolvente, `<PageTitle>` «Ludeka» y estado no-encontrado; `Home.razor` editorial.
  - `16-sorteos-novedades-y-eventos.md` — cabeceras, `EditorialModal`, `.rail-card`, fallbacks de imagen y accesibilidad de pestañas en las 3 verticales + `GiveawayCard`.
  - `README.md` — índice con el módulo 24, entrada 23 ampliada y total de pruebas automáticas verificadas actualizado a **855**.

## 7. Incremento y Roadmaps

- `docs/increments/inc-36-rediseno-paginas-editoriales.md` → movido con `git mv` a `docs/increments/archive/`, con metadatos actualizados a estado final (✅ Completado y Archivado, 855/855, PASS 13/13 y 34/34, cadena #8–#13 pendiente de merge, artefactos SDD y módulos enlazados).
- `docs/increments/ROADMAP.md`: INC-36 → **✅ Archivado** (documento enlazado al archivo); sección «Incrementos en Curso» limpiada (sin incrementos activos; se deja constancia de la cadena #8–#13 pendiente del merge ordenado).
- `docs/specs/ROADMAP_MVP_SLICES.md`: Incremento 36 → **✅ Completado y Archivado** con documento y 5 módulos del sistema enlazados.

## 8. Commits

**Del incremento (25 commits, `main..HEAD`, en orden cronológico):**

| Commit | Contenido |
|---|---|
| `59e639f` | Propuesta SDD INC-36 |
| `fd5b085` | Specs, diseño y tareas SDD |
| `4c30751` | Tokens `--on-brand` y de estado en los cinco temas |
| `b36c4cd` | Hero responsivo con foco por variante y botón accesible |
| `09c12e3` | `app.css` regenerado con la fundación INC-36 |
| `81b89c1`, `9bbaad8` | Corrección de la clase de foco del hero + fix de evaluación en `class` |
| `1f9235b`, `e943b5a` | `PageHeaderEditorial` y `EditorialModal` compartidos |
| `04f0e17`, `f09da6f`, `9184a0d` | Progreso apply PR-1 (+ nota de partición) |
| `3b1a392`, `c3eb4ea` | PR-2 Catálogo editorial + progreso |
| `5ed2cfd`, `abd77ad` | PR-3 Ficha tokenizada + progreso |
| `6fed384`, `42eceb0` | PR-4 Eventos editoriales + progreso |
| `2f67036`, `dd7f42c`, `c08cd9b` | PR-5 Sorteos + Novedades/GiveawayCard + progreso y casillas |
| `09949a2`, `310c464`, `379c074`, `d7b1f43` | Enlace PR-5, remediación F4.2 (modal siempre montado) y DD-01 (`width: 100%` del hero) + docs |

**De la fase de archivo:** `docs: archiva el change inc-36 y sincroniza delta specs` y `docs: especificacion viva, incremento y roadmaps de inc-36` (este reporte viaja en el segundo). Rama `inc/rediseno-paginas-editoriales-5` pusheada; PR **#13** se actualiza solo.

## 9. Totales de Pruebas

| Métrica | Valor |
|---|---|
| Suite total | **855/855** (exit 0 en la verificación) |
| Baseline al inicio del incremento | 854 (INC-35 cerró su archivo con 847; los commits posteriores de INC-35 elevaron la cuenta a 854) |
| Contratos (`WebMarkupContractTests`) | **103/103** |
| Añadidas por INC-36 | **+1 neto** (baseline 854 → 855: el Fact acotado del ancho del hero; el resto fueron aserciones dentro de contratos existentes) |
| Build | 0 errores / 0 advertencias |

## 10. Verificación, Warnings y Riesgos Residuales

**CRITICAL:** ninguno (0 blockers, 0 hallazgos críticos). Warnings no bloqueantes registrados para el maintainer:

1. Desborde horizontal **preexistente** del nav en 360 px (`scrollWidth` 501; `MainLayout.razor` sin cambios en el incremento) — fuera de alcance; el hero ya no contribuye.
2. La `safelist` de `tailwind.config.js` conserva utilidades cromáticas genéricas antiguas en el CSS global; los 6 archivos contratados no las usan.
3. Browserslist/caniuse-lite desactualizado durante Tailwind, sin efecto en la salida.
4. `apply-progress.md` atribuye las filas News/Giveaway de 5.3 a `dd7f42c`; `git blame` las sitúa en `2f67036` — observación documental, sin reescritura post-asentamiento.
5. Cobertura 0% de componentes Razor por ausencia de capa de render/browser (decisión de patrón INC-31/35).
6. La restauración de foco del modal no tiene cobertura automatizada (el contrato es estructural); su evidencia es el smoke de navegador real de la verificación.

**SUGGESTIONS:** (a) prueba automatizada de restauración de foco (capa de render/browser); (b) valorar contención/`scroll-margin` del desborde preexistente del nav en un incremento propio.

## 11. Trazabilidad Engram (modo hybrid)

Artefactos leídos como referencia cruzada: explore **#185**, proposal **#186**, spec **#187**, design **#188**, tasks **#190**, apply-progress + remediación **#191**; contexto de verificación **#210/#212**. El `verify-report` canónico vive como artefacto de archivo validado por `gentle-ai sdd-verify-validate` (`evidence_revision: sha256:19aa14ff…`). El presente reporte se espeja en Engram con `topic_key: sdd/rediseno-paginas-editoriales/archive-report`.

## 12. Checklist de Archivo

- [x] Delta specs sincronizadas ANTES del movimiento (1 creada FULL + 2 compuestas con comando nativo)
- [x] Carpeta del cambio movida a `archive/` con patrón del repo + fecha ISO
- [x] El archivo contiene todos los artefactos (proposal, explore, specs, design, tasks, apply-progress, verify-report, archive-report)
- [x] `tasks.md` archivado sin tareas de implementación sin marcar (22/22)
- [x] El directorio de cambios activos ya no contiene este cambio
- [x] Readback `diff -r`/`diff --strip-trailing-cr` vacío y verbatim en este reporte (specs y movimiento)
- [x] Roadmaps actualizados e incremento trasladado a `docs/increments/archive/`
- [x] Especificación viva del sistema actualizada (módulo 24 nuevo + 4 módulos + README con 855)
- [x] Rama pusheada; PR #13 actualizado

## 13. Próximo Paso (fuera de esta fase)

**Merge ordenado de la cadena por el maintainer:** PR **#8 → #9 → #10 → #11 → #12 → #13** (cada PR apilado sobre la rama del anterior; el retarget de bases en GitHub se hace al mergear en orden). Con la cadena mergeada, ejecutar `scripts/sdd-worktree.ps1 done rediseno-paginas-editoriales` para eliminar worktree y rama local. Restricciones respetadas en esta fase: **no** se ejecutó `gentle-ai sdd-attempt acquire|settle`, **no** hubo merges y **no** se ejecutó `done`.
