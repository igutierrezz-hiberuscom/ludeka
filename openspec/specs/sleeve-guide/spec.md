# Especificación: sleeve-guide

## Propósito
Definir la estructura de datos y componentes para la guía técnica de fundas de cartas (tamaños, recuentos exactos y enlaces de afiliados contextuales para compra).

## Requerimientos

### Requerimiento: Colección de Fundas Requeridas (`SleeveItem`)
El agregado `Game` DEBE mantener una colección de tipos de fundas necesarias para enfundar el juego completo.

#### Escenario: Juego con cartas de tamaño estándar
- DADO un juego como *Wingspan* con 212 cartas de formato estándar (63.5 x 88 mm)
- CUANDO se consulta la guía de fundas
- ENTONCES DEBE indicar el formato `"Estándar (63.5 x 88 mm)"`, cantidad `212` cartas
- Y generar enlaces de compra contextuales con código de afiliado (ej. `"Comprar fundas compatibles en Zacatrus / Amazon"`).

#### Escenario: Juego sin cartas
- DADO un juego como *Hive* o *Azul* que no contiene cartas
- CUANDO se consulta la guía de fundas
- ENTONCES DEBE indicar de forma limpia `"Este juego no utiliza cartas"` sin botones de compra erróneos.