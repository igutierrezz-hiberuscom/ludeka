# Especificación: bgg-live-search

## Propósito
Permitir a los usuarios y administradores consultar en vivo el índice global de BoardGameGeek (`/xmlapi2/search?query={query}&type=boardgame`), visualizando resultados con indicación de si ya están en Ludeka, y posibilitando la catalogación instantánea al vuelo con vinculación inmediata a su ludoteca sin duplicados.

## Requerimientos

### Requerimiento: Consulta Asistida en Vivo a BGG
El cliente DEBE consultar el buscador oficial de BGG y tabular los resultados.

#### Escenario: Búsqueda con resultados coincidentes
- DADO un término de búsqueda (ej. `"Ark Nova"`)
- CUANDO se invoca `IBggClient.SearchGamesAsync(query)`
- ENTONCES el parser DEBE devolver una lista de `BggSearchResultDto` con `BggId`, título principal y año de publicación.

#### Escenario: Término de búsqueda vacío o muy corto
- DADO un término de búsqueda vacío o menor a 2 caracteres
- CUANDO se invoca el buscador
- ENTONCES el sistema DEBE devolver una lista vacía sin emitir llamadas de red innecesarias hacia BGG.

---

### Requerimiento: Discriminación de Catálogo y Catalogación al Vuelo
El servicio de búsqueda asistida DEBE comprobar si los resultados de BGG ya forman parte del catálogo local de Ludeka.

#### Escenario: Juego ya existente en catálogo
- DADO un resultado de búsqueda con un `BggId` presente en `Games`
- CUANDO el usuario lo selecciona en la interfaz
- ENTONCES el sistema DEBE marcarlo como ya catalogado (`IsAlreadyCataloged = true`), asociarlo a la colección del usuario con el estado elegido y permitir la navegación directa a su ficha oficial.

#### Escenario: Catalogación instantánea al vuelo sin duplicados
- DADO un resultado de búsqueda con un `BggId` que NO existe en Ludeka
- CUANDO el usuario selecciona "Añadir a mi ludoteca"
- ENTONCES el sistema DEBE:
  1. Descargar en tiempo real la ficha completa desde BGG (`FetchGameByBggIdAsync`).
  2. Crear la entidad `Game` y persistirla en la base de datos.
  3. Vincular el juego recién creado a la colección del usuario (`UserCollectionItem`).
  4. Si el juego estaba previamente en `PendingBggImports`, marcarlo como `Completed`.
  5. Asegurar que no se creen registros duplicados con el mismo `BggId`.
