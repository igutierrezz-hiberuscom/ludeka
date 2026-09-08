# 19. Especificación de Fundas (Sleeves) por Juego y Enlaces de Compra Contextuales

> **Estado del Módulo:** ✅ Implementado y Verificado  
> **Incremento Asociado:** [INC-26 (inc-26-card-sleeves-spec-stores.md)](file:///c:/repos/Ludeka/docs/increments/archive/inc-26-card-sleeves-spec-stores.md)  
> **Pruebas Automatizadas:** 21 pruebas dedicadas  

---

## 1. Propósito y Filosofía
El módulo **"Protege tu juego: Guía Editorial de Fundas"** resuelve de forma didáctica, precisa y no invasiva una de las preguntas esenciales de todo aficionado al adquirir un nuevo juego de mesa:  
*¿Qué medidas exactas tienen sus cartas, cuántos paquetes de fundas necesito comprar y dónde puedo adquirirlas directamente sin búsquedas tediosas?*

El sistema proporciona:
1. Dimensiones exactas en milímetros (`ancho x alto mm`) y recuento total de cartas por formato.
2. Cálculo automático de paquetes requeridos en presentaciones universales de 50 y 100 fundas.
3. Silueta gráfica proporcional de la carta en SVG/CSS con esquinas redondeadas.
4. Píldora didáctica comparativa de micras (Standard 50-60 µm para insertos originales vs. Premium 100 µm para máxima durabilidad).
5. Enlaces de compra contextuales y quirúrgicos a tiendas colaboradoras (Zacatrus, Dungeon Marvels, etc.) con inyección de tags de afiliado y respeto del país del usuario (`CountryCatalog` / `IUserLocationService` de INC-29).
6. Ingesta automática desde BGG XMLAPI2 (`boardgamecardsleeve`) y catálogo maestro in-memory (`StandardSleeveCatalog`) con los 10 formatos universales del hobby.
7. Pestaña de edición y moderación editorial en `GameEditorModal.razor` con presets rápidos y registro en la bitácora universal de auditoría (`AuditLog`).

---

## 2. Componentes de Dominio (`Ludeka.Core`)

### 2.1 Value Object `SleeveItem`
Ubicación: [`src/Ludeka.Core/ValueObjects/SleeveItem.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/SleeveItem.cs)

```csharp
public record SleeveItem(
    string FormatName,
    double WidthMm,
    double HeightMm,
    int CardCount,
    string? AffiliateUrl,
    string? StoreName = null,
    string? Country = null,
    IReadOnlyList<string>? ShippingCountries = null)
{
    public int CalculatePacksNeeded(int packSize = 50) => ...;
    public int PacksNeeded50 => CalculatePacksNeeded(50);
    public int PacksNeeded100 => CalculatePacksNeeded(100);
    public string DimensionText => $"{WidthMm.ToString("0.#", CultureInfo.InvariantCulture)} x {HeightMm.ToString("0.#", CultureInfo.InvariantCulture)} mm";
    public bool ShipsTo(string? targetCountry) => ...;
}
```

### 2.2 Catálogo Maestro `StandardSleeveCatalog`
Ubicación: [`src/Ludeka.Core/ValueObjects/StandardSleeveCatalog.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/StandardSleeveCatalog.cs)

Define los 10 formatos canónicos del mercado:
- **Mini USA:** 41 x 63 mm
- **Mini Euro:** 44 x 68 mm
- **Estándar USA:** 56 x 87 mm
- **Chimera / USA:** 57 x 89 mm
- **Euro Standard:** 59 x 92 mm
- **Standard Card Game (CCG/LCG):** 63.5 x 88 mm
- **Tarot / 7 Wonders:** 65 x 100 mm
- **Tarot Grande:** 70 x 120 mm
- **Cuadrada:** 70 x 70 mm
- **Magnum / Dixit:** 80 x 120 mm

Métodos:
- `Match(double widthMm, double heightMm)`: Coincidencia dentro de la tolerancia milimétrica (1.0 a 2.0 mm).
- `FindByName(string name)`: Localización por texto parcial o insensible a mayúsculas.

### 2.3 Métodos de Mutación en `Game`
Ubicación: [`src/Ludeka.Core/Entities/Game.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/Game.cs)

- `UpdateSleeves(IEnumerable<SleeveItem> sleeves)`
- `AddSleeve(SleeveItem sleeve)`
- `ClearSleeves()`

---

## 3. Capa de Aplicación (`Ludeka.Application`)

### 3.1 Contrato e Implementación `ISleeveStoreUrlResolver`
Ubicaciones:
- [`src/Ludeka.Application/Contracts/ISleeveStoreUrlResolver.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/ISleeveStoreUrlResolver.cs)
- [`src/Ludeka.Application/Features/Sleeves/SleeveStoreUrlResolver.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Sleeves/SleeveStoreUrlResolver.cs)

- Mapea dimensiones y tiendas colaboradoras:
  - **Zacatrus:** `https://zacatrus.es/catalogsearch/result/?q=fundas+{w}x{h}&ref={tag}`
  - **Dungeon Marvels:** `https://dungeonmarvels.com/buscar?controller=search&s=fundas+{w}x{h}&ref={tag}`
- Integra la comprobación de envíos territoriales para excluir tiendas incompatibles con el país del usuario.

### 3.2 Extensión de `GameEditorService` y `UpdateGameDetailsCommand`
Ubicaciones:
- [`src/Ludeka.Application/DTOs/GameEditorDtos.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/DTOs/GameEditorDtos.cs)
- [`src/Ludeka.Application/Features/Catalog/GameEditorService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Catalog/GameEditorService.cs)

- `UpdateGameDetailsCommand` admite `IReadOnlyList<SleeveItem>? Sleeves = null`.
- `GameEditorService` aplica `game.UpdateSleeves` y emite `FieldChangeDto("Sleeves", ...)` hacia `GameEditLog` y `AuditLog`.

---

## 4. Ingesta e Infraestructura (`Ludeka.Infrastructure`)

### 4.1 Parser de Fundas BGG (`BggSleeveParser`)
Ubicación: [`src/Ludeka.Infrastructure/Bgg/BggSleeveParser.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Bgg/BggSleeveParser.cs)

- Extrae elementos `<link type="boardgamecardsleeve" ...>` desde el XML de BGG.
- Aplica expresiones regulares para recuperar ancho, alto y cantidades.
- Cruza automáticamente con `StandardSleeveCatalog` para asignar el nombre de formato estándar.

---

## 5. Componentes de Interfaz de Usuario Blazor (`Ludeka.Web`)

### 5.1 Ficha Pública: `SleeveGuideCard.razor`
Ubicación: [`src/Ludeka.Web/Components/Shared/SleeveGuideCard.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/SleeveGuideCard.razor)

- Silueta gráfica en CSS/SVG con esquinas redondeadas y relación de aspecto proporcional.
- Badges con formato, medidas en mm, recuento de cartas y cálculo dual de packs (50 y 100 fundas).
- Píldora desplegable explicativa sobre el grosor en micras (Standard vs Premium).
- Botones directos con tag de afiliado hacia Zacatrus y tiendas asociadas respetando el país activo.
- Mensaje amigable cuando el juego no contiene cartas enfundables.

### 5.2 Modal Editorial: `GameEditorModal.razor`
Ubicación: [`src/Ludeka.Web/Components/Shared/GameEditorModal.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/GameEditorModal.razor)

- Pestaña **"🛡️ Fundas"** con listado interactivo.
- Selector de presets rápidos de formatos estándar.
- Edición, adición y eliminación con validación en tiempo real.
