# editorial-page-foundations Specification

> Especificación NUEVA (no existe spec previa de este dominio). Idioma: español castellano.
> Cobertura: fundaciones editoriales compartidas por las 5 páginas del rediseño de INC-36 (Catálogo, Ficha, Eventos, Sorteos, Novedades): token `--on-brand`, tokens semánticos de estado, `PageHeaderEditorial`, shell `EditorialModal`, lenguaje `.rail-card` y regeneración del CSS compilado.
> Decisiones cerradas de la propuesta que aplica: D3, D4, D5, D6, D7, D8, D9, D11, D12 (parciales) y D13 (fuera de alcance).

## Propósito

Dotar a las 5 páginas del rediseño de un sistema de fundaciones editorial común y accesible (WCAG 2.2 AA): botones de marca con contraste garantizado en los 5 temas, colores de estado temables en vez de hardcodes, cabeceras de página homogéneas en serif display, modales con comportamiento unificado, tarjetas que hablan el lenguaje editorial de la portada y un CSS compilado siempre sincronizado con el punto único de verdad de tokens (`Styles/input.css`).

## Requirements

### Requirement: Token de contraste --on-brand por tema (D3)

Los 5 `data-theme` **DEBEN** definir el token `--on-brand`: el color de texto sobre superficies de marca (`--brand-primary`), con contraste **≥ 4,5:1** sobre `--brand-primary` en cada tema. Valores por tema: tinta oscura (≈ `#14181C`) en charcoal, tabletop y midnight; blanco en editorial y wood. Todos los botones con fondo `bg-[var(--brand-primary)]` de las 5 páginas (incluido el botón «Buscar» del hero) **DEBEN** usar `--on-brand` como color de texto y **NO DEBEN** usar `text-white` sobre la marca. El resto de la web queda fuera de alcance en este incremento (barrido futuro).

#### Scenario: Token definido y accesible en los 5 temas

- GIVEN cualquiera de los 5 `data-theme` declarados en `Styles/input.css`
- WHEN se inspecciona el bloque del tema
- THEN define `--on-brand` y su valor declarado produce un contraste ≥ 4,5:1 sobre `--brand-primary` del mismo tema

#### Scenario: Botones de marca adoptan el token

- GIVEN una de las 5 páginas con un botón de fondo `var(--brand-primary)`
- WHEN se inspecciona su markup
- THEN el texto del botón usa `var(--on-brand)`
- AND el botón no aplica `text-white`

### Requirement: Tokens semánticos de estado por tema (D8 + D9)

Los 5 `data-theme` **DEBEN** definir tokens semánticos de estado (como mínimo: error, warning, info y highlight) para texto y fondo/borde según el uso existente. Los colores Tailwind hardcodeados de estado (familias amber/indigo/purple/rose/sky/slate) presentes en las 5 páginas **DEBEN** sustituirse por estos tokens. Las variantes `dark:*` de las 5 páginas **DEBEN** eliminarse (son reglas muertas: la app temiza con `data-theme`, nunca con clase `dark`) y sustituirse por tokens temáticos. **NO DEBE** habilitarse `darkMode` por `data-theme` global en `tailwind.config.js`. El resto de la web espera a un barrido futuro.

#### Scenario: Tokens de estado declarados en los 5 temas

- GIVEN cualquiera de los 5 `data-theme`
- WHEN se inspecciona el bloque del tema en `Styles/input.css`
- THEN define los tokens semánticos de estado (error, warning, info, highlight)

#### Scenario: Sin hardcodes de estado en las 5 páginas

- GIVEN las 5 páginas del rediseño (`Home.razor`, `GameDetail.razor`, `Events.razor`, `Radar.razor`, `News.razor`) y sus componentes de página propios (p. ej. `GiveawayCard.razor`)
- WHEN se inspeccionan los colores de estado que usan
- THEN usan los tokens semánticos del tema
- AND no contienen hardcodes de las familias amber/indigo/purple/rose/sky/slate para estados

#### Scenario: Sin variantes dark: inertes en las 5 páginas

- GIVEN el markup de las 5 páginas y sus componentes de página
- WHEN se busca el patrón de variante `dark:`
- THEN no existen variantes `dark:*` en las 5 páginas
- AND `tailwind.config.js` no habilita `darkMode` por `data-theme` (sin cambio de configuración global)

### Requirement: Cabecera editorial de página compartida (D4 + D5)

**DEBE** existir un componente compartido `PageHeaderEditorial` que renderice: badge en píldora, título `h1` en serif display (`--font-display`), subtítulo opcional y zona de acción opcional. Las 4 páginas de listado — Catálogo (`/catalogo`), Eventos (`/eventos`), Sorteos (`/sorteos` y `/radar`) y Novedades (`/novedades`) — **DEBEN** adoptarlo, eliminando su cabecera duplicada inline (~75 líneas). La ficha (`GameDetail.razor`) **NO DEBE** adoptarlo: su cabecera es el hero de ficha con backdrop, y el título del juego permanece en sans bold.

#### Scenario: Cabecera editorial en los 4 listados

- GIVEN cualquiera de las 4 páginas de listado
- WHEN se inspecciona su cabecera
- THEN la renderiza vía `PageHeaderEditorial` con badge píldora, `h1` en serif display y subtítulo
- AND la cabecera ya no contiene su markup duplicado inline

#### Scenario: La ficha no adopta la cabecera compartida

- GIVEN la página de ficha `/juegos/{Slug}`
- WHEN se inspecciona su cabecera
- THEN mantiene su hero de ficha con backdrop y el título del juego en sans bold
- AND no usa `PageHeaderEditorial`

#### Scenario: Jerarquía de encabezados accesible

- GIVEN una de las 4 páginas de listado renderizada
- WHEN se inspecciona la jerarquía de encabezados del documento
- THEN existe exactamente un `<h1>` en la página, en castellano, con la familia `--font-display`

### Requirement: Shell de modal editorial (D11)

**DEBE** existir un shell compartido `EditorialModal` que unifique el comportamiento de los modales de página: overlay fijo a pantalla completa con fondo opaco, tarjeta centrada de ancho máximo acotado (≈ `max-w-lg`), botón de cierre, atributos de accesibilidad (diálogo con etiqueta accesible y estado modal) y gestión de foco coherente. Los modales de Radar y News **DEBEN** adoptarlo; el cuerpo específico de cada formulario queda fuera del shell (contenido del componente), eliminando el shell duplicado (~100-140 líneas).

#### Scenario: Shell adoptado por Radar y News

- GIVEN el modal de creación de sorteos (`Radar.razor`) y el de novedades (`News.razor`)
- WHEN se inspecciona su markup
- THEN ambos renderizan el shell `EditorialModal` (mismo overlay, tarjeta y cierre)
- AND el shell duplicado inline de cada página ya no existe

#### Scenario: Accesibilidad unificada del diálogo

- GIVEN el modal abierto en cualquiera de las 2 páginas
- WHEN se inspecciona el diálogo
- THEN declara etiqueta accesible y estado modal, con un control de cierre identificado
- AND el foco se gestiona de forma coherente con el shell (entrada al abrir, salida al cerrar)

### Requirement: Lenguaje .rail-card en tarjetas de página (D6)

Las tarjetas de Eventos, de Novedades y `GiveawayCard.razor` **DEBEN** hablar el lenguaje editorial `.rail-card` de portada: elevación y resplandor al hover, indicador `focus-visible` visible para navegación por teclado y desactivación de animaciones bajo `prefers-reduced-motion`. El Catálogo **DEBE** mantener `.game-card-editorial` en sus tarjetas (lenguaje ya conquistado) y **NO DEBE** migrar en este incremento.

#### Scenario: Tarjetas de página con .rail-card

- GIVEN las tarjetas de Eventos, de Novedades y `GiveawayCard.razor`
- WHEN se inspecciona su clase de tarjeta
- THEN usan `.rail-card` (lenguaje de portada: lift/glow/focus-visible/reduced-motion)
- AND ya no usan sus estilos ad-hoc de hover (p. ej. `hover:scale-105` suelto)

#### Scenario: Catálogo mantiene su lenguaje

- GIVEN la tarjeta de juego del Catálogo (`GameCard.razor`)
- WHEN se inspecciona su clase de tarjeta
- THEN mantiene `.game-card-editorial`
- AND no adopta `.rail-card` en este incremento

#### Scenario: Accesibilidad de teclado y movimiento reducido

- GIVEN una tarjeta de página enfocada mediante teclado
- WHEN se inspecciona el estado de foco
- THEN existe un indicador `focus-visible` visible
- AND con `prefers-reduced-motion` la animación de elevación/zoom queda desactivada

### Requirement: Fixes transversales cerrados (D7, D12 y a11y de pestañas)

El rediseño **DEBE** cerrar en el mismo cambio los fixes transversales de las 5 páginas: la fila de acciones de la ficha **DEBE** envolver en móvil (`flex-wrap`) con las acciones de moderación agrupadas (D7); la tira de filtros del Catálogo **DEBE** usar la clase real `scrollbar-none` (la clase muerta `no-scrollbar` **NO DEBE** persistir); las pestañas de Eventos **DEBEN** declarar su relación `tabpanel`/`aria-controls`; el `<PageTitle>` de la ficha **DEBE** decir «Ludeka»; y el texto `text-white` del estado «juego no encontrado» **DEBE** sustituirse por tokens temáticos visibles en los 5 temas.

#### Scenario: Back-bar de ficha sin desborde en móvil

- GIVEN la ficha con moderador activo (hasta 8-9 acciones)
- WHEN la fila de acciones se renderiza en un viewport móvil
- THEN la fila envuelve (`flex-wrap`) sin desbordar el ancho de página
- AND las acciones de moderación aparecen agrupadas en un bloque

#### Scenario: Tira de filtros del Catálogo sin banda de scroll

- GIVEN la página del Catálogo
- WHEN se inspecciona el carril de filtros
- THEN usa la clase real `scrollbar-none`
- AND no contiene la clase muerta `no-scrollbar`

#### Scenario: Pestañas de Eventos accesibles

- GIVEN la página de Eventos con su `tablist` de 2 pestañas
- WHEN se inspecciona el markup de pestañas y paneles
- THEN cada pestaña declara `aria-controls` hacia su panel con `role="tabpanel"`

#### Scenario: Título de página y estado sin juego corregidos

- GIVEN la ficha de juego
- WHEN se inspecciona el `<PageTitle>`
- THEN contiene «Ludeka» (sin la errata «Ludeca»)
- AND el estado «juego no encontrado» no usa `text-white` (usa tokens temáticos legibles en los 5 temas)

### Requirement: Regeneración del CSS compilado

Tras cualquier cambio en `Styles/input.css`, el archivo compilado `wwwroot/app.css` **DEBE** regenerarse con el pipeline vigente, de modo que los tokens y clases nuevos del rediseño estén presentes en el CSS servido.

#### Scenario: Tokens y clases nuevos presentes en app.css

- GIVEN un cambio de tokens/clases en `Styles/input.css`
- WHEN se inspecciona `wwwroot/app.css` regenerado
- THEN contiene los tokens y clases nuevos del rediseño (p. ej. `--on-brand`)
- AND las pruebas vigentes de rendimiento/a11y sobre `app.css` siguen en verde
