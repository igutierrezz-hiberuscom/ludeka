# Exploración Técnica: change-26-card-sleeves-spec-stores (Incremento 26)

## 1. Contexto y Objetivos del Slice
El **Incremento 26** ("Especificación de Fundas (Sleeves) por Juego y Enlaces de Compra Contextuales") aborda una necesidad crítica para la comunidad de jugadores de mesa ("Protege tu juego"). 
El objetivo es transformar la experiencia de cuidado de componentes en Ludeka, permitiendo a los usuarios conocer exactamente:
1. Las medidas milimétricas de cada tipo de carta de un juego (ancho x alto en mm).
2. El número total de cartas por tipo/formato.
3. El cálculo automático de paquetes requeridos según el tamaño de empaque (50 y 100 fundas).
4. La visualización gráfica de la silueta de la carta y comparativa didáctica de grosores (Standard 50-60 µm vs. Premium 100 µm).
5. Enlaces de compra quirúrgicos y contextuales a tiendas colaboradoras (ej. Zacatrus, Dungeon Marvels, etc.), llevando directamente al tamaño exacto de funda con el tag de afiliado de Ludeka.
6. Capacidad para que los moderadores editen o agreguen fundas desde el editor editorial de fichas (INC-18 / `GameEditorModal`).
7. Ingesta y detección de fundas desde BGG XMLAPI2 (`boardgamecardsleeve`), con fallback en el catálogo estándar de fundas del hobby.

---

## 2. Diagnóstico del Estado Actual del Código

### 2.1 Dominio (`Ludeka.Core`)
- **`SleeveItem` (`src/Ludeka.Core/ValueObjects/SleeveItem.cs`):**
  - Ya existe como Value Object con propiedades: `FormatName`, `WidthMm`, `HeightMm`, `CardCount`, `AffiliateUrl`, `StoreName`, `Country`, `ShippingCountries`.
  - Contiene `CalculatePacksNeeded(int packSize = 50)` y `ShipsTo(string? targetCountry)`.
  - **Oportunidades de mejora:**
    - Agregar propiedades para cálculo dual explícito (paquetes de 50 y paquetes de 100).
    - Métodos auxiliares `PacksNeeded50` y `PacksNeeded100` o permitir `StandardPackSize` configurable.
    - Soporte para asociar el formato canónico estándar (`StandardSleeveFormat`).
- **`Game` (`src/Ludeka.Core/Entities/Game.cs`):**
  - Dispone de `public List<SleeveItem> Sleeves { get; private set; } = [];`.
  - Se inicializa en el constructor si se suministran fundas, pero **carece de métodos de mutación pública** (`UpdateSleeves`, `AddSleeve`, `ClearSleeves`).
  - Necesario incorporar `UpdateSleeves(IEnumerable<SleeveItem> sleeves)` para que el editor de moderadores pueda persistir cambios.

### 2.2 Persistencia (`Ludeka.Infrastructure`)
- **`LudekaDbContext` (`src/Ludeka.Infrastructure/Data/LudekaDbContext.cs`):**
  - Mapea `game.OwnsMany(g => g.Sleeves, b => b.ToJson());`.
  - SQLite almacena la colección como un array JSON en la columna `Sleeves`, lo que permite mutar la estructura del objeto sin necesidad de migraciones destructivas de tabla relacional.
- **`seed-games.json` y `BggSimulationDataset.cs`:**
  - Ya incluyen datos predefinidos de fundas para juegos como Wingspan, Dune Imperium, 7 Wonders Duel, etc.
  - Sin embargo, las URLs de compra en muchos casos son nulas o genéricas.

### 2.3 Ingesta BGG (`Ludeka.Infrastructure.Bgg`)
- **`BggXmlParser.cs`:**
  - Actualmente parsea nombres, diseñador, editorial, año, estadísticas, ratings, escalabilidad y mecánicas, pero **ignora los elementos `<link type="boardgamecardsleeve" ...>`** devueltos por BGG XMLAPI2.
  - En BGG XMLAPI2, las fundas vienen tipadas como `boardgamecardsleeve` con nombres y dimensiones (ej. `Standard USA: 56 x 87 mm` o `Mini European: 44 x 68 mm`).
  - Es necesario implementar un parser especializado (`BggSleeveParser` o extensión en `BggXmlParser`) capaz de extraer el ancho, alto y nombre de formato desde los enlaces de BGG y mapearlos a `SleeveItem`.

### 2.4 Servicios de Aplicación (`Ludeka.Application`)
- **Falta un resolvedor formal de URLs por tienda (`ISleeveStoreUrlResolver`):**
  - Se requiere un servicio que conozca los patrones de URLs de las tiendas de juegos de mesa colaboradoras (Zacatrus, Dungeon Marvels, Tablerum, etc.).
  - Debe transformar dimensiones `(width, height)` o un `SleeveItem` en una URL de categoría o búsqueda con tag de afiliado.
  - Debe integrarse con `IUserLocationService` (INC-29) para respetar el filtrado por país del usuario.
- **Editor Editorial (`IGameEditorService` / `GameEditorService`):**
  - `UpdateGameDetailsCommand` no acepta `Sleeves`.
  - Es necesario extender `UpdateGameDetailsCommand` para admitir `IReadOnlyList<SleeveItem>? Sleeves` y registrar en auditoría (`AuditLog`) los cambios realizados sobre las fundas.

### 2.5 Interfaz de Usuario Blazor (`Ludeka.Web`)
- **`SleeveGuideCard.razor`:**
  - Existe una versión preliminar básica.
  - Debe evolucionar hacia el componente editorial de alta gama "Protege tu juego":
    - Silueta gráfica vectorial de la carta con sus proporciones relativas.
    - Badges claros con las dimensiones exactas (`56 x 87 mm`).
    - Desglose de paquetes necesarios (ej. "110 cartas → 3 paquetes de 50 o 2 de 100").
    - Píldora didáctica comparativa de micras (Standard 50-60 µm vs. Premium 100 µm).
    - Botón de compra contextual dinámico por tienda utilizando `ISleeveStoreUrlResolver`.
    - Respeto estricto del país efectivo del usuario (`UserLocationService`).
- **`GameEditorModal.razor`:**
  - Actualmente tiene pestañas de Metadatos, Mesa, Sinopsis y Carátula.
  - Requiere una nueva pestaña: **🛡️ Fundas**, permitiendo a moderadores:
    - Ver las fundas actuales.
    - Añadir nuevos formatos con presets comunes (Mini Euro, Standard USA, Magic, Tarot, etc.).
    - Ajustar ancho, alto y número de cartas.
    - Eliminar o modificar entradas erróneas.

---

## 3. Matriz de Catálogo de Medidas Estándar del Hobby
Para garantizar una experiencia sólida incluso cuando BGG no detalla el nombre comercial, crearemos un catálogo maestro de formatos de fundas (`StandardSleeveCatalog`):

| Nombre Formato | Dimensiones (mm) | Tolerancia | Juegos Emblemáticos |
|---|---|---|---|
| **Mini USA** | 41 x 63 mm | ± 1 mm | Arkham Horror, Twilight Imperium, Star Wars X-Wing |
| **Mini Euro** | 44 x 68 mm | ± 1 mm | 7 Wonders Duel, Catan, Ticket to Ride, Scythe |
| **Estándar USA** | 56 x 87 mm | ± 1.5 mm | Carcassonne, Ticket to Ride (USA), Bang! |
| **Chimera / USA** | 57 x 89 mm | ± 1.5 mm | Wingspan, Ark Nova, Concordia |
| **Euro Standard** | 59 x 92 mm | ± 1.5 mm | Dominion, Agricola, Puerto Rico |
| **Standard Card Game** | 63.5 x 88 mm | ± 1.5 mm | Terraforming Mars, Magic, Pokémon, Heat, Dune Imperium |
| **Tarot / 7 Wonders** | 65 x 100 mm | ± 2 mm | 7 Wonders, Coup, 7 Wonders Duel (Wonder cards) |
| **Tarot Grande** | 70 x 120 mm | ± 2 mm | Century Spice Road, Dixit (antiguo), Eldritch Horror |
| **Cuadrada Pequeña** | 70 x 70 mm | ± 2 mm | Codenames, Power Grid |
| **Magnum / Dixit** | 80 x 120 mm | ± 2 mm | Dixit, Mysterium, Stella |

---

## 4. Conclusión de la Exploración
La arquitectura existente en Ludeka ofrece una base limpia (EF Core con Json Columns, Value Object `SleeveItem` y filtrado geográfico por país de INC-29). 
El Incremento 26 conectará de manera definitiva la dimensión técnica (medidas BGG y presets), la dimensión comercial (enlaces de afiliados quirúrgicos por tienda y país) y la gobernanza editorial (edición manual por moderadores con auditoría).
