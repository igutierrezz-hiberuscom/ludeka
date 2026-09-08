# Checklist de Tareas: change-26-card-sleeves-spec-stores (Incremento 26)

## Fase 1: Modelos de Dominio (`Ludeka.Core`)
- [x] 1.1 Enriquecer `SleeveItem` en `Ludeka.Core/ValueObjects/SleeveItem.cs` con propiedades `PacksNeeded50` y `PacksNeeded100` y validaciones auxiliares.
- [x] 1.2 Crear `StandardSleeveFormat` y `StandardSleeveCatalog` en `Ludeka.Core/ValueObjects/StandardSleeveCatalog.cs` con los 10 formatos universales del hobby y lógica `Match(w, h)`.
- [x] 1.3 Incorporar métodos de mutación en `Game.cs`: `UpdateSleeves(IEnumerable<SleeveItem>)`, `AddSleeve(SleeveItem)` y `ClearSleeves()`.
- [x] 1.4 Pruebas unitarias en `tests/Ludeka.UnitTests/Domain/CardSleeveDomainTests.cs` validando cálculos de paquetes, dimensiones, coincidencia de formatos y mutación de entidad.

---

## Fase 2: Servicios de Aplicación y Enlaces de Compra (`Ludeka.Application`)
- [x] 2.1 Definir DTO `SleevePurchaseOptionDto` e interfaz `ISleeveStoreUrlResolver` en `Ludeka.Application/Contracts/ISleeveStoreUrlResolver.cs`.
- [x] 2.2 Implementar `SleeveStoreUrlResolver` en `Ludeka.Application/Features/Sleeves/SleeveStoreUrlResolver.cs` con soporte para Zacatrus, Dungeon Marvels y tiendas colaboradoras, filtrado por país de usuario e inyección de tags de afiliado.
- [x] 2.3 Ampliar `UpdateGameDetailsCommand` en `Ludeka.Application/DTOs/GameEditorDtos.cs` con el parámetro opcional `IReadOnlyList<SleeveItem>? Sleeves = null`.
- [x] 2.4 Actualizar `GameEditorService.UpdateGameAsync` en `Ludeka.Application/Features/Catalog/GameEditorService.cs` para aplicar las fundas enviadas y registrar los cambios en la auditoría editorial (`AuditLog`).
- [x] 2.5 Pruebas unitarias en `tests/Ludeka.UnitTests/Application/SleeveStoreUrlResolverTests.cs` y `GameEditorServiceTests.cs`.

---

## Fase 3: Ingesta e Infraestructura (`Ludeka.Infrastructure`)
- [x] 3.1 Implementar `BggSleeveParser` en `Ludeka.Infrastructure/Bgg/BggSleeveParser.cs` para extraer dimensiones milimétricas y cantidades a partir de `<link type="boardgamecardsleeve" ...>`.
- [x] 3.2 Conectar `BggSleeveParser` en `BggXmlParser.cs` para poblar automáticamente la colección de fundas durante la ingesta.
- [x] 3.3 Registrar `ISleeveStoreUrlResolver` en el contenedor de dependencias (`Program.cs` / DI).
- [x] 3.4 Pruebas unitarias en `tests/Ludeka.UnitTests/Infrastructure/BggSleeveParserTests.cs`.

---

## Fase 4: Componentes Visuales Blazor (`Ludeka.Web`)
- [x] 4.1 Rediseñar y elevar `SleeveGuideCard.razor` ("Protege tu juego: Guía de Fundas"):
  - Silueta visual de la carta con aspecto proporcional SVG/CSS.
  - Métricas claras (medidas en mm, total de cartas, paquetes de 50 y de 100).
  - Guía didáctica compacta de micras (Standard 50-60 µm vs Premium 100 µm).
  - Botones de compra contextual a tiendas mediante `ISleeveStoreUrlResolver` respetando país.
  - Estado vacío amigable cuando el juego no tiene cartas.
- [x] 4.2 Incorporar la pestaña "🛡️ Fundas" en `GameEditorModal.razor`:
  - Listado editable de fundas existentes.
  - Desplegable de presets rápidos con `StandardSleeveCatalog`.
  - Formularios para agregar y eliminar formatos de fundas.
  - Integración en el comando `UpdateGameDetailsCommand`.

---

## Fase 5: Verificación Integral y Suite de Pruebas
- [x] 5.1 Ejecución completa de la suite de pruebas unitarias (`dotnet test`).
- [x] 5.2 Asegurar que los 609 tests previos más los nuevos tests pasen al 100% en verde (636 tests pasando).
- [x] 5.3 Elaborar reporte formal de verificación en `openspec/changes/change-26-card-sleeves-spec-stores/verify-report.md`.

---

## Fase 6: Cierre y Archivado SDD (`sdd-archive`)
- [x] 6.1 Actualizar o crear módulo del sistema en `docs/specs/sistema/19-especificacion-fundas-y-enlaces-tiendas.md`.
- [x] 6.2 Actualizar el índice maestro `docs/specs/sistema/README.md` con el nuevo módulo y total de tests.
- [x] 6.3 Trasladar `docs/increments/inc-26-card-sleeves-spec-stores.md` a `docs/increments/archive/inc-26-card-sleeves-spec-stores.md`.
- [x] 6.4 Actualizar estados a `✅ Archivado` en `docs/increments/ROADMAP.md` y `docs/specs/ROADMAP_MVP_SLICES.md`.
- [x] 6.5 Archivar carpeta de cambio en `openspec/changes/archive/2026-09-08-change-26-card-sleeves-spec-stores`.
- [x] 6.6 Guardar observación en Engram MCP (`mem_save`) y resumen de sesión (`mem_session_summary`).
