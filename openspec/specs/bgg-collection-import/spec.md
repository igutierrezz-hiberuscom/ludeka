# Especificación: bgg-collection-import

## Propósito
Consumir el endpoint de colecciones de BoardGameGeek (`/xmlapi2/collection?username={username}&stats=1`), interpretar con fidelidad los estados de pertenencia lúdica (`own`, `wishlist`, `wanttobuy`, etc.), tolerar respuestas HTTP 202 con reintentos exponenciales y cruzar el resultado con el catálogo local de Ludeka de forma bidireccional (asociación directa para existentes, encolado para no catalogados).

## Requerimientos

### Requerimiento: Consulta Resiliente de Colección en BGG con Manejo de 202
El cliente BGG DEBE consultar `/xmlapi2/collection` respetando el límite de cortesía (máximo 2 req/s) y gestionar la respuesta diferida HTTP 202 de BGG mediante reintentos exponenciales.

#### Escenario: Respuesta HTTP 202 en cola de BGG
- DADO un usuario de BGG cuya colección no está en caché previa en BGG
- CUANDO el cliente realiza la petición y recibe código HTTP 202 (Accepted)
- ENTONCES el cliente DEBE esperar de forma exponencial (mínimo 2s, 4s, 6s) y reintentar hasta 3 veces
- SI transcurridos los reintentos la respuesta sigue siendo 202, DEBE propagar una excepción amigable o resultado descriptivo indicando que BGG está preparando la caché.

#### Escenario: Consulta exitosa de colección
- DADO un usuario existente con colección pública en BGG
- CUANDO el cliente recibe HTTP 200 con el XML de colección
- ENTONCES el parser DEBE extraer los ítems filtrados por subtipo `boardgame` con sus atributos `objectid` (`BggId`), nombre, año, miniaturas e indicadores de estado (`own`, `wishlist`, `wanttobuy`, `played`).

---

### Requerimiento: Mapeo de Estados de Colección
El servicio DEBE transformar las banderas de estado de BGG a los estados nativos de Ludeka (`CollectionStatus`).

#### Escenario: Mapeo de juegos en propiedad (`own="1"`)
- DADO un ítem de BGG con atributo `own="1"`
- CUANDO se procesa para la colección del usuario con la opción de importar juegos propios activada
- ENTONCES DEBE mapearse a `CollectionStatus.InCollection` (En mi ludoteca).

#### Escenario: Mapeo de lista de deseos (`wishlist="1"` o `wanttobuy="1"`)
- DADO un ítem de BGG con atributo `wishlist="1"` o `wanttobuy="1"`
- CUANDO se procesa con la opción de importar lista de deseos activada
- ENTONCES DEBE mapearse a `CollectionStatus.Wishlist` o `CollectionStatus.WantToBuy` respectivamente.

---

### Requerimiento: Cruce Bidireccional con el Catálogo Local
El servicio DEBE cotejar cada `BggId` contra el catálogo existente en Ludeka.

#### Escenario: Juego ya existente en catálogo
- DADO un juego importado con un `BggId` que ya existe en la tabla `Games` de Ludeka
- CUANDO se procesa la importación
- ENTONCES el sistema DEBE crear o actualizar inmediatamente el `UserCollectionItem` del usuario vinculado al `GameId` del catálogo local sin encolarlo.

#### Escenario: Juego no existente en catálogo
- DADO un juego importado cuyo `BggId` no existe en la base de datos de Ludeka
- CUANDO se procesa la importación
- ENTONCES el sistema DEBE crear en la colección del usuario un ítem con estado `⏳ En cola de catalogación` (`GameId = null`) conservando los metadatos provisionales (`BggId`, título y carátula)
- Y DEBE registrar o incrementar la demanda en la tabla `PendingBggImports`.
