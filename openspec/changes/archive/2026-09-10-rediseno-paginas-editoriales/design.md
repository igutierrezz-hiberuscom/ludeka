# Diseño: Rediseño Editorial del Resto de Páginas + Fix Responsive del Hero (INC-36)

> Fase SDD `sdd-design`. Idioma: español castellano (regla suprema AGENTS.md, que prevalece sobre cualquier default en inglés o presupuesto de extensión de la skill). Store: hybrid (este archivo + Engram `sdd/rediseno-paginas-editoriales/design`).
> Entradas leídas del worktree: `proposal.md` (aprobada, commit 0f13f41), `specs/editorial-page-foundations/spec.md`, `specs/home-landing-hero/spec.md`, `specs/default-image-fallbacks/spec.md`, `explore.md`, y el código real: `Styles/input.css`, `Components/Home/HeroEditorial.razor`, `HeroBackgroundVariant.cs`, las 5 páginas (`Home`, `GameDetail`, `Events`, `Radar`, `News`), `Shared/GiveawayCard.razor`, `Shared/DefaultImage.razor`, `WebMarkupContractTests.cs`, `PerformanceAndAccessibilityTests.cs`, `dev.ps1`, `Dockerfile`, `tailwind.config.js`.
> Este diseño NO reabre D1=D1(A), D3, D4/D5, D6, D7, D8, D9, D10, D11, D12, D13 de la propuesta: los aplica. Sus propias decisiones se numeran **DD-01..DD-11** para no colisionar con la numeración de la propuesta.

---

## Enfoque técnico

Extensión del sistema editorial de INC-35, no reemplazo: `Styles/input.css` sigue siendo el punto único de verdad de tokens; el fix del hero se hace en sitio (CSS del contenedor + foco por variante) sin tocar el mecanismo de variantes ni los contratos INC-35 congelados; dos componentes compartidos nuevos (`PageHeaderEditorial`, `EditorialModal`) eliminan duplicación entre páginas; cada cambio de markup entra por TDD estricto (contrato rojo → implementación verde) en el mismo commit que actualiza los contratos INC-31/35 afectados. Todo el incremento es presentación: cero cambios de DTOs, servicios, dominio o datos.

## Decisiones de diseño

### DD-01 — Modelo de altura del hero (responde al ADDED de `home-landing-hero`)

**Choice**: el contenedor `.hero-editorial` pasa a tener altura derivada del ancho, definida íntegramente en `input.css` (nuevo bloque; hoy `.hero-editorial` no tiene regla CSS, es solo un nombre en el markup):

```css
/* Hero editorial: alto derivado del ancho (INC-36, D1=A) */
.hero-editorial {
    aspect-ratio: 16 / 9;
    min-height: 200px;                       /* suelo acotado: garantiza el buscador superpuesto (~110-130px) */
    max-height: clamp(200px, 36vw, 460px);   /* cap progresivo: reaches ~460px en ultrapanorámicos */
}
```

En `HeroEditorial.razor:11` se **elimina** `min-h-[360px] sm:min-h-[460px]` y se conserva el resto (`relative overflow-hidden rounded-3xl border shadow-xl`). El control de altura queda unificado en `.hero-editorial` (punto único, contratatable por grep).

**Comportamiento verificado con la matemática de `object-fit: cover`:**

| Viewport | Contenedor | Alto resultante | Recorte |
|---|---|---|---|
| 320px (suelo) | 280×200 | 200px (suelo manda) | X ≤ 21% (focal X lo absorbe) |
| 360px (caso del bug) | 320×200 | 200px (antes 360) | X ~10% — proporcional, sin caja casi cuadrada |
| 375-588px | ratio ≤ 200px de suelo | 200px | leve |
| 640px (sm) | 600×230 | 230px (36vw) | Y (banda panorámica) |
| 1240px (contenedor máx. 1200px) | 1200×446 | 446px (36vw del viewport 1240; aún bajo el cap 460) | Y ~55-69% visible según variante |
| ≥1278px | 1200×460 | 460px (cap alcanzado a partir de 1278px: 36vw ≥ 460) | Y |

- **CLS 0 conservado**: el alto deriva del ancho (no del contenido); los hijos siguen `position: absolute` (`hero-scene`, `hero-bg-media`, `hero-actions`) y el `<img>` conserva `width`/`height` (congelados por el contrato INC-35). No hay salto al cruzar `sm` porque no hay media query: la transición es continua.
- **Congelado (no se toca)**: `<picture>` AVIF/WebP/JPG, `fetchpriority="high"`, `width="1600" height="900"`, `onerror` → `hero-bg-media--failed`, alt castellano por variante, `h1 sr-only` «La mesa está servida», handler `HandleQuickSearch`.
- **Inconsistencia documentada (no corregida aquí)**: los assets reales miden `eurogame`/`mesa-amigos` 1600×1068 (3:2), `primer-plano` 1600×2400 (vertical 2:3) y `ilustracion` 1600×893 (≈16:9); los atributos declarados 1600×900 no coinciden con 3 de 4. Impacto nulo en runtime (el `<img>` es `inset:0` con CSS `100%/100%`; el ratio declarado solo alimenta el hint previo a CSS). Corregir valores o per-variantes (`HeroBackgroundAssets.Width/Height`) pertenece a la segunda ola C (srcset/recortes autoriales).

**Alternativas descartadas**: B (`height: clamp()` puro) recrea la caja casi cuadrada del bug; C (srcset + recortes) queda a segunda ola por decisión cerrada de la propuesta.

### DD-02 — Punto focal `object-position` por variante (D2, cierra lo que la spec delegó)

El fix del recorte residual exige foco autoral **por variante**, no un centrado por defecto. Mecanismo: `HeroBackgroundAssets` incorpora un mapa `FocalClass(variant)` → clase modificadora aplicada a la `<section>` del hero (`hero-focal--{clave}`, vacío para `CssScene`), y `input.css` declara el foco por variante:

```css
.hero-focal--eurogame img     { object-position: 62% 70%; }
.hero-focal--mesa-amigos img  { object-position: 45% 55%; }
.hero-focal--primer-plano img { object-position: 50% 30%; }
.hero-focal--ilustracion img  { object-position: 35% 45%; }
```

Justificación autoral (inspección visual de los assets en `wwwroot/images/home`, con la banda visible real del nuevo modelo: escritorio deja ver ~57% del alto en assets 3:2, ~26% en el vertical y ~69% en la ilustración):

| Variante | Composición del asset | Foco elegido | Por qué |
|---|---|---|---|
| `eurogame` (3:2) | Tablero de madera con fichas y casilla azul abajo-derecha; piezas sueltas arriba-izquierda | `62% 70%` | Y=70% mantiene el tablero y la casilla azul en cuadro (corta madera vacía arriba y solo la última fila del tablero); X=62% protege el lado del tablero en el recorte residual del suelo móvil |
| `mesa-amigos` (3:2) | Caras del grupo en y≈35-55%; juego sobre mesa y≈68-88% | `45% 55%` | Y=55% conserva caras y la parte superior del juego sobre la mesa (no caben ambas franjas completas; las caras mandan — lectura editorial); X=45% protege la cara del jugador del borde izquierdo |
| `primer-plano` (2:3 vertical) | Tablero con miepas/cartas en y≈10-45%; mano+caja y≈48-78%; suelo abajo | `50% 30%` | En banda vertical solo cabe una franja: se elige el tablero en juego (miepas/cartas) + brazo/persona; se recortan reglamento y suelo. Nota: el alt («manos colocando piezas») queda parcial (el brazo sí aparece) — se documenta, el alt está congelado por INC-35 |
| `ilustracion` (≈16:9) | Logo LDK hexagonal a la izquierda (y≈18-78%); mesa con eurogame a la derecha; lámpara arriba | `35% 45%` | Y=45% mantiene el logo íntegro y la mesa; X=35% protege el logo en el recorte del suelo móvil (recorta margen derecho decorativo) |

Los 4 valores difieren del `50% 50%` por defecto (exigencia del MODIFIED de la spec). El cambio de variante con `?hero=` re-renderiza la sección con su clase de foco (comprobado: la clase va en el markup del componente).

### DD-03 — Token `--on-brand` por tema (D3)

Declarado dentro de los 5 bloques `data-theme` de `input.css`, junto a `--brand-glow`. Valores cerrados con cálculo WCAG (luminancia relativa sRGB, `(L1+0.05)/(L2+0.05)`):

| Tema | `--brand-primary` | `--on-brand` | Contraste sobre la marca |
|---|---|---|---|
| `charcoal` | `#E05A38` | `#14181C` (tinta) | ≈ 4,83:1 ✓ (antes blanco = 3,69:1 ✗) |
| `editorial` | `#B8432F` | `#FFFFFF` | ≈ 5,41:1 ✓ (la tinta daría 3,30:1 ✗) |
| `tabletop` | `#D97736` | `#14181C` | ≈ 5,64:1 ✓ |
| `midnight` | `#06B6D4` | `#14181C` | ≈ 7,35:1 ✓ |
| `wood` | `#B85323` | `#FFFFFF` | ≈ 4,88:1 ✓ |

- Se conserva la tinta global `#14181C` (coincide con `--bg-main` de charcoal) en los 3 temas de marca cálida/clara, y blanco en los 2 claros, tal como fijó D3 y validó visualmente el maintainer al aprobar la propuesta.
- **Adopción (mismo commit por página)**: sustituir `text-white` por `text-[var(--on-brand)]` en TODO botón con fondo `var(--brand-primary)` de las 5 páginas: botón «Buscar» del hero (`HeroEditorial.razor:70`), «Gestionar Eventos» y «Añadir Evento Ahora» (Events.razor:30/117), «Web Oficial» (Events.razor:208), filtros territoriales activos (Events.razor:66/73, Radar.razor:55/62), «Proponer Sorteo»/«Añadir Sorteo»/«Guardar y Publicar» (Radar.razor:30/99/253), «Añadir Novedad»/«Añadir Primer Lanzamiento»/«Guardar y Publicar» (News.razor:28/82/278), «Generar Síntesis con IA Ahora» (GameDetail.razor:323) y «Participar» (GiveawayCard.razor:112).
- **En `input.css` también**: `.filter-btn.active` (línea 597, `color: #FFFFFF` → `var(--on-brand)`) — es el estado activo de la tira de filtros del Catálogo (una de las 5 páginas).
- **Fuera de alcance documentado** (barrido futuro, no toca el contrato de este incremento): `.skip-link:focus` (shell global, `color:#FFFFFF` sobre marca), `MainLayout.razor:103` (badge equipo fundador) y `HomeGiveawayCard.razor:56` (hover `text-white` en el carril de portada).

### DD-04 — Tokens semánticos de estado por tema (D8/D9)

Nombres (patrón de la familia existente `--color-mustplay/-bg/-border`): `--state-error`, `--state-error-bg`, `--state-error-border`, y los mismos tres sufijos para `--state-warning`, `--state-info`, `--state-highlight`. Alfa de fondo 0,10 y de borde 0,28 (homogéneo con el patrón vigente). Valores por tema:

| Token | `charcoal` | `tabletop` | `midnight` | `editorial` | `wood` |
|---|---|---|---|---|---|
| `--state-error` | `#FB7185` | `#FB7185` | `#FB7185` | `#BE123C` | `#BE123C` |
| `--state-error-bg` | `rgba(251,113,133,0.10)` | idem | idem | `rgba(190,18,60,0.10)` | idem |
| `--state-error-border` | `rgba(251,113,133,0.28)` | idem | idem | `rgba(190,18,60,0.28)` | idem |
| `--state-warning` | `#FBBF24` | `#FBBF24` | `#FBBF24` | `#92400E` | `#92400E` |
| `--state-warning-bg` | `rgba(251,191,36,0.10)` | idem | idem | `rgba(146,64,14,0.10)` | idem |
| `--state-warning-border` | `rgba(251,191,36,0.28)` | idem | idem | `rgba(146,64,14,0.28)` | idem |
| `--state-info` | `#38BDF8` | `#38BDF8` | `#38BDF8` | `#0369A1` | `#0369A1` |
| `--state-info-bg` | `rgba(56,189,248,0.10)` | idem | idem | `rgba(3,105,161,0.10)` | idem |
| `--state-info-border` | `rgba(56,189,248,0.28)` | idem | idem | `rgba(3,105,161,0.28)` | idem |
| `--state-highlight` | `#C084FC` | `#C084FC` | `#C084FC` | `#A21CAF` | `#A21CAF` |
| `--state-highlight-bg` | `rgba(192,132,252,0.10)` | idem | idem | `rgba(162,28,175,0.10)` | idem |
| `--state-highlight-border` | `rgba(192,132,252,0.28)` | idem | idem | `rgba(162,28,175,0.28)` | idem |

Criterios: el texto de estado se usa sobre `--bg-card`/`--bg-surface-elevated` temáticos → los juegos oscuro/claro usan, respectivamente, tonos claros (≥5,5:1 sobre las tarjetas oscuras verificadas por cálculo) y tonos profundos (≥4,6:1 sobre blanco y sobre `--bg-surface-elevated` de `wood` #ECE2D5, el peor caso). Nota: `--color-notrec` (`#F43F5E`) fallaría como texto en oscuro (≈4,16:1); por eso el error usa `#FB7185`, no el notrec vigente (que se reserva a bordes/puntos del semáforo).

**Mapa de sustitución (clase hardcodeada → token), verificado línea a línea en el worktree:**

| Página: línea | Hardcode actual | Sustitución |
|---|---|---|
| `GameDetail.razor:16/26` | `text-slate-400` | `text-[var(--text-muted)]` |
| `GameDetail.razor:25` | `text-white` (h1 «Juego no encontrado») | `text-[var(--text-primary)]` |
| `GameDetail.razor:27` | `text-orange-400` (código del slug) | `text-[var(--text-primary)]` + `font-mono` |
| `GameDetail.razor:56` | `hover:bg-rose-500/10 hover:text-rose-400 hover:border-rose-500/30` | `hover:bg-[var(--state-error-bg)] hover:text-[var(--state-error)] hover:border-[var(--state-error-border)]` |
| `GameDetail.razor:76` | `bg-amber-500/10 hover:bg-amber-500/20 text-amber-400 border-amber-500/30` | `--state-warning` / `-bg` / `-border` |
| `GameDetail.razor:83` | `bg-indigo-500/10 hover:bg-indigo-500/20 text-indigo-400 border-indigo-500/30` | `--state-info` / `-bg` / `-border` |
| `GameDetail.razor:131` | `bg-purple-950/85 border-purple-500/40 text-purple-200` | `bg-[var(--state-highlight)] text-[var(--on-brand)]` (chip sólido) |
| `GameDetail.razor:294` | `bg-indigo-500/10 border-indigo-500/25 text-indigo-300` | `--state-info` / `-bg` / `-border` |
| `GameDetail.razor:299` | `text-slate-400 hover:text-white` | `text-[var(--text-muted)] hover:text-[var(--text-primary)]` |
| `GameDetail.razor:323` | `text-white` (botón de marca) | `text-[var(--on-brand)]` (DD-03) |
| `Events.razor:273/276` | `bg-rose-500/90 text-white border-rose-600` / `bg-amber-500/90 text-black border-amber-600` | `bg-[var(--state-error)] text-[var(--on-brand)] border-[var(--state-error-border)]` / `bg-[var(--state-warning)] text-[var(--on-brand)] border-[var(--state-warning-border)]` |
| `Events.razor:279` | `text-zinc-300 border-zinc-600` (overlay «Finalizado») | `text-white/80 border-white/10` (overlay neutro, familia blanca) |
| `Radar.razor:226-231` | `bg-amber-500/10 border-amber-500/30`, `border-amber-500/50 text-amber-500` | `--state-warning` / `-bg` / `-border` |
| `Radar.razor:240` y `News.razor:265` | `bg-rose-500/10 border-rose-500/30 text-rose-600 dark:text-rose-300` (error de formulario) | `bg-[var(--state-error-bg)] border-[var(--state-error-border)] text-[var(--state-error)]` — elimina la variante `dark:` muerta y el fallo AA 3,24:1 |
| `News.razor:53` | `text-rose-500` (× de chip) | `text-[var(--state-error)]` |
| `News.razor:164` | `text-pink-500 hover:text-pink-600 dark:text-pink-400` | `text-[var(--state-highlight)] hover:opacity-80` |
| `News.razor:171` | `hover:border-sky-500 text-sky-400` | `hover:border-[var(--state-info)] text-[var(--state-info)]` |
| `GiveawayCard.razor:13` | `bg-amber-500 text-black` | `bg-[var(--state-warning)] text-[var(--on-brand)]` |
| `GiveawayCard.razor:25/87` | `bg-amber-500/20 text-amber-500 border-amber-500/40` / `text-amber-500 hover:text-amber-600` | `--state-warning` triple / `text-[var(--state-warning)] hover:opacity-80` |
| `GiveawayCard.razor:96/103` | `text-pink-500…dark:` / `text-sky-500…dark:` | `text-[var(--state-highlight)]` / `text-[var(--state-info)]` + `hover:opacity-80` |
| `GiveawayCard.razor:146` | `bg-rose-500/15 border-rose-500/30 text-rose-600 dark:text-rose-400 animate-pulse` | `--state-error` triple (sin `dark:`) + `animate-pulse` se conserva con guarda de movimiento reducido |

**Excepción de overlay, intencional y contratada**: los chips con fondo `bg-black/60` sobre fotografía (urgencia genérica y territorial en Events.razor:152/281, overlays de News/Radar) **conservan** sus neutrales fijos: contratan contra la foto, no contra el tema; tokenizarlos rompería el contraste sobre imágenes arbitrarias. Se documentan como excepción al escenario «sin hardcodes» (el barrido prohíbe las familias amber/indigo/purple/rose/sky/slate; el overlay es familia blanca/negra).

**Guarda de movimiento reducido** (cierra el hueco detectado en explore para `animate-pulse`): en `input.css`, dentro del bloque existente `@media (prefers-reduced-motion: reduce)` de `.rail-card`, se añade `.animate-pulse { animation: none; }`.

**Alcance del barrido (DD-05)**: la sustitución cubre los 5 archivos de página + `GiveawayCard.razor` (nombrado por la spec). Los subcomponentes compartidos que GameDetail renderiza (`CollectionActionBar`, `Expansion*`, `FoundingVerdict*`, `QuickBadges`, `StoreOffersCard`, `SleeveGuideCard`, `MultimediaHub`, modales de edición compartidos…) contienen más hardcodes del mismo tipo, pero son consumidos también por portada/ludoteca/admin: tokenizarlos alteraría superficies fuera de las 5 páginas y rompería el presupuesto de PR. **Quedan fuera (segunda ola)** y así se registrará en el contrato (el barrido de estados solo aplica a los 6 archivos anteriores).

### DD-06 — `PageHeaderEditorial` (D4/D5)

**Ubicación (verificada en el repo)**: `src/Ludeka.Web/Components/Shared/PageHeaderEditorial.razor` — coherente con la estructura real: los componentes compartidos entre páginas viven en `Components/Shared/` (`GameCard`, `GiveawayCard`, `Icon`, `DefaultImage`, `CatalogSearchBar`) y los exclusivos de portada en `Components/Home/` (donde está de verdad `HeroEditorial.razor`, no en `Shared/` como decía la tabla de la propuesta — se corrige en la tabla de cambios). `_Imports.razor` ya importa `Ludeka.Web.Components.Shared`, cero `@using` nuevo.

**API (RenderFragment para el título permite conservar el punto terracota del Catálogo; el resto son strings)**:

```razor
<PageHeaderEditorial
    Badge="Calendario de Estrenos"          @* píldora, obligatoria *@
    BadgeIcon="newspaper"                    @* nombre Lucide, opcional *@
    Title="Novedades de los Viernes & Lanzamientos"  @* h1, obligatorio (RenderFragment) *@
    Subtitle="Cronología de novedades..."    @* opcional *@
    Actions="..."                            @* RenderFragment opcional (zona derecha) *@ />
```

Markup interno: réplica de la cabecera vigente de Events (13-35): contenedor `flex flex-col md:flex-row md:items-end justify-between gap-4 border-b border-[var(--border-subtle)] pb-6`, badge píldora sobre `--bg-surface-elevated`, subtítulo opcional, zona `Actions` a la derecha. El `h1` lleva la clase **`.page-header-title`**, declarada agrupada con `.rail-title` en `input.css` (misma receta: `--font-display`, `font-optical-sizing`, `weight 600`, `letter-spacing -0.01em`, color `--text-primary`) y tamaños por utilidades (`text-3xl sm:text-4xl`). Se agrupan selectores en vez de duplicar CSS; se actualiza el comentario del bloque («portada y cabeceras de página») y el contrato de `RailHeader` no cambia (sigue exigiendo `rail-title` en su propio archivo).

**Adopción y jerarquía de un solo `h1`**: el componente es el ÚNICO `<h1>` de cada página de listado (los encabezados internos ya son `h3`):

| Página | Badge (+icono) | h1 | Acción |
|---|---|---|---|
| `Home.razor` (Catálogo) | «Catálogo Colaborativo» + `dices` | «Descubre tu próxima partida.» + punto terracota (vía RenderFragment) | ninguna |
| `Events.razor` | «Calendario Oficial del Sector» + `tent` | «Grandes Citas, Ferias & Festivales» | «Gestionar Eventos» (moderador) |
| `Radar.razor` | «Radar de Sorteos Comunitarios» + `gift` | «Sorteos de Juegos de Mesa» | «Proponer Sorteo» (todos) |
| `News.razor` | «Calendario de Estrenos» + `newspaper` | «Novedades de los Viernes & Lanzamientos» | «Añadir Novedad» (moderador) |

El Catálogo abandona su variante centrada (`max-w-3xl mx-auto text-center`) y se homogeneiza al patrón izquierda de los otros tres; `CatalogSearchBar` pasa a un bloque propio inmediatamente debajo de la cabecera (`max-w-xl`, sin perder su handler). `GameDetail` **no lo adopta** (hero de ficha con backdrop, título sans bold — decisión cerrada D4/D5). Neto estimado: ~75 líneas duplicadas eliminadas.

### DD-07 — `EditorialModal` (D11)

**Ubicación**: `src/Ludeka.Web/Components/Shared/EditorialModal.razor` (ahí viven los demás modales). **Interop de foco**: precedente real en el repo (`LocationSelectorModal` usa `IJSRuntime` con funciones globales); se añade `src/Ludeka.Web/wwwroot/js/editorial-modal.js` (≤35 líneas: guarda el elemento enfocado antes de abrir, enfoca el botón de cierre al abrir, restaura el foco al cerrar, cierra con `Escape`) y su `<script src="/js/editorial-modal.js" defer></script>` en `App.razor` junto a los dos scripts existentes (líneas 76-77).

**API**:

```razor
<EditorialModal Open="@_isCreateModalOpen"
                Title="Registrar o Proponer Sorteo"
                TitleIcon="gift"
                OnClose="@(() => _isCreateModalOpen = false)">
    @* ChildContent: cuerpo específico (formulario con sus labels/inputs/checkbox) *@
    <Footer>
        <button>Cancelar</button>
        <button>Guardar y Publicar</button>
    </Footer>
</EditorialModal>
```

Parámetros: `bool Open`, `string Title`, `string? TitleIcon`, `EventCallback OnClose`, `RenderFragment? ChildContent`, `RenderFragment? Footer`. El shell renderiza: overlay `fixed inset-0 z-50 bg-black/60 backdrop-blur-sm animate-fade-in`; diálogo `role="dialog" aria-modal="true" aria-label="@Title"`; tarjeta `max-w-lg rounded-2xl bg-[var(--bg-card)] border p-6 shadow-2xl max-h-[90vh] overflow-y-auto`; cabecera con h2 + botón de cierre `aria-label="Cerrar {Title}"`.

**Fuera del shell**: campos, labels, checkbox de moderación (bordes ámbar → `--state-warning-*`, DD-04), div de error (→ `--state-error-*`) y lógica `HandleSubmit*` — quedan en Radar/News como contenido del shell. Adopción: Radar elimina su shell inline (~140 líneas, Radar.razor:120-135) y News el suyo (~100, News.razor:185-200); los pies de acciones pasan al fragment `Footer` sin cambios de handlers. Si el `IJSRuntime` no está disponible (prerender), el shell degrada a markup puro sin romper apertura/cierre (el foco coherente es mejora progresiva).

### DD-08 — Back-bar de ficha (D7) sobre `GameDetail.razor:38-108`

Solución concreta (envoltura + agrupación de moderación, sin menú «más acciones»):

```razor
<div class="container-ludeka py-3 flex flex-wrap items-center justify-between gap-x-4 gap-y-2">
    <div class="flex items-center gap-4">
        <a href="/catalogo">… Volver al catálogo</a>
        <a href="/mi-ludoteca" class="hidden sm:inline-flex …">Ir a Mi Ludoteca</a>
    </div>
    <div class="flex flex-wrap items-center gap-2">
        <!-- utilidades: BGG · Reportar · Cartel para Redes · Comprar (hovers de rose → --state-error-*) -->
        <div class="flex items-center gap-1.5 pl-2 border-l border-[var(--border-subtle)]">
            <!-- bloque de MODERACIÓN agrupado: Editar Ficha (warning) · Generar con IA (info) · Gestionar Veredicto -->
        </div>
    </div>
</div>
```

Cambios exactos: `flex items-center gap-3` (línea 42) → `flex flex-wrap items-center gap-2` con el subcontenedor de moderación separado por un borde; «Ir a Mi Ludoteca» (línea 104) pasa al grupo izquierdo con «Volver» (navegación junta, se oculta en `xs`); el agrupado envuelve como unidad en móvil y reduce 8-9 botones sueltos a 2 grupos. Contrato: `flex-wrap` presente en el back-bar y «Ir a Mi Ludoteca» fuera de la fila de acciones.

### DD-09 — Contratos de markup (TDD estricto, patrón INC-31)

Todos los cambios de markup entran con su contrato en el MISMO commit (rojo → verde). Baseline: **847 tests en verde** (`dotnet test Ludeka.sln`).

**Nuevos (en `MarkupContracts`):**
1. `PageHeaderEditorial (cabecera compartida)` — `Shared/PageHeaderEditorial.razor`: `page-header-title`, `<h1`, `badge-pill`, `Actions`.
2. `EditorialModal (shell compartido)` — `Shared/EditorialModal.razor`: `role="dialog"`, `aria-modal="true"`, `aria-label="@Title"`, `OnClose`, `ChildContent`, `Footer`, `ludekaModal.open`.
3. `Fundación INC-36 (on-brand y tokens de estado)` — dos `[Fact]` nuevos (el patrón TheoryData no sabe acotar por bloque): `FundacionCss_OnBrand_DeclaradoEnLosCincoDataTheme` y `FundacionCss_TokensEstado_DeclaradosEnLosCincoDataTheme` (parseo de los 5 bloques `[data-theme=…]`, assertion de `--on-brand` y de los 4 tokens base).

**Ajustados (misma filas TheoryData, más estrechos):**

| Contrato existente | Ajuste |
|---|---|
| `Fundación CSS (tokens, rail-card y hero)` (input.css) | mustContain +: `--on-brand`, `--state-error`, `--state-warning`, `--state-info`, `--state-highlight`, `aspect-ratio: 16 / 9`, `max-height: clamp(200px, 36vw, 460px)`, `.page-header-title`, `hero-focal--eurogame`, `hero-focal--mesa-amigos`, `hero-focal--primer-plano`, `hero-focal--ilustracion`, `object-position`; mustNotContain +: `min-height: 360px`, `min-height: 460px` |
| `HeroEditorial (picture, prioridad y escena CSS)` | mustContain +: `hero-focal--` (clase por variante), `text-[var(--on-brand)]`; mustNotContain +: `min-h-[360px]`, `sm:min-h-[460px]`, `text-white` (el chip de pruebas usa `text-[#F1F5F9]` sobre su fondo oscuro literal `#0d0a18` — par autocontenido, tema-independiente) |
| `Home catalogo (sin emojis)` | mustContain +: `<PageHeaderEditorial`, `scrollbar-none`; mustNotContain +: `<h1`, `no-scrollbar` |
| `Events (sin emojis)` | mustContain +: `<PageHeaderEditorial`, `text-[var(--on-brand)]`, `rail-card`, `role="tabpanel"`, `aria-controls="panel-`; mustNotContain +: `text-white` (solo en botones de marca; el overlay `bg-black/60` de badges es la excepción contratada de DD-04), `hover:scale-105`, `bg-rose-500/90`, `bg-amber-500/90`, `text-zinc-300`, `dark:` |
| `Radar (sin emojis)` | mustContain +: `<PageHeaderEditorial`, `<EditorialModal`; mustNotContain +: `dark:`, `text-amber-500`, `text-rose-600`, `fixed inset-0 z-50` (la aserción `rail-card` va en la fila de `GiveawayCard`, que es donde vive la clase: el contrato grepea solo el archivo de su fila) |
| `News (sin emojis)` | mustContain +: `<PageHeaderEditorial`, `<EditorialModal`, `DefaultImage`, `DefaultImageDomain.Novedad`, `novedad-default.svg`, `onerror`, `this.onerror=null`, `width=`, `height=`, `rail-card`; mustNotContain +: `dark:`, `text-pink-500`, `text-rose-600`, `hover:scale-105`, `fixed inset-0 z-50` |
| `GameDetail (ficha sin emojis)` | mustContain +: `flex-wrap`, `text-[var(--on-brand)]`; mustNotContain +: `Ludeca`, `text-white`, `text-slate-400`, `bg-amber-500`, `bg-indigo-500`, `bg-purple-950`, `bg-rose-500`, `text-orange-400` |
| `GiveawayCard (sin emojis)` | mustContain +: `rail-card`, `rail-cover`, `DefaultImage`, `DefaultImageDomain.Sorteo`, `sorteo-default.svg`, `onerror`, `this.onerror=null`, `width=`, `height=`, `text-[var(--on-brand)]`; mustNotContain +: `dark:`, `hover:scale-105`, `text-amber-500`, `text-pink-500`, `text-sky-500`, `text-rose-600` |
| `GameDetail (diseñador texto plano)` | se mantiene intacto (verificar que el rediseño de cabecera no rompe el span del diseñador) |

**Contratos INC-31/35 que DEBEN actualizarse en el mismo commit que el cambio que los toca** (recordatorio para `sdd-apply`): los de la tabla anterior, más el barrido `RestoDeLaWeb_SinEmojis…` y `PaginasEventos_TodaImagenDeEventoTieneFallbackPorDominio` (no cambian, se reprotegen en verde) y `HeroEditorialQuickSearchTests` (no cambia: el handler no se toca).

**app.css compilado** (en `PerformanceAndAccessibilityTests.cs`): nueva `[Fact]` `AppCss_FundacionInc36_Regenerada` — `wwwroot/app.css` contiene `--on-brand`, `aspect-ratio:16/9`, `hero-focal--eurogame`, `page-header-title` y NO contiene `min-height:360px` (minificado, sin espacio).

### DD-10 — Regeneración de `app.css` (pipeline vigente, leído de `dev.ps1:16` y `Dockerfile:14`)

```powershell
# desde src/Ludeka.Web (el workdir del pipeline; no hay package.json en el repo)
npx.cmd -y tailwindcss@3.4.17 -i ./Styles/input.css -o ./wwwroot/app.css --minify
```

Se ejecuta tras CADA cambio de `input.css` (PR-1; los PRs de páginas solo tocan `.razor` y no exigen regenerado, aunque regenerar al final purga utilidades muertas). `sdd-verify` debe verificar que `app.css` sirva `--on-brand` y `aspect-ratio` (fact DD-09) y que el `safelist` de `tailwind.config.js` (que mantiene patrones de color de 500-950 aunque las páginas los dejen de usar) no reimplante los hardcodes eliminados.

### DD-11 — Plan de PRs apilados (cortes concretos, mismo worktree `inc/rediseno-paginas-editoriales`)

| PR | Contenido | Estimación | Verificación (boundary) | Rollback |
|---|---|---|---|---|
| **PR-1 «Fundación»** | `input.css`: `--on-brand` ×5, 12 tokens de estado ×5, `.hero-editorial` (aspect/min/max), 4 `.hero-focal--*`, `.page-header-title` (agrupado), guarda reduced-motion de `animate-pulse`; `HeroEditorial.razor` (clases de altura + foco + Buscar accesible); `HeroBackgroundVariant.cs` (`FocalClass`); `PageHeaderEditorial.razor`; `EditorialModal.razor`; `wwwroot/js/editorial-modal.js`; `App.razor` (script); `app.css` regenerado; contratos (2 Fact + filas nuevas/ajustadas de fundación, hero, PageHeaderEditorial, EditorialModal, app.css) | ~380-450 líneas (app.css cuenta como 1-2 líneas de diff minificado) | `dotnet test` verde con los contratos de fundación; smoke de `/` y de los 5 temas con `?theme=` | Revert PR-1: tokens nuevos (aditivos) desaparecen; hero vuelve a `min-height` fijo; componentes nuevos desaparecen sin referencias (las páginas aún no los usan) |
| **PR-2 «Catálogo»** | `Home.razor`: PageHeaderEditorial + búsqueda debajo + `scrollbar-none` + contratos | ~90-120 | verde con contratos de Catálogo | revert aislable |
| **PR-3 «Ficha»** | `GameDetail.razor`: typo PageTitle, no-encontrado tokenizado, back-bar DD-08, hardcodes → tokens, botón IA accesible + contratos | ~120-160 | verde con contratos de ficha (+ QuickSearch si toca cabecera) | idem |
| **PR-4 «Eventos»** | `Events.razor`: PageHeaderEditorial, tabpanel/aria-controls, tarjetas `.rail-card` (imagen `rail-cover h-48`), badges de urgencia → tokens + `--on-brand` + contratos | ~140-180 | verde con contratos de Events | idem |
| **PR-5 «Sorteos + Novedades»** | `Radar.razor` + `News.razor`: cabeceras compartidas, adopción EditorialModal, tarjetas `.rail-card`, tokens; `GiveawayCard.razor`: `.rail-card` + `rail-cover h-44` + DefaultImage/onerror/width/height + tokens; contratos | ~250-350 (los shells se RESTAN) | verde con contratos de Radar/News/GiveawayCard + sweep completo | idem |

Reglas del encadenamiento: cada PR nace del estado del anterior (la rama se pushea con `scripts/sdd-worktree.ps1 pr`, sin flag adicional); si un PR excede 400 líneas se parte con el patrón de PRs encadenados (AGENTS.md 1-bis.7). Los fixes triviales (`no-scrollbar`, typo) van DENTRO de los PRs de su página, en las mismas líneas que el rediseño (una sola edición).

---

## Flujo de datos

```text
Styles/input.css (punto único de verdad de tokens)
        │  npx tailwindcss@3.4.17 --minify
        ▼
wwwroot/app.css (compilado servido) ──► <html data-theme="{tema}">
        │                                        │
        ▼                                        ▼
Componentes compartidos (PageHeaderEditorial, EditorialModal, DefaultImage, Icon)
        │
        ▼
5 páginas (Home, GameDetail, Events, Radar, News) + GiveawayCard ──► markup con tokens (sin hardcodes)
```

No hay datos nuevos: los parámetros `Open`/`OnClose` de los modales y los eventos de las páginas no cambian de forma.

## Cambios de archivos

| Archivo | Acción | Descripción |
|---|---|---|
| `src/Ludeka.Web/Styles/input.css` | Modificar | `--on-brand` ×5; 12 tokens de estado ×5; `.hero-editorial` (aspect-ratio/min/max); 4 `.hero-focal--*` con `object-position`; `.page-header-title` (agrupado con `.rail-title`); `.filter-btn.active` → `--on-brand`; guarda reduced-motion de `animate-pulse`; comentario del bloque rail-title actualizado |
| `src/Ludeka.Web/Components/Home/HeroEditorial.razor` | Modificar | Quitar `min-h-[360px] sm:min-h-[460px]`; clase `hero-focal--*` por variante; `text-white` → `text-[var(--on-brand)]` en Buscar; chip de pruebas con literal propio |
| `src/Ludeka.Web/Components/Home/HeroBackgroundVariant.cs` | Modificar | `FocalClass(variant)` (mapa variante → `hero-focal--{clave}`, vacío en `CssScene`) |
| `src/Ludeka.Web/Components/Shared/PageHeaderEditorial.razor` | Crear | Cabecera editorial compartida (badge + h1 serif + subtítulo + Actions) |
| `src/Ludeka.Web/Components/Shared/EditorialModal.razor` | Crear | Shell de modal (overlay/diálogo/tarjeta/cierre/aria/foco) |
| `src/Ludeka.Web/wwwroot/js/editorial-modal.js` | Crear | Foco al abrir, restaurar al cerrar, Escape |
| `src/Ludeka.Web/Components/App.razor` | Modificar | `<script src="/js/editorial-modal.js" defer>` |
| `src/Ludeka.Web/Components/Pages/Home.razor` | Modificar | Cabecera → PageHeaderEditorial; búsqueda debajo; `no-scrollbar` → `scrollbar-none` |
| `src/Ludeka.Web/Components/Pages/GameDetail.razor` | Modificar | Typo «Ludeka»; estados sin juego tokenizados; back-bar DD-08; hardcodes → tokens; botón de marca → `--on-brand` |
| `src/Ludeka.Web/Components/Pages/Events.razor` | Modificar | Cabecera compartida; tabpanel/aria-controls; tarjetas `.rail-card`; badges de urgencia → tokens; `--on-brand` |
| `src/Ludeka.Web/Components/Pages/Radar.razor` | Modificar | Cabecera; EditorialModal; filtros → tokens; `--on-brand` |
| `src/Ludeka.Web/Components/Pages/News.razor` | Modificar | Cabecera; EditorialModal; imagen siempre con DefaultImage + onerror + dimensiones; tokens; `.rail-card` |
| `src/Ludeka.Web/Components/Shared/GiveawayCard.razor` | Modificar | `.rail-card` + `rail-cover`; DefaultImage/onerror/dimensiones; tokens; `--on-brand` |
| `src/Ludeka.Web/wwwroot/app.css` | Regenerar | Pipeline DD-10 tras cada `input.css` |
| `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs` | Modificar | Contratos nuevos/ajustados DD-09 (mismo commit) |
| `tests/Ludeka.UnitTests/Web/PerformanceAndAccessibilityTests.cs` | Modificar | Fact de app.css regenerada (DD-09) |

## Estrategia de pruebas

| Capa | Qué se prueba | Cómo |
|---|---|---|
| Unitaria (contratos) | Tokens por tema, alturas del hero, focos, cabeceras compartidas, modales, fallbacks, ausencia de `dark:`/hardcodes/`text-white`-sobre-marca/`no-scrollbar`/«Ludeca» | `WebMarkupContractTests.cs` (grep Ordinal, sin bUnit) + 2 Fact de bloques CSS + fact de app.css |
| Unitaria (comportamiento) | `HandleQuickSearch` intacto tras el fix del hero | `HeroEditorialQuickSearchTests` (sin cambios) |
| Runtime (`sdd-verify`) | Los 5 `data-theme`: contraste del botón de marca y estados; hero en ~360/375/640/1240px (proporción, foco, cap, sin saltos); CLS 0 y LCP del hero; modales Radar/News (foco/cierre/Escape); pestañas de Events con lector; back-bar en móvil con moderador | `dotnet run` + inspección manual/DevTools; `dotnet test Ludeka.sln` completo (847 + nuevos) |

## Matriz de amenazas

N/A — sin cambios de routing, shell/consola, subprocessos, automatización VCS/PR (los PRs apilados usan `scripts/sdd-worktree.ps1` existente sin modificarlo), clasificación de ejecutables ni integración de procesos. Único archivo nuevo ejecutable-adyacente: `editorial-modal.js`, estático servido por wwwroot (sin lógica sensible: solo gestión de foco en el documento actual).

## Riesgos técnicos y mitigaciones

| Riesgo | P | Mitigación |
|---|---|---|
| Regresión de contratos INC-35 (hero, emojis, fallback de eventos, Fraunces en 1 petición) | Alta | TDD en el mismo commit; invariantes blindados; `HeroEditorialQuickSearchTests` intacto |
| `primer-plano` (vertical 2:3) queda como banda del 37% en móvil 16:9 | Media | Foco autoral al tablero (DD-02); validación visual del maintainer en `sdd-verify`; si no satisface, la solución es la segunda ola C (recortes autoriales), ya documentada |
| Ratios reales ≠ atributos `width/height` declarados (3:2/2:3 vs 1600×900) | Baja | Sin impacto runtime (posicionamiento CSS absoluto); documentado como deuda para la ola C; contratos INC-35 congelados |
| Cambio de identidad del botón primario (`--on-brand`) | Media | Ya validado visualmente el maintainer al aprobar la propuesta (D3, commit 0f13f41); verificación de contraste por cálculo + runtime en los 5 temas |
| Presupuesto de 400 líneas/PR | Alta | Cortes por PR de DD-11 + encadenado automático si un PR se pasa |
| Foco de modal vía JS (nuevo patrón de interop en modales) | Baja | Precedente real (`LocationSelectorModal`); degradación a markup puro si no hay JS; ningún handler de negocio depende del foco |
| Interpretación del barrido de estados (DD-05) demasiado estrecha vs letra de la spec | Baja | Decisión documentada con base (alcance de propuesta y forecast); el contrato nombra exactamente los 6 archivos cubiertos |
| `safelist` de Tailwind reimplanta colores eliminados | Baja | Regeneración final + fact de app.css; el `safelist` es necesario para clases dinámicas del semáforo (no se toca) |

## Migración / Rollout

No hay migración de datos ni flags: todo es presentación servida por SSR. Rollout por PRs apilados (DD-11); cada revert restaura el slice. Los tokens nuevos son aditivos (no rompen los 5 temas existentes si se revierten). Único efecto global real: `--on-brand` cambia el texto del botón primario en los temas de marca cálida — validado por el maintainer antes de aplicar.

## Preguntas abiertas

- [ ] Ninguna bloqueante. Notas no bloqueantes registradas: (a) `.skip-link:focus` y `MainLayout.razor:103` siguen fallando contraste sobre marca en 3 temas — barrido futuro fuera de D13; (b) `HomeGiveawayCard.razor:56` (portada) conserva `hover:text-white` — idem; (c) el alt de `primer-plano` describe manos que el foco 30% recorta parcialmente — alt congelado por INC-35, pendiente de la segunda ola C si se desea recomposición autoral.
