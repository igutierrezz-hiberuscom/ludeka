# Especificación: transparency-manifesto (Manifiesto de Transparencia y Comunidad)

## 1. Propósito y Contexto
Define el compromiso ético y la transparencia económica de Ludeka frente a la comunidad, detallando el modelo de negocio por afiliación/mecenazgo y garantizando que los fondos se reinvierten exclusivamente en el ecosistema de juegos de mesa.

---

## 2. Requerimientos de Comportamiento (RFC 2119)

### REQ-TRN-01: Página Editorial del Manifiesto de Transparencia (`/transparencia`)
- El sistema **DEBE** exponer la ruta pública accesible `/transparencia`.
- El documento **DEBE** detallar con claridad los 3 fines transparentes de los fondos:
  1. *Infraestructura y hosting:* Cobertura de servidores, dominio y herramientas.
  2. *Copias para mesa real:* Adquisición de novedades y expansiones para que la mesa fundadora elabore análisis a 2 jugadores y con familias con fotos reales de mesa.
  3. *Sorteos comunitarios:* Financiación de sorteos periódicos de juegos top para los miembros activos.
- La página **DEBE** incluir:
  - Enlaces de mecenazgo voluntario (Ko-fi / Buy Me a Coffee) con información sobre la insignia visual *"Mecenas"* en el perfil.
  - Botón de unión directa al servidor oficial de Discord.

### REQ-TRN-02: Pie de Página y Navegación Global
- El layout principal (`MainLayout.razor`) **DEBE** incorporar:
  - Enlace a `/radar` (Radar de Sorteos & Novedades).
  - Enlace a `/transparencia` (Manifiesto de Transparencia).
  - Enlace a la comunidad en Discord.
  - Leyenda del cierre del MVP de Ludeka.

---

## 3. Criterios de Aceptación (Escenarios Gherkin)

### Escenario 1: Visualización del manifiesto de transparencia
```gherkin
Given un visitante navegando por Ludeka
When accede a la ruta "/transparencia"
Then visualiza los 3 pilares éticos de reinversión de fondos
And encuentra los botones para unirse a Discord y participar como mecenas
```
