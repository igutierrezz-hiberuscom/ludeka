# 09. Arquitectura, Persistencia y Despliegue

## 1. Visión General y Estándares Técnicos
Ludeka opera bajo **.NET 10 (C# 13)** estructurado en Clean Architecture con capas estrictamente desacopladas, persistencia relacional en SQLite con auto-migración no destructiva, empaquetado Docker multi-stage y endpoints de diagnóstico de salud.

---

## 2. Capas de la Solución

| Proyecto | Tipo | Responsabilidad | Dependencias Externas |
|---|---|---|---|
| [`src/Ludeka.Core`](file:///c:/repos/Ludeka/src/Ludeka.Core) | Class Library | Entidades de dominio, Enums, Value Objects | **Ninguna** (cero dependencias) |
| [`src/Ludeka.Application`](file:///c:/repos/Ludeka/src/Ludeka.Application) | Class Library | Casos de uso, interfaces, DTOs, validaciones | `Ludeka.Core` |
| [`src/Ludeka.Infrastructure`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure) | Class Library | SQLite EF Core, cliente BGG, webhooks, seeder | `Ludeka.Core`, `Ludeka.Application`, EF Core SQLite |
| [`src/Ludeka.Web`](file:///c:/repos/Ludeka/src/Ludeka.Web) | Blazor Web App | UI Blazor SSR + Server interactivo, Tailwind CSS | Todas las capas |
| [`tests/Ludeka.UnitTests`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests) | xUnit Project | Pruebas unitarias y de integración | xUnit, Moq, FluentAssertions |

---

## 3. Persistencia y Auto-Migración SQLite

- **Contexto:** `LudekaDbContext` con configuración fluent y mapeo `ToJson()` para colecciones secundarias.
- **Reconciliador de Esquema (`SqliteSchemaMigrator`):**
  - Ubicación: [`src/Ludeka.Infrastructure/Data/SqliteSchemaMigrator.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteSchemaMigrator.cs)
  - Inspecciona `PRAGMA table_info` al arrancar la aplicación y añade dinámicamente columnas faltantes (ej. `PurchaseLinks`, `BaseGameId`, `Type`) sin borrar ni reiniciar bases de datos existentes.
- **Semillado Automático (`CatalogSeeder`):**
  - Carga inicial reproducible desde [`seed-games.json`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Seeding/seed-games.json) y sincronización de ofertas y fundas.

---

## 4. Empaquetado Docker y Orquestación

- **Dockerfile Multi-Stage ([`Dockerfile`](file:///c:/repos/Ludeka/Dockerfile)):**
  - *Build:* SDK `mcr.microsoft.com/dotnet/sdk:10.0` y compilador Tailwind CLI.
  - *Runtime:* Imagen chiseled/alpine ultra-ligera ejecutada bajo usuario no privilegiado (`appuser`, UID 10001).
- **Docker Compose:**
  - [`docker-compose.yml`](file:///c:/repos/Ludeka/docker-compose.yml): Entorno local con volumen persistente en `./data/ludeka.db`.
  - [`docker-compose.staging.yml`](file:///c:/repos/Ludeka/docker-compose.staging.yml): Entorno de pruebas con volúmenes para base de datos y uploads.

---

## 5. Diagnóstico de Salud (Health Checks)

- Ubicación: [`src/Ludeka.Web/Health/LudekaHealthCheck.cs`](file:///c:/repos/Ludeka/src/Ludeka.Web/Health/LudekaHealthCheck.cs)
- Endpoints expuestos:
  - `/healthz`: Liveness check del proceso web.
  - `/ready`: Readiness check que comprueba conectividad real con SQLite (`SELECT 1`) y espacio libre en disco (>50 MB).

---

## 6. Arquitectura PWA y Modo Consulta Offline

- **Manifiesto Web Estándar W3C:**
  - Archivo: [`src/Ludeka.Web/wwwroot/manifest.webmanifest`](file:///c:/repos/Ludeka/src/Ludeka.Web/wwwroot/manifest.webmanifest)
  - Configuración: `display: standalone`, orientación responsiva portrait/any, color temático `#d97706` y fondo `#0f172a`.
  - Iconografía: Iconos vectoriales SVG y rasterizados PNG (192x192, 512x512) y variante `maskable` con margen de seguridad del 15% para compatibilidad total con el recorte de iconos adaptativos en Android e iOS.
- **Service Worker con Estrategia Dual:**
  - Archivo: [`src/Ludeka.Web/wwwroot/service-worker.js`](file:///c:/repos/Ludeka/src/Ludeka.Web/wwwroot/service-worker.js)
  - Estrategia:
    - *Cache-First:* Para recursos estáticos versionados (`.css`, `.js`, fuentes, iconos, imágenes).
    - *Network-First:* Para peticiones de navegación y páginas HTML, con degradación elegante a [`offline.html`](file:///c:/repos/Ludeka/src/Ludeka.Web/wwwroot/offline.html) cuando la red o el servidor están inaccesibles.
    - *Purga Automática:* En el evento `activate`, elimina cachés obsoletas asegurando consistencia entre versiones.
- **Instantánea Local de Ludoteca (`localStorage`):**
  - Módulo JS: [`src/Ludeka.Web/wwwroot/js/ludeka-offline.js`](file:///c:/repos/Ludeka/src/Ludeka.Web/wwwroot/js/ludeka-offline.js)
  - DTO: [`OfflineLibrarySnapshotDto`](file:///c:/repos/Ludeka/src/Ludeka.Application/DTOs/OfflineLibrarySnapshotDto.cs)
  - Comportamiento:
    - Cada vez que el usuario consulta su ludoteca online en [`MyLibrary.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/MyLibrary.razor), se serializa una instantánea local compacta con timestamp.
    - Si el circuito SignalR o la conexión de red se interrumpe, el componente detecta el fallo, monta la instantánea de `localStorage` y muestra la colección con sus filtros y datos esenciales.
    - El modo offline está restringido a **consulta y lectura segura**, previniendo desincronizaciones o conflictos de concurrencia al no permitir mutaciones sin servidor.
- **Indicador de Conectividad Accesible:**
  - Componente: [`src/Ludeka.Web/Components/Shared/OfflineIndicator.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/OfflineIndicator.razor)
  - Integrado globalmente en [`MainLayout.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Layout/MainLayout.razor) con `role="status"` y `aria-live="polite"` notificando al usuario de desconexión o reconexión en tiempo real.

