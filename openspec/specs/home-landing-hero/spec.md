# home-landing-hero Specification

> Especificación NUEVA (no existe spec previo de este dominio). Idioma: español castellano.
> Cobertura: hero minimalista de la portada (`/`), píldoras de acceso, link de novedades y accesibilidad WCAG 2.2 AA.

## Propósito

Definir el comportamiento de la cabecera (hero) de la portada de Ludeka como bloque minimalista: sin badge decorativo ni titular visible, con párrafo descriptivo, buscador rápido y píldoras de navegación, garantizando una jerarquía de encabezados accesible (h1 único visualmente oculto) conforme a WCAG 2.2 AA.

## Requirements

### Requirement: Hero de imagen protagonista

La portada **DEBE** renderizar un hero de imagen protagonista: el fondo de ambiente hogareño (según la variante configurada) ocupa el bloque y **SOLO** el buscador rápido se superpone sobre él como tarjeta con fondo propio, sin capa intermedia (sin scrim global, sin panel ni chips de texto) para que la ilustración quede visible fuera del texto. Cada variante con foto **DEBE** declarar un punto focal (`object-position`) por variante que mantenga la composición (mesa/estantería) en cuadro en el recorte residual de escritorio; **NO DEBE** dejarse el centrado por defecto (`50% 50%`) sin intención autoral. La portada **NO DEBE** renderizar titular visible ni párrafo descriptivo dentro del hero, ni el badge decorativo («✨ PORTADA EDITORIAL»), ni píldoras de acceso (eliminadas por el requirement vigente). El `<PageTitle>` del navegador queda fuera de alcance y no cambia.
(Previously: el requisito mencionaba las píldoras de acceso entre las superposiciones (eliminadas por un requirement posterior) y no definía encuadre focal; INC-36 retira la mención de píldoras y exige punto focal `object-position` por variante como parte del fix responsivo de altura.)

#### Scenario: Portada renderiza hero de imagen protagonista

- GIVEN un visitante anónimo que solicita la portada `/`
- WHEN la página se renderiza por completo
- THEN el documento contiene el fondo de ambiente configurado y el formulario del buscador superpuesto como tarjeta
- AND el documento NO contiene el texto "PORTADA EDITORIAL", ni `hero-scrim`, ni `hero-panel`, ni `hero-text-chip`
- AND el texto de la página NO queda superpuesto al fondo sin fondo propio que garantice contraste

#### Scenario: Encuadre focal por variante

- GIVEN el hero con variante foto activa
- WHEN se inspecciona el CSS del hero en `Styles/input.css`
- THEN cada variante declara su `object-position` focal (distinta del `50% 50%` por defecto)
- AND en el recorte de escritorio la mesa/estantería de la composición queda dentro del cuadro

#### Scenario: Prohibiciones INC-35 vigentes

- GIVEN la portada renderizada
- WHEN se inspecciona el markup del hero
- THEN no contiene `hero-text-chip`, `hero-scrim`, `hero-panel`, `hero-title` ni el texto "Catálogo Completo"

### Requirement: Buscador rápido conservado

El buscador del hero **DEBE** conservar su comportamiento actual: un formulario que envía el término introducido a `/catalogo?q={termino}`.

#### Scenario: Búsqueda rápida desde la portada

- GIVEN un visitante en la portada `/`
- WHEN introduce "azul" en el buscador y lo envía
- THEN la navegación resulta en `/catalogo?q=azul`

### Requirement: Píldoras de acceso eliminadas (revisión maintainer 2026-09-10)

El hero **NO DEBE** renderizar píldoras de acceso: duplicaban la navegación superior y los carriles de portada. La navegación vive en el menú superior (visible ≥ 1024px) y en el menú móvil desplegable de MainLayout (visible < 1024px, `details`/`summary` SSR puro con los 7 destinos: Catálogo, Editoriales, Creadores, Tiendas, Sorteos, Novedades y Eventos).
(Previously: el hero debía renderizar exactamente cuatro píldoras — "Catálogo Completo", "Sorteos", "Novedades" y "Eventos" — en orden y destinos congelados por D4.)

#### Scenario: El hero sin píldoras

- GIVEN la portada `/` renderizada
- WHEN se inspecciona el markup del hero
- THEN no existen píldoras de acceso dentro del hero (sin "Catálogo Completo" ni enlaces a /sorteos, /novedades o /eventos en el hero)
- AND la navegación de la portada se realiza desde el menú superior y el menú móvil desplegable

### Requirement: Link "Ver todas las novedades" corregido

En la sección "Novedades en Tiendas" de la portada, el enlace "Ver todas las novedades" **DEBE** apuntar a `/novedades` y **NO DEBE** apuntar a `/radar`.

#### Scenario: Link de novedades apunta a su página

- GIVEN la portada `/` renderizada con la sección "Novedades en Tiendas"
- WHEN se inspecciona el enlace "Ver todas las novedades"
- THEN su atributo `href` es `/novedades`
- AND ningún enlace de la portada hacia novedades apunta a `/radar`

### Requirement: Jerarquía de encabezados accesible (WCAG 2.2 AA)

La portada **DEBE** contener exactamente un `<h1>` en el documento. Con el hero de imagen protagonista, ese `<h1>` vuelve a ser visualmente oculto (`sr-only`) con el texto "La mesa está servida" — la imagen es la narrativa y las acciones se bastan (revisión del maintainer, 2026-09-10). El `<h1>` **DEBE** estar redactado en castellano y describir la invitación a la mesa de Ludeka.
(Previously: con el hero editorial visible, el `<h1>` era visible en serif display con texto fijado por `sdd-design`.)

#### Scenario: Único h1 accesible oculto visualmente

- GIVEN la portada `/` renderizada
- WHEN se inspecciona la jerarquía de encabezados del documento
- THEN existe exactamente un elemento `<h1>` en toda la página
- AND ese `<h1>` es `sr-only` (no visible, accesible para lectores de pantalla) con el texto "La mesa está servida"

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

Si la variante activa lleva imagen, esta **DEBE** seguir el patrón del incremento INC-07: `<picture>` con fuentes AVIF y WebP y fallback JPEG, `fetchpriority="high"`, atributos `width` y `height` fijos, `onerror` que oculta el `<picture>` (`hero-bg-media--failed`) y cae a la escena CSS de fondo, y peso del asset servido **< 200 KB**. Objetivos de portada: **LCP < 2,5 s** y **CLS = 0**, conservados bajo el nuevo modelo de altura responsiva. El fix **NO DEBE** romper el buscador rápido (handler `HandleQuickSearch`), el `h1` `sr-only` («La mesa está servida») ni el `alt` descriptivo en castellano.
(Previously: el requisito exigía `<picture>`, prioridad, dimensiones y peso, sin `onerror` y sin atar el CLS al modelo de altura; INC-36 congela el `onerror` y la conservación de CLS, buscador y h1 tras el cambio de altura.)

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

#### Scenario: Fallback ante imagen rota

- GIVEN el hero con variante foto activa
- WHEN la carga de la imagen falla en el navegador
- THEN el `onerror` oculta el `<picture>` (clase `hero-bg-media--failed`)
- AND la escena CSS de fondo queda visible como fallback

#### Scenario: Buscador rápido intacto tras el fix

- GIVEN el hero con el nuevo modelo de altura
- WHEN se ejecutan las pruebas `HeroEditorialQuickSearchTests`
- THEN el handler `HandleQuickSearch` navega a `/catalogo?q={termino}` sin cambios de comportamiento
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
### Requirement: Altura responsiva del contenedor del hero (D1=A)

El contenedor del hero **NO DEBE** fijar su alto con `min-height` de bloque completo (`360px` móvil / `460px` ≥ 640px). **DEBE** derivar el alto del ancho mediante `aspect-ratio` (16/9 en móvil, fiel al asset), acotar el alto máximo en escritorio con un cap tipo `clamp()` (≈ 460px) y conservar un suelo acotado (≈ 200px) que garantice el espacio del buscador superpuesto (~110-130px). Con este modelo el alto del hero **DEBE** reducirse proporcionalmente al ancho en viewports estrechos.

#### Scenario: Viewport estrecho reduce el alto proporcionalmente

- GIVEN la portada renderizada en un viewport estrecho (~360px de ancho)
- WHEN se mide el alto renderizado del hero
- THEN el alto deriva del ancho vía `aspect-ratio` (proporcional, sin suelo fijo de 360px)
- AND la imagen 16:9 se muestra prácticamente sin recorte destructivo (caja ~16/9)

#### Scenario: Cap de alto y suelo en escritorio

- GIVEN la portada renderizada en escritorio (≥ 640px)
- WHEN el ancho del viewport crece
- THEN el alto del hero queda acotado por el cap (~460px) sin saltos bruscos entre breakpoints
- AND el suelo (~200px) garantiza el espacio del buscador superpuesto

#### Scenario: CLS 0 bajo el nuevo modelo de altura

- GIVEN el hero con el nuevo modelo de altura
- WHEN la portada carga por completo
- THEN no se registran saltos de layout atribuibles al hero (el alto deriva del ancho, no del contenido)
- AND los atributos `width`/`height` de la `<img>` siguen vigentes

#### Scenario: Contrato de CSS del contenedor

- GIVEN `Styles/input.css`
- WHEN se inspeccionan las reglas de `.hero-editorial`
- THEN contienen una declaración `aspect-ratio`
- AND dejan de contener `min-height: 360px` y `min-height: 460px` como alturas de bloque

### Requirement: Contraste AA del botón Buscar del hero (D3)

El botón «Buscar» del buscador rápido **DEBE** usar `var(--on-brand)` como color de texto sobre `var(--brand-primary)`, con contraste **≥ 4,5:1** en los 5 `data-theme`, y **NO DEBE** usar `text-white`.

#### Scenario: Botón Buscar accesible en los 5 temas

- GIVEN la portada con cualquiera de los 5 `data-theme`
- WHEN se mide el contraste del texto del botón Buscar sobre `--brand-primary`
- THEN el contraste es ≥ 4,5:1
- AND el markup del botón no contiene `text-white`

