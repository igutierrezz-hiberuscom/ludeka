# Especificación: rule-qa-consultancy (Consultorio de Reglas Q&A Estilo StackOverflow)

## 1. Propósito y Contexto
Define el consultorio de dudas de reglamento integrado en la ficha de cada juego de mesa. Permite a los jugadores resolver ambigüedades durante una partida física mediante preguntas concisas, respuestas de la comunidad y la validación de una "Respuesta Aceptada".

---

## 2. Requerimientos de Comportamiento (RFC 2119)

### REQ-RQA-01: Formulación de Preguntas de Reglamento
- El sistema **DEBE** permitir a un usuario autenticado publicar una pregunta ligada a un juego (`GameId`).
- La pregunta **DEBE** incluir:
  - `Title`: Resumen conciso de la duda (ej. "¿Se puede jugar una carta de acción fuera de turno?").
  - `Body`: Detalle y contexto de la situación de mesa.
  - `UserId` y `UserName`: Autor de la duda.
- La pregunta **DEBE** inicializarse con `VotesCount = 0` y `AcceptedAnswerId = null`.

### REQ-RQA-02: Respuestas Comunitarias y Referencia Oficial
- El sistema **DEBE** permitir a cualquier usuario responder a una pregunta existente.
- La respuesta **DEBE** incluir:
  - `Body`: Explicación de la resolución de la duda.
  - `OfficialRuleReference`: Campo opcional indicando página o sección del manual oficial (ej. *"Manual pág. 8, apartado 4.2"*).
- La respuesta **DEBE** inicializarse con `IsAccepted = false` y `VotesCount = 0`.

### REQ-RQA-03: Sistema de Votos Comunitarios (+1)
- Los usuarios registrados **DEBEN** poder emitir su voto de utilidad (+1) tanto en preguntas como en respuestas.
- El sistema **DEBE** garantizar que un mismo usuario solo pueda votar una única vez por cada pregunta o respuesta (un segundo voto retira el voto previo).

### REQ-RQA-04: Marcado de Respuesta Aceptada (Solución Oficial)
- El autor original de la pregunta o cualquier usuario con rol `FoundingTeam` o `Moderator` **DEBE** poder marcar una respuesta como la solución aceptada (`IsAccepted = true`).
- Solo **PUEDE** existir un máximo de una respuesta aceptada por pregunta en un instante dado.
- Marcar una respuesta distinta como aceptada **DEBE** desmarcar automáticamente cualquier otra respuesta aceptada previa.
- En la interfaz, la respuesta aceptada **DEBE** fijarse siempre en la primera posición con un marco verde y el distintivo *"✓ Respuesta Aceptada por el autor / moderación"*.

---

## 3. Criterios de Aceptación (Escenarios Gherkin)

### Escenario 1: Marcado de respuesta aceptada por el autor
```gherkin
Given una pregunta formulada por "Laura" con dos respuestas
When "Laura" pulsa el botón de marcar como aceptada en la segunda respuesta
Then la segunda respuesta queda con IsAccepted en true
And la pregunta actualiza AcceptedAnswerId con el identificador de dicha respuesta
And la respuesta aparece destacada en la primera posición
```

### Escenario 2: Protección contra usuarios no autorizados
```gherkin
Given una pregunta formulada por "Laura"
When otro usuario común "Pedro" intenta marcar una respuesta como aceptada
Then el sistema rechaza la acción con un error de autorización
```

### Escenario 3: Unicidad del voto por usuario
```gherkin
Given una respuesta con 3 votos
When el usuario "Marcos" emite un voto
Then los votos totales pasan a 4
When el usuario "Marcos" vuelve a pulsar el botón de votar
Then su voto se retira y los votos totales vuelven a 3
```
