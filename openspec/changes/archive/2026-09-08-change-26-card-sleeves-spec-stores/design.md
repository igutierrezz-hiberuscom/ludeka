# Documento de Diseño: change-26-card-sleeves-spec-stores (Incremento 26)

## 1. Visión Arquitectónica y Filosofía Técnica

El módulo de fundas y enlaces contextuales se diseña sobre Clean Architecture / Vertical Slices:
- **`Ludeka.Core`:** Entidades y Value Objects puros, sin acoplamiento a frameworks. `SleeveItem` enriquecido con cálculo dual de paquetes, y `StandardSleeveCatalog` como catálogo agnóstico de formatos reconocidos en la industria.
- **`Ludeka.Application`:** Interfaz `ISleeveStoreUrlResolver` y su implementación predeterminada `SleeveStoreUrlResolver`, que mapea dimensiones exactas a los parámetros de URL de tiendas socias (Zacatrus, etc.) con tags de afiliado y respeto del país del usuario (`IUserLocationService`). Extensión de `UpdateGameDetailsCommand` para persistencia editorial.
- **`Ludeka.Infrastructure`:** Extracción de enlaces `boardgamecardsleeve` en `BggXmlParser` / `BggSleeveParser` y actualización de datos simulados en `BggSimulationDataset`.
- **`Ludeka.Web`:** Componente Blazor interactivo `SleeveGuideCard.razor` (silueta proporcional, comparativa de micras, packs calculados y botones de compra directa) y nueva pestaña "🛡️ Fundas" en `GameEditorModal.razor`.

---

## 2. Modelos de Dominio (`Ludeka.Core`)

### 2.1 Enriquecimiento de `SleeveItem` (`Ludeka.Core.ValueObjects`)
```csharp
public record SleeveItem(
    string FormatName,
    double WidthMm,
    double HeightMm,
    int CardCount,
    string? AffiliateUrl = null,
    string? StoreName = null,
    string? Country = null,
    IReadOnlyList<string>? ShippingCountries = null)
{
    public int CalculatePacksNeeded(int packSize = 50)
    {
        if (packSize <= 0) throw new ArgumentOutOfRangeException(nameof(packSize), "El tamaño de paquete debe ser mayor a 0.");
        if (CardCount <= 0) return 0;
        return (int)Math.Ceiling((double)CardCount / packSize);
    }

    public int PacksNeeded50 => CalculatePacksNeeded(50);
    public int PacksNeeded100 => CalculatePacksNeeded(100);

    public string DimensionText => $"{WidthMm:0.#} x {HeightMm:0.#} mm";

    public bool ShipsTo(string? targetCountry)
    {
        // Lógica existente de CountryCatalog
        ...
    }
}
```

### 2.2 Catálogo Maestro de Formatos: `StandardSleeveCatalog` (`Ludeka.Core.ValueObjects`)
```csharp
public record StandardSleeveFormat(
    string Name,
    double WidthMm,
    double HeightMm,
    double ToleranceMm = 1.5,
    string? Description = null,
    string? PopularGamesExample = null);

public static class StandardSleeveCatalog
{
    public static readonly IReadOnlyList<StandardSleeveFormat> Formats =
    [
        new("Mini USA", 41, 63, 1.0, "Tamaño compacto habitual en cartas de objetos y daño", "Arkham Horror, Twilight Imperium"),
        new("Mini Euro", 44, 68, 1.0, "Formato europeo pequeño muy extendido", "7 Wonders Duel, Catan, Scythe"),
        new("Estándar USA", 56, 87, 1.5, "Estándar americano clásico", "Bang!, Ticket to Ride USA"),
        new("Chimera / USA", 57, 89, 1.5, "Formato Chimera estándar", "Wingspan, Ark Nova, Concordia"),
        new("Euro Standard", 59, 92, 1.5, "Formato europeo tradicional", "Dominion, Agricola, Puerto Rico"),
        new("Standard Card Game", 63.5, 88, 1.5, "El tamaño más popular del mundo (CCG / LCG)", "Terraforming Mars, Dune Imperium, Magic"),
        new("Tarot / 7 Wonders", 65, 100, 2.0, "Cartas grandes de maravillas o roles", "7 Wonders, 7 Wonders Duel, Coup"),
        new("Tarot Grande", 70, 120, 2.0, "Cartas extra grandes de tarot", "Century, Eldritch Horror"),
        new("Cuadrada", 70, 70, 2.0, "Formato cuadrado para losetas y pistas", "Código Secreto, Power Grid"),
        new("Magnum / Dixit", 80, 120, 2.0, "Formato gigante para cartas ilustradas", "Dixit, Mysterium, Stella")
    ];

    public static StandardSleeveFormat? Match(double widthMm, double heightMm)
    {
        return Formats.FirstOrDefault(f =>
            Math.Abs(f.WidthMm - widthMm) <= f.ToleranceMm &&
            Math.Abs(f.HeightMm - heightMm) <= f.ToleranceMm);
    }
}
```

### 2.3 Métodos de Dominio en `Game` (`Ludeka.Core.Entities`)
```csharp
public void UpdateSleeves(IEnumerable<SleeveItem> sleeves)
{
    ArgumentNullException.ThrowIfNull(sleeves);
    Sleeves.Clear();
    Sleeves.AddRange(sleeves);
}

public void AddSleeve(SleeveItem sleeve)
{
    ArgumentNullException.ThrowIfNull(sleeve);
    Sleeves.Add(sleeve);
}

public void ClearSleeves()
{
    Sleeves.Clear();
}
```

---

## 3. Servicios de Aplicación (`Ludeka.Application`)

### 3.1 Contrato `ISleeveStoreUrlResolver` y DTOs
```csharp
public record SleevePurchaseOptionDto(
    string StoreName,
    string StoreLogoUrl,
    string PurchaseUrl,
    string Country,
    string? FormattedPrice = null,
    string? Badge = null,
    bool IsDirectPartner = true
);

public interface ISleeveStoreUrlResolver
{
    string ResolveStoreUrl(string storeName, double widthMm, double heightMm, string? affiliateCode = null);
    IReadOnlyList<SleevePurchaseOptionDto> ResolvePurchaseOptions(SleeveItem sleeve, string? userCountry = null);
    StandardSleeveFormat? MatchStandardFormat(double widthMm, double heightMm);
}
```

### 3.2 Implementación `SleeveStoreUrlResolver`
- **Zacatrus:** Genera `https://zacatrus.es/fundas.html?tamano={width}x{height}&ref=ludeka` o `https://zacatrus.es/catalogsearch/result/?q=fundas+{width}x{height}&ref=ludeka`.
- **Dungeon Marvels:** Genera `https://dungeonmarvels.com/fundas?tamano={width}x{height}&ref=ludeka`.
- **Cuarto de Juegos:** Genera `https://cuartodejuegos.es/fundas?tamano={width}x{height}&ref=ludeka`.
- Si el `SleeveItem` ya posee un `AffiliateUrl` explícito, se prioriza o combina como opción destacada.
- Valida con `CountryCatalog` el filtro de país para no presentar opciones inviables.

### 3.3 Extensión de `UpdateGameDetailsCommand` y `GameEditorService`
- Añadir a `UpdateGameDetailsCommand`:
  `IReadOnlyList<SleeveItem>? Sleeves = null`
- En `GameEditorService.UpdateGameAsync`:
  - Si `command.Sleeves != null`:
    - `game.UpdateSleeves(command.Sleeves);`
    - Registrar en `fieldChanges` el cambio de fundas.
    - Actualizar el resumen: `"Fundas de cartas actualizadas (N formatos)"`.

---

## 4. Infraestructura e Ingesta (`Ludeka.Infrastructure`)

### 4.1 Parser de Fundas BGG (`BggSleeveParser`)
- Analiza elementos `<link type="boardgamecardsleeve" id="..." value="..." />` de BGG XMLAPI2.
- Patrones de expresión regular:
  - Detecta dimensiones: `(?<w>\d+(?:\.\d+)?)\s*[xX*×]\s*(?<h>\d+(?:\.\d+)?)\s*mm`
  - Detecta cantidad si viene en texto: `(?<qty>\d+)\s*(?:cards|cartas|uds|ct)`
  - Cruza con `StandardSleeveCatalog.Match(w, h)` para normalizar el `FormatName`.

---

## 5. Diseño de Componentes UI Blazor (`Ludeka.Web`)

### 5.1 `SleeveGuideCard.razor` ("Protege tu juego: Fundas")
- **Diseño Visual Editorial:**
  - Cabecera con icono de escudo 🛡️ y título "Protege tu juego: Guía de Fundas".
  - Por cada formato de carta:
    - **Silueta gráfica de carta:** Contenedor CSS con esquinas redondeadas y relación de aspecto proporcional, mostrando el contorno de la carta y las medidas `56 x 87 mm`.
    - **Métricas:** Recuento total de cartas (`CardCount`), paquetes necesarios de 50 fundas (`PacksNeeded50`) y de 100 fundas (`PacksNeeded100`).
    - **Comparativa Didáctica de Grosor (Micras):** Tooltip o píldora interactiva explicando:
      - *Standard (50-60 µm):* Ajuste perfecto en insertos, tacto flexible, menor coste.
      - *Premium (100 µm):* Máxima protección rígida, mayor grosor total de mazo.
    - **Botón de Compra Directo a Tienda:**
      - Generado mediante `ISleeveStoreUrlResolver`.
      - Badge de tienda (ej. "Zacatrus • Envío 24h").
      - Indicador de país si aplica.
  - **Estado Vacío Amigable:** Cuando `Sleeves == null || Sleeves.Count == 0`, mensaje destacado indicando que el juego no requiere fundas.

### 5.2 Pestaña "🛡️ Fundas" en `GameEditorModal.razor`
- Lista reactiva de fundas actuales con botones de editar y eliminar.
- Formulario para añadir nueva funda:
  - Selector de Presets Rápidos: Menú desplegable con los 10 formatos estándar de `StandardSleeveCatalog`. Al seleccionar uno, rellena automáticamente el nombre, ancho y alto en mm.
  - Campos manuales de nombre, ancho (mm), alto (mm) y cantidad de cartas.
  - Botón `[ + Añadir Formato de Funda ]`.
  - Los cambios se acumulan en memoria local del modal y se persisten atómicamente al pulsar "Guardar Cambios".
