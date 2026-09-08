# 05. Integración con BoardGameGeek (BGG)

## 1. Visión General y Propósito
Este módulo gestiona la interoperabilidad robusta con la API pública XMLAPI2 de BoardGameGeek para permitir la importación en 1 clic de la colección de cualquier usuario, la auto-catalogación comunitaria en cola nocturna y la búsqueda asistida de títulos oficiales. Tras el **Incremento 25**, el cliente cuenta con resiliencia de grado de producción, sondeo asíncrono con backoff exponencial para colecciones frías, gestión transparente de límites de tasa (`Retry-After`), soporte para tokens Bearer / ApiKey mediante un `DelegatingHandler` desacoplado y feedback visual interactivo en tiempo real.

---

## 2. Cliente HTTP, Resiliencia y Protocolo BGG

### 2.1 `BggXmlApiClient`
Ubicación: [`src/Ludeka.Infrastructure/Bgg/BggXmlApiClient.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Bgg/BggXmlApiClient.cs)

- **Contrato:** [`IBggClient`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IBggClient.cs)
  - `FetchGameByBggIdAsync(bggId, ct)`
  - `FetchUserCollectionAsync(username, progress, ct)`
  - `SearchGamesAsync(query, ct)`
  - `FetchTopGamesAsync(limit, ct)`
- **Políticas de Cortesía y Resiliencia (INC-25):**
  - *Rate Limiter Nivel 1:* `TokenBucketRateLimiter` limitado a **máximo 2 peticiones por segundo**.
  - *Manejo Asíncrono de HTTP 202 (Accepted):* Cuando BGG encola una colección para generarla en caché de servidor, se activa un ciclo de sondeo controlado con backoff progresivo (3s, 5s, 8s, 12s, 15s) con límite total de 45s (`PollingTimeoutSeconds`) y máximo 6 intentos (`MaxPollingRetries`).
  - *Manejo de Rate Limit HTTP 429 y 503:* Detección e inspección de la cabecera `Retry-After` (formato numérico o fecha HTTP). El cliente aguarda exactamente el tiempo exigido por BGG y notifica a la UI el aviso de pausa para evitar bloqueos por IP.
  - *Reporte Reactivo de Progreso (`IProgress<BggImportProgressReport>`):* Notifica en vivo las fases `Initializing`, `RequestingBgg`, `PreparingInBgg`, `RateLimitedWaiting`, `ProcessingItems`, `Completed` y `Failed`.

### 2.2 Pipeline HTTP y `BggResilienceAndAuthHandler`
Ubicación: [`src/Ludeka.Infrastructure/Bgg/BggResilienceAndAuthHandler.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Bgg/BggResilienceAndAuthHandler.cs)

- `DelegatingHandler` registrado en `Microsoft.Extensions.DependencyInjection` adjuntado al `HttpClient` de `BggXmlApiClient`:
  - Inyecta la cabecera obligatoria `User-Agent` (`LudekaApp/1.0 (https://ludeka.es; contacto@ludeka.es)`).
  - Inyecta la cabecera `Authorization: Bearer <token>` cuando `BearerToken` o `ApiToken` están configurados.
  - Inyecta la cabecera `X-BGG-API-KEY: <key>` cuando `ApiKey` está configurada.
  - Proporciona el método estático de utilidad `ExtractRetryAfterSeconds(HttpResponseMessage response)`.

### 2.3 `BggOptions`
Ubicación: [`src/Ludeka.Infrastructure/Bgg/BggOptions.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Bgg/BggOptions.cs)
- Configuración flexible mediante `appsettings.json` o variables de entorno:
  - `ApiKey`: Clave de API de BGG.
  - `BearerToken` / `ApiToken`: Token Bearer sincronizado bidireccionalmente.
  - `MaxPollingRetries`: 6 intentos.
  - `PollingTimeoutSeconds`: 45 segundos.
  - `InitialPollingDelaySeconds`: 3 segundos.
  - `MaxRateLimitRetries`: 3 reintentos.
  - `SimulateApi`: Activa el modo simulado offline.
  - `ShouldSimulate`: `true` si `SimulateApi == true` o si no hay credenciales configuradas.

### 2.4 `SimulatedBggClient` y Conmutador de Simulación Offline
Ubicación: [`src/Ludeka.Infrastructure/Bgg/SimulatedBggClient.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Bgg/SimulatedBggClient.cs)

- **Propósito:** Proveer una suite 100% autónoma y offline para desarrollo, pruebas unitarias, CI/CD y despliegues sin dependencia de la API externa de BGG.
- **Dataset Canónico (`BggSimulationDataset`):** 40 títulos reales del hobby con ADN lúdico, semáforo de comensales, fundas y enlaces de compra.
- Soporta la firma con `IProgress<BggImportProgressReport>` emitiendo eventos de progreso simulados para verificar componentes y flujos de UI.

---

## 3. Casos de Uso del Negocio (`Ludeka.Application`)

### 3.1 Importador en 1 Clic con Progreso (`BggImportService`)
- Implementa [`IBggImportService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IBggImportService.cs).
- Recibe `BggImportRequest` y `IProgress<BggImportProgressReport>? progress`.
- Extrae listas `owned` (En mi ludoteca), `wishlist` (Deseado) y `wanttobuy` (Quiero comprar).
- Cruza cada elemento con la base de datos local por `BggId`:
  - Si el juego ya está catalogado en Ludeka: Se añade directamente a la colección del usuario.
  - Si no existe en Ludeka: Se crea una entrada en estado `IsPendingCatalog = true` en su colección y se encola en `PendingBggImports`.
- Emite notificaciones de progreso cuantitativas y cualitativas en cada transición.

### 3.2 Cola Comunitaria de Auto-Catalogación (`BggCatalogQueueService`)
- Entidad de dominio: [`PendingBggImport`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/PendingBggImport.cs).
- Las solicitudes se ordenan por `RequestedCount` (demanda popular).
- Al procesar un lote (`ProcessPendingQueueBatchAsync`):
  1. Se consulta BGG por `BggId` y se construye la entidad `Game` con todos sus Value Objects.
  2. Se guarda en el catálogo general de Ludeka.
  3. **Promoción Atómica:** Se ejecuta `PromotePendingItemsAsync` en [`IUserCollectionRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IUserCollectionRepository.cs), vinculando de inmediato el juego a todos los usuarios que lo solicitaron.

### 3.3 Buscador Asistido en Vivo (`BggSearchAssistedService`)
- Permite buscar en `/xmlapi2/search` para añadir títulos a mano evitando duplicados.
- Cuenta con respaldo automático contra el catálogo local si BGG se encuentra inaccesible.

---

## 4. Componentes UI (`Ludeka.Web`)

- [`BggImportModal.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/BggImportModal.razor): Modal interactivo accesible (WCAG 2.2 AA con `role="status"` y `aria-live="polite"`).
  - Renderiza estados visuales específicos según la fase: sondeo adaptativo ante 202 con indicador de intento y tiempo de espera, aviso de calma ante rate limiting (429), contador dinámico de juegos procesados y desglose de juegos en ludoteca vs. encolados.
- [`BggSearchModal.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/BggSearchModal.razor): Búsqueda con autocompletado y vinculación directa por BggId.
- [`CatalogQueuePanel.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/CatalogQueuePanel.razor): Panel administrativo para monitorizar y procesar la cola de títulos pendientes.
