# Especificación (Delta): user-library-view

## Propósito
Enriquecer la página de la ludoteca personal (`MyLibrary.razor`) con botones de acción para la importación desde BGG y el buscador asistido, badges visuales editoriales `⏳ En cola de catalogación` en las tarjetas de juegos pendientes y un panel de control de la cola comunitaria para moderadores y equipo fundador.

## Requerimientos Modificados

### Requerimiento: Modal de Importación desde BGG
La interfaz DEBE disponer de un modal interactivo con microtextos editoriales lúdicos para importar la colección desde BGG.

#### Escenario: Flujo de importación exitoso
- DADO el usuario en su ludoteca pulsando el botón `[ 📥 Importar desde BGG ]`
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
