# Incremento 13: Módulo de Síntesis con IA (Google Gemini / Heurística) para Fichas

- **Identificador SDD:** `change-13-ai-game-summary`
- **Estado:** ✅ **Completado y Archivado** (281 tests en verde al 100%)
- **Puntos del MVP cubiertos:** 4.1 y 8.2 (Fase Inicial de Arranque con IA y auto-catalogación).
- **Objetivo Principal:** Dotar al sistema de síntesis inteligente automática en español (`🤖 Resumen generado por IA`) para fichas recién catalogadas o que no cuenten con veredicto de la mesa fundadora, resumiendo escalabilidad real, experiencia familiar y huella en mesa.

---

## 1. Alcance Funcional Implementado

1. **Servicio de Generación de Síntesis (`IAiGameSummaryService`):**
   - Abstracción desacoplada en `Ludeka.Application.Contracts`.
   - Generación de un bloque estructurado:
     - *Veredicto de Escalabilidad:* A qué número exacto de jugadores brilla según el consenso del hobby internacional.
     - *Accesibilidad & Experiencia Familiar/Infantil:* Si es apto para niños y a partir de qué edad real (caja vs. comunitaria).
     - *Ritmo y Mesa:* Duración real por comensal y espacio requerido en salón (pequeña, estándar, monstruo de mesa).
     - *Veredicto General:* Reseña objetiva que contextualiza temática, estilo y confrontación.
2. **Proveedor Google Gemini con Modo Simulado y Zero-Crash Fallback:**
   - Implementación `GeminiGameSummaryService` conectable con API Key de Gemini (`gemini-2.5-flash`) mediante salida estructurada JSON (`application/json`).
   - Motor heurístico determinista `HeuristicGameSummaryGenerator` para desarrollo offline, pruebas o cuando la API externa no está configurada o experimenta caídas.
3. **Persistencia en Dominio y SQLite:**
   - Value Object inmutable `AiGameSummary` en `Ludeka.Core.ValueObjects`.
   - Propiedad `AiSummary` y método `SetAiSummary` en `Game.cs`.
   - Mapeo nativo `OwnsOne(g => g.AiSummary, b => b.ToJson())` en `LudekaDbContext.cs`.
   - Reconciliación defensiva de columna `AiSummary` en `SqliteSchemaMigrator.cs`.
4. **Integración con la Cola de Auto-Catalogación:**
   - Al procesar títulos de `PendingBggImports` en `BggCatalogQueueService`, se ejecuta la síntesis de IA y se almacena el resumen en la ficha del juego de forma automática.
5. **Presentación en la Ficha Blazor (`AiSummaryCard.razor` y `GameDetail.razor`):**
   - Tarjeta distinguida con badge `🤖 Síntesis generada por IA` y chip con el modelo (`Gemini 2.5 Flash` / `Heurística Editorial`).
   - Se oculta o pasa a segundo plano automáticamente en cuanto se publica un Veredicto Fundador, otorgando prioridad editorial a la experiencia real de la mesa.

---

## 2. Criterios de Aceptación Verificados

```gherkin
Escenario: Generar síntesis al catalogar un juego nuevo
  Dado un juego incorporado recientemente desde la cola de BGG sin veredicto fundador
  Cuando se ejecuta el servicio de síntesis de IA
  Entonces se almacena en el juego un resumen estructurado en español con escalabilidad y edad recomendada
  Y la ficha del juego muestra la tarjeta "🤖 Resumen generado por IA"

Escenario: Reemplazo prioritario por veredicto fundador
  Dado un juego con resumen generado por IA
  Cuando un miembro del equipo fundador publica un Veredicto Oficial
  Entonces el Veredicto Fundador toma la posición prioritaria superior en la ficha
  Y el bloque de IA se retira o pasa a segundo plano

Escenario: Tolerancia a caídas (Zero-Crash Fallback)
  Dado que Gemini falla por timeout o cuota de API excedida
  Cuando se solicita la síntesis de un juego
  Entonces el sistema conmuta de inmediato a la heurística sin lanzar excepciones
  Y etiqueta el resultado como "Heurística Editorial (Fallback)"
```
