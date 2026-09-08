# Reporte de Archivo — `portada-minimalista-creadores` (INC-31)

> Fase: `sdd-archive` · Modo: hybrid · Fecha: 2026-09-08 · Idioma: español castellano (regla suprema del repo).
> Este documento es el registro terminal del ciclo SDD: describe el estado del cambio AL CIERRE, no instantáneas intermedias.

## 1. Cierre

El cambio `portada-minimalista-creadores` (INC-31) queda **archivado** tras completar las fases explore → propose → spec → design → tasks → apply → verify. Verificación final: **PASS nativo** (`gentle-ai sdd-verify-validate` `valid:true`) con 15/15 requerimientos y 22/22 escenarios COMPLIANT. La carpeta del cambio se traslada a `openspec/changes/archive/2026-09-08-portada-minimalista-creadores/`.

## 2. Estado Final del Cambio

Jerarquía de autoridad aplicada (Final-State Authority): los hechos de estado final del prompt de lanzamiento del orquestador son la cuenta más reciente y prevalecen sobre instantáneas intermedias (`verify-report`, `apply-progress`).

| Métrica | Estado final | Fuente |
|---|---|---|
| Suite de pruebas | **723/723** superadas (baseline 705 de INC-30 + 16 de INC-31 + 2 de remediación), exit 0 | Prompt del orquestador (confirmado por `verify-report` #159 y commit f8ff077) |
| Build | 0 errores / 0 advertencias | `verify-report` (sellado) |
| Verificación SDD | PASS nativo (`valid:true`), 15/15 requerimientos, 22/22 escenarios COMPLIANT, 0 UNTESTED, 0 FAILING | Prompt del orquestador + `verify-report` #159 |
| Renders reales verificados | `/`, `/sorteos`, `/radar`, `/creadores`, `/autores`, ficha de creador — HTTP 200 con invariantes del spec | `verify-report` (dos ejecuciones de verify) |
| Tareas | 20/20 marcadas `[x]` (Task Completion Gate: sin checkboxes pendientes) | `tasks.md` |
| Hallazgos CRITICAL | 0 (los 2 CRITICAL de cobertura del primer verify quedaron remediados por f8ff077) | `verify-report` re-ejecución |

Nota sobre advertencias históricas: los 2 CRITICAL de cobertura del verify anterior y el estado "721 tests" de `apply-progress` son **instantáneas intermedias ya superadas**; el estado final es el de la tabla anterior.

## 3. Entrega

- **PR único** con `size:exception` aprobada por el mantenedor (~660 líneas de código real; diff 424+/238−; presupuesto de 400 excedido con aprobación explícita — ver Engram #151 y #158).
- Sin código de producto en la fase de archivo.

## 4. Commits del Cambio (en orden)

| Commit | WU | Contenido |
|---|---|---|
| `d9d3cfe` | WU1 | Sembrar solo creadores de contenido con purga de diseñadores retirados (T1-T4) |
| `0070ebd` | WU2 | Desacoplar CreatorService del cruce por `Game.Designer` (T5) |
| `7cbd0e3` | WU3 | Fichas con diseñador en texto plano y reetiquetado a creadores (T6a) |
| `60f0b44` | WU4 | Hero minimalista en portada y radar sin banner legacy (T6b) |
| `b537bc0` | WU5 | Registrar INC-31 en roadmaps |
| `f8ff077` | Remediación | Cubrir búsqueda rápida de portada y ficha de creador sin redes (+2 tests → 723/723) |

## 5. Sincronización de Especificaciones (delta → fuente de verdad)

`openspec/specs/` pasa de 27 a **29 dominios**.

| Dominio | Acción | Detalle |
|---|---|---|
| `home-landing-hero` | **Creado (FULL)** | Promoción mecánica del delta (ya era spec completo): copia por shell + `diff` vacío + SHA256 idéntico (`992E0726DF60144E…`) |
| `creators-directory` | **Creado (FULL)** | Promoción mecánica del delta (ya era spec completo): copia por shell + `diff` vacío + SHA256 idéntico (`2E0E00A46D4ECBE1…`) |
| `giveaway-radar` | **Actualizado (delta ADDED)** | 2 requerimientos añadidos / 3 escenarios; REQ-GIV-01..04 intactos |

### 5.1 Composición de `giveaway-radar` — desviación documentada

El comando nativo obligatorio `gentle-ai sdd-archive-compose` (v2.7.0) **rechazó la composición** con el mensaje exacto:

```
Error: sdd-archive-compose: CANONICAL: canonical spec has no "### Requirement:" headings to compose against
```

Motivo: el spec canónico del repo usa el formato legacy en español (`### REQ-GIV-NN:`), incompatible con el matcher del compositor (que solo reconoce cabeceras `### Requirement:` de OpenSpec). El comando escribió **nada** (fallo limpio; spec canónico intacto, verificado con `git status`).

**Resolución:** en lugar de un merge manual vía Read/Edit (prohibido por el contrato mecánico), se aplicó el delta ADDED mediante **sutura mecánica a nivel de bytes por shell** (PowerShell: lectura/escritura de bytes; el contenido jamás pasó por generación del modelo), añadiendo los bloques `### Requirement:` extraídos byte-exactos de la cola del delta bajo la sección nueva `## 4. Requerimientos de la Página /sorteos: Renderizado y Enrutado (añadidos por INC-31)`. **Evidencia objetiva equivalente a la garantía del compositor para un delta pure-ADDED:**

- `git diff --numstat`: **31 adiciones / 0 eliminaciones** (ninguna línea existente tocada).
- `git diff -U2`: solo líneas `+` tras la última línea original.
- REQ-GIV-01..04 verificados presentes en L10/24/29/37 tras la sutura.
- Conteo previo a la escritura: 2 `### Requirement:` / 3 `#### Scenario:` exactos (coincide con verify: GR 2 requerimientos / 3 escenarios).

## 6. Volcado a la Especificación Viva del Sistema

- **Creado:** `docs/specs/sistema/22-portada-y-directorio-creadores.md` — alcance funcional (hero, píldoras, bugfix novedades, banner legacy, directorio de creadores), decisiones D1-D5 (con AD-1..AD-7 referenciadas), arquitectura tocada por capa, flujo de seed/purga, contratos, componentes y rutas, estrategia de pruebas y follow-ups.
- **Actualizado:** `docs/specs/sistema/README.md` — módulo 22 añadido al índice y total de pruebas actualizado: **723 pruebas pasando al 100%** (antes 681).

## 7. Roadmaps

- `docs/increments/ROADMAP.md`: INC-31 → **✅ Archivado**, documento enlazado al proposal archivado.
- `docs/specs/ROADMAP_MVP_SLICES.md`: Incremento 31 → **✅ Completado y Archivado (723 tests en verde al 100%)**, con módulo del sistema 22 enlazado.

## 8. Documento de Incremento (`docs/increments/inc-31.md`)

**No existe** `docs/increments/inc-31.md` (verificado; `docs/increments/` solo contiene `ROADMAP.md` y `archive/` con inc-01..inc-30). A diferencia de INC-01..INC-30, este incremento nunca tuvo documento propio en `docs/increments/`: el registro del incremento vivió en la carpeta SDD (`openspec/changes/portada-minimalista-creadores/`) y así lo reflejaba la columna "Documento" del roadmap. No se inventa ningún archivo: el rol de documento del incremento queda cubierto por el proposal archivado (enlaces de ambos roadmaps actualizados a `openspec/changes/archive/2026-09-08-portada-minimalista-creadores/proposal.md`).

## 9. Movimiento a Archivo

- Origen: `openspec/changes/portada-minimalista-creadores/`
- Destino: `openspec/changes/archive/2026-09-08-portada-minimalista-creadores/`
- **Nota de patrón:** la instrucción de lanzamiento citaba como destino literal `archive/portada-minimalista-creadores/` "patrón de los 8 archivados previos", pero el patrón real de los 8 archivados es `YYYY-MM-DD-{identificador-SDD}` (p. ej. `2026-09-08-change-27-store-live-stock-check`). Se siguió el patrón real con el prefijo de fecha ISO exigido por el skill: el identificador SDD de INC-31 en `ROADMAP_MVP_SLICES.md` es `portada-minimalista-creadores`, de ahí `2026-09-08-portada-minimalista-creadores`.
- Este reporte se escribió **antes** del movimiento y viaja dentro de la carpeta archivada (aditivo; excluido de la comparación de readback).
- Movimiento mecánico por shell (`git mv`).

**Readback (secuencia real, documentada con honestidad):**

1. El primer intento con `git diff --no-index -r` falló por switch no soportado en este git (`error: unknown switch 'r'`, exit 129) — un readback ausente no aprueba la fase, así que se re-ejecutó la verificación.
2. El segundo intento (reconstrucción de la fuente con `git show > fichero` + diff por fichero) reportó 8 mismatches que resultaron ser **artefacto del método de reconstrucción**: la redirección `>` de PowerShell 5.1 re-codifica los bytes (UTF-16 + normalización CRLF) y corrompía las copias de referencia, no los archivos movidos.
3. **Readback definitivo byte-exacto a nivel de OIDs de git** (sin pasar por la pipeline de PowerShell), los 7 ficheros rastreados tienen OID idéntico entre blob HEAD (ruta vieja) y blob indexado (ruta nueva):

| Fichero | OID HEAD (origen) | OID index (destino) | ¿Igual? |
|---|---|---|---|
| `design.md` | `7ee09b4d…` | `7ee09b4d…` | ✅ |
| `explore.md` | `42be8a58…` | `42be8a58…` | ✅ |
| `proposal.md` | `c41e4c47…` | `c41e4c47…` | ✅ |
| `specs/creators-directory/spec.md` | `1cbffa2c…` | `1cbffa2c…` | ✅ |
| `specs/giveaway-radar/spec.md` | `6e1d9215…` | `6e1d9215…` | ✅ |
| `specs/home-landing-hero/spec.md` | `3d86651a…` | `3d86651a…` | ✅ |
| `tasks.md` | `0c210bf4…` | `0c210bf4…` | ✅ |

Corroboración: `git diff --cached --summary -M100%` → **7 renombres al 100% de similitud**; `git diff --stat` sobre el destino → salida vacía (working tree ≡ index, incluido `verify-report.md` con blob `58038085…`). El directorio de cambios activos ya no contiene `portada-minimalista-creadores/`.
- Contenido del archivo: `proposal.md` ✅ · `explore.md` ✅ · `specs/` (3 deltas) ✅ · `design.md` ✅ · `tasks.md` ✅ (20/20) · `verify-report.md` ✅ · `archive-report.md` (este documento).

## 10. Totales de Pruebas

| Métrica | Valor |
|---|---|
| Suite total | **723/723** (exit 0, 2 ejecuciones completas en el cierre de verify) |
| Baseline previa (INC-30) | 705 |
| Añadidas por INC-31 (apply) | 16 |
| Añadidas por remediación (`f8ff077`) | 2 |
| Build | 0 errores / 0 advertencias |

## 11. Follow-ups (FUERA de este cambio — pendientes, no implementados aquí)

1. **BUG pre-existente de INC-30 — candidato INC-32 (aprobado por el mantenedor como fix separado inmediato):** `/juegos/{slug}` responde HTTP 500 por `NotSupportedException: SQLite does not support expressions of type 'DateTimeOffset' in ORDER BY clauses` en `SqliteGamePlayLogRepository.GetByUserAndGameAsync` (L56), vía `GameDetail.LoadUserDataAsync → RefreshPlaysCountAsync`. Introducido por el play-log (commit `fc90de0`, INC-30); ningún commit de INC-31 lo toca. Remediar con ORDER BY sobre tipo soportable o LINQ-to-Objects.
2. **Test flaky ajeno a INC-31:** `StoreStockServiceTests.GetStockAsync_WhenClientTimesOut_ShouldGracefullyDegradeToUnknown` (timing, carrera conocida); pasó en las 2 corridas completas del cierre; falló 1/3 en el verify anterior.
3. **Cosméticos fuera de contrato (limpieza futura):** CTA "Ver obras diseñadas" en `CreatorsDirectory.razor` (L123; sugerido: "Ver ficha") y la palabra "autores" en la microcopy de `AuditLogViewer.razor` (L49). Ninguno viola requerimiento del spec.

## 12. Observaciones Engram leídas (trazabilidad hybrid)

proposal **#145** · spec **#146** · design **#147** · tasks **#150** · verify-report **#159** · apoyo: exploración #143, validación de diseño #148, guard `size:exception` #151, remediación #154, reset de ledger #158.

## 13. Checklist de Archivo

- [x] Specs principales actualizados correctamente (2 creados FULL + 1 delta ADDED aplicado)
- [x] Carpeta del cambio movida a `archive/` con patrón del repo + fecha ISO
- [x] El archivo contiene todos los artefactos (proposal, specs, design, tasks, verify-report, explore)
- [x] `tasks.md` archivado sin tareas de implementación sin marcar (20/20 `[x]`)
- [x] El directorio de cambios activos ya no contiene este cambio
- [x] Readback de identidad de bytes: `diff` vacío + SHA256 idéntico en las 2 promociones FULL; `git diff --numstat` 31+/0− en el delta ADDED; OIDs idénticos HEAD↔index y renombres R100 en el movimiento (§9)
- [x] Roadmaps y especificación viva del sistema actualizados (regla 6 de `AGENTS.md`)
