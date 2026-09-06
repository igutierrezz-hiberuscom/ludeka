# Especificación: user-library-view

## Propósito
Definir la vista de la ludoteca personal en `/mi-ludoteca`, organizada en pestañas reactivas por estado lúdico y préstamos activos, con contadores actualizados en tiempo real y acción de devolución inmediata.

## Requerimientos

### Requerimiento: Navegación por Pestañas de Estado
La página `/mi-ludoteca` DEBE organizar los juegos del usuario en 5 pestañas reactivas:
1. *En mi ludoteca (N)*
2. *Jugados (N)*
3. *Deseados (N)*
4. *Quiero comprar (N)*
5. *Préstamos activos (N)*

#### Escenario: Renderizado de contadores por pestaña
- DADO un usuario con 3 juegos en `InCollection`, 5 en `Played`, 2 en `Wishlist`, 1 en `WantToBuy` y 1 préstamo activo
- CUANDO el usuario navega a `/mi-ludoteca`
- ENTONCES las pestañas DEBEN mostrar los conteos numéricos exactos `(3)`, `(5)`, `(2)`, `(1)` y `(1)`.

#### Escenario: Cambio de pestaña reactivo
- DADO el usuario en `/mi-ludoteca`
- CUANDO pulsa la pestaña *Deseados*
- ENTONCES la vista DEBE mostrar exclusivamente los juegos clasificados como `Wishlist`.

---

### Requerimiento: Gestión de Préstamos Activos en Mi Ludoteca
La pestaña de *Préstamos activos* DEBE listar todos los préstamos con `IsReturned = false`, detallando el juego, nombre del prestatario, fecha de préstamo y un botón directo para registrar la devolución.

#### Escenario: Devolver préstamo desde la pestaña de préstamos
- DADO un préstamo activo visible en la pestaña de préstamos
- CUANDO el usuario presiona `[ Marcar como devuelto ]`
- ENTONCES el préstamo DEBE marcarse como devuelto
- Y el contador de préstamos activos DEBE decrementarse en 1 inmediatamente.
