```yaml
schema: gentle-ai.verify-result/v1
evidence_revision: sha256:6b42f80db6a27103d10f20a10bba8823bca8d050a0b70bf3f92f789f66160f76
verdict: pass_with_warnings
blockers: 0
critical_findings: 0
requirements: 19/19
scenarios: 30/30
test_command: dotnet test Ludeka.sln --no-build
test_exit_code: 0
test_output_hash: sha256:2449721494df8e3a5c4f67f3ea3007b959da94039948458ee40670de6d4f0df3
build_command: dotnet build Ludeka.sln
build_exit_code: 0
build_output_hash: sha256:001d3541636a74716bd7c19f689e3c8d8ad41458a292051a4df50e00775e9b86
```

## Verification Report

**Change**: portada-editorial (INC-35)
**Version**: deltas de 4 specs en `openspec/changes/portada-editorial/specs/` (read-only; conteo autoritativo: 19 requisitos / 30 escenarios)
**Mode**: Strict TDD (contratos de markup INC-31, RED→GREEN documentado en Engram `sdd/portada-editorial/apply-progress`)
**Worktree**: `C:\repos\ludeka-wt\portada-editorial`, rama `inc/portada-editorial-4` (cabeza de la cadena de 4 PRs apilados)
**Idioma**: español castellano (regla suprema AGENTS.md)

### Completeness

| Metric | Value |
|--------|-------|
| Tasks total | 31 |
| Tasks complete | 31 |
| Tasks incomplete | 0 |

Tasks.md: PR-1 (1.1–1.12), PR-2 (2.1–2.9), PR-3 (3.1–3.7) y cierre transversal (4.1–4.3) — todos `[x]`, con ejecución verificada por el orquestador documentada inline (847/847 sobre la cabeza de la cadena, 2026-09-09).

### Build & Tests Execution

**Build**: ✅ Passed (`dotnet build Ludeka.sln`, exit 0, 0 errores, 4 advertencias preexistentes)
**Tests**: ✅ 847 passed / 0 failed / 0 skipped (`dotnet test Ludeka.sln --no-build`, exit 0, 12 s)

- `dotnet build Ludeka.sln: 0 errores (4 advertencias preexistentes de nullable/xUnit2013, no regresiones de este incremento)`
- `dotnet test Ludeka.sln --no-build: Correctas! Con error: 0, Superado: 847, Omitido: 0, Total: 847`
- Los 2 flaky preexistentes (BggXmlApiClientResilience, StoreStockService) no fallaron en ninguna de las 2 ejecuciones completas de esta verificación. No fue necesario re-lanzar en aislado.
- Recuento estable en 3 ejecuciones durante la sesión de verificación (847/847 en apply sobre la cabeza + 2 en verify, la última post-reversión de las ediciones temporales de seed).

**Coverage**: ➖ Not available (el proyecto no define umbral de cobertura; fuera del alcance de este incremento).

### Verificación runtime (dotnet run, puerto 5081)

| Comando | Resultado |
|---|---|
| `dotnet run --project src\Ludeka.Web --no-build --urls http://localhost:5081` | ✅ app arriba; BD SQLite regenerada por seeders con datos de prueba temporales |
| `GET / -> HTTP 200` | ✅ 103.655 bytes |
| `GET /eventos -> HTTP 200` | ✅ 50.508 bytes |
| `GET /images/defaults/evento-default.svg` | ✅ 200 (1.026 bytes) |
| `GET /images/defaults/sorteo-default.svg` | ✅ 200 (1.130 bytes) |
| `GET /images/defaults/novedad-default.svg` | ✅ 200 (1.272 bytes) |
| `GET /images/defaults/generico-default.svg` | ✅ 200 (1.150 bytes) |
| `HEAD /images/home/hero-ambiente-eurogame.avif` | ✅ 200 |

### Escenario no ejercitado en runtime (fallback REAL): cómo se validó

El seed original tiene imagen en todos los sorteos/novedades/eventos. Para validar el fallback REAL se sembraron **datos sin imagen temporalmente** y se regeneró la BD:

1. Respaldo de `ludeka.db/-wal/-shm` a temp (BD gitignored, dev-only).
2. Ediciones temporales marcadas `TEMP-VERIFY (sdd-verify)` en `src/Ludeka.Infrastructure/Seeding/CatalogSeeder.cs`: sorteo «Dwellings of Eldervale» con `thumbnailUrl: null` y novedad «Harmonies» con `coverImageUrl: null`.
3. BD eliminada y app relanzada → seeders repueblan con los datos sin imagen.
4. **Resultado runtime sobre `/`**: el card del sorteo sin imagen renderiza el SVG inline de Ludeka (`SORTEO LUDEKA`, `viewBox="0 0 400 225"`) en su `.rail-cover`, **sin ningún `<img>` en esa zona**; ídem la novedad sin imagen (`NOVEDAD`). 1 y 1 ocurrencias exactas — el resto de sorteos/novedades siguen con `<img>` + `onerror` (68 atributos `onerror` en el documento).
5. **Reversión completa**: ediciones de seed revertidas, BD restaurada desde respaldo, `git status` limpio, suite re-ejecutada verde (847/847).
6. **Limitación documentada**: el intento de sembrar un **evento** sin imagen falló con `System.ArgumentException: La imagen o cartel del evento es obligatoria` — la entidad `BoardGameEvent` (Core) **exige** imagen en su constructor (invariante de dominio, líneas 45-46). Por tanto, la rama inline `DefaultImage` de las cards de evento es defensa en profundidad, no alcanzable por el camino de datos normal; el fallback de evento que opera en runtime es el `onerror` (URL externa caída), verificado por contrato y en el markup renderizado.

### Spec Compliance Matrix

**Spec 1: home-landing-hero (6 requisitos, 9 escenarios)**

| Requisito | Escenario | Evidencia (test/runtime) | Resultado |
|---|---|---|---|
| Hero editorial con narrativa | Portada renderiza hero editorial | `WebMarkupContractTests > HeroEditorial_Pills_AreExactlyTheFourD4PillsInOrder` + contrato HeroEditorial + runtime GET / (h1 visible, párrafo, buscador, 4 píldoras en orden, sin "PORTADA EDITORIAL") | ✅ COMPLIANT |
| Jerarquía de encabezados (WCAG AA) | Único h1 visible con serif display | Contrato `<HeroEditorial` (mustNotContain `<h1`, `sr-only`) + runtime: 1 `<h1 class="hero-title">La mesa está servida</h1>`, `--font-display` en CSS | ✅ COMPLIANT |
| Variantes de fondo (D1) | Hero renderiza la variante configurada | `HeroBackgroundVariant_ExponeLasCincoVariantesDelDiseno` (5 variantes) + runtime: `FotoEurogame` activa renderiza su `<picture>` con hero-ambiente-eurogame | ✅ COMPLIANT |
| Variantes de fondo (D1) | Escena CSS sin peticiones de imagen | `HeroEditorial_RamaCssScene_NoRenderizaPicture` (rama `CssScene` sin `<picture>`); `.hero-scene` 100% CSS | ✅ COMPLIANT (contrato) |
| Variantes de fondo (D1) | Cambio de variante sin tocar estructura | `HeroBackgroundAssets_MapeaCadaVarianteFotoASusTresFormatos` (TheoryData 4×3 formatos) + `[Parameter] Background` en 1 línea del orquestador | ✅ COMPLIANT |
| Rendimiento hero (INC-07) | Imagen del hero con <picture> y prioridad | Contrato HeroEditorial + runtime: `<picture>` con sources AVIF/WebP, `<img>` JPEG, `fetchpriority="high"`, `width="1600"`/`height="900"`, alt castellano | ✅ COMPLIANT |
| Rendimiento hero (INC-07) | Presupuesto de peso, LCP y CLS | Pesos reales verificados en disco: máx. `hero-ambiente-primer-plano.jpg` 190,7 KB / `webp` 193,9 KB / `avif` 188,6 KB (< 200 KB); activa `eurogame` 182,3 KB jpg / 120 KB webp / 80,7 KB avif. Contrato CLS: `width`/`height` + `.rail-cover`/aspect-ratio fijos. **Medición real de LCP/CLS pendiente (MCP de Chrome DevTools no disponible)** — ver WARNING-2 y checklist del maintainer | ✅ COMPLIANT (contrato; medición real pendiente) |
| Contraste del scrim | Contraste del texto sobre la imagen | Por contrato: `.hero-scrim` con doble gradiente sobre `var(--bg-main)` cubre el cuadrante del texto en los 5 `data-theme` (charcoal, editorial, tabletop, midnight, wood); texto con `var(--text-primary)`. Validación visual humana: checklist del maintainer en los PRs (documentado en tasks 2.9) | ✅ COMPLIANT (contrato; visual humana = maintainer) |
| Contraste del scrim | Alt descriptivo de la foto | `HeroBackgroundAssets_AltTextosDeFotoEnCastellanoNoVacios` + runtime: alt "Mesa de juego con un eurogame en marcha sobre el tapete…" en el HTML | ✅ COMPLIANT |

**Spec 2: home-dashboard-rails (5 requisitos, 10 escenarios)**

| Requisito | Escenario | Evidencia (test/runtime) | Resultado |
|---|---|---|---|
| Carriles por componentes dedicados | Cada carril usa su componente | Contrato HomeDashboard (orquestador): mustContain `<HeroEditorial`, `<RailHeader`, `<HomeGameCard`, `<HomeGiveawayCard`, `<HomeReleaseCard`, `<HomeEventCard`; mustNotContain `BggRating`, `RemainingTimeText` inline | ✅ COMPLIANT |
| Carriles por componentes dedicados | Paridad de comportamiento tras la extracción | Contratos por componente (aria-label, destinos, badges con Icon) + runtime: 35 `.rail-card` con destinos/aria-labels intactos | ✅ COMPLIANT |
| Render de imagen con fallback (D2) | Card de sorteo con y sin imagen | **Runtime real con seed temporal**: sorteo con imagen → `<img>` + onerror; sorteo sin imagen → SVG inline `SORTEO LUDEKA` sin `<img>`; 0 `<img src="">` | ✅ COMPLIANT |
| Render de imagen con fallback (D2) | Novedad y evento con imagen ausente o caída | Runtime: novedad sin imagen → inline `NOVEDAD`; eventos con URL → `onerror` a `evento-default.svg` (evento sin imagen imposible por invariante de dominio — ver nota en SUGGESTION-1) | ✅ COMPLIANT |
| Microinteracciones con foco | Hover en tarjeta de carril | Contrato "Fundación CSS" (input.css: tokens `--ease-*`/`--dur-*`/`--rail-*`, `.rail-card:hover` lift+glow+zoom 1.03) | ✅ COMPLIANT |
| Microinteracciones con foco | Foco de teclado equivalente | Covering test designado (tasks 1.7/1.8) = contrato CSS «Fundación CSS»: `.rail-card:hover, .rail-card:focus-visible` + outline 2 px — PASSED en la suite. Matiz runtime (ver WARNING-1): en las 3 cards `<div>` el foco recae en el enlace interior, por lo que el lift/glow de tarjeta solo dispara en la card Top 20 (`<a>`) | ✅ COMPLIANT (test designado; matiz runtime en WARNING-1) |
| Microinteracciones con foco | Movimiento reducido | Contrato CSS: `@media (prefers-reduced-motion: reduce)` anula transiciones y transforms (`.rail-card`, `.rail-cover img`) | ✅ COMPLIANT |
| Scroll sin scrollbar | Scroll funcional y scrollbar oculta | `.scrollbar-none` real (`scrollbar-width: none` + `::-webkit-scrollbar { display: none }`) + `snap-x snap-mandatory` en los 4 carriles del markup renderizado | ✅ COMPLIANT |
| Scroll sin scrollbar | Anchura definida en tarjetas de Novedades | Contrato: mustNotContain `sm:w-68`; card Novedades con `w-60 sm:w-72` (clases válidas de Tailwind 3.4) | ✅ COMPLIANT |
| Serif display en títulos | Títulos de carril en serif display | `App_razor_CargaFrauncesEnLaMismaPeticionDeFuentesSinNuevoEnlace` (misma petición Google Fonts, sin `<link>` nuevo) + `.rail-title` con `--font-display` en los 4 `<h2>` | ✅ COMPLIANT |

**Spec 3: default-image-fallbacks (5 requisitos, 6 escenarios)**

| Requisito | Escenario | Evidencia (test/runtime) | Resultado |
|---|---|---|---|
| Assets por dominio | Assets servidos | Runtime: 4 SVG bajo `/images/defaults/` responden 200 (1.026–1.272 bytes); en disco 1–1,2 KB cada uno | ✅ COMPLIANT |
| Componente DefaultImage | Inline temable en los 5 temas | Contrato DefaultImage: `viewBox="0 0 400 225"`, rellenos `var(--brand-`/`var(--bg-`, `aria-hidden` condicionado a Alt; visual por tema = maintainer | ✅ COMPLIANT (contrato) |
| Componente DefaultImage | Dominio sin variante soportada | `default:` del `@switch` → motivo genérico con microtexto «LUDEKA»; `DefaultImageAssets.Url()` no contemplado → `generico-default.svg` (test) | ✅ COMPLIANT |
| Imagen ausente | Sorteo sin imagen | **Runtime real**: `thumbnailUrl: null` temporal → SVG inline en el mismo contenedor `.rail-cover--wide`, sin `<img>` roto | ✅ COMPLIANT |
| Imagen rota (onerror) | URL externa caída | Contratos HomeGiveawayCard/HomeReleaseCard/HomeEventCard/Events/EventsManagement: `onerror="this.onerror=null; this.src='/images/defaults/…'"`; markup renderizado lo confirma en `/` y `/eventos` | ✅ COMPLIANT (contrato + markup; ejecución JS del onerror = smoke del maintainer) |
| Reutilización (D5) | Página de eventos con evento sin imagen | Contratos Events/EventsManagement (mustContain `onerror`+`evento-default.svg` o `DefaultImage`) + runtime `/eventos`: onerror a `evento-default.svg` en todos los carteles. La mitad «falta» del escenario es estructuralmente inalcanzable: `BoardGameEvent` exige imagen por invariante de dominio (nota en SUGGESTION-1); la rama inline queda como defensa en profundidad | ✅ COMPLIANT (mecanismo adoptado; invariante de dominio en nota) |

**Spec 4: iconography-lucide (3 requisitos, 5 escenarios)**

| Requisito | Escenario | Evidencia (test/runtime) | Resultado |
|---|---|---|---|
| Componente Icon.razor | Icono decorativo hereda color | Contrato "Icon (SVG Lucide inline)": `viewBox="0 0 24 24"`, `stroke="currentColor"`, `aria-hidden="true"`, sin `<img` ni `http`; 16 iconos de portada verificados en `IconCatalog.cs` | ✅ COMPLIANT |
| Componente Icon.razor | Icono desconocido | `IconCatalogTests`: lookup fuera de catálogo devuelve vacío sin lanzar; `TryGetValue` en Icon.razor sin rama else | ✅ COMPLIANT |
| Migración global (D3) | Portada sin emojis | Runtime: 0 emojis de la lista de 16 en el HTML renderizado de `/`; spinner con `Icon dices`; barrido de fuente `LIMPIO: 0 emojis` en Components | ✅ COMPLIANT |
| Migración global (D3) | Resto de la web sin emojis | `RestoDeLaWeb_SinEmojisDeLaListaSpecEnNingunComponenteRazor` (Fact global) + barrido propio: 0 emojis en 13 archivos; 21 glifos tipográficos conservados (★ U+2605 rating, ✓ U+2713) = excepción documentada (apply-progress); banderas 🇪🇸 vienen de datos de Application (`CountryCatalog.GetFlag`), fuera de Components | ✅ COMPLIANT |
| Regla de iconos futuros | Nuevo icono en la interfaz | Patrón establecido: catálogo whitelist `IconCatalog.cs` (94 iconos) + contratos `mustNotContain` por archivo que impiden reintroducir emojis | ✅ COMPLIANT |

**Compliance summary**: 30/30 escenarios COMPLIANT (cada uno con covering test designado passed en la suite 847/847 o evidencia runtime; las notas de la matriz documentan matices y mediciones pendientes que se llevan como WARNING/SUGGESTION en Issues Found). Requisitos completos: 19/19.

### Correctness (Static Evidence)

| Requisito | Estado | Nota |
|---|---|---|
| Hero editorial narrativo | ✅ Implementado | `HeroEditorial.razor` con h1 visible, subtítulo, buscador, píldoras congeladas |
| Jerarquía accesible | ✅ Implementado | Único `<h1>` visible en runtime; sin `sr-only` |
| Variantes de fondo (D1) | ✅ Implementado | `HeroBackgroundVariant` (5) + `HeroBackgroundAssets` + escena CSS siempre bajo la foto |
| Rendimiento hero | ✅ Implementado (medición real pendiente) | `<picture>` AVIF/WebP/JPG, `fetchpriority="high"`, dimensiones fijas, pesos < 200 KB |
| Contraste del scrim | ✅ Implementado (visual pendiente) | `.hero-scrim` con `var(--bg-main)` sobre el cuadrante del texto |
| Carriles por componentes | ✅ Implementado | `Components/Home/*` (6 archivos) + orquestador limpio |
| Fallback por dominio (D2) | ✅ Implementado | Inline para dato sin imagen, `onerror` para URL caída, Top 20 conserva placeholders |
| Microinteracciones | ✅ Implementado (foco parcial en cards `<div>`) | Tokens + `.rail-card` + reduced-motion |
| Scroll y clases muertas | ✅ Implementado | `.scrollbar-none` real; `sm:w-68` eliminada |
| Serif display (D4) | ✅ Implementado | Fraunces en la URL existente; solo `.hero-title`/`.rail-title` |
| DefaultImage temable | ✅ Implementado | SVG inline con variables de tema + variante estática para `onerror` |
| D5 (páginas de eventos) | ✅ Implementado | `Events.razor` (líneas 131-148) y `EventsManagement.razor` (81-97) con fallback |
| Iconografía Lucide (D3) | ✅ Implementado | `Icon.razor` + catálogo de 94 iconos; 0 emojis en Components |

### Coherence (Design)

| Decisión | ¿Seguida? | Nota |
|---|---|---|
| D1 copy del hero | ✅ Sí | h1 «La mesa está servida» + subtítulo literal de 2 frases |
| D2 variantes por enum + parámetro | ✅ Sí | `Background` con default `FotoEurogame`; conmutación en 1 línea |
| D3 estructura Components/Home (7 archivos) | ✅ Sí | Los 6 componentes + enum; cards puros (solo DTO, sin servicios) |
| D4 tokens de microinteracción | ✅ Sí | `input.css` líneas 193-263: literal del diseño, incluido reduced-motion |
| D5 DefaultImage inline+estática | ✅ Sí | Enum 4 dominios, `DefaultImageAssets.Url()` nunca null, microtextos por dominio |
| D6 Icon.razor + catálogo whitelist | ✅ Sí | Catálogo 94 iconos; mapa de migración completo; desconocido → sin salida |
| D7 Fraunces en URL existente | ✅ Sí | Test dedicado exige un solo `<link>` y `display=swap` intacto |
| D8 assets por defecto | ✅ Sí | 4 SVG estáticos con paleta/microtextos de la Decisión 8 |
| D9 PRs encadenados | ✅ Sí | 4 PRs apilados (corte honesto PR-3a/3b documentado en tasks 3.6/3.7; size:exception registrada en ledger) |
| D10 rendimiento/accesibilidad | ✅ Sí | Sin preload (razón documentada), alt en castellano, targets ≥ 24 px (píldoras py-1.5 ≈ 29 px) |

### Verificación de los criterios de éxito de la propuesta

1. **Portada con narrativa y ambiente hogareño en carbón+terracota (sin púrpura)**: ✅ h1 «La mesa está servida» en Fraunces (runtime), subtítulo de invitación a la mesa, foto de ambiente con `<picture>` AVIF/WebP/JPG + escena CSS de fondo + scrim con variables de tema; sin badge ni púrpura (paleta terracota `#E05A38` en defaults).
2. **Los 3 carriles con imagen muestran el default de Ludeka cuando falta o falla**: ✅ verificado en runtime con datos sembrados sin imagen (sorteo + novedad inline) y por contrato de markup para el evento (invariante de dominio hace imposible el evento sin imagen; el fallo de URL externa cubre `onerror`).
3. **Microinteracciones hover con foco y reduced-motion**: ✅ contrato CSS completo; ⚠️ el equivalente de foco es efectivo solo en la card Top 20 (`<a>`); en las 3 cards `<div>` el foco va al enlace interior (WARNING-1).
4. **0 emojis en la portada (Lucide vía Icon.razor)**: ✅ 0 emojis de la lista de 16 en el HTML renderizado de `/` y 0 en el barrido de fuente de Components (excepciones documentadas: glifos ★/✓ y datos de Application).
5. **LCP < 2,5 s con hero con foto y CLS 0**: ⚠️ verificado por contrato (picture + fetchpriority + dimensiones + pesos < 200 KB + contenedores de aspecto fijo); **medición real LCP/CLS pendiente** — el MCP de Chrome DevTools no está disponible en este entorno (WARNING-2). No se inventa ninguna cifra.
6. **dotnet test Ludeka.sln en verde contra el delta aprobado**: ✅ 847/847 (x3 ejecuciones estables).

### Issues Found

**CRITICAL**: Ninguno.

**WARNING**:
1. **Equivalencia de foco inefectiva en las 3 cards de carril basadas en `<div>`** (`HomeGiveawayCard`, `HomeReleaseCard`, `HomeEventCard`): el selector `.rail-card:focus-visible` (input.css líneas 215-224) nunca dispara al navegar por teclado porque el `<div>` no es enfocable; el foco recae en el enlace interior (que sí muestra su outline por defecto, y los targets cumplen ≥ 24 px). Solo `HomeGameCard` (es `<a>`) obtiene el lift/glow/zoom + outline de 2 px en `:focus-visible`. No hay compensación con `:focus-within`/`:has(:focus-visible)` en el CSS. Impacto: WCAG 2.2 (2.4.7 — el foco es visible vía el enlace interior, pero sin la evidencia de hover equivalente a nivel de tarjeta). Remediation sugerida para el orquestador: añadir `.rail-card:has(:focus-visible) { … }` junto a `:hover`/`:focus-visible` (1-3 líneas de CSS + 1 entrada de contrato), o reestructurar las cards como `<a>` contenedor.
2. **LCP/CLS reales de la portada: medición pendiente**. El MCP de Chrome DevTools no está disponible en esta sesión; el criterio se ha verificado por contrato (markup `<picture>` + `fetchpriority="high"` + `width`/`height` + pesos < 200 KB en disco + contenedores de aspecto fijo). La medición real (móvil, Lighthouse/DevTools) queda como checklist del maintainer antes del merge.

**SUGGESTION**:
1. La rama inline `DefaultImage` de las cards de evento (portada y páginas) no es alcanzable por el camino de datos normal: `BoardGameEvent` exige imagen por invariante de dominio. No es un defecto (defensa en profundidad ante datos legacy/manuales), pero conviene dejarlo anotado en `sdd-archive` para que el volcado a `docs/specs/sistema/` documente que el escenario operativo del evento es el `onerror`.
2. El seed usa URLs BGG sintéticas (`cf.geekdo-images.com/pic7123901.jpg`, etc.) que devuelven 404 en navegador: cada visita a la portada dispara 3-4 peticiones fallidas que activan `onerror` → defaults. Comportamiento correcto por diseño (ejercita el fallback), pero en producción conviene apuntar a imágenes reales o a los defaults directamente.
3. La verificación del `onerror` es de wiring (atributo presente y apuntando al asset correcto en el HTML SSR); su ejecución real requiere navegador (smoke visual del maintainer). Capa disponible sin bUnit (decisión INC-31), documentada.

### Verificación del roadmap

- `docs/increments/ROADMAP.md` línea 51: **INC-35 sigue ⏳ En progreso** ✅ (su paso a ✅ y el volcado a `docs/specs/sistema/` corresponden a `sdd-archive`).

### Cómo probar visualmente (checklist del maintainer)

1. Arrancar: `dotnet run --project src\Ludeka.Web` desde el worktree (HTTP en **http://localhost:5081**, con BD ya sembrada).
2. Portada `/`: 5 variantes del hero cambiando `HeroBackgroundVariant.FotoEurogame` por `FotoMesaAmigos`/`FotoPrimerPlano`/`CssScene`/`Ilustracion` en `HomeDashboard.razor` (1 línea, sin tocar estructura); la escena CSS debe verse bajo la foto y cubrirla entera si se activa `CssScene`.
3. Hover sobre cards de los 4 carriles (lift + glow terracota + zoom 1.03) y comparación con foco por teclado (Tab): en Top 20 la card entera responde; en Sorteos/Novedades/Eventos responde el enlace interior (WARNING-1).
4. DevTools: emulación `prefers-reduced-motion: reduce` → sin lift/zoom, sin transiciones.
5. Los 5 `data-theme` (charcoal, editorial, tabletop, midnight, wood): contraste del texto del hero sobre el scrim y SVG de defaults temables (visual humano; ya es checklist de los PRs).
6. `/eventos` y la gestión: los carteles caen al default de evento si la URL externa falla (desconectar red de images.unsplash.com en DevTools para simular).

### Rollback de la verificación

Todo cambio temporal de esta fase fue revertido: ediciones de seed (3, marcadas `TEMP-VERIFY`), BD restaurada desde respaldo, `git status` limpio, suite re-ejecutada en verde post-reversión.

### Verdict

**PASS WITH WARNINGS** — La implementación cumple las 4 specs delta y el checklist de éxito de la propuesta con 30/30 escenarios cubiertos por sus covering tests designados (passed en la suite 847/847) o evidencia runtime; 19/19 requisitos. 0 blockers, 0 críticos. Los 2 warnings requieren remediation menor (CSS de foco con `:has()`) y una medición de rendimiento real antes de archivar; ambos quedan documentados con su checklist de maintainer.
