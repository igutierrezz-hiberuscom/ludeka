# 13. Directorio de Editoriales, Creadores y Tiendas con Redes Sociales y Foco Multimedia

## 1. Visión General y Propósito
El directorio lúdico de Ludeka constituye el mapa del ecosistema hispanohablante de juegos de mesa, articulando de forma interconectada tres tipos de entidades fundamentales:
1. **Editoriales (`Publisher`):** Firmas responsables de la publicación y localización de títulos en España y Latinoamérica (ej. Devir Iberia, Maldito Games, Tranjis Games, Asmodee, Stonemaier Games).
2. **Creadores y Autores (`Creator`):** Diseñadores, ilustradores y divulgadores del mundo lúdico (ej. Elizabeth Hargrave, Klaus Teuber, Uwe Rosenberg, Bruno Cathala, Sergio de Análisis Parálisis).
3. **Tiendas Lúdicas (`Store`):** Comercios físicos y en línea especializados en juegos de mesa (ej. Zacatrus!, Cuarto de Juegos, Jugamos Otra, Dungeon Marvels).

Además de ofrecer una experiencia editorial de descubrimiento y fichas individuales ricas en enlaces a redes sociales oficiales (YouTube, Instagram, Twitter/X, Discord, Twitch, TikTok, BGG, Podcast), el directorio actúa como el **padrón algorítmico dinámico para el foco de búsqueda audiovisual en YouTube (INC-14)**: cualquier canal de YouTube registrado en una entidad es incorporado de manera automática al radar de canales prioritarios.

---

## 2. Modelo de Dominio e Invariantes (`Ludeka.Core`)

### 2.1 Enumerados y Value Objects
- **`SocialPlatform`**: Plataformas soportadas: `Website`, `YouTube`, `Instagram`, `Twitter`, `Discord`, `Facebook`, `BoardGameGeek`, `Twitch`, `TikTok`, `Other`.
- **`StoreType`**: Clasificación del canal de venta: `PhysicalOnly`, `OnlineOnly`, `Hybrid`.
- **`SocialNetworkLink`**: Value object inmutable que almacena `Platform`, `Url`, `Handle` (ej. `@devirtv`) y `Title`. Incluye lógica de normalización y extracción del identificador del canal/perfil.

### 2.2 Entidades
- **`Publisher.cs`**: Nombre, slug único autogenerado o asignado, país, ciudad, descripción/historial editorial, logotipo, web oficial y colección `SocialLinks`.
- **`Creator.cs`**: Nombre, slug único, nacionalidad, biografía, fotografía/avatar, ID de persona BGG, web personal y colección `SocialLinks`.
- **`Store.cs`**: Nombre, slug único, tipo (`StoreType`), ciudad, dirección física, descripción de servicios, logotipo, web de compras, código de afiliado para ingresos compartidos, indicador de programa de fidelidad y colección `SocialLinks`.

---

## 3. Capa de Aplicación (`Ludeka.Application`)

- Contratos de repositorio: [`IPublisherRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IPublisherRepository.cs), [`ICreatorRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/ICreatorRepository.cs), [`IStoreRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IStoreRepository.cs) y ampliación de [`IGameRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IGameRepository.cs) con métodos `GetByPublisherAsync`, `GetByDesignerAsync` y `GetAllGamesAsync`.
- Servicios de negocio:
  - [`PublisherService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Directory/PublisherService.cs): Consultas paginadas/filtradas, obtención de ficha por slug, catálogo asociado y alta/edición restringida a moderadores y fundadores.
  - [`CreatorService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Directory/CreatorService.cs): Fichas de autores con cruce directo a sus obras catalogadas en Ludeka.
  - [`StoreService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Directory/StoreService.cs): Fichas de comercios vinculadas en tiempo real con las ofertas de compra activas registradas en `StoreOffersCard` (INC-11).
  - [`ChannelDirectoryProvider`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Directory/ChannelDirectoryProvider.cs): Implementa `IChannelDirectoryProvider` consolidando todos los canales de YouTube registrados en el directorio.

---

## 4. Persistencia e Infraestructura (`Ludeka.Infrastructure`)

- **Persistencia EF Core y SQLite:**
  - `LudekaDbContext`: DbSets para `Publishers`, `Creators` y `Stores`, con serialización JSON nativa para `SocialLinks` mediante `OwnsMany(..., b => b.ToJson())`.
  - `SqliteSchemaMigrator`: Creación idempotente y defensiva de tablas `Publishers`, `Creators` y `Stores` con índices únicos por `Slug` e índices de búsqueda por `Name`.
  - Repositorios: `SqlitePublisherRepository`, `SqliteCreatorRepository` y `SqliteStoreRepository`.
- **Sembrado Inicial Canónico (`DirectorySeeder.cs`):**
  - Editoriales de referencia en español (Devir Iberia, Maldito Games, Tranjis Games, Asmodee, Stonemaier Games).
  - Creadores y autores ilustres (Elizabeth Hargrave, Klaus Teuber, Uwe Rosenberg, Bruno Cathala, Sergio de Análisis Parálisis).
  - Tiendas especializadas (Zacatrus!, Cuarto de Juegos, Dungeon Marvels, Jugamos Otra).
- **Foco Audiovisual Dinámico (`ChannelFocusProvider.cs`):**
  - Conexión con `IServiceScopeFactory` para consultar en demanda los canales del directorio, extendiendo el padrón estático de creadores de contenido de YouTube de INC-14.

---

## 5. Vistas Blazor y Navegación Cruzada (`Ludeka.Web`)

- **Vistas Principales y Detalle:**
  - `/editoriales` y `/editoriales/{Slug}`: Directorio y ficha de editorial con lista de juegos publicados.
  - `/creadores` (o `/autores`) y `/creadores/{Slug}`: Directorio y ficha de creador con biografía y catálogo diseñado.
  - `/tiendas` y `/tiendas/{Slug}`: Directorio y ficha de tienda con ofertas activas cruzadas de afiliación.
- **Navegación Cruzada:**
  - En `GameDetail.razor`, los créditos de Diseñador y Editorial son enlaces directos a sus fichas.
  - En `StoreOffersCard.razor`, los nombres de las tiendas enlazan a su ficha oficial.
  - Menú de navegación principal y pie de página en `MainLayout.razor`.
- **Herramientas de Moderación:**
  - Modales reactivos `PublisherEditModal.razor`, `CreatorEditModal.razor` y `StoreEditModal.razor` con protección de rol (`Moderator` o `FoundingTeam`), autogeneración de slug, gestión de redes dinámicas y cumplimiento WCAG 2.2 AA.
