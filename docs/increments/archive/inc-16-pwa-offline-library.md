# Incremento 16: PWA (Progressive Web App) y Modo Consulta Offline para Ludoteca

- **Identificador SDD:** `change-16-pwa-offline-library`
- **Estado:** ✅ **Archivado**
- **Objetivo Principal:** Convertir la aplicación Blazor en una Progressive Web App (PWA) instalable en teléfonos móviles iOS y Android, permitiendo consultar la colección personal y los juegos prestados sin conexión a internet en bares o asociaciones.

---

## 1. Alcance Funcional Propuesto

1. **Manifiesto de Aplicación Web (`manifest.webmanifest`):**
   - Nombre, iconos oficiales de Ludeka en diferentes resoluciones (192x192, 512x512, máscaras adaptativas), color de tema de marca (`#d97706` o el color editorial preferido) y modo `display: standalone`.
2. **Service Worker Ligero:**
   - Estrategia *Cache First* para assets estáticos (CSS, fuentes, iconos Lucide, carátulas en caché).
   - Estrategia *Network First con Fallback Offline* para las rutas de catálogo y ludoteca.
3. **Caché Local de la Ludoteca del Usuario:**
   - Sincronización en `localStorage` o `IndexedDB` de la lista de juegos del usuario para visualización instantánea sin red.
4. **Indicador de Estado Offline:**
   - Píldora visual sutil en el pie o cabecera cuando el dispositivo no tiene conexión a internet, avisando de que opera en modo local.

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Instalar la aplicación en pantalla de inicio
  Dado un navegador compatible con PWA en móvil o escritorio
  Cuando el usuario visita Ludeka
  Entonces el navegador detecta el manifiesto válido
  Y permite añadir la aplicación como icono independiente sin barra de navegación del navegador

Escenario: Consulta de ludoteca sin conexión a internet
  Dado que el usuario ha visitado previamente "/mi-ludoteca"
  Cuando el dispositivo entra en modo avión o pierde cobertura
  Entonces la página de su ludoteca sigue cargando los títulos de su colección y sus préstamos activos
```
