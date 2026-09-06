# Especificación: media-item-core

## Propósito
Definir la entidad de dominio `MediaItem` y sus invariantes para representar contenidos de YouTube e Instagram, soportando el ciclo de vida de moderación, el estado de elementos huérfanos y la regla obligatoria de número de comensales para partidas.

## Requerimientos

### Requerimiento: Entidad de Dominio `MediaItem` e Invariantes
El sistema DEBE proveer la entidad `MediaItem` que modele un recurso audiovisual o social vinculado opcionalmente a un juego del catálogo (`GameId`).

#### Escenario: Creación válida de un Tutorial de YouTube
- DADO un título no vacío, una URL válida de YouTube, canal de autor y duración en segundos
- CUANDO se crea un `MediaItem` de tipo `Tutorial` y plataforma `YouTube`
- ENTONCES el elemento DEBE instanciarse con estado inicial `PendingApproval` (o el estado especificado), fecha de creación y formato de duración calculado.

#### Escenario: Obligatoriedad de PlayerCountBadge en Partidas Completas
- DADO un intento de crear un `MediaItem` con tipo `Playthrough`
- CUANDO el campo `PlayerCountBadge` esté vacío, nulo o compuesto solo de espacios
- ENTONCES el sistema DEBE lanzar una excepción de dominio (`ArgumentException`) exigiendo el número de jugadores (ej. `"Partida a 2"`).

#### Escenario: Creación de contenido huérfano
- DADO un vídeo o post detectado en redes que aún no coincide con ningún juego
- CUANDO se registra con `GameId = null`
- ENTONCES el elemento DEBE guardarse como huérfano (`IsOrphan == true`), quedando disponible en la cola de moderación para su asignación.

---

### Requerimiento: Transiciones de Estado de Moderación
El sistema DEBE permitir transiciones de estado controladas a través de métodos de dominio explícitos.

#### Escenario: Aprobación de contenido
- DADO un `MediaItem` en estado `PendingApproval` o `Rejected`
- CUANDO se invoca `Approve()`
- ENTONCES el estado DEBE pasar a `Approved` y la fecha `UpdatedAt` DEBE actualizarse.

#### Escenario: Rechazo de contenido
- DADO un `MediaItem` en estado `PendingApproval` o `Approved`
- CUANDO se invoca `Reject()`
- ENTONCES el estado DEBE pasar a `Rejected` y la fecha `UpdatedAt` DEBE actualizarse.

#### Escenario: Asignación de juego a contenido huérfano
- DADO un `MediaItem` con `GameId == null`
- CUANDO se invoca `AssignToGame(gameId)` con un identificador válido
- ENTONCES `GameId` DEBE actualizarse al ID indicado, `IsOrphan` DEBE ser falso y `UpdatedAt` DEBE renovarse.
- SI el `gameId` proporcionado es `Guid.Empty`, DEBE lanzar una excepción de validación.

---

### Requerimiento: Detección y Marcado de Enlaces Rotos
El sistema DEBE permitir marcar y desmarcar elementos con enlaces caídos (`IsBroken`).

#### Escenario: Marcar enlace como roto
- DADO un `MediaItem` activo
- CUANDO el proceso de verificación detecta un código HTTP 404 y llama a `MarkAsBroken(true)`
- ENTONCES la propiedad `IsBroken` DEBE ser `true`.
