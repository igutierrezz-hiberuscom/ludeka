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

---

## Estado PR-3 «Ficha»: COMPLETADO ✅

> Fase SDD `sdd-apply`, PR-3 «Ficha» (tareas 3.1–3.3). Rama `inc/rediseno-paginas-editoriales-3` creada encima de `inc/rediseno-paginas-editoriales-2` (cabeza de la cadena, PR #10). Modo: TDD estricto.

| Tarea | Estado | Ciclo TDD | Commit |
|---|---|---|---|
| 3.1 Contrato `GameDetail (ficha sin emojis)` ajustado (mustContain + `flex-wrap`, `text-[var(--on-brand)]`; mustNotContain + `Ludeca`, `text-white`, `text-slate-400`, `bg-amber-500`, `bg-indigo-500`, `bg-purple-950`, `bg-rose-500`, `text-orange-400`) | ✅ | ROJO confirmado (1 fallo exacto, la fila; diseñador texto plano read-only en verde) | incluido en `5ed2cfd` |
| 3.2 GameDetail.razor: PageTitle «Ludeka» ×2, estado no-encontrado tokenizado (`--text-primary`/`--text-muted`, código slug `--text-primary`+`font-mono`), back-bar DD-08 (envoltura `flex flex-wrap ... gap-x-4 gap-y-2`, «Ir a Mi Ludoteca» con `hidden sm:inline-flex` al grupo izquierdo, moderación agrupada en subcontenedor `border-l border-[var(--border-subtle)]`), hardcodes → tokens de estado, chip expansión sólido `--state-highlight`+`--on-brand`, botón IA `--on-brand` | ✅ | VERDE (focal 4/4, incluida fila diseñador) | `5ed2cfd` |
| 3.3 Boundary PR-3: suite completa + smoke ficha (moderador, envoltura, 5 temas) + PR (base = rama PR-2) | ✅ | 854/854 + smoke ficha/no-encontrado 5/5 | (docs) + PR |

### TDD Cycle Evidence (PR-3)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| 3.1 | `WebMarkupContractTests.cs` (fila `GameDetail (ficha sin emojis)`) | Unit (contrato) | ✅ 4/4 pre-cambio | ✅ 1 fallo exacto (los 10 mustNotContain nuevos fallan en el fuente: `Ludeca` ×2, `text-white` ×3, `text-slate-400` ×3, `bg-amber-500`, `bg-indigo-500`, `bg-purple-950`, `bg-rose-500`, `text-orange-400`; `flex-wrap` ya presente no era el rojo) | ✅ 4/4 | ➖ Estructural (swap positivo+negativo en la misma fila) | ➖ No needed |
| 3.2 | idem (contrato de 3.1) | Unit | ✅ | ✅ (mismo ciclo) | ✅ 4/4 (contrato + fila diseñador read-only) | ✅ Triangulación por runtime: smoke verifica las clases tokenizadas en el markup servido (back-bar envolvente ×1, moderación con borde, ausencia de los 8 hardcodes en el fragmento propio) | ➖ No needed |
| 3.3 | suite completa | Full | ✅ | — | ✅ 854/854 | ✅ smoke runtime ficha + no-encontrado ×5 temas | ➖ No needed |

### Verificación observada (registro PR-3)

| Comando | Resultado observado |
|---|---|
| Safety net `dotnet test Ludeka.sln --filter DisplayName~"GameDetail"` (pre-cambios) | 4/4 verde |
| `dotnet test --filter DisplayName~"GameDetail"` (ROJO 3.1) | 1 fallo exacto (fila `GameDetail (ficha sin emojis)`); diseñador texto plano y el resto de la clase en verde |
| `dotnet test --filter DisplayName~"GameDetail"` (VERDE 3.2) | 4/4 verde |
| `dotnet test Ludeka.sln` (boundary 3.3) | **854/854 verde** (baseline PR-2 exacta; sin tests nuevos, solo fila ajustada) |
| Smoke `/juegos/wingspan` (BD local con seed; servidor en 5199) | HTTP 200; `flex-wrap` presente (×19 en documento); back-bar servida con la estructura DD-08 exacta (envoltura `gap-x-4 gap-y-2`, grupo izquierdo con «Volver al catálogo» + «Ir a Mi Ludoteca» `hidden sm:inline-flex`, moderación agrupada `border-l` con Editar Ficha `--state-warning-*` y Generar con IA `--state-info-*`, Gestionar Veredicto servido); `Ludeca` 0 ocurrencias; `text-orange-400`/`text-slate-400`/`hover:bg-rose-500`/`hover:bg-amber-500`/`bg-indigo-500` 0 ocurrencias; `hover:text-white` 0 |
| Smoke `/juegos/slug-inexistente?theme={5 temas}` | HTTP 200 ×5; estado no-encontrado servido con `text-[var(--text-primary)]` (h1+código slug) y `text-[var(--text-muted)]` (2), sin `text-orange-400` |

### Desviaciones y hallazgos PR-3

1. **Escalón de hover con tokens congelados (aplicación DD-04)**: el mapa de sustitución colapsa los alfas hardcodeados (bg /10 y hover /20) sobre la familia de tokens (bg alfa 0,10, border alfa 0,28). Para no perder el feedback de hover de los botones de moderación, `hover:bg-amber-500/20` y `hover:bg-indigo-500/20` se mapean al siguiente escalón de su misma familia congelada: `hover:bg-[var(--state-warning-border)]` / `hover:bg-[var(--state-info-border)]` (mismo tono, alfa mayor). No se reabre PR-1 (sin tokens nuevos); reportado para revisión del maintainer.
2. **`text-white` residual en el DOCUMENTO servido (fuera de alcance, contratado)**: el markup propio de GameDetail.razor sirve 0 ocurrencias, pero el documento completo contiene `text-white` de componentes compartidos fuera de alcance: badge de equipo fundador en `MainLayout.razor:103` (DD-03 lo documenta como barrido futuro) y `text-white` de familias compartidas (`ExpansionEcosystemSection`, `ExpansionSisterList`, `CollectionActionBar`, `QuickBadges`, `StoreOffersCard`, `MultimediaHub`… — DD-05, segunda ola). La ausencia contratada aplica al ARCHIVO de la ficha (grep del contrato) y se verificó también en su fragmento servido.
3. **Sesión local autenticada como fundador**: el smoke sirve la rama moderador/fundador (badge de marca de MainLayout y grupo de moderación visibles), lo que permitió verificar el grupo de moderación en vivo. La rama «sin moderador» comparte el mismo contenedor `flex flex-wrap` (la envoltura es estructural para ambas ramas); la confirmación visual en móvil la añade `sdd-verify`.
4. **Chip de expansión simplificado**: al pasar a chip sólido (`bg-[var(--state-highlight)] text-[var(--on-brand)]`) se eliminan `backdrop-blur-md` y `border-purple-500/40` (ya sin translucidez que difuminar ni borde contraplantado); se conservan posición/sombra/peso tipográfico. Contraste verificado por cálculo DD-04 (tinta sobre `#C084FC` ≈ 6,7:1; blanco sobre `#A21CAF` ≈ 6,4:1).
5. GOTCHA heredado aplicado: smoke verificado por subcadenas ASCII y `RawContentStream` UTF8 (sin comparar «Catálogo»/acentos con `Invoke-WebRequest`).

### Base para PR-4

- Rama del PR-3: `inc/rediseno-paginas-editoriales-3` (encima de `inc/rediseno-paginas-editoriales-2`). El PR-4 debe crearse encima de esta rama (cadena feature-branch-chain, DD-11). Presupuesto PR-3: 110 líneas cambiadas (60+/50−), dentro del forecast ~120-160.

---

## Estado PR-4 «Eventos»: COMPLETADO ✅

> Fase SDD `sdd-apply`, PR-4 «Eventos» (tareas 4.1–4.3). Rama `inc/rediseno-paginas-editoriales-4` creada encima de `inc/rediseno-paginas-editoriales-3` (cabeza de la cadena, PR #11). Modo: TDD estricto.

| Tarea | Estado | Ciclo TDD | Commit |
|---|---|---|---|
| 4.1 Contrato `Events (sin emojis)` ajustado (mustContain + `<PageHeaderEditorial`, `text-[var(--on-brand)]`, `rail-card`, `role="tabpanel"`, `aria-controls="panel-`; mustNotContain + `hover:scale-105`, `bg-rose-500/90`, `bg-amber-500/90`, `text-zinc-300`, `dark:`, `text-white` acotado con 2 fragmentos de patrón de botón de marca) | ✅ | ROJO confirmado (1 fallo exacto, la fila: primer mustContain ausente) | incluido en `6fed384` |
| 4.2 Events.razor: PageHeaderEditorial (badge «Calendario Oficial del Sector» + `tent`, h1, subtítulo conservado, acción «Gestionar Eventos» moderador en `Actions`); pestañas con `aria-controls="panel-upcoming|past"` + panel único `role="tabpanel"` con `id`/`aria-labelledby` conmutables (helpers `ActiveTabId`/`ActivePanelId`, expresiones completas — gotcha Razor); tarjetas a `.rail-card justify-between shadow-sm` con imagen `rail-cover h-48` y sin `hover:scale-105` suelto (zoom por `.rail-card:hover .rail-cover img`); badges de urgencia → `--state-error`/`--state-warning` triple con `--on-brand`; overlay «Finalizado» → `text-white/80 border-white/10` (par autocontenido); botones de marca 30/66/73/117/208 → `text-[var(--on-brand)]` | ✅ | VERDE (contratos 102/102) | `6fed384` |
| 4.3 Boundary PR-4: suite completa + smoke `/eventos` ×4 temas + PR (base = rama PR-3) | ✅ | 854/854 + smoke 4/4 | (docs) + PR |

### TDD Cycle Evidence (PR-4)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| 4.1 | `WebMarkupContractTests.cs` (fila `Events (sin emojis)`) | Unit (contrato) | ✅ 11/11 pre-cambio (`--filter DisplayName~Events`) | ✅ 1 fallo exacto (primer mustContain: `<PageHeaderEditorial` ausente; 101 filas restantes en verde) | ✅ 102/102 | ➖ Estructural (swap positivo+negativo en la misma fila) | ➖ No needed |
| 4.2 | idem (contrato de 4.1) | Unit | ✅ | ✅ (mismo ciclo) | ✅ 102/102 (incluida invariante `PaginasEventos_TodaImagenDeEventoTieneFallbackPorDominio`) | ✅ Triangulación por runtime: smoke verifica las clases tokenizadas y la relación tab/tabpanel en el markup servido ×4 temas | ➖ No needed |
| 4.3 | suite completa | Full | ✅ | — | ✅ 854/854 | ✅ smoke runtime /eventos ×4 + emparejamiento ARIA | ➖ No needed |

### Verificación observada (registro PR-4)

| Comando | Resultado observado |
|---|---|
| Safety net `dotnet test --filter DisplayName~Events` (pre-cambios) | 11/11 verde |
| `dotnet test --filter FullyQualifiedName~WebMarkupContractTests` (ROJO 4.1) | 1 fallo exacto (fila `Events (sin emojis)`, primer mustContain ausente) |
| `dotnet test --filter FullyQualifiedName~WebMarkupContractTests` (VERDE 4.2) | 102/102 verde (incluida la invariante de eventos) |
| `dotnet test Ludeka.sln` (boundary 4.3) | **854/854 verde** (baseline PR-3 exacta; sin tests nuevos, solo fila ajustada) |
| Smoke `dotnet run --project src/Ludeka.Web --urls http://localhost:5199` → `Invoke-WebRequest /eventos` (y `?theme=charcoal|editorial|wood`) | HTTP 200 ×4; `page-header-title`, `role="tabpanel"`, `aria-controls="panel-`, `rail-card` (×6 tarjetas con seed local), `text-[var(--on-brand)]` y `badge-pill` presentes; `hover:scale-105`, `dark:`, `bg-rose-500/90`, `bg-amber-500/90`, `text-zinc-300` ausentes; `hover:opacity-90 text-white` ausente en el documento |
| Smoke ARIA del tablist | `role="tab"` ×2 + `role="tablist"`; panel activo servido `id="panel-upcoming"` + `aria-labelledby="tab-upcoming"`; ambas pestañas declaran `aria-controls="panel-upcoming"`/`panel-past` (el id del panel conmuta con la pestaña activa); un único `<h1>` en el documento |

### Desviaciones y hallazgos PR-4

1. **GOTCHA de la invariante + comentarios Razor**: el regex de `PaginasEventos_TodaImagenDeEventoTieneFallbackPorDominio` (`<img\b[^>]*>`, Singleline) captura el literal `<img>` dentro de comentarios `@* … *@` del fuente: un comentario que mencionara `<img>` rompía la invariante en verde (fallo espurio). Reescrito el comentario sin el literal («en el cartel»). Regla: ningún comentario .razor de páginas de eventos debe contener el literal `<img>`.
2. **Formulación del mustNotContain de `text-white` acotado**: dos fragmentos cubren las 5 variantes de botón de marca sin capturar el par overlay contratado `bg-black/60 + text-white` (líneas 152/281, ahora ~164/287) ni `text-white/80`: `hover:opacity-90 text-white` (botones con hover: 30/117/208) y `)] text-white` (filtros activos: 66/73, la píldora contiene `]` antes del espacio; el overlay `bg-black/60` no tiene corchetes). El smoke del DOCUMENTO sirve 1 ocurrencia de `)] text-white` desde `MainLayout.razor:103` (badge de equipo fundador con aislado CSS `b-*`) — fuera de alcance contratado en DD-03 (barrido futuro, segunda ola); el ARCHIVO Events.razor sirve 0.
3. **Panel único con id conmutable** (aplicación DD-09): el contenido de ambas pestañas comparte un bloque de render; en vez de duplicar paneles, un solo `<div role="tabpanel">` cuyo `id`/`aria-labelledby` conmutan con la pestaña activa (helpers en `@code`); `aria-controls` en cada botón es estático (`panel-upcoming`/`panel-past`). Emparejamiento tab↔panel completo servido y verificado en runtime.
4. **GOTCHA de filtros VSTest en PowerShell 5.1**: los valores con espacios se descotizan en todos los formatos probados (`DisplayName~"..."`, comillas simples incrustadas, backslash-escape → incluso rompe MSB1008 en `dotnet test`). Filtro robusto: por nombre de clase (`FullyQualifiedName~WebMarkupContractTests`), sin espacios; los `DisplayName~Events` compactos sí funcionan (sin espacios ni paréntesis).
5. **Hueco preexistente de utilidades `var(--state-*)` en app.css (registrado, no corregido aquí)**: el `app.css` compilado (última regeneración PR-1, commit `09c12e3`) contiene la utilidad `.text-[var(--on-brand)]` y `bg-[var(--brand-primary)]` (los chips de marca de Events se sirven con contraste real ✓), pero NO las utilidades alfa de estado introducidas por PR-3 (`bg-[var(--state-*-bg)]`, `hover:bg-[var(--state-*-border)]`) ni las nuevas sólidas de PR-4 (`bg-[var(--state-error)]`, `border-[var(--state-error-border)]`, `bg-[var(--state-warning)]`, `border-[var(--state-warning-border)]`, `text-white/80`). Los badges de urgencia quedan en el markup con tokens pero sin regla CSS servida hasta el barrido final de PR-5 (regeneración DD-10 contratada, que también purga utilidades muertas). Mismo patrón que PR-3 dejó; se cierra en la frontera contratada del PR-5. Verificación en app.css por subcadenas SIN corchetes (`--on-brand`, `state-error`) porque el minificado escapa los selectores (`\.text-\[var\(--on-brand\)\]`).
6. GOTCHA heredado aplicado: smoke verificado por subcadenas ASCII y `RawContentStream` UTF8 (sin comparar «Calendario»/acentos con `Invoke-WebRequest`); servidor en 5199 (`--urls` explícito, 5081 ocupado).

### Base para PR-5

- Rama del PR-4: `inc/rediseno-paginas-editoriales-4` (encima de `inc/rediseno-paginas-editoriales-3`). El PR-5 debe crearse encima de esta rama (cadena feature-branch-chain, DD-11). Presupuesto PR-4: 92 líneas cambiadas (57+/35−), dentro del forecast ~140-180.

---

## Estado PR-5 «Sorteos + Novedades»: COMPLETADO ✅

> Fase SDD `sdd-apply`, PR-5 «Sorteos + Novedades» (tareas 5.1–5.6). Rama `inc/rediseno-paginas-editoriales-5` creada desde la cabeza limpia de `inc/rediseno-paginas-editoriales-4` (PR-4 #12), conforme a `feature-branch-chain` y `delivery_strategy=auto-chain`. Modo: **TDD estricto**. Presupuesto observado del slice: por debajo de 400 líneas.

| Tarea | Estado | Ciclo TDD | Commit |
|---|---|---|---|
| 5.1 Contrato `Radar (sin emojis)` ajustado (`PageHeaderEditorial`, `EditorialModal`, prohibiciones) | ✅ | ROJO confirmado (3 fallos exactos de las tres filas PR-5) → | incluido en `2f67036` |
| 5.2 Radar.razor: cabecera compartida, `EditorialModal`, fragments, tokens de estado y `--on-brand` | ✅ | VERDE focal `DisplayName~Radar` (4/4) → suite contractual final 102/102; smoke `/sorteos` y `/radar` | `2f67036` |
| 5.3 Contratos `News` y `GiveawayCard` ajustados (fallbacks, dimensiones, rail y prohibiciones) | ✅ | ROJO confirmado (2 fallos exactos pendientes tras Radar) → | incluido en `dd7f42c` |
| 5.4 News.razor: cabecera, `EditorialModal`, imagen siempre presente y tokens | ✅ | VERDE contractual 102/102; smoke `/novedades` con default de novedad | `dd7f42c` |
| 5.5 GiveawayCard.razor: `rail-card`, `rail-cover h-44`, fallback de sorteo, tokens y botón accesible | ✅ | VERDE contractual 102/102; smoke `/sorteos` con default de sorteo | `dd7f42c` |
| 5.6 Boundary PR-5: `app.css`, suite, smokes, documentación y tareas | ✅ | Suite completa 854/854 antes y después de regenerar; smoke HTTP 200 de las 5 rutas | `c08cd9b` |

### Commits de PR-5

| Sha | Mensaje |
|---|---|
| `2f67036` | `feat: sorteos con modal editorial y tokens de estado` |
| `dd7f42c` | `feat: novedades y GiveawayCard con imagen por defecto y rail-card` |
| `c08cd9b` | `docs: progreso pr-5 inc-36 y casillas de sorteos-novedades` |

## TDD Cycle Evidence (PR-5)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| 5.1 | `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` (fila `Radar`) | Unit (contrato por texto Ordinal) | ✅ 102/102 | ✅ 3 fallos exactos al activar las aserciones PR-5: `Radar` sin `PageHeaderEditorial`, `News` sin `PageHeaderEditorial` y `GiveawayCard` sin `rail-card` | ✅ Fila Radar incluida en `DisplayName~Radar` (4/4) y suite contractual final 102/102 | ✅ `/sorteos` y `/radar`: 200, un `<h1>`, cabecera, rail y fallback de sorteo | ✅ Shell inline sustituido por `EditorialModal`; contenido y pie separados en `ChildContent`/`Footer` |
| 5.2 | `WebMarkupContractTests.cs` (fila `Radar`) | Unit + runtime SSR | ✅ 102/102 inicial | ✅ Contrato escrito antes de la implementación | ✅ `dotnet test --filter DisplayName~Radar`: 4/4; suite final 102/102 | ✅ Handlers de apertura, cierre y `HandleSubmitGiveaway` conservados mediante fragments; dos rutas sirven la misma página | ✅ Se corrigió el primer intento de composición Razor usando `ChildContent` explícito tras el error RZ9996 |
| 5.3 | `WebMarkupContractTests.cs` (filas `News`/`GiveawayCard`) | Unit (contrato por texto Ordinal) | ✅ Baseline contractual 102/102 antes de editar contratos | ✅ 2 fallos exactos tras Radar: `News` sin `PageHeaderEditorial` y `GiveawayCard` sin `rail-card` | ✅ Suite contractual final 102/102 | ✅ `/novedades` y `/sorteos`: 200, un `<h1>`, rail y fallback por dominio | ✅ Contratos expresan la API real de `PageHeaderEditorial` mediante `BadgeIcon` |
| 5.4 | `WebMarkupContractTests.cs` (fila `News`) | Unit + runtime SSR | ✅ 102/102 | ✅ Cubierto por el contrato 5.3 escrito antes del markup | ✅ 102/102 | ✅ `/novedades` sirvió `novedad-default.svg`, `rail-card`, cabecera y h1 único; `EditorialModal` quedó referenciado | ✅ Se conservaron los handlers de formulario dentro de `ChildContent`/`Footer`; URLs externas conservan `width`/`height`/`onerror` |
| 5.5 | `WebMarkupContractTests.cs` (fila `GiveawayCard`) | Unit + runtime SSR | ✅ 102/102 | ✅ Cubierto por el contrato 5.3 escrito antes del markup | ✅ 102/102 | ✅ `/sorteos` y `/` sirvieron `sorteo-default.svg`; la tarjeta externa declara dimensiones y fallback estático | ✅ `.rail-card` centraliza lift/focus/reduced-motion; `animate-pulse` conserva la guarda CSS existente |
| 5.6 | `WebMarkupContractTests.cs` + `PerformanceAndAccessibilityTests.cs` (contratos existentes) | Unit boundary + runtime HTTP | ✅ Suite completa 854/854 antes de regenerar | ➖ Contratos `AppCss_FundacionInc36_Regenerada` y barrido ya existentes; la tarea solo regenera el artefacto derivado | ✅ Suite completa final 854/854 y contrato de CSS verde | ✅ Cinco rutas HTTP 200, h1 único, cabeceras/rails/fallbacks y puerto liberado antes de la suite final | ✅ `app.css` regenerado después de todo el markup; sin cambios en `input.css` ni en scripts compartidos |

## Work Unit Evidence (PR-5)

| Unidad / commit | Prueba focal y resultado exacto | Runtime harness y resultado exacto | Límite de rollback |
|---|---|---|---|
| U5-Radar / `2f67036` | `dotnet test --filter DisplayName~Radar` → **4/4**; la suite contractual completa quedó en **102/102** al cerrar PR-5 | `dotnet run --project src/Ludeka.Web --urls http://localhost:5199` + HTTP `/sorteos` y `/radar` → **200**, `<h1>` único, `page-header-title`, `rail-card`, título y `sorteo-default.svg` | Revertir `Radar.razor` y las aserciones Radar de `WebMarkupContractTests.cs`; no afecta News/GiveawayCard ni los componentes compartidos |
| U5-News-Giveaway / `dd7f42c` | `dotnet test --filter FullyQualifiedName~WebMarkupContractTests` → **102/102** | Mismo servidor en 5199: `/novedades` → **200**, `<h1>` único, cabecera, rail, título y `novedad-default.svg`; `/sorteos` → **200** y `sorteo-default.svg` | Revertir `News.razor`, `GiveawayCard.razor` y sus contratos; no afecta Radar ni el shell `EditorialModal` |
| U5-Boundary / documentación | `dotnet test Ludeka.sln` antes y después de la regeneración → **854/854** | Smoke HTTP `/sorteos`, `/radar`, `/novedades`, `/juegos/wingspan`, `/` → **200** en las cinco; exactamente un `<h1>` en cada documento; puerto 5199 detenido antes de la suite final | Revertir únicamente `wwwroot/app.css`, `tasks.md` y esta sección de `apply-progress.md`; el comportamiento de los dos commits de código permanece independiente |

## Verificación observada (registro PR-5)

| Comando | Resultado observado |
|---|---|
| `dotnet test --filter FullyQualifiedName~WebMarkupContractTests` (safety net inicial) | **102/102 verde** |
| `dotnet test --filter FullyQualifiedName~WebMarkupContractTests` (ROJO contratos PR-5) | **3 fallos exactos, 99/102 verde**: `GiveawayCard` sin `rail-card`, `News` sin `<PageHeaderEditorial>` y `Radar` sin `<PageHeaderEditorial>` |
| `dotnet test --filter DisplayName~Radar` (Radar VERDE focal) | **4/4 verde** |
| `dotnet test --filter FullyQualifiedName~WebMarkupContractTests` (ROJO News/Giveaway tras Radar) | **2 fallos exactos, 100/102 verde**: `GiveawayCard` sin `rail-card` y `News` sin `BadgeIcon="newspaper"` |
| `dotnet test --filter FullyQualifiedName~WebMarkupContractTests` (News/Giveaway VERDE) | **102/102 verde** |
| `dotnet test Ludeka.sln` (safety net de boundary, antes de Tailwind) | **854/854 verde** |
| `npx.cmd -y tailwindcss@3.4.17 -i ./Styles/input.css -o ./wwwroot/app.css --minify` (desde `src/Ludeka.Web`) | **Correcto en 2447 ms**; advertencia no bloqueante de Browserslist desactualizado |
| Facts de `src/Ludeka.Web/wwwroot/app.css` | `--on-brand=True`, `state-error=True`, `state-warning=True`, `state-highlight=True`, `state-info=True`, `aspect-ratio:16/9=True`, `hero-focal--eurogame=True`, `page-header-title=True`; `min-height:360px=False`, `min-height:460px=False`; 248402 bytes, 1 línea minificada |
| `dotnet run --project src/Ludeka.Web --urls http://localhost:5199` | Proceso PID 19928 arrancado y estable para el smoke; detenido después, `port=5199 free` |
| Smoke HTTP UTF-8 en `/sorteos`, `/radar`, `/novedades`, `/juegos/wingspan`, `/` | **200 ×5**, exactamente **1 `<h1>` ×5**; `/sorteos` y `/radar`: cabecera, título, rail y `sorteo-fallback`; `/novedades`: cabecera, título, rail y `novedad-fallback`; ficha y portada: 200 y h1 único |
| Contrato de modal y fallback por dominio | `WebMarkupContractTests` **102/102**; `Radar.razor` y `News.razor` referencian `EditorialModal`; `News`/`GiveawayCard` declaran `DefaultImageDomain.Novedad`/`Sorteo`, dimensiones y `onerror`; `editorial-modal.js` conserva foco al abrir, restaura foco y cierra con Escape |
| Barrido de shells/hardcodes contratados | Grep Ordinal sin resultados en los tres archivos para `fixed inset-0 z-50`, `dark:`, familias de color prohibidas y `text-white`; comentarios Radar/News/GiveawayCard sin literal `<img>` |
| `dotnet test Ludeka.sln` (suite final, con servidor detenido) | **854/854 verde** |

## Desviaciones y hallazgos PR-5

1. La API de `PageHeaderEditorial` recibe el icono como `BadgeIcon`; por eso los contratos de Radar y News exigen `BadgeIcon="gift"`/`BadgeIcon="newspaper"` en vez de buscar un `<Icon>` inline que ya vive dentro del componente compartido. El markup servido sí renderiza el icono Lucide mediante el componente.
2. Razor exige expresión explícita para atributos de componentes con texto interpolado: `Alt="@($"Carátula de {release.Title}")"` y su equivalente de sorteos. El primer intento provocó RZ9986; se corrigió sin cambiar el contrato funcional.
3. Al adoptar `EditorialModal`, el contenido de formulario debe estar bajo `<ChildContent>` explícito cuando también se usa `<Footer>`; omitirlo provocó RZ9996. Los handlers de apertura, cierre y envío no cambiaron.
4. `app.css` se regeneró después de todo el markup, como exige DD-10. La advertencia de Browserslist no modificó el resultado ni los 854 tests.

## Estado acumulado

- **22/22 tareas completadas** (PR-1: 1.1–1.7; PR-2: 2.1–2.3; PR-3: 3.1–3.3; PR-4: 4.1–4.3; PR-5: 5.1–5.6).
- Boundary de PR-5 publicado en [PR #13](https://github.com/igutierrezz-hiberuscom/ludeka/pull/13), con base `inc/rediseno-paginas-editoriales-4`.
- Pendiente para el orquestador: ejecución independiente de `sdd-verify` y `sdd-archive`; no se ejecutaron desde apply.
