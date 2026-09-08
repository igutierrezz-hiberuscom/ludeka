# Incremento 11: Enlaces de Compra en Tiendas y Afiliados Contextuales

- **Identificador SDD:** `change-11-store-affiliate-links`
- **Objetivo Principal:** Permitir a los usuarios encontrar dónde comprar el juego base o sus expansiones al mejor precio en tiendas especializadas y Amazon, con monetización ética y respetuosa sin publicidad invasiva.
- **Estado:** ✅ **Completado y Archivado** (209 tests en verde al 100%).

---

## 1. Alcance Funcional y Técnico Entregado

1. **Value Object `GamePurchaseLink` en el Dominio:**
   - Atributos: `StoreName`, `AffiliateUrl`, `Price`, `Currency`, `InStock`, `Badge` (ej. *"Mejor precio"*, *"Recomendado"*), `AffiliateTag`.
   - Formateo seguro de precios con cultura y disponibilidad ("Consultar" vs precio exacto).
2. **Integración en la Entidad `Game`:**
   - Colección de enlaces contextuales manipulables mediante métodos de negocio (`AddPurchaseLink`, `UpdatePurchaseLinks`, `ClearPurchaseLinks`).
3. **Persistencia SQLite JSON Flexible:**
   - Mapeo EF Core `OwnsMany.ToJson()` para `PurchaseLinks` en `LudekaDbContext`.
   - Reconciliación automática y segura de esquema en [`SqliteSchemaMigrator`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteSchemaMigrator.cs) para bases de datos SQLite preexistentes.
4. **Semillado Real en Tiendas Españolas:**
   - Enlaces contextuales precargados para tiendas de referencia nacional (Zacatrus, Dungeon Marvels, Amazon) con precios actualizados.
5. **Componente UI Blazor Editorial:**
   - [`PurchaseLinksSection.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/PurchaseLinksSection.razor) con diseño de tarjetas de tienda, botones con `rel="noopener noreferrer sponsored"` y enlace al Manifiesto de Transparencia.

---

## 2. Artefactos Clave

- **Dominio:** [`GamePurchaseLink.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/GamePurchaseLink.cs)
- **Web UI:** [`PurchaseLinksSection.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/PurchaseLinksSection.razor)
- **Persistencia:** Migración en [`SqliteSchemaMigrator.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteSchemaMigrator.cs) y sincronización en [`CatalogSeeder.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Seeding/CatalogSeeder.cs).

---

## 3. Verificación

- Pruebas unitarias de dominio y persistencia SQLite en [`tests/Ludeka.UnitTests/Domain/GamePurchaseLinkTests.cs`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests/Domain/GamePurchaseLinkTests.cs) y [`tests/Ludeka.UnitTests/Infrastructure/SqlitePurchaseLinksPersistenceTests.cs`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests/Infrastructure/SqlitePurchaseLinksPersistenceTests.cs).
