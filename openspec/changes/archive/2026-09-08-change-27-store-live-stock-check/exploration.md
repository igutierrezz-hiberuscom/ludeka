# Exploración Técnica: change-27-store-live-stock-check (Incremento 27)

## 1. Contexto y Diagnóstico del Estado Actual

En el estado actual del sistema:
- **`Ludeka.Core.ValueObjects.GamePurchaseLink`**: Modela los enlaces de compra con datos orientativos (`StoreName`, `Country`, `AffiliateUrl`, `Price`, `Currency`, `InStock`, `Badge`, `AffiliateTag`, `ShippingCountries`). La propiedad `InStock` es un booleano estático persistido que por defecto vale `true`.
- **`Ludeka.Web.Components.Shared.StoreOffersCard.razor`**: Renderiza las ofertas de compra filtradas territorialmente por país (`IUserLocationService`, INC-29). Muestra un semáforo binario simple basado en el booleano estático `offer.InStock` sin ninguna verificación dinámica contra las tiendas.
- **Problema Detectado**: En el comercio minorista de juegos de mesa, las rotaciones de stock son muy altas (reimpresiones, agotados rápidos, preventas). Cuando un jugador hace clic esperando encontrar el juego disponible y se topa con un cartel de "Agotado" o "Descatalogado", la experiencia de usuario y la credibilidad de Ludeka se degradan severamente.

---

## 2. Requerimientos Clave del Incremento 27

1. **Estado de Disponibilidad Enriquecido (`StockStatus`):**
   - Transición de un simple booleano a una taxonomía clara:
     - `InStock` (🟢 En stock / Disponible de inmediato).
     - `LowStock` (🟡 Últimas unidades / Stock bajo).
     - `OutOfStock` (🔴 Agotado / Sin existencias).
     - `Unknown` (⚪ Comprobar en tienda / Sin respuesta reciente).
2. **Cero Impacto en el Rendimiento de la Ficha (Non-Blocking & Streaming):**
   - La ficha de juego (`GameDetail.razor`) debe servirse en menos de 100ms.
   - El renderizado inicial SSR o primer render interactivo jamás debe bloquearse esperando respuestas de APIs o páginas HTML de terceros.
   - La comprobación se ejecuta de forma asíncrona diferida (tras el primer render o vía background task en el componente Blazor), actualizando reactivamente la UI.
3. **Límite Estricto de Tiempo (Timeout 1.5s):**
   - Cada consulta individual a una tienda tiene un corte irrevocable a los 1.500 ms con `CancellationTokenSource`.
   - Si la tienda tarda más, la UI degrada inmediatamente a `StockStatus.Unknown` ("Verificar en web") sin congelar el hilo ni lanzar excepciones.
4. **Capa de Caché Inteligente con TTL Dinámico (`IStoreStockService`):**
   - Uso de `IMemoryCache` con TTL configurable (por defecto 30 minutos para éxitos, 5 minutos para fallos/timeouts).
   - Cache key unívoca por tienda y URL de oferta.
   - Si múltiples usuarios abren la misma ficha, solo se efectúa una consulta HTTP real.
5. **Estrategia Multi-Partner y Resiliencia (`IStoreStockClient`):**
   - Parser de microdatos y OpenGraph (`schema.org/ItemAvailability`, `og:availability`, patrones de texto en español: "En stock", "Agotado", "Últimas unidades").
   - Cliente simulado (`SimulationStoreStockClient`) configurable para entornos de desarrollo y pruebas automatizadas fiables sin fragilidad de scraping.
   - Degradación elegante total: ante errores 404, 500, bloqueos o caídas de tiendas asociadas, la experiencia de usuario se mantiene intacta.
6. **UI/UX Editorial y Transparencia:**
   - Badges visuales claros con contraste y tipografía nítida.
   - Para ofertas agotadas: atenuación visual del botón ("Agotado en tienda / Ver disponibilidad") para evitar compras frustradas.
   - Indicador de última comprobación ("Comprobado hace X min" / "En vivo").
   - Botón de refresco manual ("🔄 Actualizar stock").

---

## 3. Comparativa de Opciones Arquitectónicas

| Criterio | Opción A: Consulta síncrona en SSR `OnInitializedAsync` | Opción B: Job nocturno periódico masivo en base de datos | Opción C: Carga diferida en cliente Blazor + Caché en memoria + Fallback (Recomendada) |
|---|---|---|---|
| **Velocidad de carga inicial** | 🔴 Inaceptable (añadiría 1.5s - 5s de latencia a la ficha) | 🟢 Rápida (<50ms al leer de BD) | 🟢 Instantánea (<100ms, streaming rendering) |
| **Frescura del dato** | 🟢 Tiempo real | 🔴 Desfasada (datos de hace 12-24h) | 🟢 Tiempo real (bajo demanda con caché 30 min) |
| **Carga en tiendas asociadas** | 🔴 Altísima si no hay caché | 🟡 Ráfaga periódica de scraping masivo | 🟢 Minimizada por caché L1 con TTL |
| **Resiliencia ante caídas externas** | 🔴 Bloquea el renderizado de la página | 🟢 No afecta el render | 🟢 Degradación transparente a `Unknown` en 1.5s |
| **Complejidad operativa** | 🟢 Baja | 🔴 Alta (colas, workers, almacenamiento en BD) | 🟢 Media-Baja (servicio encapsulado en Application + Infrastructure) |

**Decisión**: Adoptar la **Opción C**.

---

## 4. Archivos y Componentes Impactados

- **`Ludeka.Core`:**
  - `Enums/StockStatus.cs` (Nuevo enum).
  - `ValueObjects/StoreStockInfo.cs` (Nuevo Value Object inmutable con helpers y tiempo relativo).
  - `ValueObjects/GamePurchaseLink.cs` (Compatibilidad asegurada con propiedades existentes).
- **`Ludeka.Application`:**
  - `Contracts/IStoreStockService.cs` (Contrato de servicio de stock y lote).
  - `Contracts/IStoreStockClient.cs` (Contrato para clientes/scrapers de tiendas).
  - `Options/StoreStockOptions.cs` (Opciones de configuración: TTL, timeouts, habilitación de live check y simulación).
  - `Features/Catalog/StoreStockService.cs` (Implementación de la orquestación, caché y resiliencia).
- **`Ludeka.Infrastructure`:**
  - `Stores/HtmlSchemaStoreStockClient.cs` (Cliente HTTP con parser de microdatos Schema.org / OpenGraph).
  - `Stores/SimulationStoreStockClient.cs` (Cliente de simulación determinista para tests y offline dev).
- **`Ludeka.Web`:**
  - `Components/Shared/StoreOffersCard.razor` (Actualización para carga diferida, badges dinámicos, shimmering, atenuación de agotados y timestamp de comprobación).
  - `Program.cs` (Inyección de dependencias de `StoreStockOptions`, `IStoreStockClient` y `IStoreStockService`).
- **`tests/Ludeka.UnitTests`:**
  - Tests unitarios exhaustivos para `StoreStockInfo`, `StoreStockService`, `HtmlSchemaStoreStockClient`, `SimulationStoreStockClient` y renderizado de componentes.
