# home-landing-hero Specification

> Especificación NUEVA (no existe spec previo de este dominio). Idioma: español castellano.
> Cobertura: hero minimalista de la portada (`/`), píldoras de acceso, link de novedades y accesibilidad WCAG 2.2 AA.

## Propósito

Definir el comportamiento de la cabecera (hero) de la portada de Ludeka como bloque minimalista: sin badge decorativo ni titular visible, con párrafo descriptivo, buscador rápido y píldoras de navegación, garantizando una jerarquía de encabezados accesible (h1 único visualmente oculto) conforme a WCAG 2.2 AA.

## Requirements

### Requirement: Hero sin badge ni titular visible

La portada **DEBE** renderizar el hero compuesto únicamente por el párrafo descriptivo actual, el buscador rápido y las píldoras de acceso. La portada **NO DEBE** renderizar el badge decorativo ("✨ PORTADA EDITORIAL") ni ningún titular visible. El `<PageTitle>` del navegador queda fuera de alcance y no cambia.

#### Scenario: Portada renderiza hero minimalista

- GIVEN un visitante anónimo que solicita la portada `/`
- WHEN la página se renderiza por completo
- THEN el documento contiene el párrafo descriptivo del hero y el formulario del buscador rápido
- AND el documento NO contiene el texto "PORTADA EDITORIAL" ni ningún elemento `<h1>` con contenido visible

### Requirement: Buscador rápido conservado

El buscador del hero **DEBE** conservar su comportamiento actual: un formulario que envía el término introducido a `/catalogo?q={termino}`.

#### Scenario: Búsqueda rápida desde la portada

- GIVEN un visitante en la portada `/`
- WHEN introduce "azul" en el buscador y lo envía
- THEN la navegación resulta en `/catalogo?q=azul`

### Requirement: Píldoras de acceso finales (D4)

El hero **DEBE** renderizar exactamente cuatro píldoras de acceso, en este orden y con estas etiquetas y destinos: "Catálogo Completo" → `/catalogo`, "Sorteos" → `/sorteos`, "Novedades" → `/novedades`, "Eventos" → `/eventos`. El hero **NO DEBE** renderizar píldoras hacia `/editoriales` ni hacia `/creadores`.

#### Scenario: Píldoras exactas con destinos exactos

- GIVEN la portada `/` renderizada
- WHEN se inspeccionan los enlaces del bloque de píldoras del hero
- THEN existen exactamente 4 enlaces con las etiquetas y destinos del requerimiento, en el orden indicado
- AND ninguno apunta a `/editoriales` ni a `/creadores`, y no existe etiqueta "Radar & Sorteos" ni "Autores"

#### Scenario: Etiqueta de Sorteos sin herencia legacy

- GIVEN la portada `/` renderizada
- WHEN se inspecciona la píldora de sorteos
- THEN su etiqueta es exactamente "Sorteos" y su destino es `/sorteos`

### Requirement: Link "Ver todas las novedades" corregido

En la sección "Novedades en Tiendas" de la portada, el enlace "Ver todas las novedades" **DEBE** apuntar a `/novedades` y **NO DEBE** apuntar a `/radar`.

#### Scenario: Link de novedades apunta a su página

- GIVEN la portada `/` renderizada con la sección "Novedades en Tiendas"
- WHEN se inspecciona el enlace "Ver todas las novedades"
- THEN su atributo `href` es `/novedades`
- AND ningún enlace de la portada hacia novedades apunta a `/radar`

### Requirement: Jerarquía de encabezados accesible (WCAG 2.2 AA)

La portada **DEBE** contener exactamente un `<h1>` en el documento, visualmente oculto (clase `sr-only`), con el texto definitivo: `Ludeka — Juegos de mesa en español`.

#### Scenario: Único h1 sr-only con texto propio

- GIVEN la portada `/` renderizada
- WHEN se inspecciona la jerarquía de encabezados del documento
- THEN existe exactamente un elemento `<h1>` en toda la página
- AND ese `<h1>` es visualmente oculto (`sr-only`) y su texto es "Ludeka — Juegos de mesa en español"
