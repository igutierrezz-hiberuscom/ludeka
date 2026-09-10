# Apply Progress: rediseno-paginas-editoriales (INC-36)

> Fase SDD `sdd-apply`, PR-1 «Fundación» (tareas 1.1–1.7). Store: hybrid (este archivo + Engram `sdd/rediseno-paginas-editoriales/apply-progress`).
> Worktree: `C:\repos\ludeka-wt\rediseno-paginas-editoriales`, rama `inc/rediseno-paginas-editoriales`. Modo: **TDD estricto** (patrón INC-31, contratos de markup con grep Ordinal, sin bUnit). Runner: `dotnet test Ludeka.sln`.

## Estado PR-1 «Fundación»: COMPLETADO ✅

| Tarea | Estado | Ciclo TDD | Commit |
|---|---|---|---|
| 1.1 Contratos fundación CSS (2 Fact + fila ajustada) | ✅ | ROJO confirmado (3 fallos exactos) → | incluido en 35c6451 |
| 1.2 Tokens `--on-brand` + 12 de estado ×5, hero, focos, page-header-title, filter-btn, animate-pulse | ✅ | VERDE (100/100 contratos) | `35c6451` |
| 1.3 Fact `AppCss_FundacionInc36_Regenerada` + regeneración DD-10 | ✅ | ROJO (1 fallo) → VERDE (5/5) | `02a0332` |
| 1.4 Hero: sin min-h fijo, foco por variante, Buscar `--on-brand`, chip `#F1F5F9`, `FocalClass` | ✅ | ROJO → VERDE (101/101) | `7890c0e` |
| 1.5 `PageHeaderEditorial` compartida (DD-06) | ✅ | ROJO (archivo ausente) → VERDE (106/106) | `a5d00df` |
| 1.6 `EditorialModal` + `editorial-modal.js` + script en App.razor (DD-07) | ✅ | ROJO → VERDE (108/108) | `d262108` |
| 1.7 Boundary PR-1 (suite + smoke runtime) | ✅ | Suite final 854/854 | `36115bb` (fix smoke) |

### Commits (rama inc/rediseno-paginas-editoriales, encima de 0f13f41/f618e3d)

| Sha | Mensaje |
|---|---|
| `35c6451` | feat: tokens on-brand y de estado en los cinco temas |
| `7890c0e` | feat: hero responsivo con foco por variante y boton accesible |
| `02a0332` | feat: app.css regenerado con la fundacion inc-36 |
| `3584407` | docs: progreso pr-1 inc-36 en tasks (casillas 1.3/1.4) |
| `a5d00df` | feat: PageHeaderEditorial compartida |
| `d262108` | feat: EditorialModal compartido con foco accesible |
| `36115bb` | fix: clase de foco del hero evaluada en el atributo class |

## TDD Cycle Evidence

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| 1.1 | `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` | Unit (contrato) | ✅ 103/103 | ✅ 3 fallos exactos | ✅ 100/100 | ✅ 5 bloques × 5 tokens (parseo por tema) | ➖ No needed |
| 1.2 | idem (contrato de 1.1) | Unit | ✅ | ✅ (mismo ciclo) | ✅ 100/100 | ➖ Estructural (tokens constantes) | ➖ No needed |
| 1.3 | `tests/Ludeka.UnitTests/Web/PerformanceAndAccessibilityTests.cs` | Unit | ✅ 4/4 | ✅ 1 fallo | ✅ 5/5 | ➖ Single (salida minificada única) | ➖ No needed |
| 1.4 | `WebMarkupContractTests.cs` (fila hero) | Unit | ✅ | ✅ 1 fallo | ✅ 101/101 + QuickSearch | ✅ 4 variantes (input.css) + smoke runtime | ➖ (ver fix `36115bb`) |
| 1.5 | fila nueva PageHeaderEditorial | Unit | ✅ | ✅ archivo ausente | ✅ 106/106 | ➖ Estructural (API única) | ➖ No needed |
| 1.6 | fila nueva EditorialModal | Unit | ✅ | ✅ archivo ausente | ✅ 108/108 | ➖ Estructural (API única) | ➖ No needed |
| 1.7 | suite completa | Full | ✅ | — | ✅ 854/854 | ✅ smoke 10/10 runtime | ➖ No needed |

- Nota de red (1.1): los 2 `mustNotContain` nuevos de la fila fundación (`min-height: 360px/460px` en input.css) pasan vacuos en el ROJO — son guarda profiláctica; el rojo lo producen los 13 `mustContain` ausentes y los 2 Fact.

## Verificación observada (registro)

| Comando | Resultado observado |
|---|---|
| `dotnet test Ludeka.sln` (raíz del worktree, árbol final) | **854/854 verde** (baseline real 849 + 5 pruebas nuevas; tasks.md declaraba baseline 847 — desvío de libro, todo en verde) |
| Safety net inicial (3 clases tocadas, pre-cambios) | 103/103 verde |
| `dotnet test --filter WebMarkupContractTests` (ROJO 1.1) | 3 fallos exactos (fila fundación + 2 Fact) |
| `dotnet test --filter PerformanceAndAccessibilityTests` (ROJO 1.3) | 1 fallo (solo fact nueva) |
| `npx.cmd -y tailwindcss@3.4.17 -i ./Styles/input.css -o ./wwwroot/app.css --minify` (desde src/Ludeka.Web) | Done ~2.7 s; `--on-brand`, `aspect-ratio:16/9`, `hero-focal--eurogame`, `page-header-title` presentes; `min-height:360px/460px` ausentes; utilidad `.text-[var(--on-brand)]` compilada |
| Smoke runtime `dotnet run` (puerto 5199; el 5081 lo ocupa el checkout principal `C:\repos\ludeka`) | `/` + `?theme=charcoal|tabletop|midnight|editorial|wood` → 200 ×5 + buscador del hero en el markup |
| Smoke `/?hero=` | `eurogame` → 200 + `hero-focal--eurogame`; `amigos` → 200 + `hero-focal--mesa-amigos`; `primer-plano` → 200 + `hero-focal--primer-plano`; `ilustracion` → 200 + `hero-focal--ilustracion`; clave inválida `mesa-amigos` → 200 + fallback a foco por defecto |

## Desviaciones y hallazgos

1. **Orden 1.3/1.4 invertido en ejecución** (1.4 primero): el `mustNotContain("min-height:360px")` de la fact de app.css exige el markup del hero corregido ANTES de regenerar (las utilidades `min-h-[360px]`/`sm:min-h-[460px]` se compilan desde el contenido de `HeroEditorial.razor`, no de `input.css`). Con el orden original la fact quedaría roja en el commit de 1.3; con el intercambio, todos los commits están verdes. Los commits preservan su mensaje y frontera.
2. **GOTCHA Razor**: dentro de un atributo, `class="hero-editorial@GetFocalClass() ..."` NO evalúa la expresión (patrón «email»: `@` pegado a texto se emite literal — observado en el smoke: el markup servía el texto crudo). Corrección: expresión explícita `@(GetFocalClass())` (commit `36115bb`). Válido para PR-2..5 al componer clases.
3. **Claves de querystring del selector del hero congeladas por INC-35**: `eurogame|amigos|primer-plano|ilustracion|css` — la clase renderizada es `hero-focal--mesa-amigos` pero la clave es `amigos`. El smoke de `sdd-verify`/PR-2..5 debe usar `?hero=amigos`.
4. `animate-fade-in` es una clase muerta preexistente en todo el monorepo (usada en 20+ modales, sin regla CSS): se replica tal cual en EditorialModal por coherencia con DD-07; corregirla queda fuera del alcance (sin contrato que la exija).
5. Baseline declarada 847 vs real 849 (+5 nuevas = 854): probablemente el estimate de tasks.md se redactó antes de 2 pruebas de INC-35. No afecta la verificación (todo verde).

## Riesgos / pendientes para PR-2..5

- PR-2 (Catálogo): la adopción de `PageHeaderEditorial` debe conservar el punto terracota vía RenderFragment `Title`; vigilar que quede un único `<h1>` por página.
- PR-3 (Ficha): la fila `GameDetail (diseñador texto plano)` es read-only; el back-bar DD-08 exige envolver como unidad.
- PR-4 (Eventos): el mustNotContain `text-white` debe formularse sin capturar el par overlay `bg-black/60` + `text-white` (excepción contratada DD-04).
- PR-5: interop de foco `ludekaModal` ya disponible; los shells inline de Radar/News se restan (~240 líneas). Regeneración final de app.css (purga de utilidades muertas) al cierre del PR-5.
- El `safelist` de `tailwind.config.js` mantiene patrones 500-950 aunque las páginas abandonen los hardcodes (vigilar con la fact de app.css y los mustNotContain).
- Ratios reales de assets ≠ `width`/`height` declarados: deuda documentada para la ola C (sin impacto runtime).
