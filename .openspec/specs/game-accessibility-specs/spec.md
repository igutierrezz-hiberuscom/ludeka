# Especificación: game-accessibility-specs

## Propósito
Modelar la comparativa entre la edad legal de la caja y la edad real comunitaria infantil, el factor de dependencia del idioma, la estimación de duración por jugador y el espacio físico requerido en mesa (huella).

## Requerimientos

### Requerimiento: Edad de Caja vs. Edad Real Infantil
El modelo DEBE comparar la edad normativa marcada en la caja (`BoxAge`) con la edad consensuada por familias (`CommunityAge`) y destacar si es accesible antes de tiempo.

#### Escenario: Accesible antes de tiempo
- DADO un juego con `BoxAge = 14` (por normativa de piezas pequeñas) pero `CommunityAge = 8`
- CUANDO se evalúa la accesibilidad infantil
- ENTONCES `IsAccessibleEarlier` DEBE ser `true`
- Y la UI DEBE mostrar el distintivo verde: `"🟢 Accesible antes: la comunidad dice que a partir de 8 años funciona genial"`.

---

### Requerimiento: Dependencia del Idioma en Componentes
El modelo DEBE tipificar el nivel de lectura necesario para jugar sin barreras.

#### Escenario: Clasificar dependencia nula
- DADO un juego cuyos componentes solo contienen números o simbología visual (ej. *Carcassonne*, *Azul*)
- CUANDO se evalúa la dependencia del idioma
- ENTONCES DEBE ser `None` (`"🟢 Nula (solo iconografía)"`).

#### Escenario: Clasificar dependencia baja
- DADO un juego con frases cortas de texto público o cartas de referencia (ej. *Catán*)
- CUANDO se evalúa la dependencia del idioma
- ENTONCES DEBE ser `Low` (`"🟡 Baja (frases cortas o texto visible)"`).

#### Escenario: Clasificar dependencia alta
- DADO un juego con párrafos densos o cartas secretas con texto narrativo complejo (ej. *Arkham Horror LCG*)
- CUANDO se evalúa la dependencia del idioma
- ENTONCES DEBE ser `High` (`"🔴 Alta (requiere lectura fluida)"`).

---

### Requerimiento: Huella en Mesa (Espacio Requerido)
El modelo DEBE informar del tamaño de mesa necesario para desplegar el juego.

#### Escenario: Clasificar huella en mesa pequeña
- DADO un juego filler o de cartas sin despliegue extenso
- CUANDO se evalúa la huella
- ENTONCES DEBE ser `SmallTable` (`"🪑 Mesa pequeña / Cafetería"`).

#### Escenario: Clasificar huella estándar
- DADO un eurogame medio con tablero central y tableros personales moderados
- CUANDO se evalúa la huella
- ENTONCES DEBE ser `StandardTable` (`"🍽️ Mesa de comedor estándar"`).

#### Escenario: Clasificar monstruo de mesa
- DADO un juego masivo con mercados gigantes y tableros individuales extensos
- CUANDO se evalúa la huella
- ENTONCES DEBE ser `TableMonster` (`"🏰 Monstruo de mesa (mesa grande)"`).