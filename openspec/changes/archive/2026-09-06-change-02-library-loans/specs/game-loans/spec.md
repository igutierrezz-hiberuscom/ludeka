# Especificación: game-loans

## Propósito
Definir el cuaderno privado de préstamos ("¿A quién se lo dejé?"), permitiendo a los usuarios registrar préstamos de sus juegos en propiedad, visualizar el estado activo del préstamo en la ficha y gestionar devoluciones en 1 clic.

## Requerimientos

### Requerimiento: Registro de Préstamo de Juego (`GameLoan`)
El sistema DEBE permitir registrar un préstamo de un juego exclusivamente cuando el juego se encuentra en estado `InCollection` (físico en propiedad).

#### Escenario: Prestar un juego en propiedad
- DADO un usuario con un juego marcado en su colección como `InCollection`
- CUANDO registra un préstamo indicando el nombre del prestatario `"Carlos (Asociación)"` y fecha del préstamo
- ENTONCES el sistema DEBE crear un registro `GameLoan` activo con `IsReturned = false`
- Y el prestatario y la fecha DEBEN persistirse correctamente.

#### Escenario: Impedir préstamo de un juego no poseído
- DADO un juego en estado `Wishlist` o `Played` (no poseído en propiedad)
- CUANDO se intenta registrar un préstamo para ese juego
- ENTONCES el sistema DEBE rechazar la operación arrojando una excepción de regla de negocio o error de validación.

#### Escenario: Validación de nombre de prestatario obligatorio
- DADO un formulario de préstamo donde el nombre de la persona o asociación está en blanco
- CUANDO se intenta registrar el préstamo
- ENTONCES el sistema DEBE rechazar la solicitud exigiendo un nombre válido.

---

### Requerimiento: Devolución en 1 Clic (`MarkAsReturned`)
El sistema DEBE proveer una acción rápida para marcar un préstamo activo como devuelto con un solo toque o clic.

#### Escenario: Devolver juego prestado
- DADO un préstamo activo con `IsReturned = false`
- CUANDO el usuario acciona `[ Marcar como devuelto ]`
- ENTONCES `IsReturned` DEBE actualizarse a `true`
- Y `ReturnedDate` DEBE registrar la fecha y hora de devolución.

---

### Requerimiento: Indicador de Préstamo Activo en Ficha
La ficha `/juegos/{slug}` de un juego que se encuentra actualmente prestado DEBE exhibir un distintivo visible informando a quién está prestado y desde qué fecha.

#### Escenario: Visualización de préstamo en la ficha
- DADO un juego con un préstamo activo a `"Marta"` desde `"2026-09-01"`
- CUANDO el propietario visita la ficha del juego
- ENTONCES la interfaz DEBE mostrar una tarjeta informativa: `"📦 Actualmente prestado a Marta (desde 01/09/2026)"` con el botón directo de devolución.
