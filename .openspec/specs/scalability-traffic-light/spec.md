# Especificación: scalability-traffic-light

## Propósito
Definir el modelo de semáforo dinámico de escalabilidad (1 a 7+ jugadores), el cálculo de estados por comensal (🟢 Imprescindible | 🟡 Bueno / Recomendado | 🔴 No recomendado) y la determinación textual de la configuración ideal ("Ideal a X jugadores").

## Requerimientos

### Requerimiento: Desglose Dinámico de Escalabilidad
El agregado `Game` DEBE contener una colección inmutable de recomendaciones de escalabilidad `ScalabilityEntry` para cada número de jugadores soportado (adaptable desde 1 hasta 7+ comensales).

#### Escenario: Estado Imprescindible (🟢)
- DADO un número de comensales donde el consenso de votos sitúa al juego en su mejor nivel (ej. 2 jugadores en *7 Wonders Duel* o 4 jugadores en *Blood Rage*)
- CUANDO se evalúa el estado para ese número de jugadores
- ENTONCES el estado DEBE ser `MustPlay` (🟢 Imprescindible / Brilla)
- Y el color visual en la UI DEBE ser verde esmeralda con alto contraste accesible.

#### Escenario: Estado Bueno / Recomendado (🟡)
- DADO un número de comensales donde el juego funciona bien y es disfrutable pero no es su pico máximo
- CUANDO se evalúa el estado para ese número de jugadores
- ENTONCES el estado DEBE ser `Recommended` (🟡 Bueno / Recomendado)
- Y el color visual en la UI DEBE ser ámbar dorado.

#### Escenario: Estado No Recomendado (🔴)
- DADO un número de comensales donde el entreturno es excesivo, el tablero queda desierto o el parche es artificial
- CUANDO se evalúa el estado para ese número de jugadores
- ENTONCES el estado DEBE ser `NotRecommended` (🔴 No recomendado)
- Y el color visual en la UI DEBE ser rojo suave de aviso.

---

### Requerimiento: Etiqueta "Ideal a X Jugadores"
El agregado `Game` DEBE calcular y exponer una propiedad `IdealPlayerCountText` que resuma en texto claro la mejor configuración de mesa.

#### Escenario: Único conteo ideal
- DADO un juego donde la configuración óptima indiscutible es a 2 personas
- CUANDO se genera la etiqueta
- ENTONCES el texto DEBE ser `"Ideal: 2 jugadores"`.

#### Escenario: Rango de conteos ideales
- DADO un juego que brilla a 3 y 4 jugadores
- CUANDO se genera la etiqueta
- ENTONCES el texto DEBE ser `"Ideal: 3–4 jugadores"`.