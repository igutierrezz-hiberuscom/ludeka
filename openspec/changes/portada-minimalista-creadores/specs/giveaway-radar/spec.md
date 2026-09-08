# Delta for giveaway-radar

> Delta sobre `openspec/specs/giveaway-radar/spec.md` (REQ-GIV-01 a REQ-GIV-04 permanecen intactos y no se modifican).
> Este delta añade requerimientos de comportamiento de la página `/sorteos` (renderizado y enrutado), que la spec principal no cubría. Idioma: español castellano.

## ADDED Requirements

### Requirement: Página de sorteos sin banner legacy

La página de sorteos **NO DEBE** renderizar el banner "¡Radar renovado!" bajo ninguna ruta de entrada, incluida la llegada desde un enlace legacy. La eliminación cubre tanto el bloque visual del banner como cualquier condición de ruta legacy que lo activara.

#### Scenario: Entrada por la ruta canónica

- GIVEN un visitante que navega a `/sorteos` (desde menú, portada o URL directa)
- WHEN la página se renderiza
- THEN no aparece el banner "¡Radar renovado!" ni sus enlaces asociados

#### Scenario: Entrada por la ruta legacy

- GIVEN un visitante que llega a la página de sorteos mediante la ruta legacy `/radar`
- WHEN la página se renderiza
- THEN no aparece el banner "¡Radar renovado!" bajo ninguna circunstancia

### Requirement: /radar como alias silencioso de /sorteos

La ruta `/radar` **DEBE** continuar resolviendo la página de sorteos con el mismo contenido que `/sorteos`, sin página de error y sin mensajes de migración o aviso visible al usuario (alias silencioso).

#### Scenario: Bookmark antiguo resuelto sin aviso

- GIVEN un visitante con un bookmark antiguo a `/radar`
- WHEN navega a `/radar`
- THEN recibe la página de sorteos completa, idéntica en contenido a `/sorteos`
- AND no se muestra ningún aviso de ruta antigua ni mensaje de migración
