# 01. Catálogo y Ficha Inteligente

## 1. Visión General y Propósito
El módulo de catálogo gestiona los juegos de mesa registrados en Ludeka, proveyendo metadatos editoriales completos, normalización de URLs amigables para SEO (*slugs*), píldoras de ADN Lúdico, el Semáforo Dinámico de Escalabilidad, guía de fundas y enlaces a tiendas especializadas.

---

## 2. Modelo de Dominio (`Ludeka.Core`)

### 2.1 Entidad Raíz: `Game`
Ubicación: [`src/Ludeka.Core/Entities/Game.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/Game.cs)

- **Identificadores:** `Id` (Guid), `BggId` (int), `Slug` (string único normalizado sin diacríticos ni caracteres especiales).
- **Títulos:** `OriginalTitle` (string), `SpanishTitle` (string comercial en España con fallback automático a `OriginalTitle`).
- **Créditos y Publicación:** `Designer`, `Publisher`, `YearPublished`.
- **Imágenes:** `CoverImageUrl` (alta definición con `fetchpriority="high"`), `ThumbnailUrl`.
- **Calificaciones:** `BggRating` (double), `BggRank` (int?), `LudistRating` (double [0.0 - 10.0] ponderado por la comunidad hispana).
- **ADN Lúdico:**
  - `Confrontation`: Enum `ConfrontationType` (`Competitive`, `Cooperative`, `SemiCooperative`, `HiddenRoles`, `TeamVsTeam`).
  - `Style`: Enum `GameStyle` (`Eurogame`, `Ameritrash`, `PartyGame`, `Filler`, `Abstract`, `Wargame`, `Deckbuilder`, `Drafting`, `DungeonCrawler`, `Legacy`, `RollAndWrite`, `SocialDeduction`, `Trivia`, `Dexterity`, `EngineBuilding`, `WorkerPlacement`).
  - `IsOfficialSolo`: booleano indicativo de variante oficial para 1 persona.
  - `Age`: Value Object [`AgeRating`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/AgeRating.cs) (`BoxAge` vs. `CommunityAge`).
  - `Language`: Enum `LanguageDependence` (`None`, `Low`, `Moderate`, `High`, `Critical`).
  - `Footprint`: Enum `TableFootprint` (`SmallTable`, `StandardTable`, `LargeTable`, `TableMonster`).
  - `Duration`: Value Object [`GameDuration`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/GameDuration.cs) (`MinMinutes`, `MaxMinutes`, `EstimatedPerPlayerMinutes`).
- **Semáforo de Escalabilidad:** Colección de [`ScalabilityEntry`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/ScalabilityEntry.cs) (1 a 7+ comensales).
  - Estados: `MustPlay` (🟢 Imprescindible), `Recommended` (🟡 Recomendado), `NotRecommended` (🔴 No recomendado).
  - Votos: `BestVotes`, `RecommendedVotes`, `NotRecommendedVotes`.
  - Etiqueta calculada en tiempo real: `IdealPlayersLabel` (ej. *"Ideal a 2 y 4 jugadores"*).
- **Guía de Fundas de Cartas:** Colección de [`SleeveItem`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/SleeveItem.cs) (`FormatName`, `WidthMm`, `HeightMm`, `CardCount`, `AffiliateUrl`).
- **Enlaces de Compra en Tiendas:** Colección de [`GamePurchaseLink`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/GamePurchaseLink.cs) (`StoreName`, `AffiliateUrl`, `Price`, `Currency`, `InStock`, `Badge`, `AffiliateTag`).

---

## 3. Capa de Aplicación (`Ludeka.Application`)

- **Contrato:** [`IGameRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IGameRepository.cs)
  - `GetBySlugAsync(slug, ct)`
  - `GetByBggIdAsync(bggId, ct)`
  - `SearchAsync(criteria, page, pageSize, ct)`
  - `GetTopGamesAsync(limit, ct)`
- **Servicio y Caché:**
  - [`CatalogService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Catalog/CatalogService.cs)
  - [`CachedCatalogService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Catalog/CachedCatalogService.cs): Decorador sobre `ICatalogService` con `IMemoryCache` (TTL de 10 minutos e invalidación reactiva por slug).
- **DTOs:** [`GameSummaryDto`](file:///c:/repos/Ludeka/src/Ludeka.Application/DTOs/GameSummaryDto.cs), [`GameDetailDto`](file:///c:/repos/Ludeka/src/Ludeka.Application/DTOs/GameDetailDto.cs).

---

## 4. Persistencia y Base de Datos (`Ludeka.Infrastructure`)

- Tabla `Games` mapeada mediante EF Core en `LudekaDbContext`.
- Value Objects y colecciones mapeados como JSON en SQLite:
  - `Scalability` (`OwnsMany.ToJson()`)
  - `Sleeves` (`OwnsMany.ToJson()`)
  - `PurchaseLinks` (`OwnsMany.ToJson()`)
- Índice único en `Slug` e índice en `BggId`.
- Reconciliación preventiva en tiempo de ejecución: [`SqliteSchemaMigrator`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteSchemaMigrator.cs).

---

## 5. Componentes de Interfaz (`Ludeka.Web`)

- [`Home.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/Home.razor): Buscador reactivo, filtros por estilo y comensales, carrusel de destacados. INC-36: cabecera editorial compartida (`PageHeaderEditorial`), búsqueda en bloque propio y tira de filtros con `scrollbar-none` (ver [módulo 24](file:///c:/repos/Ludeka/docs/specs/sistema/24-fundaciones-editoriales-y-componentes.md)).
- [`GameDetail.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/GameDetail.razor): Ficha inteligente completa. INC-36: tokenización editorial (estados con `--state-*`, botones de marca con `--on-brand`), back-bar con `flex-wrap` y acciones de moderación agrupadas, `<PageTitle>` «Ludeka» (corrige la errata «Ludeca») y estado «juego no encontrado» legible en los 5 temas (ver [módulo 24](file:///c:/repos/Ludeka/docs/specs/sistema/24-fundaciones-editoriales-y-componentes.md)).
- [`GameCard.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/GameCard.razor): Tarjeta de catálogo con badge de 3 segundos, portada con aspect ratio fijo y selector de estado.
- [`PurchaseLinksSection.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/PurchaseLinksSection.razor): Enlaces de compra contextuales con código de afiliado y enlace a transparencia.
