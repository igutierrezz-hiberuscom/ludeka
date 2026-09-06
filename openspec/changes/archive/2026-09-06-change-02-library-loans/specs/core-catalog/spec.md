# Delta para core-catalog

## MODIFIED Requirements

### Requerimiento: Entidad de Agregado de Juego de Mesa (`Game`)

El modelo de dominio DEBE representar un juego de mesa como un agregado raíz completo con identificadores únicos, metadatos editoriales, doble calificación con soporte para recálculo dinámico del consenso local (`LudistRating`) y Value Objects encapsulados.
(Previously: El agregado Game inicializaba calificaciones fijas sin método de actualización comunitaria del consenso local).

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

#### Escenario: Actualizar consenso local LudistRating
- DADO un juego con `LudistRating = 0.0`
- CUANDO se invoca `UpdateLudistRating(8.75)` con el nuevo promedio ponderado
- ENTONCES el valor de `LudistRating` DEBE actualizarse a `8.75` manteniéndose acotado en el rango `[0.0, 10.0]`.
