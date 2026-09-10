# Exploración: portada-editorial

> Fase SDD `sdd-explore` — incremento `portada-editorial` (INC-35 en roadmap). Worktree `C:\repos\ludeka-wt\portada-editorial`, rama `inc/portada-editorial` (9b7693c).
> Idioma: español castellano (regla suprema AGENTS.md; prevalece sobre el "default to English" de la skill).
> Store: hybrid — este documento + Engram `sdd/portada-editorial/explore`.

## Estado Actual

### Portada (HomeDashboard)
- `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` (`@page "/"`, `@rendermode InteractiveServer`, 387 líneas): hero (gradiente redondeado + `h1` sr-only + párrafo narrativo + buscador a `/catalogo?q=` + 4 píldoras) y 4 carriles con scroll horizontal (`overflow-x-auto snap-x snap-mandatory`): Top 20 (líneas 66-139), Sorteos (141-211), Novedades en Tiendas (213-275), Eventos/Ferias (277-349).
- `Home.razor` sigue siendo `/catalogo` (reusa `GameCard.razor` en línea 75); la portada es dashboard.
- El cambio previo INC-31 (`portada-minimalista-creadores`, archivado) ya eliminó badge/titular visible y corrigió el link de novedades (`/novedades`, línea 226-228). Las píldoras actuales son Catálogo/Sorteos/Novedades/Eventos.
- **Las tarjetas de los 4 carriles son markup inline duplicado** — NO reusan `GameCard.razor`, `GiveawayCard.razor` ni `StoreOffersCard.razor` (estos se usan en `/catalogo`+`PublisherDetail`, `/radar` y `GameDetail` respectivamente).
- Carga de datos: `IHomeDashboardService` → `HomeDashboardService` (`src/Ludeka.Application/Features/Home/HomeDashboardService.cs`) ya decorado con caché `CachedHomeDashboardService` (`src/Ludeka.Web/Program.cs:195-201`). Top 20 vía `ICatalogService.GetCatalogAsync` (ya cacheado), sorteos vía `IGiveawayService`, novedades vía `IWeeklyReleaseService`, eventos vía `IBoardGameEventRepository.GetUpcomingEventsAsync(20)`.
- El estado de carga muestra spinner con emoji 🎲 y bloquea TODO el dashboard (`_isLoading`, líneas 57-63): no hay streaming SSR ni esqueletos.

### Campo imagen en modelos/DTOs (punto exacto del fallback)
| Carril | DTO | Campo imagen | Se renderiza hoy | Fallback hoy |
|---|---|---|---|---|
| Top 20 | `GameSummaryDto` (`DTOs/GameSummaryDto.cs:16`) | `CoverImageUrl` (nullable) | Sí (`HomeDashboard.razor:96-103`) | `onerror` → `/images/game-placeholder.svg` / `expansion-placeholder.svg` ✅ |
| Sorteos | `GiveawayDto` (`DTOs/CommunityDtos.cs:20`) | `ThumbnailUrl` (nullable) | **NO** — la card del dashboard es 100% texto (líneas 170-207); `GiveawayCard.razor:7-18` sí lo renderiza en `/radar` pero sin fallback (si falta, desaparece la zona de imagen) | ❌ |
| Novedades | `WeeklyReleaseDto` (`DTOs/CommunityDtos.cs:50`) | `CoverImageUrl` (nullable) | **NO** — card de texto (líneas 242-271) | ❌ |
| Eventos | `BoardGameEventDto` (`DTOs/HomeDashboardDtos.cs:10`) | `ImageUrl` (**no nullable**; la entidad la exige: `CreateBoardGameEventRequest.ImageUrl` es `string`) | Sí (`HomeDashboard.razor:298-302`) | **NINGUNO** — si llega vacío, `<img>` roto; además sin `width`/`height` (riesgo CLS). Igual en `Events.razor:132-135` y `EventsManagement.razor:81` |
- Datos reales: `BoardGameEventSeeder.cs` siembra 6 eventos con URLs **externas de Unsplash** (p. ej. `https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800...`). Sorteos y novedades NO tienen seeder (se crean por servicio, `GiveawayService.cs:75`, `WeeklyReleaseService.cs:31`), así que el dashboard debe ser robusto con y sin imagen.
- Convención de subida de imágenes: `IImageStorageService`/`PhysicalFileImageStorageService` (Program.cs:118) guarda bajo `wwwroot/images/...` (placeholder del formulario: `https://... o /images/events/...`, `EventsManagement.razor:212`).

### Assets existentes (wwwroot)
- `wwwroot/images/`: `games/*` (40+ carátulas locales del mock BGG), `game-placeholder.svg`, `expansion-placeholder.svg`, `logo{,-light,-dark}.png`, `icons/icon-*`.
- **No existe ninguna imagen por defecto para eventos, sorteos ni novedades** — es el hueco exacto que pide el requisito 3.
- Patrón de placeholder vigente: SVG estático + `onerror="this.onerror=null; this.src='…'"` (repetido en GameCard, MyLibrary, PublicProfile, GameDetail, directorios).

### Sistema de estilos (Styles/input.css, 481 líneas)
- 5 temas por `data-theme` en `<html>` (App.razor:24-32, sin FOUC): `charcoal` (default, terracota `--brand-primary: #E05A38`), `editorial` (claro), `tabletop` (madera cálida — el más cercano al ambiente hogareño pedido), `midnight`, `wood`.
- Component classes ya útiles: `.game-card-editorial` (hover `translateY(-4px)` + border + shadow, líneas 356-370), `.cover-wrapper` (aspect-ratio 1/1 + scale 1.03 al hover, 372-388), `.badge-pill`, `.traffic-chip`, `.search-input`, `.filter-btn`, `.detail-hero-backdrop` (gradiente radial con `--brand-glow`).
- **Huecos para microinteracciones consistentes**: no hay tokens de transición compartidos (`--ease-*`, `--duration-*`), no hay `prefers-reduced-motion` en todo el CSS, y las variantes de hover de las cards del dashboard son utilidades ad-hoc (`hover:border-*`, `hover:scale-105`).
- **Clases muertas detectadas**: `.scrollbar-none` (usada en los 4 carriles) NO está definida en ningún CSS → las bandas de scroll horizontales se ven en escritorio. `sm:w-68` (línea 242) no existe en la escala Tailwind 3.4 → clase inválida ignorada.

### Tipografía
- Se carga vía Google Fonts en `App.razor:13` (stylesheet render-blocking ya presente) con preconnects (App.razor:10-12): `Plus+Jakarta+Sans:wght@400..800` + `JetBrains+Mono:wght@500;700`. No hay serif display.
- Añadir una serif display = ampliar la MISMA URL (sin request extra) + woff2 adicional (~15-40 KB por peso con subset latin, `display=swap`). El coste real es el peso de fuente, no latencia nueva.

### Iconografía
- Emojis inline en TODO el componente: ~18 en la portada (🔍🎲🎁📰🎪🏆⭐⏱️🚀🔄🆕🗓️📅📍🌐🧩). Lucide Icons NO está instalado por ninguna vía (ni paquete ni SVGs). AGENTS.md pide Lucide — la migración global de la app (41 componentes) sería enorme; el alcance realista es la portada + un componente `Icon.razor` como fundación.

### Especificación viva impactada
- `openspec/specs/home-landing-hero/spec.md` hoy exige hero minimalista SIN badge/titular visible y fija las 4 píldoras y el `h1` sr-only "Ludeka — Juegos de mesa en español". Este incremento **modificará esa spec** (delta con scenarios nuevos para narrativa, imagen de ambiente y microinteracciones), no creará una capability paralela.
- Roadmap: 30 incrementos archivados + INC-31/32/33/34 (archivados) → este cambio corresponde a **INC-35**. No hay worktrees activos en conflicto.

## Áreas Afectadas

- `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` — núcleo del cambio: hero con narrativa/ambiente, carriles con imágenes y microinteracciones, fallbacks de imagen.
- `src/Ludeka.Web/wwwroot/images/` — NUEVOS assets por defecto de Ludeka (eventos, sorteos, novedades) + hero (si se elige foto/ilustración). Convención propuesta: `wwwroot/images/defaults/{evento,sorteo,novedad}-default.svg` y `wwwroot/images/home/hero-*.{avif,webp,jpg}`.
- `src/Ludeka.Web/Styles/input.css` — tokens de transición/microinteracción, clases de carril (`.rail-track` con scrollbar oculta real), serif display (`--font-display`), soporte `prefers-reduced-motion`.
- `src/Ludeka.Web/Components/App.razor` — familia serif añadida a la URL de Google Fonts (misma petición).
- `src/Ludeka.Web/Components/Shared/Icon.razor` (NUEVO) — iconografía Lucide inline SVG con `currentColor`, fundación reutilizable.
- Componentes de carril (NUEVOS, recomendado): `HomeGameCard`/`HomeGiveawayCard`/`HomeReleaseCard`/`HomeEventCard` (o `Components/Home/`) para eliminar el markup duplicado y centralizar imagen+fallback+hover; `Events.razor:132` y `EventsManagement.razor:81` comparten el bug de imagen sin fallback (candidatos a reusar el mismo tratamiento).
- `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` (spinner de carga) — esqueletos de carril en lugar del spinner bloqueante (opcional, recomendado para narrativa).
- `openspec/specs/home-landing-hero/spec.md` — delta en `sdd-spec`.
- `docs/increments/ROADMAP.md` — registro INC-35.

## Enfoques

### Decisión 1 — Hero "hogareño" (mesa con tapete, eurogame, estantería al fondo)

1. **Foto real local** (requisito 4 tal cual): fotografía de mesa con tapete + eurogame + estantería, asset propio en `wwwroot/images/home/`, `<picture>` con AVIF/WebP/JPEG, `fetchpriority="high"`, `width`/`height`, scrim degradado hacia `var(--bg-main)` para fundir con cualquier tema y garantizar contraste 4.5:1 del texto.
   - Pros: máxima evocación hogareña; identidad propia (nada de púrpura meeplay); control total del peso (100-160 KB móvil).
   - Contras: LCP pasa a ser la imagen (mitigable: preload + dimensiones + formato moderno); hay que producir/licenciar la foto; debe verse bien en los 5 temas (scrim obligatorio).
   - Esfuerzo: Medio (diseño + asset).

2. **Escena CSS pura** (sin imagen): gradientes con `--brand-glow`, textura de madera sugerida con CSS, siluetas/tarjetas decorativas; sigue `.detail-hero-backdrop` como precedente.
   - Pros: 0 requests, 0 impacto LCP, 100% themable con las variables existentes; ya existe un patrón (`detail-hero-backdrop`).
   - Contras: menos literalmente "hogareño"; riesgo de caer en decorado genérico si no hay disciplina.
   - Esfuerzo: Medio (puro CSS).

3. **Ilustración editorial propia** (SVG/AVIF plana estilo editorial): mesa y estantería ilustradas, paleta carbón+terracota.
   - Pros: identidad única e inconfundible; peso bajo (20-60 KB); funciona en los 5 temas si se diseña con los dos modos.
   - Contras: requiere artista/tiempo de ilustración; el resultado puede envejecer peor que una foto.
   - Esfuerzo: Alto (asset artístico).

### Decisión 2 — Imágenes por defecto de Ludeka (eventos/sorteos/novedades)

1. **SVG estático por dominio** (`/images/defaults/*.svg`): composición plana carbón+terracota con iconografía (carrusel de feria, caja de sorteo, etiqueta "nuevo"), patrón vigente `game-placeholder.svg` + `onerror`.
   - Pros: consistente con lo existente; peso ínfimo; cacheable.
   - Contras: un SVG como `<img>` NO lee variables CSS → mismo look en temas claros (editorial/wood) y oscuros; se necesita paleta intermedia o dos variantes.
2. **Componente Blazor placeholder SVG inline** (recomendado complementar): un `DefaultImage.razor` que renderiza el SVG inline con `fill="var(--brand-glow)"` etc. → hereda el tema activo en los 5 data-themes; el `onerror` de fuentes externas sustituye por el SVG estático.
   - Pros: perfecta integración temática; un solo punto de lógica de fallback (dato+onerror+default).
   - Contras: markup extra por card (aceptable en SSR).
3. **Fotos ilustradas AVIF/WebP por dominio**: máxima "bonitez" fotográfica (feria, mesa de sorteo, caja nueva).
   - Pros: estética premium; reutilizable como hero secundario.
   - Contras: producción de 3 assets; peso mayor que SVG; misma fijación temática que la opción 1.
   - Esfuerzo: Medio.

### Decisión 3 — Microinteracciones hover en cards

1. **Tokens CSS + component classes en `input.css`** (recomendado): extender el sistema existente (`.game-card-editorial` ya define el estándar) con variables `--ease-out-expo`, `--dur-fast/base`, clase `.rail-card` (lift + border + glow con `--brand-glow` + zoom de imagen 1.03) y un bloque `@media (prefers-reduced-motion: reduce)` global.
   - Pros: coherente con lo ya construido; accesible (reduced-motion); themable; las cards del dashboard adoptan el MISMO lenguaje que `/catalogo`.
   - Contras: exige tocar input.css y sustituir utilidades ad-hoc.
2. **Solo utilidades Tailwind inline** (`hover:-translate-y-1 hover:shadow-xl transition-all`): rápido, sin CSS nuevo.
   - Pros: velocidad de implementación.
   - Contras: perpetúa la inconsistencia actual, no cubre reduced-motion, sin tokens.
3. **Librería de animación JS**: fuera de contexto (Blazor SSR + server-side; coste de hidratación innecesario).

### Decisión 4 — Emojis → Lucide

1. **`Icon.razor` con SVGs inline de Lucide** (recomendado): componente con switch de paths (search, gift, newspaper, tent/ticket, trophy, calendar, map-pin, clock, globe, puzzle, sparkles, rocket, refresh, star), `stroke="currentColor"`, `aria-hidden="true"` + texto accesible donde el emoji era semántico.
   - Pros: cero dependencias; hereda color de tema; árbol de render ligero; fundación para futuras migraciones; cumple AGENTS.md.
   - Contras: mantener el catálogo de paths a mano.
2. **SVGs estáticos en wwwroot + `<img src>`**: pierde `currentColor` (no themable) — descartable.
3. **Paquete NuGet de iconos**: dependencia extra por algo que son paths SVG; contradice la ligereza buscada.

## Recomendación

- **Hero: opción 1 (foto real local)** con scrim dirigido por variables de tema y fallback interno a la escena CSS (opción 2) si la foto no está lista al aplicar. La narrativa pedida ("hoy es dashboard utilitario" → portada con historia) se resuelve con: párrafo editorial reescrito como invitación a la mesa + imagen ambiente + serif display.
- **Imágenes por defecto: opción 2** (componente `DefaultImage.razor` SVG inline temático) **+ variantes estáticas SVG** en `/images/defaults/` para el caso `onerror` de URLs externas caídas. Requisito 3 cubierto en los 3 carriles: Sorteos estrena render de `ThumbnailUrl` con fallback, Novedades estrena render de `CoverImageUrl` con fallback, Eventos obtiene el fallback que hoy no tiene.
- **Microinteracciones: opción 1** (tokens + `.rail-card` + reduced-motion). Además, arreglar de paso las clases muertas (`scrollbar-none` real, `sm:w-68`).
- **Lucide: opción 1** (`Icon.razor`), migrando SOLO los ~18 emojis de la portada; el resto de la app queda para incrementos futuros (evita un PR de 400+ líneas).
- **Tipografía: añadir una serif display** (propuesta: Fraunces variable `opsz,wght@9..144,600;700` — editorial, cálida, con carácter; alternativas: Playfair Display, Newsreader) a la URL existente de Google Fonts y `--font-display` en input.css aplicado al hero y, opcionalmente, a los títulos de sección de la portada (no global).

## Riesgos

- **[Alta] Regresión sobre spec viva `home-landing-hero`**: el spec actual PROHÍBE badge y titular visible y congela píldoras/h1. El delta de `sdd-spec` debe modificar explícitamente esos requirements o `sdd-verify` chocará con la spec vigente. No empezar a aplicar sin el delta aprobado.
- **[Alta] LCP si el hero lleva foto**: el hero hoy no tiene imagen (LCP probablemente textual). Con foto, la imagen se vuelve LCP: obligatorio `fetchpriority="high"`, `preload`/`<picture>` AVIF+WebP, dimensiones fijas (CLS 0, patrón inc-07) y scrim con contraste ≥ 4.5:1 sobre texto blanco (WCAG 1.4.3). Presupuesto: < 200 KB, viewport-only.
- **[Media] Scope creep de emojis→Lucide**: la app completa tiene cientos de emojis; migrar todo rompería el presupuesto de 400 líneas del PR. Acotar a la portada y dejar `Icon.razor` como fundación documentada.
- **[Media] Accesibilidad de las microinteracciones**: todo hover debe tener equivalente `:focus-visible` (el foco de teclado ya hoy no está estilizado en las cards inline), `@media (prefers-reduced-motion: reduce)` para lift/scale, y targets ≥ 24×24 en píldoras/botones (WCAG 2.5.8). Los emojis semánticos convertidos a icono decorativo necesitan `aria-hidden` + texto visible.
- **[Media] Imagen rota de eventos sin fallback (bug vigente)**: `HomeDashboard.razor:298`, `Events.razor:132` y `EventsManagement.razor:81` pintan `evt.ImageUrl` (no nullable en DTO) sin `onerror`. Si el fix se hace solo en la portada, las otras páginas quedan inconsistentes — decidir si el fix de componentes compartidos entra en este incremento o en uno propio (recomendado: el componente `DefaultImage.razor` reutilizable entra aquí; la migración de Events/*, evaluada por líneas).
- **[Baja] Dependencia hotlink de Unsplash**: eventos sembrados apuntan a `images.unsplash.com` y NO hay preconnect a ese origen (App.razor:10-12 solo preconecta fonts y cf.geekdo-images.com). Opción barata: añadir preconnect; opción de identidad: reemplazar por defaults de Ludeka (fuera de alcance probable).
- **[Baja] Clases CSS muertas**: `.scrollbar-none` sin definir y `sm:w-68` inválida (Tailwind 3.4 no tiene 68) → corrección incluida en el cambio (bajo riesgo, líneas mínimas).
- **[Baja] `@rendermode InteractiveServer` en la portada**: el spinner de carga bloqueante degrada la percepción de velocidad; mover a streaming/skeletons es deseable pero puede quedar fuera si dispara el tamaño del PR.

## Ready for Proposal

**Sí.** Siguiente fase: `sdd-propose` sobre el cambio `portada-editorial` (INC-35), incorporando estas decisiones:

- **D1 (usuario):** Hero — ¿foto real local (recomendada) o escena CSS si no hay foto licenciada?
- **D2 (usuario):** ¿Se acepta estrenar el render de imágenes en Sorteos/Novedades (las cards dejan de ser 100% texto)? Recomendado: sí, con fallback por defecto de Ludeka.
- **D3 (usuario):** Alcance Lucide — solo portada (recomendado) vs. portada + carriles de /sorteos /novedades /eventos.
- **D4 (usuario):** Serif display concreta (propuesta Fraunces) y si se aplica solo al hero o también a títulos de sección.
- **D5 (técnica, proponer en sdd-design):** componentes nuevos de carril vs. refactor inline; fix de `Events.razor`/`EventsManagement.razor` dentro o fuera del incremento.

## Key Learnings

1. La portada no reusa los componentes Shared: sus 4 carriles son markup inline duplicado con reglas de hover ad-hoc.
2. Sorteos y Novedades tienen campos imagen en sus DTOs que el dashboard nunca renderiza; Eventos renderiza sin fallback y sin width/height.
3. No existen assets por defecto para eventos, sorteos ni novedades en wwwroot/images.
4. `.scrollbar-none` y `sm:w-68` son clases muertas; no hay soporte `prefers-reduced-motion` en todo el CSS.
5. La spec viva `home-landing-hero` prohíbe badge y titular visible: este incremento exige delta de spec antes de aplicar.
