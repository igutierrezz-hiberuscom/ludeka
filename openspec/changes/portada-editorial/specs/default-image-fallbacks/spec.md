# default-image-fallbacks Specification

> Especificación NUEVA. Idioma: español castellano.
> Cobertura: assets por defecto de Ludeka por dominio (evento, sorteo, novedad), componente `DefaultImage.razor` (SVG inline temable + variante estática para `onerror`) y comportamiento ante imagen ausente o rota, reutilizable fuera de la portada (D5: fix en `Events.razor` y `EventsManagement.razor`).

## Propósito

Garantizar que ninguna tarjeta de Ludeka muestre una imagen rota o una zona vacía: ante la ausencia o el fallo de la imagen del dato, el sistema muestra el asset por defecto del dominio con la identidad carbón+terracota, temable en los 5 temas y reutilizable en cualquier página.

## Requirements

### Requirement: Assets por defecto de Ludeka por dominio

**DEBEN** existir assets estáticos por defecto para los dominios evento, sorteo y novedad bajo `wwwroot/images/defaults/`: composiciones SVG planas carbón+terracota con iconografía propia (carrusel de feria para eventos, caja de sorteo para sorteos, etiqueta de novedad para novedades). El placeholder de juego (`game-placeholder.svg`) **NO DEBE** reutilizarse para estos dominios.

#### Scenario: Assets servidos

- GIVEN un despliegue con los assets nuevos
- WHEN se solicitan `/images/defaults/evento-default.svg`, `/images/defaults/sorteo-default.svg` y `/images/defaults/novedad-default.svg`
- THEN los tres responden 200 con un SVG de Ludeka distinto por dominio

### Requirement: Componente DefaultImage.razor (SVG inline temable)

**DEBE** existir un componente `DefaultImage.razor` que renderice el SVG **inline** con variables CSS del tema (`var(--brand-*)`, `var(--bg-*)`) para heredar los 5 temas (`data-theme`), con variante por dominio (evento, sorteo, novedad) y modo decorativo (`aria-hidden="true"`) cuando acompaña texto. Para el caso `onerror` (donde no hay Blazor), **DEBE** existir una variante estática equivalente servida desde `/images/defaults/`.

#### Scenario: Inline temable en los 5 temas

- GIVEN la página con cualquiera de los 5 `data-theme`
- WHEN se renderiza `DefaultImage.razor`
- THEN el SVG inline usa variables CSS y su paleta se adapta al tema activo

#### Scenario: Dominio sin variante soportada

- GIVEN un uso de `DefaultImage.razor` con un dominio sin variante
- WHEN el componente se renderiza
- THEN muestra una variante genérica de Ludeka sin fallar y sin quedar en blanco

### Requirement: Comportamiento ante imagen ausente

Cuando el campo imagen del DTO es nulo o vacío, la tarjeta **DEBE** renderizar `DefaultImage.razor` del dominio correspondiente en el mismo contenedor que usaría la imagen real (mismo aspecto fijo), sin `<img>` roto.

#### Scenario: Sorteo sin imagen

- GIVEN un sorteo sin `ThumbnailUrl`
- WHEN su tarjeta se renderiza
- THEN el contenedor de imagen muestra el default inline de sorteos con el mismo aspecto que la imagen real
- AND el documento no contiene un `<img>` con `src` vacío

### Requirement: Comportamiento ante imagen rota (onerror)

Cuando la imagen proviene de una URL externa y falla en cliente, **DEBE** activarse un `onerror` que sustituya por la variante estática del dominio y anule el bucle (`this.onerror=null`), siguiendo el patrón vigente de placeholders.

#### Scenario: URL externa caída

- GIVEN una tarjeta cuya imagen apunta a una URL externa
- WHEN la carga de la imagen falla en el navegador
- THEN la imagen se sustituye por el SVG estático del dominio
- AND el handler queda anulado (`this.onerror=null`) para evitar bucles

### Requirement: Reutilización fuera de la portada (D5)

Las páginas que renderizan imágenes de evento sin fallback — hoy `Events.razor` y `EventsManagement.razor` — **DEBEN** adoptar el mismo mecanismo (`DefaultImage.razor` o `onerror` con variante estática). El componente **DEBE** ser reutilizable en cualquier página sin lógica específica de portada.

#### Scenario: Página de eventos con evento sin imagen

- GIVEN la página `/eventos` con un evento cuya imagen falta o falla
- WHEN la tarjeta del evento se renderiza
- THEN muestra el default de eventos y no un `<img>` roto
- AND la gestión de eventos presenta el mismo comportamiento
