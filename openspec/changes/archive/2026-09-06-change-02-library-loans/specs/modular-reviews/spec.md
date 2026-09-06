# Especificación: modular-reviews

## Propósito
Definir el formulario interactivo unificado de valoración rápida en 45 segundos, que captura puntuación numérica, micro-reseña de 280 caracteres, votos de escalabilidad por número de comensales, experiencia familiar y contexto de juego, así como la tarjeta destacada de valoración propia para el usuario.

## Requerimientos

### Requerimiento: Datos Básicos de la Valoración (`Score` y `MicroReview`)
El sistema DEBE permitir registrar una puntuación entre 1.0 y 10.0 (con resolución de 0.5 o 1.0) y una micro-reseña textual opcional de máximo 280 caracteres.

#### Escenario: Registrar puntuación y micro-reseña válida
- DADO un usuario autenticado y un juego
- CUANDO el usuario envía una nota `8.5` y el texto `"Partidas muy dinámicas y tensas a 2 jugadores."` (52 caracteres)
- ENTONCES el sistema DEBE almacenar la valoración
- Y la fecha de creación DEBE registrar la marca temporal actual.

#### Escenario: Rechazar micro-reseña superior a 280 caracteres
- DADO un texto de reseña con 281 caracteres o más
- CUANDO se intenta registrar o actualizar la valoración
- ENTONCES el sistema DEBE rechazar la solicitud por exceder el límite permitido de 280 caracteres.

#### Escenario: Rechazar puntuación fuera de rango
- DADO un valor de puntuación menor a 1.0 o mayor a 10.0
- CUANDO se intenta guardar la valoración
- ENTONCES el sistema DEBE rechazar la solicitud con una excepción de validación.

---

### Requerimiento: Votación de Escalabilidad por Comensales (Chips 1J a 7J+)
El formulario DEBE ofrecer chips interactivos para cada número de jugadores (`1J`, `2J`, `3J`, `4J`, `5J`, `6J`, `7J+`), permitiendo encender sólo los recuentos jugados y asignar un semáforo personal (`MustPlay` 🟢, `Recommended` 🟡, `NotRecommended` 🔴).

#### Escenario: Votar escalabilidad a 2 y 4 jugadores
- DADO un usuario que activa el chip `2J` con estado `MustPlay` y el chip `4J` con estado `Recommended`
- CUANDO guarda su valoración
- ENTONCES el registro DEBE contener exactamente 2 votos de escalabilidad (`2J: MustPlay`, `4J: Recommended`).

---

### Requerimiento: Experiencia Infantil y Familiar Opcional
El formulario DEBE ofrecer una sección opcional para reflejar partidas con niños o en familia, indicando edad mínima sugerida y si se usaron reglas oficiales o adaptadas.

#### Escenario: Registrar experiencia infantil
- DADO que el usuario marca la casilla *"Lo he jugado con niños / en familia"*
- CUANDO selecciona edad mínima `8+` y adaptaciones de reglas
- ENTONCES los datos familiares DEBEN persistirse vinculados a la reseña del usuario.

---

### Requerimiento: Tarjeta de Valoración Propia y Edición Directa
Si el usuario ya ha emitido una valoración para el juego, la ficha `/juegos/{slug}` DEBE mostrar de forma prioritaria su propia tarjeta (`⭐️ Tu valoración: X/10`) con un botón `[ ✏️ Editar mi valoración ]`.

#### Escenario: Visualizar y abrir editor de valoración existente
- DADO un usuario con una valoración previa de nota `9.0`
- CUANDO accede a la ficha del juego
- ENTONCES la tarjeta de valoración propia DEBE mostrarse con su nota, su texto y los chips votados
- Y al pulsar `[ ✏️ Editar mi valoración ]`, el formulario DEBE precargarse con sus datos actuales.
