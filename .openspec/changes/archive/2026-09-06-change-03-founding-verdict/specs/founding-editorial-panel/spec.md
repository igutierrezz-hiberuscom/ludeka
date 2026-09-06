# Especificación: founding-editorial-panel

## Propósito
Definir el panel editorial móvil y modal interactivo para miembros del equipo fundador y moderadores, permitiendo la redacción, edición y publicación de veredictos, la asignación de sellos y la gestión de la galería fotográfica de mesa real.

## Requerimientos

### Requerimiento: Visibilidad Condicional del Botón Editorial
El botón `[ 🛡️ Gestionar Veredicto Fundador ]` DEBE ser visible en la ficha del juego `/juegos/{slug}` EXCLUSIVAMENTE si el usuario autenticado posee el rol `FoundingTeam` o `Moderator`.

#### Escenario: Usuario de la mesa fundadora visualiza el botón
- DADO un usuario con rol `FoundingTeam`
- CUANDO accede a la ficha de cualquier juego
- ENTONCES la interfaz DEBE mostrar el botón `[ 🛡️ Gestionar Veredicto Fundador ]`.

#### Escenario: Usuario estándar no visualiza el botón
- DADO un usuario con rol regular o no autenticado
- CUANDO accede a la ficha de un juego
- ENTONCES la interfaz NO DEBE mostrar el botón de gestión editorial.

---

### Requerimiento: Modal Interactivo de Redacción (`FoundingVerdictModal`)
Al accionar el botón de gestión, el sistema DEBE abrir un modal ergonómico (`FoundingVerdictModal`) precargado con el veredicto existente si ya fue creado, o un formulario en blanco si es la primera redacción.

#### Escenario: Carga de veredicto existente en el formulario
- DADO un juego con veredicto previo emitido
- CUANDO el moderador pulsa `[ 🛡️ Gestionar Veredicto Fundador ]`
- ENTONCES el modal DEBE abrirse mostrando el sello actual, el texto del análisis general, análisis 2J, análisis familiar y la lista de fotos con sus descripciones para permitir edición directa.

---

### Requerimiento: Gestión de Fotos de Mesa Real en Modal
El modal DEBE permitir añadir URLs de hasta 3 fotografías reales, especificar un pie de foto para cada una, previsualizar la miniatura y eliminar fotos antes de guardar.

#### Escenario: Añadir y previsualizar foto en tiempo real
- DADO el modal editorial abierto
- CUANDO el usuario introduce una URL de imagen válida y un pie de foto descriptivo y pulsa `[ + Añadir Foto ]`
- ENTONCES la foto DEBE agregarse a la lista temporal mostrándose la miniatura de previsualización.

#### Escenario: Guardar veredicto y refrescar ficha
- DADO el formulario con datos válidos completados
- CUANDO el usuario pulsa `[ 💾 Publicar Veredicto Oficial ]`
- ENTONCES el sistema DEBE persistir los cambios, cerrar el modal y actualizar la vista del juego en tiempo real reflejando el nuevo veredicto.
