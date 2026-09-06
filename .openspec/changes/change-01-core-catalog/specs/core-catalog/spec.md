# Especificación: core-catalog

## Propósito
Definir la entidad raíz de agregado `Game`, la interfaz de repositorio para persistencia, el motor de búsqueda y filtrado multi-criterio, y el enrutamiento amigable para SEO basado en slug para el catálogo de juegos de mesa de Ludeca.

## Requerimientos

### Requerimiento: Entidad de Agregado de Juego de Mesa (`Game`)
El modelo de dominio DEBE representar un juego de mesa como un agregado raíz completo con identificadores únicos, metadatos editoriales, doble calificación y Value Objects encapsulados.

#### Escenario: Instanciar agregado Game válido
- DADO un identificador GUID único, un ID de BGG positivo `266192`, título original `"Wingspan"`, título en español `"Wingspan"` y año de publicación `2019`
- CUANDO se instancia la entidad `Game` con los parámetros editoriales requeridos
- ENTONCES la entidad DEBE tener sus campos `Id`, `BggId`, `Slug` establecido en `"wingspan"`, `OriginalTitle` y `SpanishTitle` correctamente asignados
- Y las calificaciones por defecto DEBEN inicializarse en `0.0` con rankings nulos.

#### Escenario: Normalización automática del slug
- DADO un juego con título `"Los Castillos de Borgoña"` o `"7 Wonders: Duel!"`
- CUANDO se ejecuta la lógica de generación del slug
- ENTONCES DEBE producir slugs en minúsculas seguros para URL, sin diacríticos, tildes ni caracteres especiales (ej. `"los-castillos-de-borgona"` y `"7-wonders-duel"`).

#### Escenario: Nombre en español de respaldo (fallback)
- DADO un juego importado de BGG donde no se ha definido un título alternativo oficial en español
- CUANDO se construye o actualiza la entidad `Game`
- ENTONCES `SpanishTitle` DEBE tomar como respaldo el valor de `OriginalTitle`
- Y `SpanishTitle` NO DEBE ser nulo ni vacío.

---

### Requerimiento: Contrato de Repositorio del Catálogo (`IGameRepository`)
La capa de aplicación DEBE definir una abstracción `IGameRepository` que provea métodos asíncronos para búsqueda, paginación, recuperación por slug y operaciones de persistencia.

#### Escenario: Recuperar juego por slug
- DADO un juego existente en el catálogo con slug `"catan"`
- CUANDO se invoca `GetBySlugAsync("catan", cancellationToken)`
- ENTONCES el repositorio DEBE devolver el agregado `Game` completo con todos sus Value Objects asociados cargados.

#### Escenario: Recuperar juego por slug inexistente
- DADO un slug `"juego-inexistente-xyz"` que no existe en el catálogo
- CUANDO se invoca `GetBySlugAsync("juego-inexistente-xyz", cancellationToken)`
- ENTONCES el repositorio DEBE devolver `null` sin lanzar excepciones no controladas.

#### Escenario: Recuperar juego por BGG ID
- DADO un juego existente con ID de BGG `13` (*Catán*)
- CUANDO se invoca `GetByBggIdAsync(13, cancellationToken)`
- ENTONCES el repositorio DEBE devolver la entidad `Game` correspondiente.

---

### Requerimiento: Búsqueda y Filtrado Multi-Criterio del Catálogo
El motor de consultas DEBE soportar búsqueda multi-criterio combinando coincidencias de texto sobre títulos en español y original, rangos de jugadores, estilos lúdicos, modos de confrontación, restricciones de duración y filtros rápidos predefinidos.

#### Escenario: Búsqueda de texto en doble título
- DADO juegos con `SpanishTitle = "Los Castillos de Borgoña"` (`OriginalTitle = "The Castles of Burgundy"`) y `SpanishTitle = "Catán"`
- CUANDO un usuario busca con el término `"Burgundy"` o `"Borgoña"`
- ENTONCES la consulta DEBE devolver `"Los Castillos de Borgoña"` en los resultados independientemente de si buscó en inglés o español.

#### Escenario: Filtrar por ajuste predefinido "Especial Parejas"
- DADO un criterio de filtrado donde `EspecialParejas` está marcado como `true`
- CUANDO se ejecuta `SearchAsync(criteria, page, pageSize, cancellationToken)`
- ENTONCES solo DEBEN devolverse juegos cuyo semáforo de escalabilidad para 2 jugadores sea `MustPlay` (Imprescindible 🟢).

#### Escenario: Filtrar por ajuste predefinido "Mesa Familiar"
- DADO un criterio de filtrado donde `MesaFamiliar` está marcado como `true`
- CUANDO se ejecuta la búsqueda
- ENTONCES solo DEBEN devolverse juegos con `CommunityAge <= 8` (o `IsAccessibleEarlier == true`) y dependencia del idioma `None` o `Low`.