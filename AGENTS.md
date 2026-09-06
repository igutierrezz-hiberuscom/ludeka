# Ludeca / Ludist — Reglas Maestras del Monorepo y Guía de Agentes

> **Proyecto:** Ludist / Ludeca ("El Letterboxd de los juegos de mesa en español")  
> **Arquitectura:** .NET 10 (C# 13), Blazor Web App (SSR + Interactivo), Clean Architecture / Vertical Slices, Tailwind CSS + Componentes Editoriales, Engram Persistent Memory.

---

## 1. Filosofía de Desarrollo: Spec-Driven Development (SDD)

Este proyecto se construye bajo la metodología **Spec-Driven Development (SDD)** de Gentle-AI.
**REGLA DE ORO:** Nunca implementar directamente en código cambios arquitectónicos o funcionalidades mayores sin pasar por las fases del ciclo SDD:

```
[ sdd-explore ] ➔ [ sdd-propose ] ➔ [ sdd-spec ] ➔ [ sdd-design ] ➔ [ sdd-tasks ] ➔ [ sdd-apply ] ➔ [ sdd-verify ] ➔ [ sdd-archive ]
```

### Principios de la Máquina de Estados de SDD
1. **File-System como Fuente de la Verdad:** El estado de las fases reside en `.openspec/` y `docs/specs/`. No confiar en la memoria volátil del chat.
2. **Lossless Blocking Prompts:** Antes de pasar de `sdd-propose` a `sdd-spec` o de `sdd-design` a `sdd-apply`, presentar la propuesta o diseño al usuario y esperar aprobación explícita.
3. **Delegación con Subagentes:** Emplear `define_subagent` e `invoke_subagent` para delegar exploraciones profundas, investigación externa y verificaciones independientes.
4. **Presupuestos y CAS (Compare-And-Swap):** En `sdd-apply`, implementar exclusivamente contra los requerimientos acordados en la especificación y tareas definidas.

---

## 2. Protocolo de Memoria Persistente (Engram MCP)

El proyecto cuenta con el servidor MCP de **Engram** conectado en `.tools/bin/engram.exe`.
- **Cuándo guardar (`mem_save`):** Inmediatamente tras resolver un bug, tomar una decisión de diseño, aprender una regla de negocio o establecer un patrón.
- **Cuándo consultar (`mem_context` / `mem_search`):** Al inicio de sesión o al retomar una funcionalidad para no perder el contexto previo.
- **Cierre de sesión (`mem_session_summary`):** Obligatorio antes de finalizar la sesión de trabajo.

---

## 3. Principios de UI/UX: Anti-Plantillas y Cero "AI Slop"

- **Identidad Propia:** Ludeca no es una base de datos corporativa ni un clon genérico con gradientes púrpura o tarjetas estándar. Debe respirar la pasión de los juegos de mesa (estilo editorial moderno, tipografía nítida con contraste, badges compactos de 3 segundos, microtextos con personalidad lúdica).
- **Mobile-First Radical:** Barra de acciones al alcance del pulgar (`Tengo`, `Jugado`, `Deseado`, `Prestar`), fichas por pestañas horizontales limpias, tiempo de carga instantáneo.
- **Componentes Gratuitos y Abiertos:** Uso de Tailwind CSS con utilidades bien estructuradas, componentes Razor propios e iconografía abierta (Lucide Icons). Cumplimiento estricto de WCAG 2.2 AA (accesibilidad).

---

## 4. Estándares Técnicos (.NET 10 & C# 13)

- **Solución y Capas:**
  - `src/Ludeca.Web`: Frontend Blazor Web App (SSR estático donde sea posible, interactividad por componentes, Streaming Rendering).
  - `src/Ludeca.Core`: Entidades de dominio y reglas de negocio puras (cero dependencias de framework).
  - `src/Ludeca.Application`: Casos de uso, interfaces, validaciones (FluentValidation o data annotations) y DTOs.
  - `src/Ludeca.Infrastructure`: Integración BGG XMLAPI2, APIs de YouTube/Instagram, persistencia y autenticación OAuth.
  - `tests/`: Pruebas unitarias con xUnit y pruebas de componentes/integración.
- **Async/Await:** Emplear `ValueTask` cuando proceda, pasar siempre `CancellationToken`, evitar `.Result` o `.Wait()`.
- **Commits Convencionales:** Formato `feat:`, `fix:`, `refactor:`, `test:`, `docs:`. Prohibido añadir atribuciones "Co-Authored-By" de IA.
