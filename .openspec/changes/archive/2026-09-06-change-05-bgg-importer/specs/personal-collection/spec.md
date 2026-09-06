# Especificación (Delta): personal-collection

## Propósito
Extender la entidad `UserCollectionItem` y el servicio `IUserLibraryService` para admitir juegos en estado pendiente de catalogación (`IsPendingCataloging`), conservando metadatos provisionales (título, año, miniatura de BGG) y permitiendo la promoción atómica a ficha catalogada definitiva una vez enriquecida.

## Requerimientos Modificados

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
