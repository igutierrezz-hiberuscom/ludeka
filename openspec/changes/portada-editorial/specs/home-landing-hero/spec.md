# Delta for home-landing-hero

> Delta de la spec viva `openspec/specs/home-landing-hero/spec.md`. Idioma: español castellano.
> Aprobado por el usuario (2026-09-09): el titular visible ya no está prohibido; el hero editorial lo reemplaza.
> Fuente del patrón de rendimiento: incremento INC-07, `docs/increments/archive/inc-07-production-assets-perf.md` (`fetchpriority="high"` en carátulas LCP, CLS 0 con contenedores de dimensiones fijas). No existe capability de rendimiento en `openspec/specs/`.

## RENAMED Requirements

### Requirement: Hero sin badge ni titular visible → Hero editorial con narrativa

(Reason: decisión del usuario: la prohibición del titular visible no era una regla de producto; el nuevo hero editorial con titular en serif display queda mejor y lo reemplaza.)
(Migration: las pruebas que asertaban la ausencia de `<h1>` visible se sustituyen por las del requirement "Hero editorial con narrativa"; el badge "✨ PORTADA EDITORIAL" sigue prohibido.)

## MODIFIED Requirements

### Requirement: Hero editorial con narrativa

La portada **DEBE** renderizar un hero editorial con narrativa visible compuesto por: titular visible en serif display (`--font-display`), párrafo descriptivo reescrito como invitación a la mesa, fondo de ambiente hogareño según la variante configurada, el buscador rápido y las píldoras de acceso. La portada **NO DEBE** renderizar el badge decorativo ("✨ PORTADA EDITORIAL"). El `<PageTitle>` del navegador queda fuera de alcance y no cambia.
(Previously: el hero era minimalista, sin badge ni titular visible, compuesto solo por párrafo, buscador y píldoras.)

#### Scenario: Portada renderiza hero editorial

- GIVEN un visitante anónimo que solicita la portada `/`
- WHEN la página se renderiza por completo
- THEN el documento contiene un titular visible en serif display, el párrafo descriptivo, el formulario del buscador y las 4 píldoras de acceso
- AND el documento NO contiene el texto "PORTADA EDITORIAL"

### Requirement: Jerarquía de encabezados accesible (WCAG 2.2 AA)

La portada **DEBE** contener exactamente un `<h1>` en el documento. Con el hero editorial, ese `<h1>` es visible y es el titular del hero (deja de ser `sr-only`). Su texto definitivo lo fija `sdd-design` y **DEBE** estar redactado en castellano y describir la invitación a la mesa de Ludeka.
(Previously: el `<h1>` era único y visualmente oculto (`sr-only`) con el texto fijo "Ludeka — Juegos de mesa en español".)

#### Scenario: Único h1 visible con serif display

- GIVEN la portada `/` renderizada
- WHEN se inspecciona la jerarquía de encabezados del documento
- THEN existe exactamente un elemento `<h1>` en toda la página
- AND ese `<h1>` es visible (ya no `sr-only`) y aplica la familia `--font-display`

## ADDED Requirements

### Requirement: Variantes de fondo intercambiables del hero (D1)

El hero **DEBE** soportar exactamente tres variantes de fondo intercambiables in situ mediante un mecanismo de conmutación simple (constante o parámetro del componente):

1. **Foto real de ambiente**: assets locales en `wwwroot/images/home/hero-ambiente-*.jpg` (3 candidatas ya disponibles: `hero-ambiente-primer-plano`, `hero-ambiente-mesa-amigos`, `hero-ambiente-eurogame`; origen Pexels 8111356/8111252/6333910).
2. **Escena CSS de serie**: composición con gradientes y variables de tema (precedente `.detail-hero-backdrop`), disponible siempre como fallback.
3. **Ilustración futura**: generada por el usuario; la spec solo exige que encaje en el mismo mecanismo de variante.

La conmutación **NO DEBE** exigir cambios estructurales en el markup del hero.

#### Scenario: Hero renderiza la variante configurada

- GIVEN el hero configurado con una variante (foto, escena CSS o ilustración)
- WHEN la portada se renderiza
- THEN el fondo del hero corresponde exactamente a la variante configurada

#### Scenario: Escena CSS sin peticiones de imagen

- GIVEN la variante escena CSS activa
- WHEN la portada se renderiza
- THEN el fondo se compone íntegramente con CSS y variables de tema
- AND el navegador no solicita ninguna imagen del hero

#### Scenario: Cambio de variante sin tocar estructura

- GIVEN el componente del hero con la variante A activa
- WHEN se cambia el valor de la constante o parámetro a la variante B
- THEN el hero renderiza la variante B sin alterar el markup del resto del hero

### Requirement: Rendimiento del hero con imagen (patrón INC-07)

Si la variante activa lleva imagen, esta **DEBE** seguir el patrón del incremento INC-07: `<picture>` con fuentes AVIF y WebP y fallback JPEG, `fetchpriority="high"`, atributos `width` y `height` fijos, y peso del asset servido **< 200 KB**. Objetivos de portada: **LCP < 2,5 s** y **CLS = 0**.

#### Scenario: Imagen del hero con <picture> y prioridad

- GIVEN el hero con variante foto activa
- WHEN se inspecciona el markup del hero
- THEN existe un `<picture>` con `<source>` AVIF y WebP y `<img>` JPEG de fallback
- AND la `<img>` tiene `fetchpriority="high"`, `width`, `height` y `alt`

#### Scenario: Presupuesto de peso, LCP y CLS

- GIVEN el asset del hero servido en producción
- WHEN se mide el peso del archivo y las Core Web Vitals de la portada en móvil
- THEN el asset pesa menos de 200 KB y el LCP es inferior a 2,5 s
- AND no se registran saltos de layout atribuibles al hero (CLS 0)

### Requirement: Contraste del scrim y texto alternativo (WCAG 2.2 AA)

Con cualquier variante con imagen, el hero **DEBE** aplicar un scrim (degradado dirigido por variables de tema) que garantice contraste **≥ 4,5:1** entre el texto del hero y el fondo efectivo. La imagen **DEBE** tener un `alt` descriptivo, no vacío, en castellano que evoque la escena de ambiente.

#### Scenario: Contraste del texto sobre la imagen

- GIVEN el hero con foto y scrim renderizado sobre cualquiera de los 5 temas (`data-theme`)
- WHEN se mide el contraste entre el texto del hero y el fondo efectivo bajo el scrim
- THEN el contraste es ≥ 4,5:1 en los 5 temas

#### Scenario: Alt descriptivo de la foto de ambiente

- GIVEN el hero con variante foto activa
- WHEN se inspecciona la `<img>` del hero
- THEN su atributo `alt` es no vacío y describe la escena en castellano
