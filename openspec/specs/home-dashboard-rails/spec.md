# home-dashboard-rails Specification

> Especificación NUEVA (capability sin spec previa). Idioma: español castellano.
> Cobertura: los 4 carriles de la portada (Top 20, Sorteos, Novedades, Eventos): componentes dedicados, render de imagen con fallback por dominio, microinteracciones, scroll y accesibilidad WCAG 2.2 AA.

## Propósito

Definir el comportamiento de las bandas horizontales (carriles) del dashboard de la portada: extracción del markup duplicado a componentes dedicados, render de imagen con fallback por dominio, microinteracciones hover con equivalencia de foco, scroll horizontal sin scrollbar visible, serif display en títulos de sección y ausencia de saltos de layout.

## Requirements

### Requirement: Carriles renderizados mediante componentes dedicados

Los 4 carriles (Top 20, Sorteos, Novedades, Eventos) **DEBEN** renderizar sus tarjetas mediante componentes dedicados en `Components/Home/` (uno por dominio) que encapsulen imagen con fallback, microinteracciones y contenido. `HomeDashboard.razor` **NO DEBE** contener el markup de tarjeta duplicado inline por ítem.

#### Scenario: Cada carril usa su componente

- GIVEN la portada `/` renderizada con datos en los 4 carriles
- WHEN se inspecciona el markup de cada sección
- THEN las tarjetas de cada carril provienen de su componente dedicado en `Components/Home/`
- AND el markup de tarjeta ya no está repetido inline en `HomeDashboard.razor`

#### Scenario: Paridad de comportamiento tras la extracción

- GIVEN los datos actuales del dashboard (títulos, enlaces, metadatos y badges)
- WHEN se compara la portada antes y después de la extracción
- THEN cada tarjeta conserva su destino (ficha, sorteo, novedad o evento), su `aria-label` y su contenido informativo

### Requirement: Render de imagen con fallback por dominio (D2)

Los carriles Sorteos, Novedades y Eventos **DEBEN** renderizar la imagen de su DTO (`GiveawayDto.ThumbnailUrl`, `WeeklyReleaseDto.CoverImageUrl`, `BoardGameEventDto.ImageUrl` respectivamente) con fallback al default de Ludeka de su dominio (capability `default-image-fallbacks`) cuando falta o falla. Top 20 **DEBE** conservar su fallback actual (`game-placeholder.svg` / `expansion-placeholder.svg`). Toda imagen de carril **DEBE** tener `width` y `height` o contenedor de aspecto fijo, con `loading="lazy"` y `decoding="async"`.

#### Scenario: Card de sorteo con y sin imagen

- GIVEN un sorteo con `ThumbnailUrl` y otro sin ella en el carril Sorteos
- WHEN el carril se renderiza
- THEN el primero muestra su imagen y el segundo muestra el default de Ludeka para sorteos
- AND ningún carril renderiza una zona de imagen vacía ni un `<img>` roto

#### Scenario: Novedad y evento con imagen ausente o caída

- GIVEN una novedad con `CoverImageUrl` nula y un evento con `ImageUrl` vacío o con URL externa que falla en cliente
- WHEN sus carriles se renderizan, o la imagen produce error en el navegador
- THEN ambos muestran el default de su dominio (inline o vía `onerror`)
- AND el evento con `ImageUrl` vacío NO produce un `<img>` sin fuente

### Requirement: Microinteracciones hover con equivalencia de foco

Las tarjetas de los carriles **DEBEN** usar un lenguaje común (clase `.rail-card`): elevación, borde o glow con `--brand-glow` y zoom de imagen 1,03, definidos con tokens de transición compartidos. Todo efecto hover **DEBE** tener equivalente visible en `:focus-visible`, y **DEBE** respetarse `prefers-reduced-motion: reduce`.

#### Scenario: Hover en tarjeta de carril

- GIVEN una tarjeta de cualquier carril
- WHEN el puntero pasa sobre ella
- THEN la tarjeta aplica elevación, glow y zoom de imagen con las transiciones de los tokens compartidos

#### Scenario: Foco de teclado equivalente

- GIVEN el usuario navega por teclado (Tab) hasta una tarjeta
- WHEN la tarjeta recibe `:focus-visible`
- THEN recibe la misma evidencia visual que en hover más un contorno de foco de al menos 2 px
- AND ningún elemento interactivo del carril mide menos de 24×24 px (WCAG 2.5.8)

#### Scenario: Movimiento reducido

- GIVEN el sistema del usuario con `prefers-reduced-motion: reduce`
- WHEN se hace hover o foco sobre una tarjeta
- THEN no se ejecutan lift, zoom ni transiciones
- AND la evidencia visual queda en estado estático (borde o glow)

### Requirement: Scroll horizontal sin scrollbar visible

Las bandas de los carriles **DEBEN** mantener el scroll horizontal con snap y **NO DEBEN** mostrar scrollbar en escritorio. La clase `.scrollbar-none` **DEBE** estar definida realmente en el CSS (hoy es una clase muerta).

#### Scenario: Scroll funcional y scrollbar oculta

- GIVEN un carril con más tarjetas de las que caben en el viewport
- WHEN el usuario hace scroll horizontal (rueda, táctil o teclado)
- THEN el contenido desplaza con snap y la scrollbar no es visible

#### Scenario: Anchura definida en tarjetas de Novedades

- GIVEN un viewport `sm` y el carril Novedades renderizado
- WHEN se inspecciona la anchura efectiva de sus tarjetas
- THEN las tarjetas tienen anchura definida por clases válidas de Tailwind 3.4 (la clase inválida `sm:w-68` está eliminada)

### Requirement: Serif display en títulos de sección (D4)

Los títulos `<h2>` de los 4 carriles **DEBEN** usar la familia display `--font-display` (Fraunces variable cargada en la URL existente de Google Fonts, sin petición adicional). El resto de la web **NO DEBE** verse afectada por esta familia.

#### Scenario: Títulos de carril en serif display

- GIVEN la portada `/` renderizada
- WHEN se inspeccionan los títulos de sección de los 4 carriles
- THEN los 4 `<h2>` aplican `--font-display`
- AND la carga de fuentes se resuelve en la misma petición de Google Fonts ya existente
