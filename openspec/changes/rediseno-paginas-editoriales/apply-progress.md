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

## Partición del PR-1 → PR-1a / PR-1b (decisión del maintainer, 2026-09-10)

- El PR-1 original (#7, 464 líneas de código) superó el presupuesto de revisión de 400 líneas/PR; el ledger nativo registró el intento como `passed` con 549 líneas cambiadas y exigió decisión de maintainer. El maintainer decidió **partir el PR** (no size:exception) y **autorizó el reset nativo** del work unit (revisión del ledger consumida; exceso auditado en lifetime).
- **PR-1a (#8)**: `inc/rediseno-paginas-editoriales-1a` (base `main`) = artefactos SDD + tokens + hero responsivo + app.css + fix de clase de foco. Verificación: **852/852 verde**.
- **PR-1b (#9)**: `inc/rediseno-paginas-editoriales-1b` (base PR-1a) = `PageHeaderEditorial` + `EditorialModal` + marcas de tareas + `apply-progress.md`. Verificación: **854/854 verde**; árbol final idéntico al original (`git diff 1b7485c` vacío). El PR #7 quedó cerrado con comentario.
- Reconstrucción por cherry-picks en orden: PR-1a = 0f13f41, f618e3d, 35c6451, 7890c0e, 02a0332, 36115bb(.razor); PR-1b = 3584407, a5d00df, d262108, 36115bb(tasks), 1b7485c. El hunk de tasks.md del fix (casilla 1.7) se resolvió a favor de PR-1b.
- Base para PR-2: rama `inc/rediseno-paginas-editoriales-1b` (cabeza de la cadena).

---

## Estado PR-2 «Catálogo»: COMPLETADO ✅

> Fase SDD `sdd-apply`, PR-2 «Catálogo» (tareas 2.1–2.3). Rama `inc/rediseno-paginas-editoriales-2` creada encima de `inc/rediseno-paginas-editoriales-1b` (cabeza de la cadena, PR #9). Modo: TDD estricto.

| Tarea | Estado | Ciclo TDD | Commit |
|---|---|---|---|
| 2.1 Contrato `Home catalogo` ajustado (mustContain + `<PageHeaderEditorial`, `scrollbar-none`; mustNotContain + `<h1`, `no-scrollbar`) | ✅ | ROJO confirmado (1 fallo exacto, la fila) | incluido en `3b1a392` |
| 2.2 Home.razor: PageHeaderEditorial (badge «Catálogo Colaborativo» + `dices`, punto terracota vía RenderFragment `<Title>`, subtítulo conservado, sin acción), buscador a bloque propio (`max-w-xl mt-6 mb-8`, handler intacto), `no-scrollbar` → `scrollbar-none`, variante centrada abandonada | ✅ | VERDE (focal 1/1; suite 854/854) | `3b1a392` |
| 2.3 Boundary PR-2: suite completa + smoke `/catalogo` + PR (base = 1b) | ✅ | 854/854 + smoke 9/9 | (docs) + PR |

### TDD Cycle Evidence (PR-2)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| 2.1 | `WebMarkupContractTests.cs` (fila `Home catalogo (sin emojis)`) | Unit (contrato) | ✅ 1/1 pre-cambio | ✅ 1 fallo exacto | ✅ 1/1 | ➖ Estructural (swap de markup, positivo+negativo en la misma fila) | ➖ No needed |
| 2.2 | idem (contrato de 2.1) | Unit | ✅ | ✅ (mismo ciclo) | ✅ 1/1 | ➖ Triangulación por runtime: smoke `/catalogo` verifica h1 único + clases en el markup servido | ➖ No needed |
| 2.3 | suite completa | Full | ✅ | — | ✅ 854/854 | ✅ smoke runtime 9/9 | ➖ No needed |

- Nota de triangulación (2.2): el contrato grepea el archivo (presencia + ausencia); la variante de comportamiento real (render SSR con un solo `<h1>`) se cubre en el smoke runtime del boundary, que es donde la página se ejecuta de verdad.

### Verificación observada (registro PR-2)

| Comando | Resultado observado |
|---|---|
| Safety net `dotnet test --filter DisplayName~"Home catalogo"` (pre-cambios) | 1/1 verde |
| `dotnet test --filter DisplayName~"Home catalogo"` (ROJO 2.1) | 1 fallo exacto (fila `Home catalogo (sin emojis)`) |
| `dotnet test --filter DisplayName~"Home catalogo"` (VERDE 2.2) | 1/1 verde |
| `dotnet test Ludeka.sln` (boundary 2.3) | **854/854 verde** (baseline PR-1b exacta; sin tests nuevos, solo fila ajustada) |
| Smoke runtime `dotnet run --project src/Ludeka.Web --urls http://localhost:5199` → `Invoke-WebRequest /catalogo` | HTTP 200; `page-header-title` presente; `scrollbar-none` presente; `no-scrollbar` ausente; **exactamente 1 `<h1`** en el documento; badge «Catálogo Colaborativo» (pill `badge-pill` con `dices`), subtítulo y buscador presentes |

### Desviaciones y hallazgos PR-2

1. **Subtítulo conservado** (decisión de aplicación): tasks.md 2.2 no menciona el subtítulo pero el escenario de la spec («la renderiza vía `PageHeaderEditorial` con badge píldora, h1 en serif display **y subtítulo**») lo exige; se pasa el texto vigente «Catálogo colaborativo, valoraciones comunitarias y escalabilidad en mesa.» al parámetro `Subtitle` del componente en vez de perderlo al eliminar la cabecera inline. DD-06 lo declara opcional; la spec lo pide para los 4 listados.
2. **GOTCHA encoding en smoke PowerShell**: `Invoke-WebRequest -UseBasicParsing` decodifica el body con la codepage de la consola y «Catálogo» se corrompe (mojibake); un `Contains('Catálogo Colaborativo')` da **falso negativo**. Verificar por subcadena ASCII (`'Colaborativo'`) o decodificar `$r.RawContentStream.ToArray()` con `[System.Text.Encoding]::UTF8`. Afecta a los smokes de PR-3..5 con texto castellano acentuado.
3. El span terracota del punto (`text-[var(--brand-primary)]`) aparece 88 veces en la página completa (las GameCard también lo usan); el check estructural fiable del punto es la presencia del fragment `<Title>` en el markup fuente + 1 único `<h1>`, no un conteo global.
4. `HeroEditorialQuickSearchTests` intacto (el handler del buscador no se tocó: `Value`/`ValueChanged` pasan idénticos).

### Base para PR-3

- Rama del PR-2: `inc/rediseno-paginas-editoriales-2` (encima de `inc/rediseno-paginas-editoriales-1b`). El PR-3 debe crearse encima de esta rama (cadena feature-branch-chain, DD-11).
