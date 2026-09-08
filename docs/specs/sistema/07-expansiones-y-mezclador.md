# 07. Expansiones, Ecosistema y Mezclador de Mesa

## 1. Visión General y Propósito
Este módulo dota a las expansiones de ficha propia, metadatos y valoraciones independientes, vinculación bidireccional con el juego base, tarjeta editorial de aportes, matriz de sinergia par-a-par y un Mezclador interactivo de mesa con detección de sobrecarga.

---

## 2. Modelo de Dominio Polimórfico (`Ludeka.Core`)

### 2.1 Atributos de Expansión en `Game`
Ubicación: [`src/Ludeka.Core/Entities/Game.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/Game.cs)

- `Type`: Enum `GameType` (`BaseGame`, `Expansion`, `StandaloneExpansion`).
- `BaseGameId`: Guid opcional que referencia al juego base.
- `ExpansionNecessity`: Enum `ExpansionNecessity` (`MustHave`, `Recommended`, `OnlyForCompletionists`, `Avoid`).
- `ImpactTags`: Colección de [`ExpansionImpactTag`](file:///c:/repos/Ludeka/src/Ludeka.Core/Enums/ExpansionImpactTag.cs) (`AddsPlayers`, `FixesBalance`, `AddsSoloMode`, `ModularContent`, `NarrativeCampaign`, `ImprovesTwoPlayers`, `AddsVariability`).
- `WhatItBringsSummary`: Resumen editorial de aportes lúdicos.
- `ExtraPlayerCount`: Incremento en el número máximo de jugadores.
- `ExtraDurationMinutes`: Minutos adicionales aproximados que suma a la partida.

### 2.2 Entidad `ExpansionSynergy` (Matriz Par-a-Par)
Ubicación: [`src/Ludeka.Core/Entities/ExpansionSynergy.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/ExpansionSynergy.cs)

- `BaseGameId`, `ExpansionIdA`, `ExpansionIdB`.
- `Level`: Enum `ExpansionSynergyLevel`:
  - `PerfectCombo`: Sinergia excelente y recomendada.
  - `CompatibleWithCaution`: Compatibles con advertencias de reglas o sobrecarga.
  - `IncompatibleRedundant`: Módulos que se solapan o no deben jugarse juntos.
- `Explanation`: Descripción narrativa de la compatibilidad entre ambas.

### 2.3 Entidad `ExpansionRecipe` (Recetas de Mesa)
Ubicación: [`src/Ludeka.Core/Entities/ExpansionRecipe.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/ExpansionRecipe.cs)

- Packs prediseñados para configuraciones específicas (ej. *"Duelo Táctico a 2"*, *"El Ecosistema Completo"*).
- `BaseGameId`, `Title`, `Description`, `TargetProfile`, `ExpansionIds`.

---

## 3. Motor de Evaluación en Tiempo Real (`Ludeka.Application`)

- **Contrato:** [`IExpansionService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IExpansionService.cs) implementado en [`ExpansionService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Expansions/ExpansionService.cs).
- **Evaluación del Mezclador (`EvaluateMixerSelectionAsync`):**
  - Calcula el tiempo total resultante sumando la duración base más los deltas de cada expansión elegida.
  - Detecta sobrecarga si el incremento supera **+45 minutos**.
  - Detecta sobrecarga si se seleccionan más de **2 expansiones de alto impacto**.
  - Evalúa la matriz de sinergias par-a-par para advertir de incompatibilidades entre los módulos seleccionados.

---

## 4. Componentes UI (`Ludeka.Web`)

- [`ParentGameBanner.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/ParentGameBanner.razor): Banner en la cabecera de la ficha de expansión para navegar al juego base.
- [`ExpansionAporteCard.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/ExpansionAporteCard.razor): Tarjeta de aportes con badges de impacto y necesidad.
- [`ExpansionSisterList.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/ExpansionSisterList.razor): Carrusel de expansiones hermanas.
- [`ExpansionEcosystemSection.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/ExpansionEcosystemSection.razor): Sección en el juego base con 3 pestañas: Catálogo de expansiones, Mezclador interactivo de mesa y Recetas recomendadas.
