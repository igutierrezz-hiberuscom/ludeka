# Incremento 21: Dashboard de Inicio Editorial, Desacople de Catálogo, Limpieza de Navbar y Enlace Canónico BGG

- **Identificador SDD:** `change-21-home-dashboard`
- **Estado:** ✅ **Completado y Archivado (453 tests pasando al 100%)**
- **Puntos de la Especificación:** Experiencia de Bienvenida Editorial, Navegación Mobile-First, Arquitectura de Dashboard, Integración de Catálogo y Ficha.
- **Objetivo Principal:** Transformar la ruta raíz (`/`) de la aplicación en una portada viva tipo *Dashboard Editorial* al estilo Letterboxd/streaming lúdico, desacoplando el catálogo completo de juegos a su propia ruta (`/catalogo`). La nueva portada agrupa cuatro carriles con desplazamiento horizontal fluido (*mobile-first* táctil): Mejores Juegos (Top 20), Sorteos Activos y Promocionados, Novedades del Sector y Próximos Eventos Lúdicos. Asimismo, se simplifica la barra superior eliminando el selector de temas redundante y se añade a cada ficha de juego un enlace canónico y directo a BoardGameGeek.

---

## 1. Alcance Funcional y Técnico

1. **Desacople de Rutas y Navegación Principal:**
   - La ruta raíz (`/`) pasa a albergar el nuevo componente `HomeDashboard.razor`.
   - La vista actual de catálogo y filtrado exhaustivo se reubica limpiamente en `/catalogo` (`Games.razor` / `CatalogPage.razor`), manteniendo intacta la paginación, filtros de ADN lúdico, duración y comensales.
   - Actualización de los enlaces del menú de navegación móvil y de escritorio para apuntar a `/catalogo`.

2. **Dashboard de Inicio Multi-Carril (`HomeDashboard.razor`):**
   - **Carril 1 — Top 20 Mejores Juegos:**
     - En esta primera fase, se nutre de los 20 títulos más destacados del catálogo ordenados por ranking BGG (o valoración media comunitaria ponderada de Ludeka una vez superado el umbral mínimo de votos).
     - Presentación *mobile-first*: carrusel de tarjetas compactas con carátula vertical con esquinas redondeadas, nota media, semáforo de escalabilidad y año. En móvil se muestran 3 a 4 juegos visibles con desplazamiento táctil horizontal suave (*invisible horizontal scroll snap*, sin barras de scroll invasivas).
   - **Carril 2 — Sorteos Activos y Destacados:**
     - Muestra hasta 20 sorteos vigentes ordenados prioritariamente por sorteos promocionados (`IsPromoted == true`) en cabecera de carril, y a continuación por proximidad de fecha de finalización (`EndsAt` ascendente).
     - Tarjeta compacta con imagen de portada, título del juego/sorteo, organizador/cuenta y badge dinámico de tiempo restante (ej. "Finaliza en 2 días").
   - **Carril 3 — Novedades del Sector:**
     - Muestra hasta 20 novedades recientes de editoriales y tiendas ordenadas cronológicamente por fecha de publicación (`PublishedAt` descendente).
     - Tarjeta visual con imagen, titular de la novedad, editorial/tienda emisora y fecha relativa (ej. "Ayer", "Hace 3 días").
   - **Carril 4 — Próximos Eventos y Ferias Lúdicas:**
     - Visualización horizontal de grandes citas del calendario lúdico (Essen SPIEL, Festival de Córdoba, InterOcio, Gen Con, etc.) ordenadas por proximidad (`StartDate` ascendente).
     - Tarjeta panorámica con imagen del evento, fechas (ej. "11-13 Octubre 2026"), ciudad/formato y enlace directo a la web oficial o a la sección `/eventos`.

3. **Limpieza del Navbar Superior:**
   - Retirar el selector de temas/paleta de colores de la barra de navegación superior (Header/Navbar).
   - Dicha funcionalidad ya se encuentra integrada en el perfil personal del usuario (`/perfil` / configuración de preferencias), evitando ruido visual y carga cognitiva innecesaria en la cabecera.

4. **Enlace Canónico a BoardGameGeek en la Ficha de Juego:**
   - En la cabecera editorial o sección de metadatos de la ficha de juego (`GameDetail.razor`), incorporar un enlace directo y visible: `[ 🌐 Ver en BoardGameGeek ]` apuntando a `https://boardgamegeek.com/boardgame/{BggId}`.
   - El enlace debe abrir en pestaña nueva (`target="_blank" rel="noopener noreferrer"`) e incluir el logotipo/icono de BGG.

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Acceso a la página principal como Dashboard Editorial
  Dado que un usuario entra en la raíz de la web "/"
  Entonces visualiza el Dashboard de Inicio con sus cuatro carriles temáticos
  Y la barra de navegación contiene un enlace directo a "/catalogo"
  Y la cabecera superior no muestra el selector de paleta de colores

Escenario: Desplazamiento táctil en carril de mejores juegos en dispositivo móvil
  Dado un usuario que navega en un dispositivo móvil en "/"
  Cuando visualiza el carril "Top 20 Mejores Juegos"
  Entonces puede desplazar horizontalmente con el dedo el carrusel de 20 juegos
  Y las tarjetas muestran carátula, título, puntuación y semáforo de comensales

Escenario: Prioridad de sorteos promocionados en el Dashboard
  Dado un sorteo promocionado que finaliza dentro de 7 días
  Y un sorteo estándar que finaliza mañana
  Cuando el usuario visualiza el carril de Sorteos en la home
  Entonces el sorteo promocionado aparece en primera posición antes que el estándar

Escenario: Navegación externa a la ficha de BGG
  Dado que un usuario visita la ficha del juego con BggId 13 ("Catan")
  Cuando hace clic en el botón "[ 🌐 Ver en BoardGameGeek ]"
  Entonces se abre una nueva pestaña del navegador en "https://boardgamegeek.com/boardgame/13"
```

---

## 3. Consideraciones Arquitectónicas y Dependencias

- **Rendimiento:** Las consultas para los 4 carriles de la portada deben realizarse en una única consulta o consultas ultra-rápidas indexadas (`Take(20)`), con caché en memoria (*In-Memory Cache* de 5-15 minutos) para garantizar tiempos de respuesta SSR inferiores a 100ms.
- **CSS / UI:** Uso de utilidades Tailwind `flex overflow-x-auto snap-x snap-mandatory scrollbar-none` para asegurar una experiencia táctil idéntica a una aplicación nativa.
