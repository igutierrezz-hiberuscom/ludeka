# Especificación Modificada: core-catalog (Delta para Incremento 3)

## Propósito
Modificar la capacidad `core-catalog` para integrar en la ficha de juego `/juegos/{slug}` el bloque editorial del veredicto fundador y el resumen de IA, junto con el botón contextual de gestión editorial para moderadores.

## Requerimientos

### Requerimiento: Bloque Editorial en Ficha Inteligente
La página de detalle del juego DEBE renderizar en su zona superior (por encima de las valoraciones comunitarias y del semáforo detallado) el bloque editorial correspondiente según la fase del juego:
1. `FoundingVerdictCard`: Si existe veredicto fundador registrado.
2. `AiSummaryCard`: Si aún no existe veredicto fundador (Fase 1).

#### Escenario: Renderizado de la ficha con veredicto fundador
- DADO un juego con veredicto fundador registrado
- CUANDO cualquier usuario visita `/juegos/{slug}`
- ENTONCES la tarjeta del veredicto oficial con su sello, textos y fotos DEBE posicionarse de manera fija y prioritaria sobre las opiniones públicas.

#### Escenario: Renderizado de la ficha sin veredicto fundador
- DADO un juego sin veredicto fundador
- CUANDO cualquier usuario visita `/juegos/{slug}`
- ENTONCES la tarjeta `🤖 Resumen generado por IA` DEBE mostrarse en esa misma posición destacada.

---

### Requerimiento: Botón de Administración Editorial en Cabecera de Ficha
La ficha de juego DEBE incluir el botón de acción `[ 🛡️ Gestionar Veredicto Fundador ]` cuando el usuario en sesión disponga del rol `FoundingTeam` o `Moderator`.

#### Escenario: Acceso editorial directo desde la ficha
- DADO un usuario con rol `FoundingTeam`
- CUANDO se encuentra en la ficha de un juego
- ENTONCES el botón `[ 🛡️ Gestionar Veredicto Fundador ]` DEBE ser visible y al pulsarlo debe abrir el modal editorial `FoundingVerdictModal`.
