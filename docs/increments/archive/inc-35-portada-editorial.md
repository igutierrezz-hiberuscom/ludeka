# Incremento 35: Portada Editorial — Narrativa Hogareña, Microinteracciones e Imágenes por Defecto

- **Identificador SDD:** `portada-editorial`
- **Estado:** ✅ **Completado y Archivado** (suite **847/847** en verde al 100%; +142 sobre el baseline nominal 705, +108 sobre el baseline real 739 de INC-34; build con 0 errores)
- **Entrega:** cadena de **4 PRs apilados** (`inc/portada-editorial` → `-2` → `-3` → `-4`): PR-1 (GitHub #2), PR-2 (GitHub #3), PR-3a (GitHub #4) y PR-3b (rama `-4`, iconografía Lucide del resto de la web, abierto por el orquestador tras el archivo)
- **Verificación SDD:** **PASS WITH WARNINGS** — 19/19 requerimientos, 30/30 escenarios COMPLIANT, 0 blockers, 0 críticos. WARNING-1 (equivalencia de foco en cards `<div>`) **corregido en el commit 44e5ae4** (`:has(:focus-visible)` en los 4 grupos de selectores); WARNING-2 (medición real de LCP/CLS) queda como checklist del maintainer.
- **Artefactos SDD archivados:** [`openspec/changes/archive/2026-09-10-portada-editorial/`](file:///c:/repos/Ludeka/openspec/changes/archive/2026-09-10-portada-editorial/proposal.md) (proposal, deltas de specs, design, tasks 31/31, verify-report)
- **Módulo de la Especificación Viva:** [`23-portada-editorial.md`](file:///c:/repos/Ludeka/docs/specs/sistema/23-portada-editorial.md)

---

## 1. Alcance Funcional y Técnico

1. **Hero editorial con narrativa (D1):** la portada deja el hero minimalista de INC-31 y renderiza un hero con titular visible «La mesa está servida» en serif display Fraunces (`--font-display`), párrafo reescrito como invitación a la mesa, foto de ambiente hogareño y buscador + 4 píldoras de acceso intactas (D4). El badge decorativo «✨ PORTADA EDITORIAL» sigue prohibido y el documento conserva exactamente un `<h1>`, ahora visible. Delta `home-landing-hero` aprobado por el usuario (RENAMED + MODIFIED + ADDED).
2. **Variantes de fondo intercambiables:** `HeroBackgroundVariant` con 5 valores (3 fotos reales de ambiente `hero-ambiente-*.{avif,webp,jpg}` < 200 KB, escena CSS de serie y variante para ilustración futura). Conmutación in situ cambiando 1 línea (parámetro `Background`, default `FotoEurogame`), sin tocar el markup. `<picture>` AVIF/WebP/JPEG con `fetchpriority="high"`, `width`/`height` fijos y `alt` descriptivo en castellano; scrim `.hero-scrim` por variables de tema; la escena CSS queda siempre bajo la foto como fallback.
3. **Carriles componentizados (D2/D3-estructura):** el markup de tarjeta duplicado de `HomeDashboard.razor` se extrae a componentes dedicados en `Components/Home/` (`HomeGameCard`, `HomeGiveawayCard`, `HomeReleaseCard`, `HomeEventCard` + `RailHeader`/`HeroEditorial`); el orquestador queda limpio y cada carril encapsula imagen, fallback, microinteracciones y contenido.
4. **Imágenes por defecto por dominio (D2/D5):** assets de Ludeka en `/images/defaults/` (`evento-default.svg`, `sorteo-default.svg`, `novedad-default.svg`, `generico-default.svg`), componente `DefaultImage.razor` (SVG inline temable con `var(--brand-*)`/`var(--bg-*)`) y `onerror` con variante estática (`this.onerror=null`). Estrena render de imagen en Sorteos y Novedades, fallback para Eventos y adopción en `Events.razor` y `EventsManagement.razor`.
5. **Microinteracciones con lenguaje común:** tokens de transición (`--ease-*`, `--dur-*`, `--rail-*`), clase `.rail-card` (lift + glow `--brand-glow` + zoom de imagen 1,03), equivalencia visible de `:focus-visible` (incluido `.rail-card:has(:focus-visible)` para cards `<div>`) y respeto a `prefers-reduced-motion: reduce`.
6. **Iconografía Lucide global (D3):** componente `Icon.razor` (SVG inline, `currentColor`, `aria-hidden`) con catálogo whitelist `IconCatalog.cs` (94 iconos); migración de ~18 emojis de la portada y, como slice propio encadenado (PR-3b), el resto de la web; regla que prohíbe emojis futuros como iconografía, con contratos `mustNotContain` que lo blindan.
7. **Corrección de clases muertas:** `.scrollbar-none` definida realmente en CSS y eliminación de la clase inválida `sm:w-68` (Tailwind 3.4).

**Fuera de alcance:** rediseño editorial del resto de páginas (**INC-36**, pendiente en el roadmap); datos/servicios del dashboard (`IHomeDashboardService`, caché, DTOs y seeders sin cambios).

---

## 2. Verificación de Calidad

- **Suite de Pruebas Automatizadas:** 847/847 superadas (`dotnet test Ludeka.sln --no-build`), estable en 3 ejecuciones del ciclo; build 0 errores (4 advertencias preexistentes).
- **Verificación runtime (`dotnet run`, puerto 5081):** portada, `/eventos` y los 4 SVG de `/images/defaults/` responden 200; fallback REAL validado sembrando temporalmente un sorteo y una novedad sin imagen (ediciones marcadas `TEMP-VERIFY`), con reversión completa del seed y de la BD tras la verificación (`git status` limpio).
- **Invariante de dominio descubierto:** la entidad `BoardGameEvent` exige imagen en su constructor («La imagen o cartel del evento es obligatoria»), por lo que la rama inline de `DefaultImage` en cards de evento es defensa en profundidad; el fallback operativo de evento es el `onerror` ante URL externa caída. Documentado en el módulo 23 del sistema.
- **Excepciones iconográficas documentadas:** `CountryCatalog.GetFlag`/`FlagEmoji` y `Stats.Badge.IconEmoji` (capa Application, data-driven, fuera de `Components`) y la estrella tipográfica ★ del rating.
- **Especificación Viva:** creado `docs/specs/sistema/23-portada-editorial.md`; specs vivas actualizadas en `openspec/specs/` (`home-landing-hero` modificada; `home-dashboard-rails`, `default-image-fallbacks` e `iconography-lucide` nuevas).

---

## 3. Checklist de Revisión del Maintainer (antes de mergear PR-3b)

- [ ] **Scrim del hero:** revisión visual del contraste del texto sobre la foto en los 5 `data-theme` (charcoal, editorial, tabletop, midnight, wood) — validación humana pendiente de la verificación.
- [ ] **LCP/CLS reales:** medir portada en móvil con Lighthouse/DevTools (objetivo LCP < 2,5 s y CLS = 0) — WARNING-2 de la verificación.
- [ ] **5 variantes del hero:** probar `FotoEurogame`, `FotoMesaAmigos`, `FotoPrimerPlano`, `CssScene` e `Ilustracion` cambiando 1 línea en `HomeDashboard.razor`.
- [ ] **URLs BGG sintéticas del seed (SUGGESTION-2):** las imágenes `cf.geekdo-images.com/pic7123901.jpg` del sembrado devuelven 404 y activan `onerror` → defaults. Comportamiento correcto por diseño, pero en producción conviene apuntar a imágenes reales o a los defaults directamente.
- [ ] **Smoke visual del `onerror` (SUGGESTION-3):** la verificación del atributo es de wiring SSR; ejecutar 1 visita con DevTools bloqueando la carga de imágenes externas para ver caer al default.
