# Propuesta: change-26-card-sleeves-spec-stores (Incremento 26: Especificación de Fundas por Juego y Enlaces de Compra Contextuales)

## 1. Resumen Ejecutivo y Motivación

Uno de los mayores dolores de cabeza de los aficionados a los juegos de mesa al adquirir un título nuevo es:  
> *"¿Qué fundas necesito comprar exactamente para no quedarme corto, no equivocarme de milímetros y dónde las compro al mejor precio?"*

El **Incremento 26** introduce el módulo completo **"Protege tu juego: Guía Editorial de Fundas"** en Ludeka:
1. **Precisión Milimétrica y Didáctica:** Presenta de un vistazo las cartas del juego con dimensiones exactas en mm (`56 x 87 mm`), silueta visual de la carta proporcional en SVG/CSS, recuento total y paquetes recomendados (de 50 y de 100 unidades).
2. **Guía de Calidad y Grosor:** Introduce una comparativa clara entre fundas *Standard* (50-60 micras) y *Premium* (100 micras), orientando sobre barajeo e inserción en la cuna original.
3. **Enlaces Quirúrgicos a Tiendas Colaboradoras:** Mediante un nuevo motor de resolución contextual (`ISleeveStoreUrlResolver`), el botón de compra no envía a la página principal de la tienda, sino directamente a los resultados o categoría del tamaño exacto en tiendas como **Zacatrus**, **Dungeon Marvels** o tiendas locales del país seleccionado por el usuario (INC-29).
4. **Gobernanza Editorial:** Panel de edición en el modal editorial (`GameEditorModal`) para que los moderadores puedan ingresar, ajustar o corregir fundas con presets rápidos de formatos estándar y registro en la bitácora universal de auditoría (`AuditLog`).
5. **Ingesta Automática BGG:** Parser de enlaces `boardgamecardsleeve` para extraer fundas desde la API XML de BoardGameGeek.

---

## 2. Alcance por Capas del Sistema

### 2.1 Dominio (`Ludeka.Core`)
- **`SleeveItem`:**
  - Enriquecer el Value Object con cálculo dual de paquetes:
    - `CalculatePacksNeeded(int packSize = 50)`
    - Propiedades calculadas: `PacksNeeded50 => CalculatePacksNeeded(50)` y `PacksNeeded100 => CalculatePacksNeeded(100)`.
  - Mantener compatibilidad total con `FormatName`, `WidthMm`, `HeightMm`, `CardCount`, `AffiliateUrl`, `StoreName`, `Country`, `ShippingCountries`.
- **`StandardSleeveFormat` y `StandardSleeveCatalog`:**
  - Catálogo estático in-memory con los 10 formatos estándar del hobby (Mini USA, Mini Euro, USA Standard, Chimera, Euro Standard, Standard Card Game / Magic, Tarot, Tarot Grande, Cuadrada, Magnum).
  - Método `Match(double width, double height)` para vincular automáticamente cualquier medida a su nombre canónico y tolerancia.
- **`Game`:**
  - Incorporar métodos de dominio:
    - `public void UpdateSleeves(IEnumerable<SleeveItem> sleeves)`
    - `public void AddSleeve(SleeveItem sleeve)`
    - `public void ClearSleeves()`

### 2.2 Aplicación (`Ludeka.Application`)
- **`ISleeveStoreUrlResolver` / `SleeveStoreUrlResolver`:**
  - Resuelve URLs directas a la categoría o filtro de medida de cada tienda socia (ej. Zacatrus `https://zacatrus.es/accesorios/fundas.html?tamano=...&ref=ludeka`).
  - Provee `IReadOnlyList<SleevePurchaseOptionDto> ResolvePurchaseOptions(SleeveItem sleeve, string? userCountry)`.
  - Respeta el país del usuario (`IUserLocationService`) para excluir tiendas que no envían al territorio activo.
- **`IGameEditorService` & `UpdateGameDetailsCommand`:**
  - Agregar parámetro opcional `IReadOnlyList<SleeveItem>? Sleeves = null` a `UpdateGameDetailsCommand`.
  - Registrar en `GameEditorService` las modificaciones en el resumen de auditoría y generar `FieldChangeDto` para `AuditLog`.

### 2.3 Infraestructura (`Ludeka.Infrastructure`)
- **`BggSleeveParser` & Integración en `BggXmlParser`:**
  - Extraer elementos `<link type="boardgamecardsleeve" ...>` desde el XML de BGG.
  - Expresión regular para detectar dimensiones (`(\d+(?:\.\d+)?)\s*[xX*×]\s*(\d+(?:\.\d+)?)\s*mm`).
  - Si BGG no provee el conteo de cartas, inferirlo o permitir que el moderador lo complete.
- **Actualización de Dataset Simulado (`BggSimulationDataset`):**
  - Asegurar que los títulos representativos (7 Wonders Duel, Terraforming Mars, Wingspan, Dune Imperium, etc.) cuenten con especificaciones completas de fundas con enlaces contextuales válidos.

### 2.4 Presentación Blazor (`Ludeka.Web`)
- **Evolución de `SleeveGuideCard.razor`:**
  - Silueta visual de carta con proporción CSS dinámica `aspect-[width/height]`.
  - Badges con formato, dimensiones milimétricas y recuento de cartas.
  - Indicador didáctico de paquetes requeridos (packs de 50 vs. packs de 100).
  - Pestaña / acordeón explicativo "Grosor de fundas: 50µ vs 100µ".
  - Botones de compra por tienda (Zacatrus, etc.) con flags de país y tag de afiliado.
  - Estado vacío claro para juegos sin cartas ("¡Buenas noticias! Este juego no contiene cartas enfundables").
- **Ampliación de `GameEditorModal.razor`:**
  - Pestaña editorial **"🛡️ Fundas"** con lista reactiva de fundas.
  - Selector de presets (`StandardSleeveCatalog`) que rellena nombre, ancho y alto en 1 clic.
  - Validación de campos (ancho > 0, alto > 0, cantidad > 0).

---

## 3. Matriz de Compatibilidad y Cero Regresiones
- SQLite ya utiliza `OwnsMany(g => g.Sleeves, b => b.ToJson())`. No se requieren migraciones destructivas ni cambios de esquema en la base de datos.
- Los 609 tests existentes de la suite continuarán pasando al 100%.
- Se crearán nuevas pruebas unitarias exhaustivas en `Ludeka.UnitTests` cubriendo todas las nuevas clases y métodos.
