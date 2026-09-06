# Especificación: founding-verdict-core

## Propósito
Definir la entidad de dominio y reglas del análisis oficial de la casa emitido por la mesa fundadora, incluyendo el sello de recomendación, el análisis estructurado (general, en pareja a 2 jugadores y familias con niños) y la galería fotográfica de mesa real.

## Requerimientos

### Requerimiento: Entidad de Dominio `FoundingVerdict`
El sistema DEBE proveer una entidad de dominio `FoundingVerdict` asociada de forma única a un `Game` (`GameId`), registrando la autoría del equipo fundador, el sello de recomendación oficial y los bloques de análisis editorial.

#### Escenario: Creación válida de un veredicto fundador
- DADO un juego existente en el catálogo y un usuario con rol `FoundingTeam`
- CUANDO se redacta un veredicto con:
  - Sello de recomendación `MustPlay` (Imprescindible de la Mesa)
  - Análisis general de al menos 20 caracteres
  - Análisis enfocado a 2 jugadores (experiencia en pareja)
  - Análisis enfocado a familias/niños (adaptación de reglas y edad real)
- ENTONCES el sistema DEBE instanciar `FoundingVerdict` correctamente con fecha de creación y actualización.

#### Escenario: Validación de campos obligatorios
- DADO un intento de crear un veredicto con análisis general en blanco o vacío
- CUANDO se ejecuta la validación de dominio
- ENTONCES el sistema DEBE lanzar una excepción de validación impidiendo la creación del registro.

---

### Requerimiento: Sello de Recomendación Oficial (`FoundingRecommendation`)
El veredicto DEBE clasificar el título bajo una de las 3 insignias oficiales de la mesa fundadora:
- `MustPlay`: 🏆 *Imprescindible de la Mesa* (brilla al máximo, compra o partida obligatoria).
- `RecommendedWithAdaptations`: 🏷️ *Recomendado con adaptaciones* (buen juego con ajustes de reglas o comensales específicos).
- `Skippable`: 📦 *Prescindible* (no aporta frente a otras alternativas del mismo estilo).

#### Escenario: Asignación y cambio de sello
- DADO un veredicto existente con sello `RecommendedWithAdaptations`
- CUANDO la mesa fundadora decide elevar su categoría a `MustPlay`
- ENTONCES la recomendación DEBE actualizarse inmediatamente reflejando el nuevo sello.

---

### Requerimiento: Galería de Fotos Reales de Partida (`FoundingPhoto`)
El veredicto DEBE permitir adjuntar de 1 a 3 fotografías reales tomadas en mesa física (`FoundingPhoto`), con URL de imagen y pie de foto descriptivo (`Caption`).

#### Escenario: Adjuntar fotos de mesa real válidas
- DADO un veredicto en redacción
- CUANDO se añaden 2 fotos reales con URL válida y pie de foto (ej. `"Despliegue a 2 jugadores en mesa de salón"`)
- ENTONCES la colección `Photos` del veredicto DEBE contener exactamente 2 elementos inmutables.

#### Escenario: Restricción de máximo 3 fotos
- DADO un veredicto que ya contiene 3 fotos
- CUANDO se intenta agregar una 4ª fotografía
- ENTONCES el sistema DEBE rechazar la adición con un error de dominio indicando que el límite es de 3 fotografías reales por juego.
