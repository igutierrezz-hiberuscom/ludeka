# Incremento 29: Localización Geográfica por País, Filtrado Territorial y Detección de Ubicación

- **Identificador SDD:** `change-29-country-location-filtering`
- **Estado:** ✅ **Archivado / Completado**
- **Puntos de la Especificación:** Filtrado territorial de contenidos lúdicos, Gestión de País en Editoriales, Tiendas, Sorteos, Eventos y Usuarios, Advertencia de filtrado, Marcado visual en fichas y compras, y Detección de ubicación con ordenación prioritaria.
- **Objetivo Principal:** Aunque Ludeka es una plataforma hispanohablante global, los aspectos comerciales y comunitarios presenciales (tiendas físicas/online con envíos locales, eventos presenciales y sorteos sujetos a bases legales territoriales) son estrictamente locales. Este incremento dota a Ludeka de un sistema de localización territorial que permite asignar país a cada entidad, configurar el país en el perfil de usuario con advertencia explícita de filtrado, filtrar compras de juegos y fundas mostrando solo las disponibles en el país del usuario, marcar visiblemente el país en listados y fichas, y permitir la detección de ubicación desde móvil u ordenador para priorizar inteligentemente los contenidos locales sin excluir el ecosistema internacional.

---

## 1. Alcance Funcional y Técnico

1. **Catálogo de Países y Valores del Dominio (`CountryInfo` / `CountryOptions`):**
   - Catálogo estandarizado de países hispanohablantes y referencias globales:
     - 🇪🇸 España (`ES`, "España")
     - 🇲🇽 México (`MX`, "México")
     - 🇦🇷 Argentina (`AR`, "Argentina")
     - 🇨🇱 Chile (`CL`, "Chile")
     - 🇨🇴 Colombia (`CO`, "Colombia")
     - 🇵🇪 Perú (`PE`, "Perú")
     - 🇺🇾 Uruguay (`UY`, "Uruguay")
     - 🌎 Internacional (`INT`, "Internacional / Global")
   - Helper de utilidades para obtener banderas, nombres legibles y normalización insensible a mayúsculas/acentos (`CountryCatalog`).

2. **Extensión del Modelo de Dominio y Persistencia SQLite:**
   - **`Store` (Tiendas):** Incorporación de las propiedades `Country` y `ShippingCountries` con índice en base de datos.
   - **`Giveaway` (Sorteos):** Incorporación de `Country` (string, default "España", con soporte de "Internacional") e índice.
   - **`BoardGameEvent` (Grandes Eventos):** Incorporación de `Country` (string, default "España") manteniendo `Location` para recinto/ciudad.
   - **`Publisher` (Editoriales):** Homogeneización y validación del campo `Country` existente contra el catálogo de países.
   - **`GamePurchaseLink` (Value Object de Compra de Juegos):** Incorporación de `Country` y `ShippingCountries` para que cada oferta comercial identifique su territorio de entrega.
   - **`SleeveItem` (Guía de Fundas):** Soporte de `Country` y `ShippingCountries`.
   - **`AppUser` y `UserPreference`:** Incorporación de la propiedad `Country` (string opcional / nullable).
   - **Migración Idempotente (`SqliteSchemaMigrator`):** Inclusión de sentencias `ALTER TABLE ADD COLUMN` e índices para garantizar compatibilidad retroactiva sin pérdida de datos.

3. **Preferencia de Usuario y Advertencia Explícita de Filtrado:**
   - En el perfil de usuario (`MyLibrary.razor`), en la barra superior (`MainLayout.razor`) y en el modal dedicado (`LocationSelectorModal.razor`):
     - Selector de país ("Sin especificar / Todos los países", España, México, Argentina, etc.).
     - **Advertencia obligatoria:** Mensaje claro y visible al interactuar con el selector:
       > *"⚠️ Al seleccionar un país, los sorteos, eventos y tiendas se filtrarán automáticamente para mostrar únicamente los disponibles en tu territorio. Si prefieres explorar toda la información de la comunidad global sin filtrar, déjalo sin seleccionar."*
     - Guardado persistente en `UserPreferences` y actualización del estado de sesión.

4. **Filtrado Territorial Estricto y Estados Vacíos en Compras:**
   - **Dónde Comprar Juegos y Fundas (`StoreOffersCard.razor` / `SleeveGuideCard.razor`):**
     - Si el usuario tiene un país seleccionado (o selecciona un país activo), solo se muestran las ofertas comerciales y marcas de fundas que realizan envíos a ese país.
     - **Regla de negocio:** Si para el país del usuario no existe tienda de juegos o tienda de fundas vinculada para ese título, no saldrá ninguna tienda de otro país; en su lugar, se muestra un estado vacío específico informando de la ausencia de tiendas en su territorio (*"Actualmente no hay tiendas colaboradoras con envíos a {País} para este juego o sus fundas"*).
     - Marcado de badge de país en cada tarjeta de oferta (ej. 🇪🇸 España, 🇲🇽 México) y destinos de envío.
   - **Sorteos (`/sorteos` / `GiveawayCard.razor`):**
     - Filtrado automático por país del usuario (incluyendo siempre los de ámbito "Internacional").
     - Badge visual identificativo del país en cada tarjeta de sorteo.
   - **Eventos (`/eventos` / `EventsManagement.razor`):**
     - Filtrado automático por país con opción de cambiar de país o ver "Todos".
     - Badge visual del país en las fichas del calendario.
   - **Directorio de Tiendas (`/tiendas` / `StoreDetail.razor`):**
     - Selector y filtrado por país con badge en cada ficha comercial y desglose de envíos multipaís.

5. **Detección de Ubicación y Ordenación Priorizada:**
   - **Servicio de Detección de Ubicación (`IUserLocationService` + JS Interop `locationInterop.js`):**
     - Inferencia ligera inicial vía zona horaria del cliente (`Intl.DateTimeFormat().resolvedOptions().timeZone`) como sugerencia instantánea sin solicitud invasiva de permisos.
     - Botón explícito *"📍 Detectar mi país"*.
   - **Ordenación por Proximidad/Prioridad Territorial:**
     - Para usuarios que no deseen filtrar estrictamente (manteniendo la vista global de la comunidad), se ofrece la opción *"Priorizar los de mi país primero"*: los sorteos, eventos y tiendas de su país se listan en las primeras posiciones (score 2), seguidos por los de ámbito internacional (score 1) y los del resto de países (score 0).

---

## 2. Criterios de Aceptación (Gherkin Verificados)

```gherkin
Escenario: Advertencia explícita al configurar el país en el perfil de usuario
  Dado un usuario en la configuración de su perfil en Ludeka
  Cuando despliega el selector de país para elegir "México"
  Entonces se muestra una advertencia indicando: "⚠️ Al seleccionar un país, los sorteos, eventos y tiendas se filtrarán automáticamente para mostrar únicamente los disponibles en tu territorio. Si prefieres explorar toda la información de la comunidad global sin filtrar, déjalo sin seleccionar."
  Y al confirmar, su preferencia de país queda guardada como "México"

Escenario: Filtrado de ofertas de compra en ficha de juego con país seleccionado
  Dado un usuario con país configurado como "Argentina"
  Y una ficha de juego con tiendas colaboradoras de España y ninguna de Argentina
  Cuando el usuario consulta la sección "Dónde Comprar"
  Entonces no se muestran las tiendas de España
  Y se muestra un mensaje informativo indicando que no hay tiendas disponibles en Argentina para este título

Escenario: Marcado visible de país en tarjetas de sorteos y tiendas
  Dado un listado de sorteos activos en "/sorteos"
  Cuando se renderizan las tarjetas de sorteos
  Entonces cada tarjeta muestra un badge visible con el emoji de la bandera y el nombre del país (ej. "🇪🇸 España" o "🌎 Internacional")

Escenario: Ordenación prioritaria por ubicación detectada
  Dado un usuario que no tiene filtro estricto pero ha configurado o detectado España
  Cuando consulta tiendas, ofertas o eventos
  Entonces los elementos de España aparecen ordenados en las primeras posiciones
  Seguidos por los elementos de cobertura internacional
```

---

## 3. Pruebas y Validación
- 573 pruebas unitarias e integración en `Ludeka.UnitTests` pasando al 100% (0 errores).
- Documentación viva volcada en `docs/specs/sistema/17-localizacion-territorial-pais.md`.
