# Incremento 26: Especificación de Fundas (Sleeves) por Juego y Enlaces de Compra Contextuales

- **Identificador SDD:** `change-26-card-sleeves-spec-stores`
- **Estado:** ✅ **Completado y Archivado (636 tests pasando al 100%)**
- **Puntos de la Especificación:** Ficha Inteligente y ADN Lúdico (INC-01), Enlaces a Tiendas y Afiliados (INC-11), Ingesta BGG XMLAPI2, Utilidades para Jugadores ("Protege tu juego").
- **Objetivo Principal:** Dotar a la ficha de cada juego de una sección especializada en el cuidado de componentes ("Protege tu juego: Fundas"), extrayendo o mapeando las medidas exactas de cartas (ancho y alto en milímetros), el número de cartas y la cantidad estimada de paquetes necesarios. Adicionalmente, se conectan estas especificaciones con enlaces de compra contextuales en tiendas colaboradoras (como Zacatrus), dirigiendo al usuario directamente a la categoría o producto del tamaño de funda exacto requerido.

---

## 1. Alcance Funcional y Técnico

1. **Investigación e Ingesta de Datos de Fundas:**
   - **Fuente Primaria (BGG Links & GeekList):** En BGG XMLAPI2 (`/xmlapi2/thing?id={id}&stats=1`), analizar los enlaces de tipo `boardgamecardsleeve` asociados a las versiones y componentes del juego, así como la base de datos comunitaria de fundas (*BGG Card Sleeve Sizes Database*).
   - **Modelo de Dominio (`SleeveItem` y `StandardSleeveCatalog`):**
     - Formatos de cartas con `FormatName`, `WidthMm`, `HeightMm`, `CardCount`, `PacksNeeded50`, `PacksNeeded100` y `DimensionText`.
     - Catálogo canónico con los 10 formatos universales del hobby (Mini USA, Mini Euro, USA Standard, Chimera, Euro Standard, Standard Card Game, Tarot, Tarot Grande, Cuadrada, Magnum).

2. **Componente Visual en la Ficha ("Protege tu Juego"):**
   - Nueva tarjeta interactiva y limpia en la ficha del juego (`SleeveGuideCard.razor`):
     - Visualización gráfica de la silueta de la carta con sus dimensiones en milímetros (`56 x 87 mm`).
     - Contador total de cartas y cálculo automático de paquetes requeridos (packs de 50 y de 100).
     - Píldora didáctica desplegable con la comparativa de grosores típicos (Standard 50-60 micras vs. Premium 100 micras).

3. **Generador de Enlaces Contextuales a Tiendas:**
   - **Enlace Quirúrgico por Tamaño (`ISleeveStoreUrlResolver`):** En lugar de enviar a la portada genérica de la tienda, los botones de compra generan enlaces parametrizados hacia la búsqueda o categoría exacta de la tienda asociada (Zacatrus, Dungeon Marvels, etc., con su código de afiliado `ref=ludeka`).
   - Respeto del filtrado territorial por país del usuario (`CountryCatalog` e `IUserLocationService` de INC-29).

4. **Gestión y Corrección para Moderadores:**
   - Pestaña "🛡️ Fundas" en el panel editorial (`GameEditorModal.razor`) con presets de catálogo rápido, alta, edición y eliminación de fundas, con persistencia en `GameEditorService` y registro en la bitácora universal de auditoría (`AuditLog`).

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Visualización de especificaciones de fundas en la ficha de un juego
  Dado que un usuario visita la ficha de "7 Wonders Duel"
  Cuando consulta la sección "Protege tu juego"
  Entonces observa que contiene cartas en formato "Mini Euro (44 x 68 mm)" y "Large / Tarot (65 x 100 mm)"
  Y el sistema calcula cuántos paquetes de fundas son necesarios para cubrir todas las cartas

Escenario: Navegación al tamaño de funda exacto en la tienda colaboradora
  Dado un usuario en la sección de fundas de "Terraforming Mars" (cartas 63.5 x 88 mm)
  Cuando pulsa el botón de compra de fundas para Zacatrus
  Entonces la URL generada apunta directamente al catálogo de fundas de 63.5 x 88 mm de Zacatrus
  Y contiene el parámetro de afiliado oficial de Ludeka

Escenario: Corrección manual de medidas de cartas por un moderador
  Dado un juego recién catalogado sin datos de fundas en BGG
  Cuando un moderador edita la ficha y añade "90 cartas de 59 x 92 mm (Euro Standard)"
  Entonces la sección de fundas de la ficha pública se actualiza al instante con las nuevas medidas y paquetes
```

---

## 3. Consideraciones Arquitectónicas y Módulo del Sistema

- **Persistencia:** Almacenamiento en SQLite mediante EF Core `OwnsMany(g => g.Sleeves, b => b.ToJson())`.
- **Módulo de la Especificación:** [`docs/specs/sistema/19-especificacion-fundas-y-enlaces-tiendas.md`](file:///c:/repos/Ludeka/docs/specs/sistema/19-especificacion-fundas-y-enlaces-tiendas.md)
