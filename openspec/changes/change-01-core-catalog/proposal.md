# Propuesta: Incremento 1 — Catálogo Base, Ficha Inteligente y Semáforo de Escalabilidad

## Intención
Modelo de dominio, catálogo base y ficha inteligente con semáforo de escalabilidad, ADN lúdico y fundas, operando en modo offline-first con cliente BGG XMLAPI2 y componentes Blazor con estilo editorial en Tailwind CSS.

## Alcance
### Dentro del Alcance
- Entidad de agregado `Game`, repositorio, filtros multi-criterio y rutas por slug.
- ADN Lúdico (Confrontación, Estilo, Modo Solitario).
- Semáforo de escalabilidad (1 a 7+ jugadores, 🟢/🟡/🔴, etiqueta de comensales ideales).
- Accesibilidad (edad de caja vs. comunitaria, dependencia del idioma, duración y huella en mesa).
- Guía de fundas (medidas, conteos y enlaces contextuales).
- Cliente BGG XMLAPI2 con LINQ-to-XML y control de ratio de peticiones (rate limiting).
- SQLite EF Core 10 con mapeo JSON y seeder local (25–50 títulos top en español).
- Blazor Web App (Vistas de Catálogo y Ficha `/juegos/{slug}`).

### Fuera del Alcance
- Colección personal y listas de deseos (Incremento 2).
- Panel y veredictos de la mesa fundadora con fotos reales (Incremento 3).
- Hub multimedia segregado de YouTube e Instagram (Incremento 4).

## Capacidades
### Nuevas Capacidades
- `core-catalog`: Entidad del juego de mesa, repositorio, filtrado de consultas y enrutamiento por slug.
- `game-dna-badges`: Value Objects y badges de UI para Confrontación, Estilo Lúdico y Modo Solitario.
- `scalability-traffic-light`: Desglose de 1 a 7+ jugadores con estados Imprescindible/Recomendado/No recomendado y lógica de "Ideal a X jugadores".
- `game-accessibility-specs`: Edad de caja vs. edad comunitaria (accesibilidad temprana), dependencia del idioma, duración por jugador y huella en mesa.
- `sleeve-guide`: Guía de medidas de fundas, conteo de cartas y recomendaciones contextuales de afiliación.
- `bgg-xmlapi-client`: Cliente HTTP resiliente y parser LINQ-to-XML con limitación de tasa (rate limiting) para BGG XMLAPI2.
- `offline-catalog-seeder`: Contexto de base de datos SQLite EF Core y cargador de semillas JSON con 25-50 juegos de mesa en español.

### Capacidades Modificadas
- Ninguna.

## Enfoque Arquitectónico
- **Dominio (`Ludeca.Core`)**: Agregado `Game` y Value Objects inmutables con lógica de negocio pura.
- **Aplicación (`Ludeca.Application`)**: Manejadores de consulta e interfaces (`IGameRepository`, `IBggClient`).
- **Infraestructura (`Ludeca.Infrastructure`)**: SQLite EF Core 10 con mapeo JSON (`ToJson()`), cliente BGG con `XDocument` y seeder.
- **UI (`Ludeca.Web`)**: Blazor Web App con SSR y renderizado interactivo en componentes, Tailwind CSS mobile-first.

## Áreas Afectadas
| Área | Impacto | Descripción |
|---|---|---|
| `Ludeca.Core` | Alto | Agregado `Game` y Value Objects de ADN, escalabilidad y fundas |
| `Ludeca.Application` | Medio | DTOs, interfaces de repositorio y queries |
| `Ludeca.Infrastructure` | Alto | `LudecaDbContext`, SQLite, cliente BGG y seeder JSON |
| `Ludeca.Web` | Alto | Vista de catálogo, `/juegos/{slug}` y componentes Razor |
| `tests/Ludeca.UnitTests` | Medio | Pruebas unitarias de dominio, parser BGG y semáforo |

## Riesgos y Mitigaciones
| Riesgo | Severidad | Mitigación |
|---|---|---|
| Límites de cuota en BGG XMLAPI2 | Media | Modo offline-first con catálogo base pre-cargado y rate limiter |
| Mapeo de Value Objects complejos en EF Core | Baja | Uso del soporte nativo JSON (`ToJson()`) de EF Core |
| Búsqueda de nombres en inglés vs. español | Baja | Indexación y búsqueda simultánea en `OriginalTitle` y `SpanishTitle` |

## Plan de Rollback
- Revertir commits del incremento.
- Eliminar base de datos local `ludeca.db`.

## Criterios de Éxito
- 100% de tests unitarios pasando en xUnit (.NET 10).
- Catálogo y filtros completamente operativos con datos locales.
- Ficha de juego con carga instantánea, ADN lúdico, semáforo dinámico de 1 a 7+ y guía de fundas.
