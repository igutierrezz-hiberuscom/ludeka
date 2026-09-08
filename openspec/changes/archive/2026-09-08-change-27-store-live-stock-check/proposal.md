# Propuesta: change-27-store-live-stock-check (Incremento 27: Monitorización y Verificación de Stock en Tiempo Real en Enlaces de Compra)

## 1. Resumen Ejecutivo y Motivación

Uno de los principales factores de fricción para los aficionados al consultar comparativas de precios o enlaces de compra es acceder a una tienda colaboradora solo para encontrarse con que el juego está **agotado**, en **reimpresión indeterminada** o **descatalogado**.

El **Incremento 27** introduce en Ludeka una solución integral de verificación de disponibilidad y stock en tiempo real con una filosofía estricta de **cero penalización en el rendimiento web**:
1. **Latencia Cero en SSR:** La ficha del juego (`GameDetail.razor`) se sirve de forma instantánea sin esperar a tiendas externas.
2. **Carga Diferida y Reactiva:** `StoreOffersCard.razor` solicita de manera no bloqueante la disponibilidad en vivo una vez montada la vista, mostrando un estado transicional sutil (*shimmer*) que muta limpiamente al badge de disponibilidad final.
3. **Taxonomía de Stock Realista:** Pasa del simple booleano a cuatro estados: `InStock` (🟢 En stock), `LowStock` (🟡 Últimas unidades), `OutOfStock` (🔴 Agotado) y `Unknown` (⚪ Verificar en tienda).
4. **Caché Distribuida L1 con TTL:** Consultas cacheadas en memoria durante 30-60 minutos para minimizar peticiones redundantes y no sobrecargar a tiendas asociadas.
5. **Resiliencia Extrema y Timeout de 1.5s:** Cancelación limpia e irrevocable ante demoras superiores a 1.500 ms o fallos HTTP (404/500/red), degradando a `Unknown` sin excepciones.
6. **Transparencia y Anti-Frustración:** Cuando una oferta está agotada, el botón de compra se atenúa visualmente indicando con honestidad "Agotado en tienda / Ver disponibilidad".

---

## 2. Alcance por Capas del Sistema

### 2.1 Dominio (`Ludeka.Core`)
- **`StockStatus`:**
  - Enum: `Unknown = 0`, `InStock = 1`, `LowStock = 2`, `OutOfStock = 3`.
- **`StoreStockInfo`:**
  - Value Object inmutable que encapsula:
    - `StockStatus Status`
    - `int? AvailableQuantity`
    - `decimal? CurrentPrice`
    - `DateTime? LastCheckedUtc`
    - `string? StatusNote`
    - `bool IsChecking`
    - Propiedades auxiliares: `IsInStock`, `IsLowStock`, `IsOutOfStock`, `IsUnknown`, `GetRelativeTimeText(DateTime now)`.
- **`GamePurchaseLink`:**
  - Mantiene retrocompatibilidad íntegra. Incorpora propiedad calculada o método auxiliar para mapear `InStock` a `StockStatus`.

### 2.2 Aplicación (`Ludeka.Application`)
- **`IStoreStockClient`:**
  - `bool CanHandle(string storeName, string affiliateUrl);`
  - `Task<StoreStockInfo> CheckStockAsync(string storeName, string affiliateUrl, CancellationToken cancellationToken = default);`
- **`IStoreStockService` & `StoreStockService`:**
  - `ValueTask<StoreStockInfo> GetStockAsync(string storeName, string affiliateUrl, CancellationToken cancellationToken = default);`
  - `ValueTask<IReadOnlyDictionary<string, StoreStockInfo>> GetStockBatchAsync(IEnumerable<GamePurchaseLink> offers, CancellationToken cancellationToken = default);`
  - `void InvalidateStockCache(string affiliateUrl);`
  - Gestiona `IMemoryCache`, aplica el timeout de 1.5s por tienda con `CancellationTokenSource`, y orquesta la degradación elegante ante excepciones.
- **`StoreStockOptions`:**
  - Configuración inyectable: `DefaultTtlMinutes` (30), `ErrorTtlMinutes` (5), `TimeoutMilliseconds` (1500), `EnableLiveChecking` (true), `EnableSimulation` (false/true).

### 2.3 Infraestructura (`Ludeka.Infrastructure`)
- **`HtmlSchemaStoreStockClient`:**
  - Cliente HTTP resiliente con `User-Agent` adecuado.
  - Parser de microdatos:
    - Schema.org JSON-LD / Microdata: `ItemAvailability` (`InStock`, `OutOfStock`, `PreOrder`, `LimitedAvailability`).
    - OpenGraph: `og:availability` y `product:availability`.
    - Patrones de contenido en español para tiendas de juegos de mesa conocidas (Zacatrus, Cuarto de Juegos, Dungeon Marvels, etc.).
- **`SimulationStoreStockClient`:**
  - Cliente determinista para entornos de desarrollo y pruebas automatizadas (simula respuestas en stock, pocas unidades, agotado o errores controlados).

### 2.4 Presentación Blazor (`Ludeka.Web`)
- **`StoreOffersCard.razor`:**
  - Inyección de `IStoreStockService`.
  - Carga diferida en segundo plano iniciada tras el primer render (`OnAfterRenderAsync`).
  - Renderizado reactivo con micro-badges:
    - 🟢 "En stock" / "En stock (X uds)"
    - 🟡 "Últimas unidades"
    - 🔴 "Agotado en tienda"
    - ⚪ "Verificar en web"
  - Tratamiento visual de ofertas agotadas: botón atenuado/diferenciado ("Agotado / Ver en tienda").
  - Tiempo de última comprobación relativo ("Comprobado hace X min" / "En vivo").
  - Botón de refresco manual opcional por tienda o global ("🔄 Actualizar").
- **`Program.cs`:**
  - Registro de `StoreStockOptions`, `IStoreStockClient` (simulado y real) y `IStoreStockService`.

---

## 3. Matriz de Compatibilidad y Cero Regresiones
- Cero migraciones de base de datos requeridas (la información en vivo reside en caché de memoria transitoria).
- Cero bloqueos en SSR: la entrega del HTML inicial no añade ningún milisegundo extra de latencia.
- Los 636 tests existentes de la solución continuarán ejecutándose al 100% en verde.
- Se desarrollará una suite exhaustiva de tests unitarios que verifique timeouts, caché, parsing de Schema.org/OG y renderizado de componentes.
