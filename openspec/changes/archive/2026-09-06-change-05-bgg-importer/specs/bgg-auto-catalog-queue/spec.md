# Especificación: bgg-auto-catalog-queue

## Propósito
Gestionar la tabla persistente `PendingBggImports` de títulos solicitados que aún no figuran en el catálogo de Ludeka, ordenándolos por popularidad e impacto comunitario (`RequestedCount` descendente) y proporcionando un worker/servicio de procesamiento por lotes para catalogarlos automáticamente desde BGG y promover de forma atómica a todos los usuarios que los tenían pendientes en su ludoteca.

## Requerimientos

### Requerimiento: Entidad de Dominio `PendingBggImport` e Invariantes
El sistema DEBE proveer la entidad `PendingBggImport` para registrar de forma única los juegos solicitados no catalogados.

#### Escenario: Creación de nueva entrada en cola
- DADO un `BggId` válido mayor que cero y un título
- CUANDO se crea un `PendingBggImport`
- ENTONCES DEBE inicializarse con `RequestedCount = 1`, estado `Pending`, fecha de creación actual y sin errores registrados.

#### Escenario: Incremento de demanda comunitaria
- DADO un `PendingBggImport` existente para un `BggId`
- CUANDO otro usuario importa su colección y solicita el mismo juego
- ENTONCES el sistema DEBE invocar `IncrementRequestCount()`, elevando en 1 el contador `RequestedCount`.

#### Escenario: Ciclo de vida de procesamiento
- DADO un `PendingBggImport` en estado `Pending`
- CUANDO se inicia su auto-catalogación, DEBE transicionar a `Processing`
- Y SI se completa con éxito, DEBE pasar a `Completed` registrando la fecha `ProcessedAt`
- Y SI falla la descarga o procesamiento, DEBE pasar a `Failed` registrando el mensaje `ErrorMessage`.

---

### Requerimiento: Procesamiento por Lotes de la Cola
El servicio `IBggCatalogQueueService` DEBE seleccionar los $N$ títulos con mayor demanda comunitaria en estado `Pending` y enriquecerlos en el catálogo general.

#### Escenario: Auto-catalogación exitosa de lote
- DADO un conjunto de juegos en estado `Pending` en la tabla `PendingBggImports`
- CUANDO se ejecuta `ProcessPendingQueueBatchAsync(batchSize)`
- ENTONCES el servicio DEBE:
  1. Tomar los juegos ordenados por `RequestedCount` descendente.
  2. Descargar la ficha oficial completa mediante `IBggClient.FetchGameByBggIdAsync(bggId)`.
  3. Guardar la nueva entidad `Game` en la tabla `Games`.
  4. Actualizar todos los `UserCollectionItem` que tenían ese `BggId` y `GameId == null`, asignándoles `GameId = game.Id` y desactivando el estado provisional.
  5. Actualizar los préstamos activos correspondientes si existieran.
  6. Marcar el `PendingBggImport` como `Completed`.

#### Escenario: Disparador manual para administradores
- DADO un usuario con rol `FoundingTeam` o `Moderator` en la interfaz
- CUANDO pulsa el botón `[ ⚡ Ejecutar Auto-Catalogación Ahora ]`
- ENTONCES el sistema DEBE procesar de inmediato el lote de juegos más demandados y devolver un resumen detallado con la cantidad de títulos catalogados y usuarios promovidos.
