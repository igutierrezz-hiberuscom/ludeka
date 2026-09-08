# Incremento 1: Catálogo Base, Ficha Inteligente y Semáforo de Escalabilidad

- **Identificador SDD:** `change-01-core-catalog`
- **Puntos del MVP cubiertos:** 3.1 a 3.6, 9.1, 9.2 (Fichas, ADN Lúdico, Ratings y Semáforo).
- **Estado:** ✅ **Completado y Archivado** (Commit `a9314da`).

---

## 1. Alcance Funcional Entregado

1. **Modelo de datos del Juego:** Título original, nombre comercial en español, diseñador, editorial, año, URL carátula oficial BGG y slug normalizado para SEO.
2. **Píldoras de ADN Lúdico (Badges):** Confrontación (Cooperativo/Competitivo/Roles), Estilo (Euro/Ameritrash/Party/Filler), Modo Solitario oficial.
3. **Semáforo Dinámico de Escalabilidad (1 a 7+ jugadores):** Cálculo de estado (🟢 *Imprescindible* / `MustPlay` \| 🟡 *Recomendado* / `Recommended` \| 🔴 *No recomendado* / `NotRecommended`) y etiqueta calculada *"Ideal a X jugadores"*.
4. **Edad de caja legal vs. Edad real comunitaria:** Indicador visual comparativo y factor de dependencia del idioma (Nula/Baja/Alta).
5. **Duración y Huella en Mesa:** Duración estimada total y por comensal, junto con el indicador de huella en mesa (*Mesa pequeña*, *Comedor*, *Monstruo de mesa*).
6. **Guía de Fundas de Cartas:** Medidas en milímetros, cantidad requerida y enlaces contextuales.
7. **Cliente de Ingesta BGG XMLAPI2:** Integración para sincronizar y enriquecer fichas del Top de BGG.
8. **Componentes UI Blazor con Tailwind CSS:** Cabecera visual editorial, ficha con badges compactos de lectura en 3 segundos y buscador reactivo con debounce.

---

## 2. Artefactos y Componentes Clave

- **Dominio:** [`Game`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/Game.cs), [`ScalabilityEntry`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/ScalabilityEntry.cs), [`AgeRating`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/AgeRating.cs), [`GameDuration`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/GameDuration.cs), [`SleeveItem`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/SleeveItem.cs).
- **Aplicación:** [`ICatalogService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/ICatalogService.cs), [`CatalogService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Catalog/CatalogService.cs), [`GameDetailDto`](file:///c:/repos/Ludeka/src/Ludeka.Application/DTOs/GameDetailDto.cs), [`GameSummaryDto`](file:///c:/repos/Ludeka/src/Ludeka.Application/DTOs/GameSummaryDto.cs).
- **Infraestructura:** [`SqliteGameRepository`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteGameRepository.cs), [`BggXmlApiClient`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Bgg/BggXmlApiClient.cs).
- **Web UI:** [`Home.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/Home.razor), [`GameDetail.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/GameDetail.razor), [`GameCard.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/GameCard.razor).

---

## 3. Verificación

- Pruebas unitarias de dominio, cálculo de escalabilidad y repositorio en [`tests/Ludeka.UnitTests`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests).
