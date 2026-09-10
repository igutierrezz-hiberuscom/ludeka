# Delta for default-image-fallbacks

> Delta de la spec viva `openspec/specs/default-image-fallbacks/spec.md`. Idioma: español castellano.
> Motivo: INC-36 (D10). La página de Novedades (`News.razor`) y `GiveawayCard.razor` incumplen el patrón vigente: `<img>` sin `width`/`height` (CLS en el grid), sin `onerror` (imagen rota si la URL externa cae) y, en News, zona de imagen que no se renderiza si falta la URL (el carril de portada `HomeReleaseCard` sí pinta su default de dominio). Este delta nombra las páginas incumplidas y exige paridad con los carriles de portada.
> Los requirements genéricos vigentes (assets por dominio, componente `DefaultImage.razor`, comportamiento ante imagen ausente/rota y reutilización en eventos D5) **NO cambian**: la extensión va como ADDED.

## ADDED Requirements

### Requirement: Paridad de imagen en Novedades (News.razor) (D10)

La página de Novedades **DEBE** renderizar siempre la zona de imagen de la tarjeta (también sin URL): si el dato carece de imagen, la zona muestra `DefaultImage.razor` del dominio novedad en el mismo contenedor y aspecto que la imagen real (paridad con `HomeReleaseCard`); si la imagen proviene de una URL externa, la `<img>` **DEBE** declarar `width`, `height` y `onerror` hacia la variante estática de novedad con el handler anulado.

#### Scenario: Novedad sin imagen muestra el default del dominio

- GIVEN la página `/novedades` con una novedad sin imagen (URL nula o vacía)
- WHEN su tarjeta se renderiza
- THEN la zona de imagen existe y muestra el default inline del dominio novedad
- AND la tarjeta no queda sin zona de imagen ni contiene un `<img>` con `src` vacío

#### Scenario: Novedad con imagen externa con dimensiones y fallback

- GIVEN una novedad con URL externa de imagen
- WHEN se inspecciona la `<img>` de la tarjeta
- THEN declara `width`, `height` y `onerror` hacia la variante estática de novedad con `this.onerror=null`

### Requirement: Paridad de imagen en tarjetas de sorteos (GiveawayCard.razor) (D10)

`GiveawayCard.razor` **DEBE** adoptar el patrón completo: ante ausencia de imagen, `DefaultImage.razor` del dominio sorteo en el mismo contenedor con el aspecto de la imagen real; ante URL externa, `width`, `height` y `onerror` hacia la variante estática de sorteos, en paridad con el carril `HomeGiveawayCard` de portada.

#### Scenario: Sorteo sin imagen en la página de sorteos

- GIVEN la página `/sorteos` con un sorteo sin `ThumbnailUrl`
- WHEN su tarjeta se renderiza
- THEN el contenedor de imagen muestra el default inline de sorteos con el mismo aspecto que la imagen real
- AND el documento no contiene un `<img>` con `src` vacío

#### Scenario: Sorteo con imagen externa caída

- GIVEN un sorteo con URL externa de miniatura
- WHEN la carga de la imagen falla en el navegador
- THEN la imagen se sustituye por el SVG estático de sorteos con el handler anulado (`this.onerror=null`)
- AND la `<img>` declara `width` y `height` (sin CLS en el grid)
