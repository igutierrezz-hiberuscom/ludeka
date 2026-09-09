# iconography-lucide Specification

> Especificación NUEVA. Idioma: español castellano.
> Cobertura: componente `Icon.razor` (SVG Lucide inline), migración global de emojis a Lucide (D3: portada como primer slice, resto de la web como slice propio del mismo incremento) y regla de iconos futuros.

## Propósito

Establecer la iconografía de Ludeka: Lucide Icons vía un componente `Icon.razor` con SVG inline, `currentColor` y semántica accesible, sustituyendo todos los emojis de la interfaz y prohibiendo su uso futuro.

## Requirements

### Requirement: Componente Icon.razor

**DEBE** existir un componente `Icon.razor` en `Components/Shared/` que renderice SVGs inline del catálogo Lucide con `stroke="currentColor"` (hereda el color del tema), tamaño configurable y `aria-hidden="true"` por defecto (decorativo). Ante un icono no presente en el catálogo, el componente **DEBE** renderizar sin salida y sin lanzar error, nunca un sustituto.

#### Scenario: Icono decorativo hereda color

- GIVEN un uso de `Icon.razor` con un nombre del catálogo (p. ej. "search", "gift", "trophy")
- WHEN el icono se renderiza
- THEN el SVG es inline, usa `currentColor` y es `aria-hidden="true"`
- AND no realiza ninguna petición de red (sin fuente externa ni `<img>`)

#### Scenario: Icono desconocido

- GIVEN un uso de `Icon.razor` con un nombre fuera del catálogo
- WHEN el componente se renderiza
- THEN no renderiza SVG ni emoji y la página no falla

### Requirement: Migración global de emojis a Lucide (D3)

Toda la interfaz de la web **DEBE** dejar de renderizar emojis como iconografía: la portada como primer slice (~18 emojis) y el resto de los componentes de la web como slice propio dentro del mismo incremento. Donde el emoji era la única señal semántica, **DEBE** añadirse texto visible o accesible equivalente junto al icono.

#### Scenario: Portada sin emojis

- GIVEN la portada `/` renderizada, incluido su estado de carga
- WHEN se inspecciona el documento
- THEN no contiene caracteres emoji de la lista migrada (🔍 🎲 🎁 📰 🎪 🏆 ⭐ ⏱️ 🚀 🔄 🆕 🗓️ 📅 📍 🌐 🧩)
- AND cada icono anterior procede de `Icon.razor`

#### Scenario: Resto de la web sin emojis

- GIVEN las páginas migradas del resto de la web (catálogo, fichas, eventos, biblioteca, perfil, etc.)
- WHEN se inspecciona su markup
- THEN ningún componente renderiza emojis de interfaz
- AND los usos previos de emoji son iconos de `Icon.razor`

### Requirement: Regla de iconos futuros

Cualquier icono nuevo de la interfaz **DEBE** añadirse y usarse a través de `Icon.razor` (paths Lucide). La interfaz **NO DEBE** incorporar nuevos emojis como iconografía en ningún componente.

#### Scenario: Nuevo icono en la interfaz

- GIVEN una funcionalidad futura que necesite un icono
- WHEN el desarrollador lo incorpora
- THEN usa `Icon.razor`, añadiendo el path Lucide al catálogo si no existe
- AND no inserta un emoji en el markup
