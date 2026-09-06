# Especificación: multimedia-hub-tabs

## Propósito
Definir la organización y experiencia de visualización multimedia en la ficha del juego (`GameDetail.razor`), garantizando una navegación por pestañas horizontales limpias que segreguen de manera estricta los distintos formatos visuales (16:9, 1:1 y 9:16) y permitan la reproducción embebida en modal o navegación a la fuente oficial.

## Requerimientos

### Requerimiento: Segregación Estricta de Formatos por Pestañas
El sistema DEBE presentar los contenidos multimedia agrupados en 3 pestañas horizontales independientes:
1. 🎬 **Tutoriales (YouTube)**: Contenidos orientados a explicación de reglas y montaje.
2. 🎲 **Partidas Completas (YouTube)**: Sesiones de juego completas.
3. 💬 **Opiniones y Redes (Instagram + Shorts/Reels)**: Posts fotográficos de Instagram y vídeos verticales breves.

#### Escenario: Visualización de Pestaña de Tutoriales
- DADO un juego con tutoriales disponibles
- CUANDO el usuario selecciona la pestaña de Tutoriales
- ENTONCES el sistema DEBE mostrar tarjetas en formato apaisado 16:9 con:
  - Imagen de miniatura de alta resolución
  - Título del tutorial
  - Nombre del canal o autor
  - Duración formateada (ej. `18:45`)
  - Botón de reproducción interactivo

#### Escenario: Visualización de Pestaña de Partidas con Badge de Comensales
- DADO un juego con partidas completas registradas
- CUANDO el usuario selecciona la pestaña de Partidas Completas
- ENTONCES el sistema DEBE mostrar cada partida en formato 16:9 incluyendo de forma OBLIGATORIA y PROMINENTE el distintivo de número de jugadores (ej. `"Partida a 2"`, `"Partida a 4"`), canal y duración.

#### Escenario: Visualización de Pestaña de Opiniones y Redes
- DADO un juego con publicaciones sociales registradas
- CUANDO el usuario selecciona la pestaña de Opiniones y Redes
- ENTONCES el sistema DEBE renderizar dos secciones visuales diferenciadas:
  - **Posts de Instagram**: Tarjetas cuadradas 1:1 con foto, autor (`@canal`), contador de likes y extracto de texto.
  - **Reels y Shorts**: Tarjetas en formato vertical 9:16 con icono de reproducción y duración (ej. `0:58`).

---

### Requerimiento: Reproductor Embebido Responsive (`MediaEmbedModal`)
Al interactuar con un elemento de vídeo (Tutorial, Partida o Short), el sistema DEBE permitir su visualización sin abandonar la aplicación.

#### Escenario: Reproducción de vídeo en modal
- DADO un usuario visualizando un tutorial o partida
- CUANDO pulsa sobre la tarjeta o el botón de reproducir
- ENTONCES se DEBE abrir un modal centrado con el reproductor responsive embebido (`iframe` seguro) y un botón para abrir el enlace externo en la plataforma original si el usuario lo desea.

---

### Requerimiento: Estados Vacíos con Tono Editorial
Cuando un juego no cuente con contenidos en alguna de las pestañas, el sistema DEBE mostrar un mensaje con personalidad lúdica invitando a la comunidad.

#### Escenario: Pestaña sin contenidos
- DADO un juego que no tiene partidas grabadas registradas
- CUANDO el usuario pulsa en la pestaña "Partidas Completas"
- ENTONCES el sistema DEBE mostrar una tarjeta informativa con microtexto editorial cercano (ej. *"Aún no hay partidas grabadas para este juego en la comunidad hispana. Si conoces alguna, ¡avísanos en moderación!"*).
