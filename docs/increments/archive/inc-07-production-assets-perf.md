# Incremento 7: Compilación de Producción, Assets y Rendimiento Web

- **Identificador SDD:** `change-07-production-assets-perf`
- **Objetivo Principal:** Optimización extrema de rendimiento en carga móvil, purga de estilos Tailwind y cumplimiento de Core Web Vitals y accesibilidad.
- **Estado:** ✅ **Completado y Archivado** (154 tests en verde).

---

## 1. Alcance Funcional y Técnico Entregado

1. **Pipeline de Assets y Tailwind CSS:** Compilación optimizada y purga de clases mediante Tailwind CLI v3.4.17 (`app.css` minificado), eliminando CDN en producción.
2. **Optimización Core Web Vitals:**
   - Largest Contentful Paint (LCP < 1.2s mediante preconexiones y `fetchpriority="high"` en carátulas).
   - Cumulative Layout Shift (CLS = 0 con contenedores rígidos `aspect-square` y `aspect-video`).
   - Interaction to Next Paint (INP optimizado con debounce y cancelación de tareas asíncronas).
3. **Estrategia de Caché de 2 Niveles:**
   - *Nivel 1 (Aplicación):* [`CachedCatalogService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Catalog/CachedCatalogService.cs) decorando `ICatalogService` con `IMemoryCache` (TTL 10m e invalidación por slug).
   - *Nivel 2 (HTTP / Output Caching):* Políticas en `Program.cs` con tags (`tag-catalog`, `tag-radar`, `tag-static`).
4. **Accesibilidad WCAG 2.2 Nivel AA:** Skip Link accesible, estandarización de modales con `role="dialog"` y `aria-labelledby`, semántica en pestañas (`role="tablist"` / `role="tab"` / `role="tabpanel"`).

---

## 2. Artefactos Clave

- [`CachedCatalogService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Catalog/CachedCatalogService.cs)
- [`App.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/App.razor), [`Routes.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Routes.razor), [`MainLayout.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Layout/MainLayout.razor).
- Configuración de caché HTTP en [`Program.cs`](file:///c:/repos/Ludeka/src/Ludeka.Web/Program.cs).

---

## 3. Verificación

- Pruebas unitarias de decorador de caché e invalidación de claves en [`tests/Ludeka.UnitTests`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests).
