# Incremento 8: Fichas de Expansión, Ecosistema y Mezclador de Mesa

- **Identificador SDD:** `change-08-game-expansions`
- **Objetivo Principal:** Fichas propias de expansión, tarjeta editorial de aportes, matriz de sinergia par-a-par y Mezclador interactivo de mesa con detección de sobrecarga.
- **Estado:** ✅ **Completado y Archivado** (170 tests en verde al 100%).

---

## 1. Alcance Funcional y Técnico Entregado

1. **Modelo de Dominio Polimórfico en `Game`:**
   - Tipado formal `GameType` (`BaseGame`, `Expansion`, `StandaloneExpansion`).
   - Relación reflexiva `BaseGameId`, deltas de duración (`ExtraDurationMinutes`) y comensales (`ExtraPlayerCount`).
   - Insignia de necesidad (`ExpansionNecessity`) y etiquetas de impacto (`ExpansionImpactTag`).
2. **Matriz de Sinergias Par-a-Par y Recetas de Mesa:**
   - Entidad `ExpansionSynergy` con niveles de compatibilidad (`PerfectCombo`, `CompatibleWithCaution`, `IncompatibleRedundant`).
   - Entidad `ExpansionRecipe` con packs prediseñados para configuraciones específicas de mesa.
3. **Motor de Evaluación en Tiempo Real (`IExpansionService`):**
   - Análisis dinámico en el "Mezclador de Mesa": detección de incompatibilidades, sobrecarga por duración (+45 min) o exceso de módulos (+2 módulos pesados).
4. **Persistencia e Índices en SQLite:**
   - `SqliteExpansionRepository` con consultas optimizadas por juego base y pares de expansiones.
5. **Componentes UI Editoriales Blazor:**
   - `ParentGameBanner.razor`: Acceso al juego base desde la expansión.
   - `ExpansionAporteCard.razor`: Aportes, necesidad y etiquetas de impacto.
   - `ExpansionSisterList.razor`: Expansiones hermanas con badges de compatibilidad.
   - `ExpansionEcosystemSection.razor`: Sección en juego base con 3 pestañas (Catálogo, Mezclador y Recetas).

---

## 2. Artefactos Clave

- **Dominio:** [`ExpansionSynergy.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/ExpansionSynergy.cs), [`ExpansionRecipe.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/ExpansionRecipe.cs), Enums en [`Ludeka.Core/Enums/`](file:///c:/repos/Ludeka/src/Ludeka.Core/Enums).
- **Aplicación:** [`IExpansionService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IExpansionService.cs), [`ExpansionService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Expansions/ExpansionService.cs).
- **Infraestructura:** [`SqliteExpansionRepository.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteExpansionRepository.cs).
- **Web UI:** Componentes en [`Ludeka.Web/Components/Shared/`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared).

---

## 3. Verificación

- Pruebas unitarias de sinergias, detección de sobrecarga en el mezclador y compatibilidad conmutativa en [`tests/Ludeka.UnitTests`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests).
