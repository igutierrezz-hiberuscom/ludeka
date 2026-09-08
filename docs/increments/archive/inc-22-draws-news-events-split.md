# Incremento 22: Segregación de Sorteos, Novedades y Nuevo Módulo de Grandes Eventos Lúdicos

- **Identificador SDD:** `change-22-draws-news-events-split`
- **Estado:** ✅ **Completado y Archivado (472 tests pasando al 100%)**
- **Puntos de la Especificación:** Reestructuración de la Navegación de Radar, Gestión de Eventos y Convenciones, Ingesta y Carga de Imágenes de Portada, Gobernanza de Sorteos Promocionados.
- **Objetivo Principal:** Disolver la sección aglutinadora de "Radar" para estructurar tres verticales independientes con sus correspondientes rutas y menús: `/sorteos`, `/novedades` y la nueva sección `/eventos`. Además, se implementa la captura y persistencia de la imagen original de Instagram en sorteos y novedades (o la carga manual de imagen por moderadores), el control administrativo de sorteos promocionados (`IsPromoted`) y el ciclo de vida completo para ferias y convenciones lúdicas masivas.

---

## 1. Alcance Funcional y Técnico

1. **Reestructuración de Rutas y Navegación:**
   - Supresión del concepto monolítico de "Radar" en el menú principal y barra inferior.
   - Creación de tres accesos diferenciados:
     - `🎁 Sorteos` (`/sorteos`): Radar especializado de sorteos externos de la comunidad.
     - `📰 Novedades` (`/novedades`): Línea temporal de anuncios, lanzamientos y primicias editoriales.
     - `🎪 Eventos` (`/eventos`): Calendario oficial de ferias, jornadas y convenciones del sector.

2. **Módulo Completo de Eventos y Ferias Lúdicas (`/eventos`):**
   - **Enfoque de Negocio:** Diseñado para macro-eventos, ferias y festivales de juegos de mesa (ej. *Festival Internacional de Juegos de Córdoba*, *InterOcio Madrid*, *Essen SPIEL*, *Gen Con*, *DAU Barcelona*), excluyendo torneos locales en tiendas de barrio.
   - **Modelo de Datos (`BoardGameEvent`):**
     - `Id`, `Title`, `Description`, `ImageUrl`, `StartDate`, `EndDate`, `Location` (Ciudad, Recinto), `WebsiteUrl`, `Organizer`, `IsOfficial`.
   - **Visualización Comunitaria:**
     - Listado ordenado cronológicamente por fecha de inicio (`StartDate >= Hoy` en orden ascendente), con pestañas para eventos futuros y archivo de eventos pasados.
     - Ficha / tarjeta visual con fecha formateada en calendario, cartel promocional, días restantes ("En 18 días") y botón saliente a la web oficial de venta de entradas o información.
   - **Gestión Editorial de Eventos (`/admin/eventos`):**
     - Formulario para moderadores y Mesa Fundadora: alta, edición, baja y subida directa de carteles/imágenes promocionales.

3. **Imágenes de Instagram y Carga Manual en Sorteos y Novedades:**
   - **Ingesta Automatizada:** En los sorteos y novedades detectados mediante los bots/batches de Instagram, persistir y mostrar la URL directa de la imagen del post de Instagram (`MediaUrl` / `ThumbnailUrl`).
   - **Carga Manual para Moderadores:** Si un moderador da de alta un sorteo o novedad manualmente desde la web, dispondrá del componente de subida de imágenes (o selector de URL externa), alojando el asset en el almacenamiento del sistema (`/uploads/draws` o `/uploads/news`).

4. **Sorteos Promocionados y Filtros Avanzados (`/sorteos`):**
   - **Propiedad de Dominio:** `IsPromoted` (booleano, por defecto `false`).
   - **Control de Acceso:** La casilla de verificación *Promocionado* solo es visible y editable para usuarios con roles `FoundingTeam` o `Moderator`.
   - **Regla de Ordenación:** Los sorteos marcados como promocionados se sitúan siempre en las primeras posiciones de la lista y del carril de la home, con un badge distintivo "⭐ Promocionado", ordenándose entre ellos por fecha de caducidad. Los sorteos estándar se ordenan a continuación por fecha de expiración inminente (`EndsAt` ascendente).

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Segregación de menús en la navegación principal
  Dado un usuario que navega por Ludeka
  Cuando inspecciona el menú principal
  Entonces no visualiza la sección genérica "Radar"
  Y visualiza accesos independientes para "Sorteos", "Novedades" y "Eventos"

Escenario: Consulta del calendario de eventos lúdicos
  Dado que existen los eventos "Festival de Córdoba" (Octubre) e "InterOcio" (Marzo siguiente)
  Cuando el usuario accede a "/eventos"
  Entonces los eventos aparecen ordenados por fecha de inicio más cercana
  Y cada tarjeta muestra el cartel, las fechas de celebración y el enlace oficial externo

Escenario: Marcado de sorteo promocionado por un moderador
  Dado un moderador autenticado con permiso "CanResolveReports" en "/sorteos"
  Cuando edita un sorteo y activa la casilla "Promocionado"
  Entonces el sorteo adquiere el estado "IsPromoted = true"
  Y pasa a figurar en la cabecera de la lista de sorteos con el distintivo "⭐ Promocionado"
  Pero un usuario comunitario no puede ver ni conmutar la casilla "Promocionado"

Escenario: Subida de imagen al crear una novedad manual
  Dado un moderador en el formulario de nueva publicación en "/novedades"
  Cuando carga un archivo de imagen en formato PNG/JPG
  Entonces la imagen se almacena en el servidor
  Y la novedad se publica luciendo dicha carátula visual en el listado y en el dashboard
```

---

## 3. Consideraciones Arquitectónicas y Dependencias

- **Persistencia:** Nuevas tablas en SQLite: `BoardGameEvents` y migración de columnas en `CommunityDraws` (`IsPromoted`, `ImageUrl`) y `CommunityNews` (`ImageUrl`).
- **Seguridad:** Autorización estricta por roles mediante directivas Blazor `AuthorizeView` y validación de seguridad en la capa de aplicación para la mutación de `IsPromoted`.
