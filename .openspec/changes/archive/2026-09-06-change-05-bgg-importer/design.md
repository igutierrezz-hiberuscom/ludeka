# Diseño Técnico: Incremento 5 — Importador BGG en 1 Clic y Auto-Catalogación (`change-05-bgg-importer`)

## Enfoque Técnico
Implementación del motor de importación, catalogación asistida y cola comunitaria de BoardGameGeek en Ludeka bajo Clean Architecture en .NET 10 (C# 13) y Blazor Web App con Tailwind CSS. Permite la ingesta de colecciones de usuario en 1 clic, cruce bidireccional contra el catálogo local, persistencia y procesamiento por lotes de títulos pendientes en `PendingBggImports` y buscador asistido en tiempo real contra `/xmlapi2/search` garantizando cero duplicados.

---

## Decisiones de Arquitectura

| Decisión | Opción Elegida | Alternativas Evaluadas | Justificación |
|---|---|---|---|
| **Modelo de Juegos en Cola de Colección** | `UserCollectionItem` con clave foránea opcional `GameId` (`Guid?`) y metadatos provisionales (`BggId`, `PendingTitle`, `PendingThumbnailUrl`) | Tabla separada `UserPendingCollectionItem` | Evita duplicar lógica de repositorio, vistas y pestañas en `MyLibrary.razor`. El usuario puede gestionar préstamos y notas privadas de inmediato, y la promoción a juego catalogado es una simple actualización de clave foránea. |
| **Persistencia de la Cola Comunitaria** | Entidad dedicada `PendingBggImport` en `LudekaDbContext` con índice único sobre `BggId` e índice compuesto `(Status, RequestedCount)` | Lista en memoria o cálculo al vuelo | Permite acumular la demanda de múltiples usuarios (`RequestedCount++`), priorizar el procesamiento de los títulos más populares y mantener trazabilidad de errores o completitud. |
| **Manejo de Respuestas HTTP 202 en BGG** | Reintentos progresivos con backoff exponencial (2s, 4s, 6s) y fallback informativo | Falla inmediata o bucle infinito | BGG XMLAPI2 devuelve HTTP 202 frecuentemente para compilar la caché de colecciones no consultadas recientemente. Un backoff controlado resuelve la mayoría de casos sin penalizar al usuario. |
| **Buscador Asistido con Prevención de Duplicados** | Consulta a `/xmlapi2/search` con verificación en vivo contra `IGameRepository.GetByBggIdAsync` | Descargar siempre la ficha antes de verificar | Reduce llamadas a BGG, verifica si el juego ya existe en local y si no existe, cataloga en vivo al vuelo creando el juego oficial con su `BggId` canónico. |
| **Promoción Atómica de Usuarios en Cola** | Actualización transaccional de `UserCollectionItem` al procesar un `PendingBggImport` | Consultar BGG en cada visualización de usuario | Rendimiento óptimo: una vez que el juego entra al catálogo oficial, todos los usuarios que lo solicitaron ven instantáneamente su ficha y carátula oficial en su ludoteca. |

---

## Flujo de Datos

```
[Flujo 1: Importación de Colección BGG en 1 Clic]
Usuario en Mi Ludoteca ──► [BggImportModal.razor] ──► Introduce Usuario BGG
                                  │
                                  ▼
                     IBggImportService.ImportUserCollectionAsync
                                  │
                                  ▼
           BggXmlApiClient ──► GET /xmlapi2/collection?username={user}&stats=1
           (Manejo de HTTP 202 con backoff exponencial)
                                  │
                                  ▼
                  Cruce con Catálogo Local (por BggId)
                 ┌────────────────┴────────────────┐
                 ▼                                 ▼
         ¿Existe en Games?                 ¿No existe en Games?
                 │                                 │
                 ├─► SÍ                            ├─► NO
                 │                                 │
       Crea/Actualiza                     1. Crea UserCollectionItem
       UserCollectionItem                    con GameId = null y metadatos
       (GameId = game.Id)                    provisionales (⏳ En cola)
                                          2. Registra o incrementa
                                             RequestedCount en PendingBggImports

[Flujo 2: Cola Nocturna / Manual de Auto-Catalogación]
Worker / Moderador ──► IBggCatalogQueueService.ProcessPendingQueueBatchAsync(batchSize)
                                  │
                                  ▼
           Obtiene Top N títulos en PendingBggImports (RequestedCount DESC)
                                  │
               Por cada título encolado:
               1. BggXmlApiClient.FetchGameByBggIdAsync(bggId)
               2. Crea entidad Game y la inserta en Games
               3. Actualiza UserCollectionItem donde BggId == bggId:
                  GameId = game.Id, limpia campos pendientes
               4. Actualiza GameLoan si existieran préstamos
               5. Marca PendingBggImport como Completed con timestamp

[Flujo 3: Añadir Título a Mano Asistido por BGG]
Usuario en Buscador ──► [BggSearchModal.razor] ──► BggXmlApiClient.SearchGamesAsync(query)
                                  │
                                  ▼
              Muestra resultados con BggId, Título, Año
              e indicación si ya está catalogado en Ludeka
                                  │
                 ┌────────────────┴────────────────┐
                 ▼                                 ▼
           Ya Catalogado                     No Catalogado
                 │                                 │
        Asocia a colección /              Descarga ficha en vivo,
        Abre ficha oficial                crea Game oficial sin duplicados
                                          y lo vincula a la ludoteca del usuario
```

---

## Cambios en Archivos y Componentes

| Archivo | Acción | Capa | Descripción |
|---|---|---|---|
| `src/Ludeka.Core/Enums/CatalogQueueStatus.cs` | Crear | `Core` | Enum de estados de cola: `Pending`, `Processing`, `Completed`, `Failed` |
| `src/Ludeka.Core/Entities/PendingBggImport.cs` | Crear | `Core` | Entidad de juego en cola con contador comunitario de demanda, estado y marcas temporales |
| `src/Ludeka.Core/Entities/UserCollectionItem.cs` | Modificar | `Core` | Clave foránea `GameId` nullable (`Guid?`), campos provisionales `BggId`, `PendingTitle`, `PendingThumbnailUrl` y método `PromoteToCataloged` |
| `src/Ludeka.Application/Contracts/IBggClient.cs` | Modificar | `Application` | Nuevos métodos `FetchUserCollectionAsync` y `SearchGamesAsync` |
| `src/Ludeka.Application/DTOs/BggImportDtos.cs` | Crear | `Application` | DTOs de colección, búsqueda, solicitud/respuesta de importación y cola comunitaria |
| `src/Ludeka.Application/Contracts/IPendingBggImportRepository.cs` | Crear | `Application` | Contrato de persistencia para la cola de importaciones pendientes |
| `src/Ludeka.Application/Contracts/IBggImportService.cs` | Crear | `Application` | Contrato del servicio de orquestación de importación de colecciones |
| `src/Ludeka.Application/Features/Bgg/BggImportService.cs` | Crear | `Application` | Implementación del cruce de colecciones, mapeo de estados y encolado |
| `src/Ludeka.Application/Contracts/IBggCatalogQueueService.cs` | Crear | `Application` | Contrato del servicio de procesamiento por lotes de la cola |
| `src/Ludeka.Application/Features/Bgg/BggCatalogQueueService.cs` | Crear | `Application` | Implementación del procesador de lotes y promoción atómica de usuarios |
| `src/Ludeka.Application/Contracts/IBggSearchAssistedService.cs` | Crear | `Application` | Contrato de búsqueda asistida en vivo |
| `src/Ludeka.Application/Features/Bgg/BggSearchAssistedService.cs` | Crear | `Application` | Implementación de búsqueda asistida y catalogación instantánea |
| `src/Ludeka.Infrastructure/Bgg/BggXmlApiClient.cs` | Modificar | `Infrastructure` | Implementación de `/xmlapi2/collection` con reintentos 202 y `/xmlapi2/search` |
| `src/Ludeka.Infrastructure/Bgg/BggXmlParser.cs` | Modificar | `Infrastructure` | Parsers LINQ-to-XML para colecciones y resultados de búsqueda de BGG |
| `src/Ludeka.Infrastructure/Data/LudekaDbContext.cs` | Modificar | `Infrastructure` | Adición de `DbSet<PendingBggImport>`, índices únicos/compuestos y ajuste de `UserCollectionItem` |
| `src/Ludeka.Infrastructure/Data/SqlitePendingBggImportRepository.cs` | Crear | `Infrastructure` | Implementación SQLite EF Core de la cola comunitaria |
| `src/Ludeka.Infrastructure/Data/SqliteUserCollectionRepository.cs` | Modificar | `Infrastructure` | Métodos para consultar ítems pendientes por `BggId` y actualizar referencias |
| `src/Ludeka.Web/Components/Shared/BggImportModal.razor` | Crear | `Web` | Modal interactivo de importación BGG con estados de carga y resumen lúdico |
| `src/Ludeka.Web/Components/Shared/BggSearchModal.razor` | Crear | `Web` | Modal de búsqueda asistida en vivo y catalogación en 1 toque |
| `src/Ludeka.Web/Components/Shared/CatalogQueuePanel.razor` | Crear | `Web` | Panel de monitorización de la cola con botón de ejecución inmediata |
| `src/Ludeka.Web/Components/Pages/MyLibrary.razor` | Modificar | `Web` | Botones de importación y búsqueda BGG, y renderizado de badges `⏳ En cola de catalogación` |
| `src/Ludeka.Web/Program.cs` | Modificar | `Web` | Inyección de dependencias de los nuevos servicios BGG |
| `tests/Ludeka.UnitTests/Domain/PendingBggImportTests.cs` | Crear | `Tests` | Tests de invariantes de dominio de cola y transiciones de estado |
| `tests/Ludeka.UnitTests/Application/BggImportServiceTests.cs` | Crear | `Tests` | Tests de cruce de colección, vinculación inmediata y encolado |
| `tests/Ludeka.UnitTests/Application/BggCatalogQueueServiceTests.cs` | Crear | `Tests` | Tests de procesamiento por lotes y promoción masiva de usuarios |
| `tests/Ludeka.UnitTests/Infrastructure/BggXmlCollectionParserTests.cs` | Crear | `Tests` | Tests de parsing de colección y búsqueda con fixtures XML reales |
