# Especificación Delta: core-catalog (Integración Multimedia)

## Propósito
Extender la ficha inteligente de juego (`GameDetail.razor`) para integrar de forma armónica la sección `MultimediaHub.razor`, garantizando la consulta reactiva de recursos audiovisuales aprobados.

## Requerimientos Modificados

### Requerimiento: Integración de la Sección Multimedia en la Ficha
La ficha de juego DEBE incorporar la sección multimedia entre el bloque de escalabilidad/fundas y la descripción, o como bloque editorial de primer nivel.

#### Escenario: Renderizado del hub multimedia en la ficha
- DADO un usuario navegando a la ficha `/juegos/{Slug}`
- CUANDO la ficha termina de cargar
- ENTONCES el componente `MultimediaHub` DEBE inicializarse con los tutoriales, partidas completas y opiniones aprobadas correspondientes a dicho `GameId`.
