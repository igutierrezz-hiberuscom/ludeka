# Especificación: weekly-releases (Radar de Lanzamientos Semanales de Tiendas)

## 1. Propósito y Contexto
Define el funcionamiento del **Radar de Lanzamientos de los Viernes**, que informa a la comunidad sobre los nuevos juegos de mesa y reimpresiones que llegan físicamente a las tiendas especializadas españolas cada semana.

---

## 2. Requerimientos de Comportamiento (RFC 2119)

### REQ-REL-01: Estructura del Lanzamiento Semanal
- El sistema **DEBE** registrar cada lanzamiento semanal con:
  - `Title`: Título comercial en español.
  - `Publisher`: Editorial que lo distribuye o publica (ej. Devir, Maldito Games, Asmodee, Tranjis).
  - `ReleaseDate`: Fecha de puesta a la venta en tiendas (habitualmente el viernes).
  - `IsReprint`: Booleano que distingue entre novedad absoluta (`false`) y reimpresión de stock (`true`).
- El sistema **PUEDE** asociar:
  - `GameId`: Vinculación directa al catálogo de Ludeka si ya está fichado (`Guid?`).
  - `EstimatedPvp`: Precio de venta al público recomendado aproximado en euros (`decimal?`).
  - `CoverImageUrl`: Carátula para previsualización (`string?`).
  - `Notes`: Información adicional (ej. "Incluye miniexpansión de preventa").

### REQ-REL-02: Agrupación y Filtro Temporal
- El sistema **DEBE** proporcionar un método de consulta para obtener los lanzamientos de la semana actual o próxima semana (`GetReleasesForWeekAsync`).
- La vista **DEBE** diferenciar visualmente con badges claros:
  - 🆕 *"Novedad"* (para títulos nuevos).
  - 🔄 *"Reimpresión"* (para juegos repuestos).

---

## 3. Criterios de Aceptación (Escenarios Gherkin)

### Escenario 1: Distinción de reimpresión frente a novedad
```gherkin
Given un lanzamiento registrado con IsReprint en true
When se visualiza en el radar de novedades
Then debe mostrar el badge "🔄 Reimpresión"
And si IsReprint es false debe mostrar el badge "🆕 Novedad"
```
