# Especificación: personal-collection

## Propósito
Definir la gestión de la colección lúdica personal de cada usuario en 4 estados interactivos (*En mi ludoteca*, *Jugado*, *Deseado*, *Quiero comprar*), con barra de acción accesible al pulgar en la ficha, sincronización persistente y soporte para juegos en cola de catalogación provisional (`IsPendingCataloging`).

## Requerimientos

### Requerimiento: Estados de Colección Lúdica (`CollectionStatus`)
El sistema DEBE soportar exactamente 4 estados mutuamente excluyentes para vincular un juego con la ludoteca del usuario:
- `InCollection`: Copia física en propiedad (*En mi ludoteca*).
- `Played`: Título jugado en club, bar o amigos (*Jugado*).
- `Wishlist`: Título en radar de interés (*Deseado*).
- `WantToBuy`: Título en seguimiento de ofertas (*Quiero comprar*).
El usuario PUEDE desmarcar el estado para quitar el juego de su colección.

#### Escenario: Asignar estado a un juego
- DADO un usuario autenticado y un juego válido en el catálogo
- CUANDO el usuario selecciona el estado `InCollection`
- ENTONCES el sistema DEBE registrar o actualizar el ítem de colección con estado `InCollection`
- Y la fecha de actualización DEBE registrar la marca temporal actual.

#### Escenario: Alternar entre estados
- DADO un juego previamente marcado como `Wishlist` por el usuario
- CUANDO el usuario pulsa `Played`
- ENTONCES el estado del ítem DEBE cambiar a `Played`
- Y NO DEBE duplicar el registro en la base de datos para ese par `(UserId, GameId)`.

#### Escenario: Quitar un juego de la colección
- DADO un juego marcado actualmente como `WantToBuy`
- CUANDO el usuario vuelve a presionar el botón activo `Quiero comprar`
- ENTONCES el sistema DEBE eliminar el ítem de colección para ese juego
- Y el estado del juego para el usuario DEBE quedar sin marcar.

---

### Requerimiento: Barra de Acción de Colección en Ficha (`CollectionActionBar`)
La ficha de juego en `/juegos/{slug}` DEBE mostrar una barra de interacción adaptada a dispositivos móviles (mobile-first) con los 4 botones de estado, indicando visualmente el estado actual y disparando acciones inmediatas.

#### Escenario: Renderizado del estado actual
- DADO un usuario que tiene el juego en estado `InCollection`
- CUANDO accede a la ficha `/juegos/{slug}`
- ENTONCES el botón `En mi ludoteca` DEBE mostrarse resaltado con estilo activo (verde)
- Y los otros tres botones DEBEN mostrarse en estado inactivo neutral.

#### Escenario: Sugerencia de valoración tras marcar Jugado o En mi ludoteca
- DADO un juego sin valoración previa del usuario
- CUANDO el usuario pulsa por primera vez `Jugado` o `En mi ludoteca`
- ENTONCES el sistema DEBE invocar el disparador de valoración rápida sugiriendo dejar su reseña en 45 segundos.

---

### Requerimiento: Ítems de Colección Pendientes de Catalogación
La entidad `UserCollectionItem` DEBE soportar pertenencia a la ludoteca personal sin obligar a la existencia previa de una entidad `Game` en la base de datos.

#### Escenario: Registro de ítem en cola de catalogación
- DADO un juego importado de BGG que aún no existe en el catálogo de Ludeka
- CUANDO se crea su entrada en la colección del usuario
- ENTONCES `GameId` DEBE ser `null`, `BggId` DEBE almacenar el identificador de BGG, `PendingTitle` su nombre y `IsPendingCataloging` DEBE ser `true`.

#### Escenario: Promoción atómica a juego catalogado
- DADO un `UserCollectionItem` con `IsPendingCataloging == true`
- CUANDO el juego oficial se incorpora al catálogo con un `gameId` válido
- ENTONCES al invocar `PromoteToCataloged(gameId)`, `GameId` DEBE asignarse, `IsPendingCataloging` DEBE ser `false` y los campos provisionales DEBEN limpiarse.

#### Escenario: Gestión de préstamos sobre juegos en cola
- DADO un juego en la ludoteca personal con `IsPendingCataloging == true`
- CUANDO el usuario registra un préstamo a un tercero
- ENTONCES el sistema DEBE permitir registrar el préstamo asociándolo al ítem de la colección y mostrando el título provisional.
