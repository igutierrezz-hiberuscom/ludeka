# Checklist de Tareas: change-27-store-live-stock-check (Incremento 27)

## Fase 1: Modelos de Dominio (`Ludeka.Core`)
- [x] 1.1 Crear enum `StockStatus` en `Ludeka.Core/Enums/StockStatus.cs` (`Unknown = 0`, `InStock = 1`, `LowStock = 2`, `OutOfStock = 3`).
- [x] 1.2 Crear Value Object `StoreStockInfo` en `Ludeka.Core/ValueObjects/StoreStockInfo.cs` con propiedades `Status`, `AvailableQuantity`, `CurrentPrice`, `LastCheckedUtc`, `StatusNote`, `IsChecking` y métodos helper (`IsInStock`, `IsLowStock`, `IsOutOfStock`, `IsUnknown`, `GetRelativeTimeText`).
- [x] 1.3 Actualizar `GamePurchaseLink` en `Ludeka.Core/ValueObjects/GamePurchaseLink.cs` con método o propiedad helper para mapear a `StockStatus`.
- [x] 1.4 Pruebas unitarias en `tests/Ludeka.UnitTests/Domain/StoreStockDomainTests.cs` validando `StockStatus`, `StoreStockInfo` y helpers temporales.

---

## Fase 2: Casos de Uso y Orquestación (`Ludeka.Application`)
- [x] 2.1 Definir interfaz `IStoreStockClient` en `Ludeka.Application/Contracts/IStoreStockClient.cs`.
- [x] 2.2 Definir interfaz `IStoreStockService` en `Ludeka.Application/Contracts/IStoreStockService.cs`.
- [x] 2.3 Crear clase de opciones `StoreStockOptions` en `Ludeka.Application/Options/StoreStockOptions.cs` (TTL default 30m, TTL error 5m, timeout 1500ms, flags de simulación y live check).
- [x] 2.4 Implementar `StoreStockService` en `Ludeka.Application/Features/Catalog/StoreStockService.cs` con gestión de `IMemoryCache`, timeout de 1.5s vía `CancellationTokenSource`, batching y degradación elegante ante excepciones.
- [x] 2.5 Pruebas unitarias en `tests/Ludeka.UnitTests/Application/StoreStockServiceTests.cs` validando cache hit/miss, expiración, timeouts y gestión de fallos.

---

## Fase 3: Adaptadores de Infraestructura (`Ludeka.Infrastructure`)
- [x] 3.1 Implementar `SimulationStoreStockClient` en `Ludeka.Infrastructure/Stores/SimulationStoreStockClient.cs` para pruebas controladas y desarrollo sin dependencia de red.
- [x] 3.2 Implementar `HtmlSchemaStoreStockClient` en `Ludeka.Infrastructure/Stores/HtmlSchemaStoreStockClient.cs` con `HttpClient`, parsing de Schema.org JSON-LD/Microdata, OpenGraph y heurísticas de tiendas lúdicas.
- [x] 3.3 Registrar servicios en el contenedor de dependencias (`Program.cs`): `StoreStockOptions`, `IStoreStockClient` y `IStoreStockService`.
- [x] 3.4 Pruebas unitarias en `tests/Ludeka.UnitTests/Infrastructure/StoreStockClientTests.cs` evaluando parseo de HTML Schema.org, OpenGraph y cliente simulado.

---

## Fase 4: Componentes Visuales Blazor (`Ludeka.Web`)
- [x] 4.1 Actualizar `StoreOffersCard.razor`:
  - Inyectar `IStoreStockService`.
  - Carga asíncrona diferida en `OnAfterRenderAsync` sin penalizar la respuesta inicial de la ficha.
  - Badges temáticos: 🟢 En stock, 🟡 Últimas unidades, 🔴 Agotado en tienda, ⚪ Verificar en web.
  - Tratamiento visual de ofertas agotadas (botón secundario/atenuado "Agotado en tienda / Ver disponibilidad").
  - Muestra del tiempo relativo de comprobación.
  - Botón de refresco manual ("🔄").
- [x] 4.2 Verificar que `GameDetail.razor` renderiza correctamente las ofertas sin demoras perceptibles.

---

## Fase 5: Verificación Integral y Suite de Pruebas
- [x] 5.1 Ejecución completa de la suite de pruebas unitarias (`dotnet test`).
- [x] 5.2 Asegurar que los 636 tests existentes continúen pasando al 100% en verde junto con todas las nuevas pruebas del Incremento 27 (656 superados).
- [x] 5.3 Elaborar reporte formal de verificación en `openspec/changes/change-27-store-live-stock-check/verify-report.md`.

---

## Fase 6: Cierre y Archivado SDD (`sdd-archive`)
- [ ] 6.1 Crear módulo en `docs/specs/sistema/20-verificacion-stock-tiempo-real-tiendas.md`.
- [ ] 6.2 Actualizar el índice maestro `docs/specs/sistema/README.md` con el nuevo módulo y total de tests.
- [ ] 6.3 Trasladar `docs/increments/inc-27-store-live-stock-check.md` a `docs/increments/archive/inc-27-store-live-stock-check.md`.
- [ ] 6.4 Actualizar estados a `✅ Archivado` en `docs/increments/ROADMAP.md` y `docs/specs/ROADMAP_MVP_SLICES.md`.
- [ ] 6.5 Archivar carpeta de cambio en `openspec/changes/archive/2026-09-08-change-27-store-live-stock-check`.
- [ ] 6.6 Guardar observación en Engram MCP (`mem_save`) y registrar resumen de sesión (`mem_session_summary`).
