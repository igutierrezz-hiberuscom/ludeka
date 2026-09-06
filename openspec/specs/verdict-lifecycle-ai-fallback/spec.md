# Especificación: verdict-lifecycle-ai-fallback

## Propósito
Definir el algoritmo del ciclo de vida del veredicto en 3 fases: renderizado del resumen de IA para evitar fichas vacías (Fase 1), sustitución visual prioritaria al publicar el veredicto oficial de la casa (Fase 2) y ponderación reforzada en el ranking local (Fase 3).

## Requerimientos

### Requerimiento: Fase 1 — Arranque con Resumen de IA (`AiGameSummary`)
Cuando un juego en catálogo NO dispone de un veredicto de la mesa fundadora registrado, el sistema DEBE suministrar y renderizar un bloque identificado claramente como `🤖 Resumen generado por IA`.

#### Escenario: Juego sin veredicto fundador renderiza resumen de IA
- DADO un juego recién consultado que no tiene registro en `FoundingVerdicts`
- CUANDO se carga la ficha del juego
- ENTONCES el sistema DEBE generar/mostrar el bloque `🤖 Resumen generado por IA`
- Y el bloque DEBE incluir síntesis textual de escalabilidad, edad sugerida y huella en mesa.

---

### Requerimiento: Fase 2 — Sustitución Visual Prioritaria
En el momento en que un juego recibe un `FoundingVerdict`, el sistema DEBE ocultar el bloque de IA y renderizar en su lugar de forma prominente la tarjeta `👤 Veredicto de la Mesa Fundadora`.

#### Escenario: Sustitución inmediata tras publicar veredicto
- DADO un juego que mostraba el bloque `🤖 Resumen generado por IA`
- CUANDO un miembro de la mesa fundadora guarda un veredicto oficial
- ENTONCES la tarjeta de IA DEBE desaparecer por completo de la ficha
- Y la tarjeta `👤 Veredicto de la Mesa Fundadora` DEBE renderizarse en su lugar con su sello y análisis.

---

### Requerimiento: Fase 3 — Ponderación Reforzada en `LudistRating`
El veredicto de la mesa fundadora DEBE tener un peso de voto prioritario en el recálculo ponderado del `LudistRating` del juego, equivalente al peso de 3 votos comunitarios.

#### Escenario: Impacto del sello fundador en la nota media
- DADO un juego con `LudistRating` inicial de 7.0 basado en votos comunitarios
- CUANDO la mesa fundadora otorga el sello `MustPlay` (equivalente a 10.0 con peso triple)
- ENTONCES el nuevo `LudistRating` calculado DEBE ponderar los 3 votos fundadores sobre el total, elevando la nota media del consenso.
