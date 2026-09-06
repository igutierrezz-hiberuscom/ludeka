# 🗺️ Ludeka / Ludist — Hoja de Ruta de Incrementos SDD (Roadmap MVP)

Este documento desglosa los 13 bloques de la especificación funcional maestra (`LUDIST_SPEC_FUNCIONAL_MVP.md`) en **6 Vertical Slices (Incrementos Entregables)**. Cada incremento atraviesa todas las capas de la arquitectura (Dominio -> Casos de Uso -> Infraestructura -> UI Blazor -> Tests) y se implementará mediante el ciclo formal **Spec-Driven Development (SDD)**.

---

## Incremento 1: Catálogo Base, Ficha Inteligente y Semáforo de Escalabilidad
- **Identificador SDD:** `change-01-core-catalog`
- **Puntos del MVP cubiertos:** 3.1 a 3.6, 9.1, 9.2 (Fichas, ADN Lúdico, Ratings y Semáforo).
- **Alcance Funcional:**
  1. Modelo de datos del Juego: título original, nombre comercial en español, diseñador, editorial, año, URL carátula oficial BGG.
  2. Píldoras de ADN Lúdico (Badges): Confrontación (Cooperativo/Competitivo/Roles), Estilo (Euro/Ameritrash/Party/Filler), Modo Solitario.
  3. Semáforo Dinámico de Escalabilidad (1 a 7+ jugadores): cálculo de estado (🟢 Imprescindible | 🟡 Recomendado | 🔴 No recomendado) y etiqueta "Ideal a X jugadores".
  4. Edad de caja legal vs. Edad real comunitaria + indicador de dependencia del idioma (Nula/Baja/Alta).
  5. Duración estimada por persona y nivel de huella en mesa (Mesa pequeña, Comedor, Monstruo de mesa).
  6. Cliente de ingesta BGG XMLAPI2 para enriquecer fichas del Top 1.000.
  7. Componentes UI Blazor con Tailwind CSS: cabecera visual editorial, ficha con badges compactos de lectura en 3 segundos y buscador reactivo.

---

## Incremento 2: Ludoteca Personal, Colección en 4 Estados y Préstamos
- **Identificador SDD:** `change-02-library-loans`
- **Puntos del MVP cubiertos:** 7.1, 7.2, 7.3 (Colección, Préstamos y Formulario modular).
- **Alcance Funcional:**
  1. Barra de acción interactiva en la ficha del juego con 4 estados:
     - 🟢 *En mi ludoteca* (físico propio).
     - 🔵 *Jugado* (asociación, bar, amigos).
     - 🟡 *Deseado* (radar de interés).
     - 🔴 *Quiero comprar* (lista de seguimiento de ofertas).
  2. Módulo privado de préstamos ("¿A quién se lo dejé?"): registrar persona/asociación y fecha; listado en perfil con devolución en 1 clic.
  3. Formulario modular de valoración rápida en 45 segundos:
     - Puntuación 1 a 10 y micro-reseña de máximo 280 caracteres.
     - Chips de comensales (1J a 7J+) para votar el semáforo personal.
     - Experiencia infantil opcional (edad mínima sugerida y adaptación de reglas).
  4. Edición de valoración propia con tarjeta destacada encima de opiniones públicas.

---

## Incremento 3: Panel y Veredicto de la Mesa Fundadora
- **Identificador SDD:** `change-03-founding-verdict`
- **Puntos del MVP cubiertos:** 4.1, 4.2 (Ciclo de vida del veredicto y panel editorial).
- **Alcance Funcional:**
  1. Autenticación y roles de usuario: rol `FoundingTeam` / `Moderator`.
  2. Acceso directo desde la ficha: botón `[ 🛡️ Gestionar Veredicto Fundador ]`.
  3. Análisis oficial de la casa con foco en juego a 2 personas (parejas) y familias/niños.
  4. Galería fotográfica de mesa real: subida y visualización de 1 a 3 fotos tomadas en mesa propia.
  5. Sello de recomendación de la mesa fundadora (*Imprescindible*, *Recomendado con adaptaciones*, *Prescindible*).
  6. Algoritmo de reemplazo visual: el veredicto fundador tiene prioridad sobre la síntesis de IA inicial.

---

## Incremento 4: Hub Multimedia (YouTube e Instagram)
- **Identificador SDD:** `change-04-multimedia-hub`
- **Puntos del MVP cubiertos:** 5.1, 5.2, 5.3 (Hub multimedia segregado e ingesta).
- **Alcance Funcional:**
  1. Pestañas horizontales limpias en la ficha sin mezclar formatos:
     - *Pestaña 1:* 🎬 Tutoriales de YouTube (16:9 con canal y duración).
     - *Pestaña 2:* 🎲 Partidas completas de YouTube (16:9 con badge obligatorio de número de jugadores, ej. "Partida a 2").
     - *Pestaña 3:* 💬 Opiniones y Redes (posts cuadrados de Instagram y Reels/Shorts 9:16).
  2. Pipeline de ingesta acotado: búsqueda quirúrgica para juegos del catálogo (máximo 2 mejores vídeos por juego).
  3. Panel de moderación rápida móvil: aprobar/descartar contenidos y asignación de vídeos huérfanos.

---

## Incremento 5: Importador BGG en 1 Clic y Auto-Catalogación
- **Identificador SDD:** `change-05-bgg-importer`
- **Puntos del MVP cubiertos:** 8.1, 8.2, 8.3 (Importación y cola nocturna).
- **Alcance Funcional:**
  1. Integración con BGG: el usuario introduce su usuario de BGG y se importan sus listas (`Owned`, `Wishlist`).
  2. Cruce con el catálogo local: vinculación inmediata para juegos existentes; juegos no catalogados pasan a `⏳ En cola de catalogación`.
  3. Cola de auto-catalogación para enriquecer títulos pendientes ordenados por popularidad.
  4. Buscador asistido en vivo contra `/xmlapi2/search` de BGG para añadir juegos a mano con su `BggId` único sin duplicados.

---

## Incremento 6: Automatización Omnicanal, Radar de Sorteos y Comunidad
- **Identificador SDD:** `change-06-automation-community`
- **Puntos del MVP cubiertos:** 6.1 a 6.3, 10.1 a 10.5, 11.1, 11.2 (Generador de plantillas, Sorteos, Q&A).
- **Alcance Funcional:**
  1. Formulario de creación rápida de sorteos externos y novedades de tiendas de los viernes.
  2. Motor de composición de imagen de marca (1:1 cuadrada) para Instagram con portada, píldora identificativa y pie de marca.
  3. Radar de Sorteos en la web con fecha límite de expiración automática.
  4. Consultorio de reglas Q&A estilo StackOverflow por juego (pregunta, respuestas, votos y respuesta aceptada).
  5. Manifiesto de Transparencia de Fondos en el pie de página de la plataforma.

---

## Convención de Trabajo para Cada Incremento (Ciclo SDD)

Cada incremento se ejecutará siguiendo estrictamente las 7 fases de Spec-Driven Development:
1. `sdd-explore`: Análisis del estado actual del código y requerimientos del slice.
2. `sdd-propose`: Propuesta de arquitectura y alcance con aprobación previa.
3. `sdd-spec`: Especificación de requerimientos técnicos y criterios de aceptación Gherkin.
4. `sdd-design`: Diseño de clases, interfaces, endpoints y componentes Razor.
5. `sdd-tasks`: Checklist de tareas secuenciadas.
6. `sdd-apply`: Implementación rigurosa con pruebas unitarias en verde.
7. `sdd-verify`: Verificación independiente contra requerimientos antes de cerrar el ciclo.
