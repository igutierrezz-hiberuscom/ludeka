# Exploration: change-01-core-catalog (Incremento 1: Catálogo Base, Ficha Inteligente y Semáforo de Escalabilidad)

## Current State

### Estado del Código y Arquitectura
- **Solución y Plataforma:**
  - Solución multi-proyecto `src/Ludeca.slnx` con destino **.NET 10 (`net10.0`, C# 13)**.
  - SDK disponible en el entorno en `$HOME\.dotnet` (versión `10.0.400`), con configuración `global.json` fijada en `10.0.400`.
  - La suite de pruebas actual (`tests/Ludeca.UnitTests`) compila y pasa exitosamente con xUnit.
- **Estructura de Capas:**
  - `src/Ludeca.Core`: Biblioteca de clases vacía (`Class1.cs`). No existen entidades, objetos de valor (Value Objects) ni reglas de dominio implementadas.
  - `src/Ludeca.Application`: Biblioteca de clases vacía (`Class1.cs`) con referencia a `Ludeca.Core`. Sin DTOs, interfaces de repositorio ni casos de uso.
  - `src/Ludeca.Infrastructure`: Biblioteca de clases vacía (`Class1.cs`) con referencia a `Ludeca.Application`. Sin cliente BGG, sin contexto de persistencia ni repositorios.
  - `src/Ludeca.Web`: Proyecto ASP.NET Core 10 Blazor Web App con soporte para componentes interactivos de servidor (`AddInteractiveServerComponents()`, `AddInteractiveServerRenderMode()`). Plantilla base con `App.razor`, `MainLayout.razor`, `Home.razor`, `NotFound.razor` y `Error.razor`. Hoja de estilos `app.css` genérica sin configuración de utilidades de Tailwind CSS.
  - `tests/Ludeca.UnitTests`: Proyecto xUnit que referencia `Ludeca.Core` y `Ludeca.Application`. Contiene prueba de ejemplo `UnitTest1.cs`.
- **Especificación Funcional Relevante:**
  - Cubre los puntos 3.1 a 3.6, 9.1 y 9.2 de `docs/specs/LUDIST_SPEC_FUNCIONAL_MVP.md`:
    - 3.1 Cabecera Visual y ADN Lúdico (títulos original/español, carátula BGG CDN, píldoras de confrontación, estilo y modo solitario).
    - 3.2 Doble Rating y Doble Ranking (BGG vs. Ludist).
    - 3.3 Semáforo Dinámico de Escalabilidad (1J a 7J+, estados 🟢/🟡/🔴, etiqueta "Ideal a X jugadores").
    - 3.4 Edad de Caja vs. Edad Real Comunitaria y Dependencia del Idioma.
    - 3.5 Duración Estimada por Jugador y Huella en Mesa (Pequeña, Comedor, Monstruo).
    - 3.6 Guía de Fundas (Sleeves) y Afiliación.
    - 9.1 y 9.2 Tops y Filtros Prácticos de Situación Real (Especial Parejas, Mesa Familiar, Grupos Grandes, Partidas Rápidas).

---

## Affected Areas

### 1. Dominio (`src/Ludeca.Core`)
- **Agregado Raíz `Game`:**
  - Identidad: `GameId` (GUID / Strongly-typed ID), `BggId` (entero único BGG), `Slug` (identificador amigable para URLs amigables como `/juegos/wingspan`).
  - Información Editorial: `OriginalTitle`, `SpanishTitle`, `Designer`, `Publisher`, `YearPublished`, `CoverImageUrl`, `ThumbnailUrl`, `Description`.
  - Píldoras de ADN Lúdico:
    - `ConfrontationType` (Enum: `Cooperative`, `Competitive`, `HiddenRolesOrTeams`, `SemiCooperative`).
    - `GameStyle` (Enum: `Eurogame`, `Ameritrash`, `PartyGame`, `FillerOrAbstract`, `NarrativeOrCampaign`).
    - `SoloMode` (Value Object o struct con soporte oficial y detalles).
  - Semáforo de Escalabilidad (`ScalabilityTrafficLight`):
    - Desglose por recuento de comensales: `1`, `2`, `3`, `4`, `5`, `6`, `7+`.
    - `ScalabilityStatus` (Enum: `MustPlay` / 🟢 Imprescindible, `Recommended` / 🟡 Recomendado, `NotRecommended` / 🔴 No recomendado).
    - `IdealPlayerCount` (Resumen textual: ej. "Ideal: 2 jugadores" o "2-3 jugadores").
    - Lógica de cálculo automático basada en votos de BGG (`suggested_numplayers`).
  - Accesibilidad e Idioma (`AgeRating`, `LanguageDependence`):
    - `BoxAge` (edad legal) vs. `CommunityAge` (edad real consensuada).
    - Propiedad calculada `IsAccessibleEarlier`: indica si la comunidad lo considera jugable a edad menor que la caja.
    - `LanguageDependence` (Enum: `None` / Nula, `Low` / Baja, `High` / Alta).
  - Tiempo y Espacio (`GameDuration`, `TableFootprint`):
    - `MinPlayTimeMinutes`, `MaxPlayTimeMinutes`, `EstimatedMinutesPerPlayer`.
    - `TableFootprint` (Enum: `Small` / Mesa pequeña, `Medium` / Comedor estándar, `Large` / Monstruo de mesa).
  - Guía de Fundas (`SleeveRequirement`):
    - `CardCount`, `WidthMm`, `HeightMm`, `FormatName`, `AffiliateUrl`.
  - Doble Puntuación (`RatingSummary`):
    - `BggRating`, `BggRank`, `LudistRating`, `LudistRank`.

### 2. Casos de Uso y Aplicación (`src/Ludeca.Application`)
- **Interfaces de Repositorio:**
  - `IGameRepository`: Métodos de consulta paginada, filtrado reactivo (`SearchAsync(GameFilterCriteria criteria)`), obtención por ID y por Slug (`GetBySlugAsync(string slug)`), conteo y alta/actualización masiva.
- **Interfaces de Servicio Externo:**
  - `IBggClient`: Consulta a BGG XMLAPI2 para obtener detalles técnicos y estadísticas (`GetGameByIdAsync(int bggId)`).
- **DTOs y Modelos de Consulta:**
  - `GameSummaryDto` (modelo optimizado para tarjetas del catálogo y búsqueda rápida).
  - `GameDetailDto` (modelo completo con todos los metadatos, fundas, desglose de escalabilidad y ratings).
  - `GameFilterCriteria` (término de búsqueda, estilo lúdico, número de comensales, duración máxima, especial parejas).
- **Casos de Uso (Queries & Commands):**
  - `GetCatalogGamesQuery`: Obtiene listado paginado/filtrado con ordenación (popularidad BGG, alfabético, rating).
  - `GetGameDetailBySlugQuery`: Recupera la ficha completa para la vista de detalle.
  - `GetQuickSearchResultsQuery`: Búsqueda reactiva instantánea para el buscador de cabecera.

### 3. Infraestructura (`src/Ludeca.Infrastructure`)
- **Cliente BGG XMLAPI2 (`BggXmlApiClient`):**
  - Ingesta contra `https://boardgamegeek.com/xmlapi2/thing?id={id}&stats=1`.
  - Parser robusto con `System.Xml.Linq.XDocument` para manejar estructuras de encuestas (`<poll name="suggested_numplayers">`, `<poll name="language_dependence">`, `<poll name="suggested_playerage">`).
  - Control de ratio y cortesía de peticiones (`System.Threading.RateLimiting` o Polly) para evitar HTTP 429.
  - Mecanismo de reintento para respuestas HTTP 202 (encoladas por BGG).
- **Persistencia con SQLite & EF Core (`LudecaDbContext`):**
  - Mapeo relacional de la entidad `Game` y mapeo como Owned Types o JSON de los Value Objects (`ScalabilityTrafficLight`, `AgeRating`, `GameDuration`, `SleeveRequirement`).
  - `SqliteGameRepository` implementando `IGameRepository`.
- **Carga de Datos Inicial (Seeder Offline-First):**
  - `CatalogSeeder` que inicializa el catálogo con un fichero `seed_games.json` pre-enriquecido (Top 25-50 títulos populares del mercado hispano: Wingspan, Catán, Gloomhaven, 7 Wonders Duel, Carcassonne, Terraforming Mars, Azul, etc.) si la base de datos está vacía.

### 4. Capa Web & Interfaz Blazor (`src/Ludeca.Web`)
- **Rutas y Páginas:**
  - `/` y `/catalogo`: Catálogo general con buscador reactivo, filtros por ADN lúdico y cuadrícula responsive de tarjetas.
  - `/juegos/{slug}`: Ficha inteligente del juego con cabecera editorial y lectura en 3 segundos.
- **Componentes Razor:**
  - `GameCard.razor`: Tarjeta compacta móvil con portada CDN, doble título, píldoras de ADN, semáforo resumido y puntuación BGG.
  - `ScalabilityTrafficLight.razor`: Semáforo dinámico de 1J a 7J+ con fichas interactivas en verde/amarillo/rojo y banner "Ideal: X jugadores".
  - `QuickBadges.razor`: Fila de badges visuales compactos (Confrontación, Estilo, Solitario, Edad Caja vs Real, Idioma, Tiempo/Jugador, Huella).
  - `GameDetailHeader.razor`: Cabecera editorial con imagen de alta calidad, autores, editorial y doble rating BGG/Ludist.
  - `SleeveGuideCard.razor`: Bloque de especificación de fundas de cartas con enlace de compra.
  - `CatalogSearchBar.razor`: Barra de búsqueda rápida con debounce e interactividad de servidor.
- **Estilos y Tailwind CSS:**
  - Configuración de utilidades Tailwind compiladas en `Ludeca.Web/wwwroot/app.css`. Paleta corporativa con indicadores semafóricos accesibles (Verde `#10B981`, Amarillo `#F59E0B`, Rojo `#EF4444`).

### 5. Pruebas Unitarias (`tests/Ludeca.UnitTests`)
- Pruebas de dominio:
  - Cálculo del semáforo de escalabilidad a partir de votos (reglas de mayorías y umbrales para `MustPlay`, `Recommended`, `NotRecommended`).
  - Detección automática del número ideal de comensales.
  - Verificación de la regla `IsAccessibleEarlier` en `AgeRating`.
  - Formateo y cálculo de tiempos por jugador en `GameDuration`.
- Pruebas de infraestructura / parser:
  - Pruebas del parser BGG contra fixtures XML reales (títulos con expansiones, juegos solitarios puros, juegos de 2 jugadores, juegos con dependencias altas de idioma).
- Pruebas de casos de uso:
  - Verificación de filtrado y búsqueda en `GetCatalogGamesQuery`.

---

## Approaches

### 1. Modelado de Dominio: Entidades y Value Objects
- **Enfoque A (Modelo Anémico con primitivas sueltas y JSON sin tipar):**
  - Propiedades primitivas en `Game` (`int MinPlayers`, `string ScalabilityJson`, etc.).
  - *Ventajas:* Rápido de generar inicialmente.
  - *Desventajas:* Pérdida de encapsulación de reglas de negocio, duplicación de lógica de cálculo de semáforo en controladores/vistas, propenso a errores al añadir votos de usuario en Slices 2 y 3.
- **Enfoque B (Modelo Rico con Value Objects tipados y encapsulación DDD):**
  - Entidad `Game` como agregado raíz.
  - Value Objects inmutables: `ScalabilityTrafficLight`, `PlayerScalability`, `AgeRating`, `GameDuration`, `SleeveRequirement`.
  - Métodos puros de cálculo: `ScalabilityTrafficLight.CalculateStatus(votes)` y `ScalabilityTrafficLight.GetIdealPlayerCount()`.
  - *Ventajas:* Cumple estrictamente los principios de diseño de Ludist, testeabilidad unitaria pura sin dependencias de base de datos, evolución limpia hacia valoraciones comunitarias.
  - *Desventajas:* Requiere configuración explícita de mapeo en EF Core (mediante `ComplexProperty` o `.ToJson()`).

### 2. Ingesta y Cliente BGG XMLAPI2
- **Enfoque A (`System.Xml.Serialization.XmlSerializer`):**
  - Generación de clases de serialización XML fuertemente tipadas.
  - *Ventajas:* Mapeo automático directo a clases C#.
  - *Desventajas:* La API XML2 de BGG contiene frecuentes irregularidades (etiquetas vacías, atributos variables como `numplayers="4+"`, nodos `<poll>` polimórficos). `XmlSerializer` es sumamente frágil y falla ante discrepancias menores en el esquema.
- **Enfoque B (`System.Xml.Linq.XDocument` / LINQ to XML):**
  - Carga en `XDocument` y extracción mediante expresiones LINQ robustas con navegación segura contra nulos (`?.Value`).
  - *Ventajas:* Máxima flexibilidad y tolerancia a fallos ante cambios en la respuesta de BGG; extracción limpia de títulos alternativos en español y encuestas anidadas; fácil de mockear y probar con archivos XML de prueba.
  - *Desventajas:* Requiere código explícito de mapeo, pero este queda completamente aislado y cubierto por tests unitarios.
- **Enfoque C (`XmlReader` de bajo nivel):**
  - Lectura secuencial por flujos de texto XML.
  - *Ventajas:* Mínimo consumo de memoria en ingestas masivas.
  - *Desventajas:* Complejidad de desarrollo innecesariamente alta para consultas individuales o por lotes pequeños (20 juegos).

### 3. Estrategia de Persistencia para el Incremento 1
- **Enfoque A (Repositorio en Memoria con semilla JSON estática):**
  - `InMemoryGameRepository` que lee de un `seed_games.json` en memoria.
  - *Ventajas:* Cero dependencias de base de datos, arranque instantáneo.
  - *Desventajas:* No persiste datos reales importados desde BGG, no permite validar consultas LINQ reales de EF Core ni migraciones, obligaría a rehacer la capa de datos en el Incremento 2.
- **Enfoque B (SQLite con Entity Framework Core + Seeder Automático):**
  - `LudecaDbContext` sobre base de datos SQLite local (`ludeca.db`).
  - Mapeo moderno con EF Core 10, persistiendo los Value Objects mediante columnas JSON o propiedades complejas.
  - `CatalogSeeder` que inicializa automáticamente el catálogo con títulos pre-procesados si la BD está recién creada.
  - *Ventajas:* Persistencia real local, soporte inmediato para búsquedas con índices, reutilización directa en el Incremento 2 (Ludoteca personal y préstamos), posibilidad de migrar a PostgreSQL en producción sin tocar la lógica de dominio.
  - *Desventajas:* Requiere añadir paquetes NuGet de EF Core SQLite.

### 4. Estrategia de Blazor UI y Tailwind CSS
- **Enfoque A (Tailwind Play CDN en tiempo de ejecución):**
  - Inclusión de `<script src="https://cdn.tailwindcss.com"></script>` en `App.razor`.
  - *Ventajas:* Cero configuración de compiladores en la máquina local.
  - *Desventajas:* No recomendado para producción por Tailwind Labs, posible parpadeo de contenido sin estilo (FOUC), advertencias en consola del navegador.
- **Enfoque B (CLI Standalone de Tailwind CSS en paso de compilación):**
  - Ejecutable standalone de Tailwind o script npm que compila hacia `wwwroot/app.css`.
  - *Ventajas:* CSS final minificado y purgado con todas las clases utilitarias del proyecto.
  - *Desventajas:* Requiere herramientas externas o ejecutable adicional en el pipeline de build local.
- **Enfoque C (Híbrido: Hoja de Estilos Tailwind Base Enriquecida en `app.css` + script de build CLI opcional):**
  - Integrar en `Ludeca.Web/wwwroot/app.css` un bundle completo de utilidades de Tailwind con paleta de colores y componentes específicos de Ludeca (Badges, Semáforos, Tarjetas), complementado con la configuración estándar para permitir compilación CLI.
  - *Ventajas:* El proyecto compila y se renderiza con fidelidad visual inmediata sin requerir npm/node en cualquier entorno, manteniendo la extensibilidad de Tailwind.

---

## Recommendation

Se recomienda la combinación de enfoques más robusta, limpia y alineada con los principios de Clean Architecture y Spec-Driven Development:

1. **Dominio (Enfoque B - Modelo Rico DDD):**
   - Implementar `Game` como agregado raíz en `src/Ludeca.Core`.
   - Crear Value Objects inmutables con semántica clara: `ScalabilityTrafficLight`, `PlayerScalability`, `AgeRating`, `GameDuration`, `SleeveRequirement`.
   - La lógica de cálculo del semáforo (estados 🟢, 🟡, 🔴 y etiqueta "Ideal a X jugadores") residirá exclusivamente en el dominio, con tests unitarios exhaustivos.
2. **Ingesta BGG (Enfoque B - `System.Xml.Linq.XDocument`):**
   - Implementar `BggXmlApiClient` en `src/Ludeca.Infrastructure` utilizando `XDocument`.
   - Incorporar `TokenBucketRateLimiter` o politeness handler para respetar la tasa de BGG.
   - Diseñar suite de pruebas unitarias en `tests/Ludeca.UnitTests` con respuestas XML de muestra.
3. **Persistencia (Enfoque B - SQLite con EF Core + CatalogSeeder):**
   - Configurar `LudecaDbContext` con SQLite en `src/Ludeca.Infrastructure`.
   - Implementar `CatalogSeeder` que garantice un catálogo inicial listo para usar con 20-30 juegos top (Gloomhaven, Wingspan, Catán, 7 Wonders Duel, etc.) con sus nombres en español y atributos completos.
4. **UI Blazor & Tailwind (Enfoque C - CSS Tailwind Bundle Integrado y Componentes Segregados):**
   - Diseñar la jerarquía de componentes Razor en `src/Ludeca.Web`:
     - `Pages/CatalogPage.razor` (catálogo y buscador reactivo).
     - `Pages/GameDetailPage.razor` (ficha inteligente en `/juegos/{slug}`).
     - `Components/GameCard.razor` (tarjeta de lectura en 3 segundos).
     - `Components/ScalabilityTrafficLight.razor` (semáforo dinámico adaptativo 1-7+).
     - `Components/QuickBadges.razor` (ADN lúdico, edades, huella y tiempo).
     - `Components/SleeveGuideCard.razor` (guía de fundas con enlaces).
   - Asegurar diseño Mobile-First radical con navegación táctil fluida.

---

## Risks

1. **Resolución de PATH para el SDK de .NET 10 en Windows:**
   - *Riesgo:* La consola por defecto puede resolver `C:\Program Files\dotnet` (que contiene .NET 6 y 7) antes que `$HOME\.dotnet` (donde reside .NET 10.0.400), causando errores de versión al ejecutar `dotnet` sin ruta absoluta.
   - *Mitigación:* Documentar y asegurar en los comandos de build/test el ajuste de entorno (`$env:PATH = "$HOME\.dotnet;$env:PATH"`).
2. **Limitaciones de Ratio y Disponibilidad de BGG XMLAPI2:**
   - *Riesgo:* Bloqueos temporales HTTP 429 o respuestas diferidas HTTP 202 al consultar BGG durante pruebas o uso frecuente.
   - *Mitigación:* El sistema operará bajo arquitectura *Offline-First* con un catálogo semilla local (`seed_games.json`), haciendo que la aplicación sea 100% funcional sin depender de la conectividad en vivo con BGG para el desarrollo y las pruebas.
3. **Diferencias entre Títulos Originales y Títulos Comerciales Hispanos:**
   - *Riesgo:* Los jugadores hispanohablantes buscan habitualmente por el nombre traducido comercializado en España o Latinoamérica (ej. "Los Castillos de Borgoña" o "Catán"), mientras que la clave primaria en BGG es el título en inglés.
   - *Mitigación:* El modelo separa `OriginalTitle` de `SpanishTitle`, y los índices y algoritmos de búsqueda buscan de forma ponderada en ambos campos.
4. **Mapeo de Colecciones y Value Objects en SQLite con EF Core:**
   - *Riesgo:* El mapeo relacional de listas anidadas de objetos de valor (como los votos de escalabilidad para cada número de jugadores) puede complejizar las tablas relacionales.
   - *Mitigación:* Aprovechar las capacidades nativas de EF Core para almacenar los desglose de escalabilidad como columnas JSON (`ToJson()`) o como propiedades complejas inmutables.

---

## Ready for Proposal

El análisis de requerimientos, dependencias técnicas, modelos de dominio, componentes UI y estrategias de persistencia para el **Incremento 1 (`change-01-core-catalog`)** está completado con éxito. Se cuenta con claridad absoluta sobre el alcance para avanzar a la fase formal de **Propuesta (`sdd-propose`)**.
