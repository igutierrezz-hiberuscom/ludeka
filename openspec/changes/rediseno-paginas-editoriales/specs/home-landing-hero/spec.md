# Delta for home-landing-hero

> Delta de la spec viva `openspec/specs/home-landing-hero/spec.md`. Idioma: español castellano.
> Motivo: INC-36 (D1=A, D2, D3). El maintainer reporta (con captura) que en móvil el hero ocupa demasiado y la composición queda recortada por los bordes. Causa raíz (`explore.md`): `min-height` fija (360px/460px) sin contenido en flujo + recorte destructivo del asset 16:9 en caja casi cuadrada de móvil (`object-fit: cover` muestra el 50% central). El fix pasa el modelo de altura a ratio derivada del ancho con encuadre focal por variante.
> No reabre decisiones INC-35: prohibiciones (`hero-text-chip|hero-scrim|hero-panel|hero-title|Catálogo Completo`), `<picture>` AVIF/WebP/JPG, `fetchpriority="high"`, `width`/`height`, alt castellano, `onerror`, h1 `sr-only` «La mesa está servida» y el buscador rápido con su handler siguen congelados. La fórmula exacta del cap `clamp()` y los puntos focales por variante los fija `sdd-design`.

## ADDED Requirements

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

## MODIFIED Requirements

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
