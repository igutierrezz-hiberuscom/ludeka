# Exploración: change-06-automation-community (Incremento 6: Automatización Omnicanal, Radar de Sorteos y Comunidad)

## 1. Estado Actual del Monorepo y la Solución

### 1.1 Arquitectura y Proyectos (.NET 10 y C# 13)
- **Solución `Ludeka.slnx`:** 4 capas en Clean Architecture (`Ludeka.Core`, `Ludeka.Application`, `Ludeka.Infrastructure`, `Ludeka.Web`) y proyecto de pruebas `Ludeka.UnitTests`.
- **125 pruebas unitarias y de integración pasando al 100%**:
  - Incremento 1 (`change-01-core-catalog`): Catálogo base, fichas inteligentes, badges de ADN lúdico, semáforo dinámico de escalabilidad hasta 7+ jugadores, doble rating (BGG vs Ludist), guía de fundas y SQLite con colecciones JSON nativas.
  - Incremento 2 (`change-02-library-loans`): Ludoteca personal en 4 estados (`InCollection`, `Played`, `Wishlist`, `WantToBuy`), préstamos con devolución en 1 clic y reseñas comunitarias con votos de escalabilidad.
  - Incremento 3 (`change-03-founding-verdict`): Veredicto oficial de la Mesa Fundadora con foco en parejas y familias, galería de fotos reales de mesa y sustitución de síntesis IA.
  - Incremento 4 (`change-04-multimedia-hub`): Hub multimedia con pestañas segregadas (Tutoriales YT, Partidas YT con número de jugadores, Opiniones IG/Shorts) y panel táctil de moderación móvil.
  - Incremento 5 (`change-05-bgg-importer`): Importador BGG en 1 clic, cola comunitaria nocturna/priorizada de catalogación para títulos huérfanos y buscador asistido en vivo contra la API de BGG.

### 1.2 Carencias Actuales frente a la Especificación Maestra (Secciones 6, 10 y 11)
1. **Radar de Sorteos y Colaboraciones (Punto 10.1 y 10.3):**
   - No existen entidades ni tablas para registrar sorteos externos o comunitarios.
   - No existe la lógica de negocio para la **fusión de colaboraciones** (cuando una editorial y un influencer sortean el mismo lote, debe presentarse en una tarjeta única unificada).
   - No hay control de expiración automática de sorteos por fecha límite (`DeadlineAt`).
2. **Radar de Lanzamientos Semanales (Punto 10.2):**
   - No hay modelo ni vista para las novedades y reimpresiones de los viernes en tiendas españolas.
3. **Consultorio de Reglas Q&A Estilo StackOverflow (Punto 10.4):**
   - En la ficha del juego (`GameDetail.razor`) no hay pestaña ni sección para resolver dudas de reglamento a mitad de partida.
   - No existen entidades para preguntas, respuestas, votos de utilidad ni marca de "respuesta aceptada".
4. **Motor de Composición Gráfica Omnicanal (Punto 6.1 a 6.3):**
   - No existe una utilidad o componente para generar imágenes 1:1 (formato cuadrado Instagram) con la carátula oficial de BGG, píldora identificativa de estilo/veredicto, y marca tipográfica de Ludeka.
   - No existe generador de textos (copywriting) con hashtags temáticos en español listos para copiar al portapapeles.
5. **Manifiesto de Transparencia de Fondos y Comunidad (Punto 10.5, 11.1 y 11.2):**
   - El pie de página aún no contiene el enlace al Manifiesto de Transparencia ni la explicación ética de los 3 destinos de los fondos recaudados por afiliación/mecenazgo.
   - No existe la página `/transparencia` ni enlace al servidor oficial de Discord.

---

## 2. Requerimientos del Incremento 6

### 2.1 Motor de Automatización Gráfica Omnicanal (Puntos 6.1 - 6.3)
- **Generador de Cartel 1:1 para Redes:**
  - Capacidad de componer una imagen en formato cuadrado (1:1, ej. 1080x1080 o SVG escalable de alta fidelidad) a partir de cualquier juego del catálogo.
  - Composición visual con jerarquía tipográfica editorial:
    - Carátula oficial de alta resolución del juego.
    - Píldora destacada del ADN Lúdico (ej. "⚔️ Eurogame Competitivo") o Veredicto Fundador ("🛡️ Imprescindible a 2").
    - Título comercial en español y año de publicación.
    - Pie de marca con isotipo y lema *"Ludeka • El Letterboxd de los juegos de mesa"*.
  - Vista previa interactiva en modal con botones de acción:
    - `[ 📥 Descargar Imagen PNG/SVG ]`
    - `[ 📋 Copiar Texto para Instagram ]` (con texto formateado y hashtags: `#juegosdemesa #ludeka #boardgames #devir #asmodee` etc.).

### 2.2 Radar de Sorteos y Novedades Semanales (Puntos 10.1, 10.2 y 10.3)
- **Entidad `Giveaway`:**
  - Título, Organizador principal (editorial/tienda), Colaborador opcional (influencer/canal), Enlace a la publicación oficial, Plataforma (Instagram, X, YouTube, Comunidad), Fecha límite (`DeadlineAt`), Juego asociado (`GameId` o título libre), Imagen/portada, Exclusivo de la comunidad (`IsCommunityExclusive`).
  - Regla de negocio: Sorteos caducados (`DeadlineAt < Ahora`) se marcan como expirados automáticamente.
  - Regla de negocio de **Fusión de Colaboraciones:** Posibilidad de vincular un segundo creador a un sorteo existente en lugar de duplicar la entrada.
- **Entidad `WeeklyRelease`:**
  - Título del juego, Editorial, Fecha de llegada a tiendas (viernes de la semana), Carátula, PVP estimado, Distintivo de Reimpresión vs Novedad absoluta.
- **Vista interactiva `/sorteos` (o `/radar`):**
  - Selector de pestañas: `🎁 Radar de Sorteos` y `🚀 Novedades del Viernes`.
  - Tarjetas visuales con countdown ("Quedan 2 días", "Finaliza hoy", "Expirado").
  - Modal o formulario rápido para sugerir o registrar nuevos sorteos (con validación de rol de moderador o aporte comunitario).

### 2.3 Consultorio de Reglas Q&A (Punto 10.4)
- **Entidades `RuleQuestion`, `RuleAnswer` y `RuleVote`:**
  - Preguntas vinculadas a un juego (`GameId`).
  - Título breve de la duda + cuerpo explicativo.
  - Votos comunitarios (+1) para priorizar las dudas más frecuentes.
  - Respuestas comunitarias con referencia opcional al manual (ej. *"Pág. 14, sección 3"*).
  - Votos en respuestas.
  - Marcado de **Respuesta Aceptada (`IsAccepted`)**: Exclusivo para el autor de la pregunta o miembros de la Mesa Fundadora / Moderadores.
- **Integración en la Ficha de Juego (`GameDetail.razor`):**
  - Pestaña o sección dedicada con diseño limpio estilo StackOverflow para lectura ágil durante una partida física.

### 2.4 Manifiesto de Transparencia y Comunidad (Puntos 10.5, 11.1 y 11.2)
- **Página `/transparencia`:**
  - Explicación de los 3 pilares éticos de financiación:
    1. Servidores y base de datos (coste 0€ o mínimo).
    2. Adquisición de juegos para análisis en mesa propia a 2 jugadores y familias.
    3. Financiación de sorteos comunitarios mensuales.
  - Insignia de Mecenas (Ko-fi / Buy Me a Coffee) y botón de unión al Discord oficial.
- **Actualización del Pie de Página (`MainLayout.razor`):**
  - Enlaces a `/radar`, `/transparencia`, Discord oficial y actualización del resumen del MVP completo.

---

## 3. Estrategia de Implementación y Arquitectura Limpia
- **Dominio (`src/Ludeka.Core`):**
  - Entidades: `Giveaway`, `WeeklyRelease`, `RuleQuestion`, `RuleAnswer`, `RuleVote`.
  - Enums: `GiveawayPlatform`, `ReleaseType`.
  - Métodos con lógica pura y validaciones de invariantes.
- **Aplicación (`src/Ludeka.Application`):**
  - Interfaces de repositorio: `IGiveawayRepository`, `IWeeklyReleaseRepository`, `IRuleQuestionRepository`.
  - Servicios y Casos de Uso:
    - `IGiveawayService`: Listado activo con filtro de expiración, fusión de colaboraciones, registro de sorteos.
    - `IWeeklyReleaseService`: Listado de lanzamientos vigentes.
    - `IRuleQAService`: Crear pregunta, responder, votar pregunta/respuesta, marcar respuesta aceptada.
    - `ISocialCardService`: Generación de SVG/HTML5 renderizable en 1:1 y generador de copy para redes sociales.
  - DTOs correspondientes.
- **Infraestructura (`src/Ludeka.Infrastructure`):**
  - Configuración EF Core 10 en `LudekaDbContext`: `DbSet<Giveaway>`, `DbSet<WeeklyRelease>`, `DbSet<RuleQuestion>`, `DbSet<RuleAnswer>`, `DbSet<RuleVote>`.
  - Repositorios SQLite concretos.
  - Sembrado de datos iniciales (`CatalogSeeder.cs`) para tener sorteos vigentes, novedades de este viernes y preguntas resueltas de ejemplo.
- **Interfaz Blazor (`src/Ludeka.Web`):**
  - Componentes Razor:
    - `SocialCardModal.razor`: Modal interactivo con previsualización del cartel 1:1, selector de estilo, copia de texto y descarga.
    - `RuleQuestionsSection.razor`: Sección de Q&A con votos y respuestas aceptadas.
    - `GiveawayCard.razor`: Tarjeta de sorteo con badge de caducidad y colaboradores fusionados.
  - Páginas:
    - `Radar.razor` (`/radar` y `/sorteos`): Radar de sorteos y novedades de los viernes.
    - `Transparency.razor` (`/transparencia`): Manifiesto de transparencia de fondos.
  - Ajustes en `GameDetail.razor` y `MainLayout.razor`.
- **Pruebas Unitarias (`tests/Ludeka.UnitTests`):**
  - Pruebas de dominio para `Giveaway`, `RuleQuestion`, `RuleAnswer`.
  - Pruebas de aplicación para `GiveawayService`, `RuleQAService`, `SocialCardService`.
  - Pruebas de repositorios SQLite en memoria.
