# Incremento 19: Directorio de Editoriales, Creadores y Tiendas: Fichas, Redes Sociales y Foco Multimedia

- **Identificador SDD:** `change-19-publishers-creators-directory`
- **Estado:** ✅ **Completado** (Superado en SDD change-19 con 413/413 tests en verde)
- **Puntos del MVP cubiertos:** Identidad de autor, ecosistema editorial, directorio lúdico de tiendas, enlaces sociales y foco central de canales de YouTube.
- **Objetivo Principal:** Crear un directorio completo, navegable y gestionable de **Editoriales**, **Creadores** (autores/diseñadores) y **Tiendas** de juegos de mesa, con fichas dedicadas (historia/bio, web oficial, redes sociales, catálogo/ofertas asociadas) y herramientas de moderación. Asimismo, **todos los canales de YouTube registrados en estas entidades constituyen el foco central de monitorización y análisis del motor de YouTube (INC-14)**.

---

## 1. Alcance Funcional Propuesto

1. **Directorio y Fichas de Editoriales:**
   - **Listado General (`/editoriales`):** Directorio visual con buscador reactivo por nombre o país, tarjetas con logo, país, recuento de juegos en catálogo y enlaces rápidos.
   - **Ficha de la Editorial (`/editoriales/{slug}`):** Logotipo, historia, canales oficiales (web, YouTube, Instagram, Discord, Twitter/X) y catálogo de juegos publicados en Ludeka.
2. **Directorio y Fichas de Creadores / Autores:**
   - **Listado General (`/creadores` o `/autores`):** Directorio visual con buscador reactivo, avatar, nacionalidad y juegos asociados.
   - **Ficha del Creador (`/creadores/{slug}`):** Biografía, perfil BGG, web personal, redes sociales (YouTube, Twitter/X, Instagram) y obras diseñadas registradas en el catálogo.
3. **Directorio y Fichas de Tiendas Lúdicas:**
   - **Listado General (`/tiendas`):** Directorio de tiendas de juegos de mesa (físicas y online), con buscador por nombre, tipo (física/online), ciudad/provincia y enlaces.
   - **Ficha de la Tienda (`/tiendas/{slug}`):** Logotipo, descripción, dirección física si procede, web oficial para compras, programas de fidelidad y redes sociales (canal de YouTube con tutoriales/unboxing, cuenta de Instagram).
   - Sinergia con INC-11 (Enlaces de compra y afiliados): vinculación automática entre las ofertas de compra de los juegos y las fichas oficiales de las tiendas.
4. **Foco Audiovisual Central para Análisis de YouTube:**
   - Todos los canales de YouTube vinculados a las editoriales (ej. Devir TV, Asmodee, Tranjis Games), a los creadores/divulgadores (ej. Análisis Parálisis, Meepletopía, El Agujero de Hobbit, etc.) y a las tiendas (ej. Zacatrus!) se agregan automáticamente como el **padrón oficial de canales hispanos de referencia**.
   - El motor de búsqueda quirúrgica de YouTube (INC-14) consulta este padrón dinámico para ponderar la relevancia y priorizar sugerencias de tutoriales, partidas y vídeos de "cómo funciona".
5. **Navegación Cruzada desde las Fichas de Juego (`GameDetail.razor`):**
   - Los campos `Editorial` y `Diseñador` enlazan directamente a sus fichas `/editoriales/{slug}` y `/creadores/{slug}`.
   - Las tiendas con ofertas activas para el juego enlazan a `/tiendas/{slug}`.
6. **Herramientas de Moderación (Rol `Moderator` o `FoundingTeam`):**
   - Botones `[ ➕ Nueva Editorial ]`, `[ ➕ Nuevo Creador ]` y `[ ➕ Nueva Tienda ]` con modales reactivos para alta inmediata y generación automática de slug.
   - Botón contextual `[ ✏️ Editar Ficha ]` para actualizar información, logotipos, enlaces web y perfiles sociales al instante.

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Visitante consulta la ficha de una tienda con sus redes y ofertas
  Dado que un usuario entra en "/tiendas/zacatrus"
  Cuando la página carga correctamente
  Entonces visualiza el logo, descripción y ubicación de Zacatrus
  Y visualiza el enlace a su tienda online y a su canal oficial de YouTube
  Y visualiza las ofertas de compra de juegos vinculadas a esta tienda

Escenario: Los canales de YouTube de editoriales, creadores y tiendas nutren el foco de análisis
  Dado que se registra el canal de YouTube de una editorial o tienda
  Cuando el motor de búsqueda quirúrgica de YouTube (INC-14) busca tutoriales o partidas
  Entonces los vídeos pertenecientes a ese canal obtienen la máxima prioridad algorítmica

Escenario: Navegación cruzada desde la ficha de un juego hacia editorial y creador
  Dado un visitante en la ficha del juego "Wingspan"
  Cuando pulsa sobre "Elizabeth Hargrave" o "Stonemaier Games"
  Entonces el sistema navega a su ficha correspondiente con su biografía y redes sociales

Escenario: Moderador da de alta una tienda desde el directorio
  Dado un moderador autenticado en "/tiendas"
  Cuando pulsa "➕ Nueva Tienda" y completa nombre, web y canal de YouTube
  Entonces la tienda se almacena en base de datos y aparece en el directorio de inmediato
```

---

## 3. Arquitectura y Componentes Clave

- **Dominio (`Ludeka.Core`):**
  - Entidades `Publisher.cs`, `Creator.cs` y `Store.cs`.
  - Objeto de valor `SocialNetworkLink.cs` y enumerado `SocialPlatform.cs` (`Website`, `Twitter`, `Instagram`, `YouTube`, `Discord`, `Facebook`, `BGG`).
  - Tipo de tienda `StoreType.cs` (`OnlineOnly`, `PhysicalOnly`, `Hybrid`).
- **Aplicación (`Ludeka.Application`):**
  - Interfaces `IPublisherService.cs`, `ICreatorService.cs`, `IStoreService.cs`.
  - DTOs correspondientes (`PublisherDetailDto`, `CreatorDetailDto`, `StoreDetailDto`).
  - Abstracción `IChannelDirectoryProvider` que extrae todos los canales de YouTube registrados para alimentar `IYouTubeSearchService`.
- **Infraestructura (`Ludeka.Infrastructure`):**
  - Repositorios `SqlitePublisherRepository.cs`, `SqliteCreatorRepository.cs` y `SqliteStoreRepository.cs`.
  - Tablas SQLite reconciliadas en `LudekaDbContext`.
- **Presentación Web (`Ludeka.Web`):**
  - Vistas `/editoriales`, `/editoriales/{slug}`, `/creadores`, `/creadores/{slug}`, `/tiendas`, `/tiendas/{slug}`.
  - Modales de edición y creación de moderador.
