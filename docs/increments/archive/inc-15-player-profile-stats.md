# Incremento 15: Estadísticas Avanzadas de Colección y ADN del Jugador

- **Identificador SDD:** `change-15-player-profile-stats`
- **Estado:** ✅ **Archivado**
- **Puntos del MVP cubiertos:** 7.4 (Gamificación, Retención y Perfil Lúdico).
- **Objetivo Principal:** Ofrecer al usuario un panel visual interactivo de métricas sobre su colección personal ("El ADN de tu estantería"), calculando horas de juego disponibles, estilos dominantes, autores favoritos y valor estimado de protección con fundas.

---

## 1. Alcance Funcional Propuesto

1. **Cálculo de Métricas de la Ludoteca Personal (`IUserLibraryStatsService`):**
   - *Horas en estantería:* Sumatorio de minutos mínimos y máximos de los juegos en estado *En mi ludoteca*.
   - *Distribución de ADN Lúdico:* Porcentaje de la colección en Eurogames, Ameritrash/Temáticos, Party Games, Fillers y Cooperativos.
   - *Top Autores y Editoriales:* Diseñadores con mayor número de títulos en la ludoteca del usuario.
   - *Escalabilidad de la Colección:* Con cuántos comensales brilla más tu colección (ej. *"Tu ludoteca es perfecta para 2 y 4 jugadores"*).
   - *Radar de Protección:* Total de cartas en la colección y estimación de paquetes de fundas requeridos.
2. **Componente Visual en Perfil (`LibraryStatsDashboard.razor`):**
   - Gráficos de barras de progreso y anillos de distribución con Tailwind CSS.
   - Insignia de perfil basada en la actividad ("Explorador Lúdico", "Veterano de Mesa", "Mecenas").
3. **Página Compartible del Perfil Público:**
   - Visualización opcional pública para compartir la ludoteca con amigos o grupos de juego.

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Visualizar estadísticas de la ludoteca personal
  Dado un usuario con 10 juegos en su ludoteca
  Cuando accede a su panel de estadísticas en "/mi-ludoteca"
  Entonces el sistema muestra el total acumulado de horas de juego estimadas
  Y desglosa el porcentaje por estilo lúdico
  Y resalta a qué número de comensales está mejor equipada su colección
```
