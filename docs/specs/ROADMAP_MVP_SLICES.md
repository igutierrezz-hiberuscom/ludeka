# 🗺️ Ludeka / Ludist — Hoja de Ruta de Incrementos SDD (Roadmap MVP & Post-MVP)

Este documento desglosa los bloques de la especificación funcional maestra (`LUDIST_SPEC_FUNCIONAL_MVP.md`) en **Vertical Slices (Incrementos Entregables)**. Los 6 primeros incrementos del MVP inicial se encuentran **100% completados, verificados y archivados**, dando paso a la fase de Consolidación, Comunidad y Operaciones. Cada incremento atraviesa todas las capas de la arquitectura (Dominio -> Casos de Uso -> Infraestructura -> UI Blazor -> Tests) bajo el ciclo formal **Spec-Driven Development (SDD)**.

---

## Incremento 1: Catálogo Base, Ficha Inteligente y Semáforo de Escalabilidad
- **Identificador SDD:** `change-01-core-catalog`
- **Puntos del MVP cubiertos:** 3.1 a 3.6, 9.1, 9.2 (Fichas, ADN Lúdico, Ratings y Semáforo).
- **Estado:** ✅ **Completado y Archivado** (Commit `a9314da`).
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
- **Estado:** ✅ **Completado y Archivado** (Commit `e4d777e`).
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
- **Estado:** ✅ **Completado y Archivado** (Commit `4404e6b`).
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
- **Estado:** ✅ **Completado y Archivado** (Commit `eb03473`).
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
- **Estado:** ✅ **Completado y Archivado** (Commit `95e656d`).
- **Alcance Funcional:**
  1. Integración con BGG: el usuario introduce su usuario de BGG y se importan sus listas (`Owned`, `Wishlist`).
  2. Cruce con el catálogo local: vinculación inmediata para juegos existentes; juegos no catalogados pasan a `⏳ En cola de catalogación`.
  3. Cola de auto-catalogación para enriquecer títulos pendientes ordenados por popularidad.
  4. Buscador asistido en vivo contra `/xmlapi2/search` de BGG para añadir juegos a mano con su `BggId` único sin duplicados.

---

## Incremento 6: Automatización Omnicanal, Radar de Sorteos y Comunidad
- **Identificador SDD:** `change-06-automation-community`
- **Puntos del MVP cubiertos:** 6.1 a 6.3, 10.1 a 10.5, 11.1, 11.2 (Generador de plantillas, Sorteos, Q&A).
- **Estado:** ✅ **Completado y Archivado** (Commit `f71e98f`, 146 tests en verde).
- **Alcance Funcional:**
  1. Formulario de creación rápida de sorteos externos y novedades de tiendas de los viernes.
  2. Motor de composición de imagen de marca (1:1 cuadrada) para Instagram con portada, píldora identificativa y pie de marca.
  3. Radar de Sorteos en la web con fecha límite de expiración automática.
  4. Consultorio de reglas Q&A estilo StackOverflow por juego (pregunta, respuestas, votos y respuesta aceptada).
  5. Manifiesto de Transparencia de Fondos en el pie de página de la plataforma.

---

## 🚀 Fase Post-MVP: Consolidación, Comunidad y Operaciones

---

## Incremento 7: Compilación de Producción, Optimización de Assets y Rendimiento Web
- **Identificador SDD:** `change-07-production-assets-perf`
- **Objetivo Principal:** Optimización extrema de rendimiento en carga móvil, purga de estilos y cumplimiento estricto de Core Web Vitals y accesibilidad.
- **Estado:** 📋 **Registrado (Pendiente de inicio)**
- **Alcance Funcional y Técnico:**
  1. **Pipeline de Assets y Tailwind CSS:** Configuración de compilación optimizada y purga de clases CSS no utilizadas, minimizando el bundle descargado en dispositivos móviles.
  2. **Auditoría Core Web Vitals:** Optimización de Largest Contentful Paint (LCP < 1.2s), Cumulative Layout Shift (CLS = 0) e Interaction to Next Paint (INP) tanto en Blazor SSR como en componentes interactivos.
  3. **Estrategia de Caché Avanzada:** Implementación de Output Caching y Response Caching en memoria para rutas de lectura intensiva (`/`, `/juegos`, `/radar`, `/transparencia`).
  4. **Optimización Multimedia:** Atributos `loading="lazy"`, `decoding="async"`, ratios de aspecto y dimensiones fijas para evitar saltos de layout en carátulas BGG y fotos reales de mesa.
  5. **Accesibilidad WCAG 2.2 AA:** Auditoría de contraste cromático, navegación completa por teclado, gestión de foco en modales y atributos semánticos ARIA en pestañas y controles.

---

## Incremento 8: Sistema de Notificaciones y Webhooks de Comunidad (Discord & Telegram)
- **Identificador SDD:** `change-08-notifications-webhooks`
- **Objetivo Principal:** Difusión multicanal automatizada para dinamizar la comunidad avisando de eventos clave en Discord y Telegram sin intervención manual.
- **Estado:** 📋 **Registrado (Pendiente de inicio)**
- **Alcance Funcional y Técnico:**
  1. **Motor de Webhooks Multicanal (`ICommunityNotificationService`):** Integración con Discord Webhooks y Telegram Bot API con plantillas enriquecidas (Embeds con color de marca, portada del juego, enlaces directos y botones de acción).
  2. **Cola de Despacho en Segundo Plano (Outbox Pattern / `Channel<T>`):** Desacoplamiento asíncrono mediante `BackgroundService` para no penalizar la latencia de las peticiones HTTP del usuario.
  3. **Eventos Automatizados del Sistema:**
     - ⚠️ **Alerta de Sorteo a punto de expirar:** Aviso automático 24 horas antes del cierre del plazo para maximizar participación comunitaria.
     - 🛍️ **Boletín de Lanzamientos de Viernes:** Resumen automatizado de novedades y reimpresiones en tiendas de juegos de mesa cada viernes por la mañana.
     - 🛡️ **Nuevo Veredicto Fundador publicado:** Notificación con el sello de recomendación (*Imprescindible* / *Recomendado*) y enlace a la ficha.
     - 💡 **Duda de Reglas resuelta:** Difusión de preguntas con solución aceptada para nutrir el conocimiento lúdico común.
  4. **Gestión de Configuración y Seguridad:** Parámetros configurables en `appsettings.json` (habilitar/deshabilitar canales individualmente, URLs de webhook seguras y modo simulado/dry-run para pruebas unitarias).

---

## Incremento 9: Despliegue, Empaquetado Docker y Configuración de Staging/Producción
- **Identificador SDD:** `change-09-docker-deployment-staging`
- **Objetivo Principal:** Empaquetado reproducible, seguro y listo para producción de toda la solución Ludeka para su despliegue en cualquier servidor VPS o entorno en la nube.
- **Estado:** 📋 **Registrado (Pendiente de inicio)**
- **Alcance Funcional y Técnico:**
  1. **Dockerfile Multi-Stage Optimizado:** Imagen de construcción .NET 10 SDK, compilación de frontend y runtime chiseled/alpine ultra-ligero y seguro ejecutándose con usuario no-root.
  2. **Orquestación con Docker Compose (`docker-compose.yml`):** Definición de servicios para entornos local, staging y producción, con volúmenes persistentes para la base de datos SQLite (`ludeka.db`), uploads de fotos de mesa y logs estructurados.
  3. **Endpoints de Health Checks (`/healthz` y `/ready`):** Diagnóstico en tiempo real del estado de la aplicación, conectividad con la base de datos SQLite, espacio en disco y disponibilidad del runtime.
  4. **Seguridad y Gestión de Secretos:** Configuración mediante variables de entorno (`ASPNETCORE_ENVIRONMENT`, cadenas de conexión, tokens y webhooks) sin credenciales en el repositorio.
  5. **Guía Operativa de Despliegue y Mantenimiento:** Documentación técnica paso a paso para despliegue en VPS Linux, procedimientos de backup/restore de la base de datos SQLite y rotación de registros.

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
