# Incremento 5: Importador BGG en 1 Clic y Auto-Catalogación

- **Identificador SDD:** `change-05-bgg-importer`
- **Puntos del MVP cubiertos:** 8.1, 8.2, 8.3 (Importación, Cola nocturna y Búsqueda asistida).
- **Estado:** ✅ **Completado y Archivado** (Commit `95e656d`).

---

## 1. Alcance Funcional Entregado

1. **Importación en 1 Clic desde BGG:**
   - El usuario introduce su nombre de usuario de BGG y se importan sus listas (`Owned`, `Wishlist`, `WantToBuy`).
2. **Cruce Inteligente con el Catálogo Local:**
   - Vinculación inmediata para juegos ya existentes en Ludeka.
   - Los títulos no catalogados pasan a `⏳ En cola de catalogación` en el perfil del usuario.
3. **Cola Nocturna y Auto-Catalogación:**
   - Registro de peticiones en `PendingBggImports` ordenadas por demanda comunitaria (`RequestedCount`).
   - Procesamiento por lotes con promoción atómica de las colecciones de usuario en espera cuando el juego se incorpora al catálogo.
4. **Buscador Asistido en Vivo contra BGG (`/xmlapi2/search`):**
   - Modal asistido para buscar títulos directamente en BGG y añadirlos sin duplicados mediante su `BggId` oficial.

---

## 2. Artefactos y Componentes Clave

- **Dominio:** [`PendingBggImport`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/PendingBggImport.cs), [`ImportStatus`](file:///c:/repos/Ludeka/src/Ludeka.Core/Enums/ImportStatus.cs).
- **Aplicación:** [`IBggImportService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IBggImportService.cs), [`IBggCatalogQueueService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IBggCatalogQueueService.cs), [`IBggSearchAssistedService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IBggSearchAssistedService.cs).
- **Infraestructura:** [`SqlitePendingBggImportRepository`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqlitePendingBggImportRepository.cs), [`BggXmlApiClient`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Bgg/BggXmlApiClient.cs).
- **Web UI:** [`BggImportModal.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/BggImportModal.razor), [`BggSearchModal.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/BggSearchModal.razor), [`CatalogQueuePanel.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/CatalogQueuePanel.razor).

---

## 3. Verificación

- Pruebas unitarias de parsing XML de BGG, importación con cruce local y cola de auto-catalogación en [`tests/Ludeka.UnitTests`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests).
