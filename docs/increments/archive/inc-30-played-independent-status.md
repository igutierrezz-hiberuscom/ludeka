# Incremento 30: Estado 'Jugado' Independiente, Supresión de 'Deseado', Radar de Compra y Diario de Partidas

- **Identificador SDD:** `change-30-played-independent-status`
- **Estado:** ✅ **Completado y Archivado** (705 tests en verde al 100%)
- **Puntos de la Especificación:** Reestructuración de la colección en 3 estados de alto valor (`En mi ludoteca`, `Jugado`, `Comprar`), Desacoplamiento ortogonal de `Jugado`, Regla de negocio que restringe valoraciones a juegos jugados, y Nuevo subsistema de Diario y Registro de Partidas con analíticas personales.
- **Objetivo Principal:** Depurar y potenciar la gestión de la colección personal de Ludeka eliminando estados redundantes como 'Deseado' en favor de 'Comprar' (con notificaciones de ofertas, sorteos y reimpresiones), permitir que el estado 'Jugado' conviva simultáneamente con la posesión o intención de compra, blindar el sistema de valoraciones para que solo se pueda opinar sobre juegos previamente jugados, y dotar a la plataforma de un diario de registro de partidas ágil y visual con estadísticas personales.

---

## 1. Alcance Funcional y Técnico

1. **Reestructuración de Estados de Colección:**
   - **`En mi ludoteca` (Propiedad):** Títulos físicos propios. Permite registrar préstamos a terceros y emitir valoraciones.
   - **`Jugado` (Experiencia):** Título probado por el usuario. Ortogonal e independiente. Habilita la emisión de valoraciones y micro-reseñas.
   - **`Comprar` (Interés Comercial / Seguimiento):** Títulos que el usuario desea comprar. Se vincula al radar de ofertas de tiendas, sorteos comunitarios y avisos de reimpresión.
   - **Supresión de `Deseado` (`Wishlist`):** Se fusiona en `Comprar`, eliminando duplicidades conceptuales sin valor funcional.

2. **Regla de Integridad en Valoraciones:**
   - Para poder emitir una puntuación de 1 a 10, reseña o votos de comensales desde la ficha de compra, el juego debe estar marcado como `Jugado` (`IsPlayed = true`). Si no lo está, la interfaz explica el motivo y guía al usuario a registrar partida o marcar como jugado.

3. **Subsistema de Diario de Partidas (`GamePlayLog`):**
   - Registro de sesiones lúdicas: qué juego, fecha de la partida, ubicación (casa, club/asociación, bar, online/BGA, etc.), número de comensales y comentario/anécdotas breves.
   - Al registrar una partida, el juego se marca automáticamente como `Jugado` (`IsPlayed = true`).
   - Visualización de partidas en la ficha de juego y en la pestaña `📝 Diario de Partidas` de `Mi Ludoteca`.
   - Analíticas personales: total de partidas, juego más jugado, ubicación más habitual y comensales habituales.

4. **Persistencia y Migraciones SQLite:**
   - Columna `IsPlayed` en `UserCollectionItems`.
   - Migración automática: `Status = 2` (`Played`) -> `IsPlayed = 1, Status = NULL`.
   - Migración automática: `Status = 3` (`Wishlist`) -> `Status = 4` (`WantToBuy`).
   - Nueva tabla `GamePlayLogs` e índices correspondientes `(UserId, PlayDate)` y `(GameId)`.

---

## 2. Verificación de Calidad
- **Suite de Pruebas Automatizadas:** 705/705 pruebas unitarias y de integración superadas con éxito (`dotnet test`).
- **Persistencia y Migraciones:** Pruebas de integración SQLite verificando la transformación de datos heredados y la convivencia de estados.
- **Especificación Viva:** Actualizado `docs/specs/sistema/02-ludoteca-y-prestamos.md`.
