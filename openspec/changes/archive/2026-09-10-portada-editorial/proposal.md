# Propuesta: Portada Editorial — INC-35

> Fase SDD `sdd-propose`. Idioma: español castellano (regla suprema AGENTS.md). Store: hybrid (este archivo + Engram `sdd/portada-editorial/proposal`).

## Intención

La portada `/` es un dashboard utilitario: hero sin imagen, 4 carriles con markup inline duplicado, cards de Sorteos/Novedades 100% texto, imagen de Eventos sin fallback y ~18 emojis como iconografía. El usuario pide una portada **con narrativa y ambiente hogareño** (mesa con tapete, eurogame, estantería ordenada al fondo: cálida pero limpia y legible), **identidad propia carbón+terracota** (sin el púrpura de meeplay.io), **microinteracciones al pasar el ratón por las cards** e **imágenes para Eventos/Novedades/Sorteos con imágenes por defecto bonitas de Ludeka** cuando falte.

## Alcance

### Dentro del alcance

- Hero editorial con narrativa: titular con serif display, párrafo reescrito como invitación a la mesa e imagen de ambiente hogareño (D1).
- Imágenes por defecto de Ludeka por dominio (evento, sorteo, novedad) en `wwwroot/images/defaults/` + componente `DefaultImage.razor` (SVG temático con variables CSS) y fallback `onerror`.
- Estrenar render de imagen en los carriles Sorteos (`GiveawayDto.ThumbnailUrl`) y Novedades (`WeeklyReleaseDto.CoverImageUrl`); fallback para Eventos (`BoardGameEventDto.ImageUrl`), hoy sin él ni `width`/`height` (D2).
- Microinteracciones: tokens de transición (`--ease-*`, `--dur-*`), clase `.rail-card` (lift + glow `--brand-glow` + zoom de imagen 1.03), `prefers-reduced-motion` y equivalente `:focus-visible`.
- `Icon.razor` (Lucide inline SVG, `currentColor`, `aria-hidden`) migrando los ~18 emojis de la portada (D3) **y, como slice propio encadenado, el resto de emojis de la web** (decisión D3 cerrada: migración global).
- Serif display (propuesta: Fraunces variable) en la URL existente de Google Fonts + `--font-display` en `input.css` (D4).
- Corrección de clases muertas: `.scrollbar-none` (sin definir) y `sm:w-68` (inválida en Tailwind 3.4).
- Delta de spec `home-landing-hero` como prerrequisito de apply.

### Fuera del alcance

- Rediseño editorial del resto de páginas (catálogo, fichas, eventos…): registrado como **INC-36** (pendiente) en `docs/increments/ROADMAP.md`.
- Datos y servicios: `IHomeDashboardService`, caché, DTOs y seeders no cambian (se consumen campos imagen ya existentes).
- Reemplazo de las URLs de Unsplash de eventos sembrados (solo se evalúa añadir preconnect).
- Streaming SSR / esqueletos globales: opcional, solo si no dispara el presupuesto de líneas.

> **Nota sobre migración de iconos:** la decisión D3 del usuario amplía la migración emojis→Lucide a **toda la web** (no solo la portada). Ver "Puntos de decisión — CERRADOS".

## Capacidades (contrato con sdd-spec)

### Nuevas capacidades

- `home-dashboard-rails`: comportamiento de los 4 carriles de la portada (Top 20, Sorteos, Novedades, Eventos): render de imagen con fallback por dominio, microinteracciones hover/foco y scroll de bandas sin scrollbar visible.
- `default-image-fallbacks`: assets por defecto de Ludeka (evento/sorteo/novedad), componente `DefaultImage.razor` y comportamiento cuando falta o falla la imagen (reutilizable fuera de la portada según D5).

### Capacidades modificadas

- `home-landing-hero`: el hero pasa de minimalista sin titular a editorial con narrativa visible, ambiente hogareño y serif display; se conservan buscador, las 4 píldoras exactas y el h1 único accesible (el delta decidirá si el h1 pasa a visible). **Requisito previo: delta aprobado antes de sdd-apply.**

## Enfoque

Extender el sistema existente, no reemplazarlo: `.game-card-editorial`/`.cover-wrapper` ya definen el lenguaje de hover; se generaliza a tokens + `.rail-card`. Los 4 carriles se extraen a componentes (`Components/Home/*Card.razor`) para eliminar el markup duplicado y centralizar imagen+fallback+hover (detalle en sdd-design). Hero con foto local `<picture>` AVIF/WebP/JPEG, `fetchpriority="high"`, dimensiones fijas y scrim por variables de tema; escena CSS como fallback si la foto no está lista.

## Áreas afectadas

| Área | Impacto |
|---|---|
| `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` | Modificado — hero narrativo, carriles por componentes, carga |
| `src/Ludeka.Web/Components/Home/*Card.razor` | Nuevo — 4 cards de carril |
| `src/Ludeka.Web/Components/Shared/Icon.razor`, `DefaultImage.razor` | Nuevo |
| `src/Ludeka.Web/Styles/input.css` | Modificado — tokens, `.rail-card`, serif, reduced-motion, fixes |
| `src/Ludeka.Web/Components/App.razor` | Modificado — serif en Google Fonts (+ preconnect según D5) |
| `src/Ludeka.Web/wwwroot/images/defaults/*`, `home/*` | Nuevo — assets |
| `Pages/Events.razor`, `Pages/EventsManagement.razor` | Modificado solo si D5 = sí |
| `openspec/specs/home-landing-hero/spec.md` | Delta en sdd-spec |

## Puntos de decisión — CERRADOS (usuario, 2026-09-09)

| # | Decisión | Resolución del usuario |
|---|---|---|
| D1 | Hero: foto real vs escena CSS vs ilustración | **Variantes intercambiables in situ** (probar las 3): el orquestador prepara 2 fotos reales de ambiente; la escena CSS va de serie; la ilustración la generará el usuario con nano banana (prompt suministrado). El hero debe permitir alternar variante fácilmente. |
| D2 | ¿Estrenar render de imagen en Sorteos/Novedades? | **Sí, los 3 carriles** con default de Ludeka. |
| D3 | Alcance Lucide | **TODA la web** (no solo la portada). Al usuario no le importan los PRs grandes; la estrategia auto-chain encadena slices. La migración global queda DENTRO de este incremento como slice propio. |
| D4 | Serif concreta y alcance | **Fraunces variable; hero + títulos de sección** de la portada (no global). |
| D5 | ¿Fix de img sin fallback en `Events.razor:132`/`EventsManagement.razor:81`? | **Dentro del incremento** (~30-50 líneas, reusa `DefaultImage.razor`). |
| — | Delta de `home-landing-hero` (titular visible) | **Aprobado**. Contexto del usuario: la prohibición no fue una regla de producto; al retirar la portada anterior no le gustó cómo quedaba el titular. Si el nuevo titular queda mejor, se reemplaza sin reparo. |

## Forecast de tamaño (estrategia: auto-chain)

- Estimación: ~950-1.250 líneas cambiadas (código + CSS + assets SVG autorados + migración global de iconos), tras cerrar D3 = migración global.
- Excede el presupuesto de 400 → **PRs encadenados** desde el mismo worktree:
  1. PR-1 "Fundación editorial": tokens CSS, `Icon.razor`, `DefaultImage.razor`, assets por defecto, serif, fixes de clases muertas (~300 líneas).
  2. PR-2 "Portada narrativa": hero editorial con variantes de fondo (foto/escena CSS/ilustración), componentes de carril con imágenes/microinteracciones y delta de spec (~350 líneas).
  3. PR-3 "Iconografía Lucide global": migración del resto de emojis de la web a `Icon.razor` (~300-600 líneas según componentes).
- Las guardas formales (`Decision needed before apply`, `400-line budget risk: High`) las fija `sdd-tasks`.

## Riesgos

| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Delta de `home-landing-hero` (hoy prohíbe titular visible) no aprobado antes de aplicar | Alta | sdd-spec antes de sdd-apply; verificar contra el delta |
| LCP del hero si lleva foto | Alta | `<picture>` AVIF/WebP, preload, `fetchpriority="high"`, <200 KB, dimensiones fijas (CLS 0) |
| Accesibilidad de microinteracciones (WCAG 2.2 AA) | Media | `:focus-visible` en todo hover, `prefers-reduced-motion`, targets ≥24px, `aria-hidden` en iconos decorativos |
| Scope creep (Lucide global, esqueletos, ilustración del hero) | Media | Alcance acotado por D1/D3; fuera de alcance explícito |
| SVG por defecto no themable en temas claros | Baja | `DefaultImage.razor` inline con variables CSS + variante estática para `onerror` |

## Plan de rollback

Revertir el PR del slice afectado restaura la portada anterior; los slices son independientes (PR-1 fundación aditiva, PR-2 portada). Sin migraciones de datos ni cambios de contrato de servicios. Los assets nuevos son aditivos y el hero conserva fallback interno a escena CSS.

## Dependencias

- Decisiones D1–D5 cerradas antes de sdd-spec.
- Delta de `home-landing-hero` aprobado antes de sdd-apply (prerrequisito duro).
- Foto de ambiente licenciada/producida si D1 = foto; si no está lista, escena CSS.

## Criterios de éxito

- [ ] Portada con narrativa y ambiente hogareño en identidad carbón+terracota (sin púrpura).
- [ ] Los 3 carriles con imagen muestran la imagen por defecto de Ludeka cuando falta o falla.
- [ ] Microinteracciones hover en cards con equivalente de foco y `prefers-reduced-motion`.
- [ ] 0 emojis en la portada (Lucide vía `Icon.razor`).
- [ ] LCP de portada < 2.5 s con hero con foto y CLS 0.
- [ ] `dotnet test Ludeka.sln` en verde y verificación contra el delta de spec aprobado.
