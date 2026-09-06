# Especificación: user-library-view

## Propósito
Definir la vista de la ludoteca personal en `/mi-ludoteca`, organizada en pestañas reactivas por estado lúdico y préstamos activos, con contadores actualizados en tiempo real, acción de devolución inmediata, botones para importación BGG y buscador asistido, badges visuales `⏳ En cola de catalogación` y panel de control de la cola comunitaria para moderadores y equipo fundador.

## Requerimientos

### Requerimiento: Navegación por Pestañas de Estado
La página `/mi-ludoteca` DEBE organizar los juegos del usuario en pestañas reactivas:
1. *En mi ludoteca (N)*
2. *Jugados (N)*
3. *Deseados (N)*
4. *Quiero comprar (N)*
5. *Préstamos activos (N)*
6. *Cola comunitaria (N)* (visible para usuarios con rol de moderador o equipo fundador).

#### Escenario: Renderizado de contadores por pestaña
- DADO un usuario con 3 juegos en `InCollection`, 5 en `Played`, 2 en `Wishlist`, 1 en `WantToBuy` y 1 préstamo activo
- CUANDO el usuario navega a `/mi-ludoteca`
- ENTONCES las pestañas DEBEN mostrar los conteos numéricos exactos `(3)`, `(5)`, `(2)`, `(1)` y `(1)`.

#### Escenario: Cambio de pestaña reactivo
- DADO el usuario en `/mi-ludoteca`
- CUANDO pulsa la pestaña *Deseados*
- ENTONCES la vista DEBE mostrar exclusivamente los juegos clasificados como `Wishlist`.

---

### Requerimiento: Gestión de Préstamos Activos en Mi Ludoteca
La pestaña de *Préstamos activos` DEBE listar todos los préstamos con `IsReturned = false`, detallando el juego, nombre del prestatario, fecha de préstamo y un botón directo para registrar la devolución.

#### Escenario: Devolver préstamo desde la pestaña de préstamos
- DADO un préstamo activo visible en la pestaña de préstamos
- CUANDO el usuario presiona `[ Marcar como devuelto ]`
- ENTONCES el préstamo DEBE marcarse como devuelto
- Y el contador de préstamos activos DEBE decrementarse en 1 inmediatamente.

---

### Requerimiento: Modal de Importación desde BGG
La interfaz DEBE disponer de un modal interactivo con microtextos editoriales lúdicos para importar la colección desde BGG.

#### Escenario: Flujo de importación exitoso
- DADO el usuario en su ludoteca pulsando el botón `[ 📥 Importar BGG ]`
- CUANDO introduce su usuario de BGG y selecciona las listas deseadas
- ENTONCES el modal DEBE mostrar una animación temática de carga mientras consulta BGG
- Y presentar al finalizar una tarjeta de resumen con dos cifras destacadas:
  1. Cantidad de títulos asociados de inmediato a su ludoteca.
  2. Cantidad de títulos nuevos añadidos a la cola de catalogación comunitaria.

---

### Requerimiento: Distintivo Visual de Juego en Cola
Las tarjetas de juegos que aún no estén catalogados DEBEN indicar de forma clara su estado transitorio.

#### Escenario: Visualización de tarjeta en cola
- DADO un juego en la colección con `IsPendingCataloging == true`
- CUANDO se renderiza en la pestaña correspondiente de `MyLibrary.razor`
- ENTONCES la tarjeta DEBE mostrar un badge ámbar nítido `⏳ En cola de catalogación` con tooltip explicativo
- Y desactivar la navegación a ficha completa hasta que sea catalogado, manteniendo activas las acciones de préstamo.

---

### Requerimiento: Panel de Control de la Cola Comunitaria
La interfaz DEBE ofrecer a los moderadores y mesa fundadora una vista de la cola comunitaria.

#### Escenario: Ejecución de auto-catalogación bajo demanda
- DADO un usuario con permisos de moderación o equipo fundador
- CUANDO visualiza el panel de cola comunitaria y pulsa `[ ⚡ Ejecutar Auto-Catalogación Ahora ]`
- ENTONCES el sistema DEBE disparar el proceso por lotes y refrescar la vista mostrando los títulos recién promovidos al catálogo general.
