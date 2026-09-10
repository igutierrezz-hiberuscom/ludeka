# 24. Fundaciones Editoriales Compartidas y Rediseño de Páginas (INC-36)

> **Estado del Módulo:** ✅ Implementado y Verificado  
> **Incremento Asociado:** [INC-36 — `rediseno-paginas-editoriales` (archivado)](file:///c:/repos/Ludeka/openspec/changes/archive/2026-09-10-rediseno-paginas-editoriales/proposal.md)  
> **Pruebas Automatizadas:** suite total **855/855** en verde; verificación SDD **PASS** con 13/13 requerimientos y 34/34 escenarios COMPLIANT; contratos de markup **103/103**  
> **Specs Vivas:** [`editorial-page-foundations`](file:///c:/repos/Ludeka/openspec/specs/editorial-page-foundations/spec.md) (nueva), [`home-landing-hero`](file:///c:/repos/Ludeka/openspec/specs/home-landing-hero/spec.md) y [`default-image-fallbacks`](file:///c:/repos/Ludeka/openspec/specs/default-image-fallbacks/spec.md) (actualizadas)

---

## 1. Propósito y Filosofía

Este módulo extiende el sistema editorial de INC-35 (ver [módulo 23](file:///c:/repos/Ludeka/docs/specs/sistema/23-portada-editorial.md)) al resto de páginas —Catálogo, Ficha, Eventos, Sorteos y Novedades— bajo un principio rector: **fundaciones compartidas antes que parches por página**. Todo el incremento es presentación: cero cambios de DTOs, servicios, dominio, datos o rutas.

1. **Accesibilidad medible:** el token `--on-brand` resuelve el fallo WCAG 2.2 AA del texto sobre botones de marca (3,69:1 → ≥ 4,5:1 en los 5 temas).
2. **Tokens, no hardcodes:** los colores de estado (error/warning/info/highlight) abandonan las familias Tailwind fijas y pasan a tokens temáticos; las variantes `dark:` inertes desaparecen de las páginas contratadas.
3. **Componentes sobre duplicación:** `PageHeaderEditorial` (cabecera de listado) y `EditorialModal` (shell de diálogo) eliminan ~175 líneas de markup duplicado entre páginas.
4. **Lenguaje editorial común:** las tarjetas de Eventos, Novedades y Sorteos adoptan `.rail-card` (mismo lift/glow/foco/movimiento reducido de la portada); el Catálogo conserva `.game-card-editorial`.

---

## 2. Fundaciones de Tokens (`Styles/input.css`)

### 2.1 Token `--on-brand` (D3)

Color de texto sobre superficies de marca (`--brand-primary`), declarado en los 5 bloques `data-theme`:

| Tema | `--brand-primary` | `--on-brand` | Contraste verificado |
|---|---|---|---|
| `charcoal` | `#E05A38` | `#14181C` (tinta) | ≈ 4,83:1 |
| `editorial` | `#B8432F` | `#FFFFFF` | ≈ 5,41:1 |
| `tabletop` | `#D97736` | `#14181C` | ≈ 5,64:1 |
| `midnight` | `#06B6D4` | `#14181C` | ≈ 7,35:1 |
| `wood` | `#B85323` | `#FFFFFF` | ≈ 4,88:1 |

Adopción: todo botón con fondo `var(--brand-primary)` de las 5 páginas y `GiveawayCard` (buscar del hero, filtros territoriales y botones de acción de Events/Radar/News, «Generar Síntesis con IA Ahora» de la ficha, «Participar» de sorteos) más `.filter-btn.active` de la tira de filtros del Catálogo. Fuera del barrido (segunda ola): `.skip-link:focus`, el badge de `MainLayout.razor` y el hover de `HomeGiveawayCard.razor`.

### 2.2 Tokens semánticos de estado (D8/D9)

12 tokens nuevos en los 5 temas —`--state-error`, `--state-warning`, `--state-info` y `--state-highlight`, cada uno con sus sufijos `-bg` (alfa 0,10) y `-border` (alfa 0,28)— con tonos claros (`#FB7185`, `#FBBF24`, `#38BDF8`, `#C084FC`) en los temas oscuros (charcoal, tabletop, midnight) y tonos profundos (`#BE123C`, `#92400E`, `#0369A1`, `#A21CAF`) en los claros (editorial, wood). Sustituyen los hardcodes de las familias amber/indigo/purple/rose/sky/slate en los 6 archivos contratados (`Home`, `GameDetail`, `Events`, `Radar`, `News` y `GiveawayCard`) y eliminan las variantes `dark:` inertes: la app temiza por `data-theme`, nunca con la clase `dark`, y `tailwind.config.js` no habilita `darkMode` por tema.

**Excepción contratada:** los chips/overlays con fondo `bg-black/60` sobre fotografía (urgencia genérica y territorial) conservan sus neutrales fijos porque contrastan contra la imagen, no contra el tema.

### 2.3 Guarda de movimiento reducido

Dentro del `@media (prefers-reduced-motion: reduce)` existente en `.rail-card` se añade `.animate-pulse { animation: none; }`, cerrando el hueco del pulso de urgencia de sorteos.

---

## 3. Componentes Compartidos

### 3.1 `PageHeaderEditorial.razor` (`Components/Shared/`)

Cabecera editorial de página: badge en píldora (`Badge` + `BadgeIcon` Lucide opcional), título `h1` único con la clase `.page-header-title` (serif display `--font-display`, agrupada con `.rail-title` en `input.css`), subtítulo opcional y zona `Actions` (RenderFragment) a la derecha. El título es RenderFragment para conservar composición rica (punto terracota del Catálogo).

| Página | Badge (+icono) | h1 | Acción |
|---|---|---|---|
| `Home.razor` (Catálogo) | «Catálogo Colaborativo» + `dices` | «Descubre tu próxima partida.» + punto terracota | ninguna |
| `Events.razor` | «Calendario Oficial del Sector» + `tent` | «Grandes Citas, Ferias & Festivales» | «Gestionar Eventos» (moderador) |
| `Radar.razor` | «Radar de Sorteos Comunitarios» + `gift` | «Sorteos de Juegos de Mesa» | «Proponer Sorteo» (todos) |
| `News.razor` | «Calendario de Estrenos» + `newspaper` | «Novedades de los Viernes & Lanzamientos» | «Añadir Novedad» (moderador) |

La ficha (`GameDetail.razor`) **no** la adopta: su cabecera es el hero de ficha con backdrop y el título del juego en sans bold. Cada listado queda con exactamente un `<h1>` (verificado en navegador en las 7 rutas del incremento).

### 3.2 `EditorialModal.razor` (`Components/Shared/`) + `editorial-modal.js`

Shell compartido de modal: overlay `fixed inset-0 z-50 bg-black/60 backdrop-blur-sm`, tarjeta `max-w-lg` con `max-h-[90vh]` y scroll, diálogo `role="dialog" aria-modal="true" aria-label="{Title}"`, cabecera con `TitleIcon` y botón de cierre `aria-label="Cerrar {Title}"`; API con `Open`, `Title`, `TitleIcon`, `OnClose`, `ChildContent` y `Footer` (los pies de acción de Radar/News pasan a este fragment sin cambios de handlers).

`wwwroot/js/editorial-modal.js` (objeto global `ludekaModal`) gestiona el foco: guarda el elemento enfocado al abrir, enfoca el cierre, restaura el foco al disparador al cerrar y cierra con `Escape`. El componente permanece **siempre montado** (la visibilidad la gobierna el shell), lo que garantiza que `ludekaModal.close` se invoque y el foco vuelva al mismo nodo disparador — verificado con navegador real en `/sorteos` (Escape) y `/novedades` (clic en la X). Sin JS disponible degrada a markup puro sin romper apertura/cierre.

### 3.3 Lenguaje `.rail-card` en tarjetas de página

`GiveawayCard.razor`, las tarjetas de `Events.razor` y las de `News.razor` usan `.rail-card` (lift + glow `--brand-glow`, `focus-visible` visible ≥ 2 px, `prefers-reduced-motion`) con contenedor de imagen `rail-cover`. El Catálogo mantiene `.game-card-editorial` en `GameCard.razor` (decisión cerrada, sin migrar).

---

## 4. Adopción por Página

| Página | Cambio funcional | Detalle |
|---|---|---|
| Catálogo (`/catalogo`) | Cabecera compartida + búsqueda propia + `scrollbar-none` | Ver [módulo 15](file:///c:/repos/Ludeka/docs/specs/sistema/15-dashboard-inicio-editorial.md) |
| Ficha (`/juegos/{slug}`) | Tokenización, back-bar envolvente, `<PageTitle>` «Ludeka» | Ver [módulo 01](file:///c:/repos/Ludeka/docs/specs/sistema/01-catalogo-y-fichas.md) |
| Eventos (`/eventos`) | Cabecera, `.rail-card`, badges tokenizados, `tabpanel`/`aria-controls` | Ver [módulo 16](file:///c:/repos/Ludeka/docs/specs/sistema/16-sorteos-novedades-y-eventos.md) |
| Sorteos (`/sorteos`, alias `/radar`) | Cabecera, `EditorialModal`, chips de estado, `GiveawayCard` con fallback | Ver [módulo 16](file:///c:/repos/Ludeka/docs/specs/sistema/16-sorteos-novedades-y-eventos.md) |
| Novedades (`/novedades`) | Cabecera, `EditorialModal`, zona de imagen siempre renderizada | Ver [módulo 16](file:///c:/repos/Ludeka/docs/specs/sistema/16-sorteos-novedades-y-eventos.md) y [módulo 23](file:///c:/repos/Ludeka/docs/specs/sistema/23-portada-editorial.md) |
| Portada (`/`) | Fix responsive del hero + foco por variante + botón Buscar AA | Ver [módulo 23](file:///c:/repos/Ludeka/docs/specs/sistema/23-portada-editorial.md) |

### 4.1 Fixes transversales cerrados

- **Back-bar de la ficha (D7):** `flex-wrap` con dos grupos (navegación y acciones) y las acciones de moderación agrupadas tras un borde; «Ir a Mi Ludoteca» pasa al grupo de navegación. Sin desborde en móvil con moderador activo.
- **`scrollbar-none` real:** la tira de filtros del Catálogo usa la clase definida en CSS; la clase muerta `no-scrollbar` no persiste.
- **Pestañas de Eventos accesibles:** cada pestaña declara `aria-controls` hacia su panel con `role="tabpanel"` (`panel-upcoming`/`panel-past`).
- **Errata corregida:** `<PageTitle>` de la ficha dice «Ludeka» (antes «Ludeca»).
- **Estado «juego no encontrado»:** sin `text-white`; legible en los 5 temas con tokens de texto.

---

## 5. Pipeline del CSS Compilado

`Styles/input.css` sigue siendo el punto único de verdad de tokens. Tras cada cambio se regenera `wwwroot/app.css` con el pipeline vigente (desde `src/Ludeka.Web`, sin `package.json`):

```powershell
npx.cmd -y tailwindcss@3.4.17 -i ./Styles/input.css -o ./wwwroot/app.css --minify
```

La verificación de INC-36 comprueba el CSS servido: la regla compilada `.hero-editorial{width:100%;aspect-ratio:16/9;min-height:200px;max-height:clamp(200px,36vw,460px)}` presente, `--on-brand` y `page-header-title` en `app.css`, y ausencia de `min-height:360px`/`min-height:460px`.

---

## 6. Arquitectura Tocada (por Capa)

| Capa | Archivos | Cambio |
|---|---|---|
| **Web** | `Styles/input.css` | Tokens `--on-brand` y de estado ×5, bloque `.hero-editorial`, 4 `.hero-focal--*`, `.page-header-title`, `.filter-btn.active`, guarda reduced-motion |
| **Web** | `Components/Shared/PageHeaderEditorial.razor`, `EditorialModal.razor` | Nuevos |
| **Web** | `wwwroot/js/editorial-modal.js` | Nuevo: foco al abrir/cerrar y Escape |
| **Web** | `Components/App.razor` | `<script src="/js/editorial-modal.js" defer>` |
| **Web** | `Pages/Home.razor`, `GameDetail.razor`, `Events.razor`, `Radar.razor`, `News.razor`, `Shared/GiveawayCard.razor` | Adopción de fundaciones, componentes y fixes |
| **Web** | `Components/Home/HeroEditorial.razor`, `HeroBackgroundVariant.cs` | Fix responsive y `FocalClass` (ver módulo 23) |
| **Web** | `wwwroot/app.css` | Regenerado (pipeline Tailwind 3.4.17) |
| **Core / Application / Infrastructure** | — | Sin cambios: el incremento es presentación pura |

---

## 7. Estrategia de Pruebas

Suite **855/855** (baseline 854 previo a INC-36 + 1 Fact acotado; `WebMarkupContractTests` **103/103**, `PerformanceAndAccessibilityTests` **5/5**). TDD estricto patrón INC-31 (sin bUnit): cada cambio de markup entró con su contrato rojo → verde en el mismo commit.

| Foco | Mecanismo |
|---|---|
| Tokens CSS por tema | Facts que parsean los 5 bloques `[data-theme=…]` (`--on-brand`, 4 tokens de estado) |
| Contratos de markup | Filas `mustContain`/`mustNotContain` por archivo (cabeceras, modal, `.rail-card`, fallbacks, ausencia de `dark:`/hardcodes/`no-scrollbar`/«Ludeca») |
| CSS compilado | Facts sobre `app.css` (fundación INC-36 y regla `.hero-editorial` acotada) |
| Runtime | Chrome real (DevTools MCP): 7 rutas con 1 `<h1>`, hero ×3 viewports, modales con restauración de foco, CLS/LCP |

---

## 8. Fuera de Alcance (Segunda Ola)

Barrido global de tokens fuera de las 5 páginas (MyLibrary, directorios, admin, subcomponentes compartidos consumidos por varias superficies), `darkMode` por `data-theme` global, `srcset`/recortes autoriales del hero (estrategia C), reemplazo de assets hotlink de Unsplash en Eventos, `.skip-link:focus`/badge de `MainLayout`/hover de `HomeGiveawayCard`, skeletons/streaming SSR y pruebas automatizadas de restauración de foco (capa de render/browser).

---

## 9. Referencias

- Diseño con DD-01..DD-11: [`design.md` del cambio archivado](file:///c:/repos/Ludeka/openspec/changes/archive/2026-09-10-rediseno-paginas-editoriales/design.md).
- Verificación con evidencia de navegador: [`verify-report.md` archivado](file:///c:/repos/Ludeka/openspec/changes/archive/2026-09-10-rediseno-paginas-editoriales/verify-report.md).
- Spec viva de fundaciones: [`editorial-page-foundations`](file:///c:/repos/Ludeka/openspec/specs/editorial-page-foundations/spec.md).
