# 06. Comunidad, Sorteos y Consultorio de Reglas Q&A

## 1. Visión General y Propósito
Este módulo dinamiza la comunidad de jugadores mediante el Radar de Sorteos (con control de expiración automática), el boletín de novedades semanales de tiendas, el motor de generación de imágenes de marca para Instagram y el consultorio técnico de reglas para resolver dudas durante las partidas.

---

## 2. Modelo de Dominio (`Ludeka.Core`)

### 2.1 Entidad `Giveaway` (Radar de Sorteos)
Ubicación: [`src/Ludeka.Core/Entities/Giveaway.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/Giveaway.cs)

- `Id` (Guid), `GameId` (Guid?), `Title`, `SourceUrl`, `OrganizerAccount`, `Platform` (`Instagram`, `Twitter`, `YouTube`, `Telegram`, `WebEditorial`).
- `DeadlineAt` (DateTime): Fecha límite de participación.
- `IsActive`: booleano calculado que expira automáticamente al superar `DeadlineAt`.
- `CustomImageUrl`: Fotografía específica del sorteo o fallback a la portada del juego.

### 2.2 Entidad `WeeklyRelease` (Novedades de Viernes)
Ubicación: [`src/Ludeka.Core/Entities/WeeklyRelease.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/WeeklyRelease.cs)

- `Id` (Guid), `GameId` (Guid), `ReleaseDate` (DateTime), `Publisher`, `Price` (decimal?), `StoreUrl`.

### 2.3 Entidades `RuleQA` y `RuleAnswer` (Consultorio de Reglas)
Ubicación: [`src/Ludeka.Core/Entities/RuleQA.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/RuleQA.cs)

- Preguntas asociadas a un juego con título, detalle y autor.
- Respuestas con contador de votos de la comunidad y marca `IsAccepted` (Respuesta oficial / aceptada por la comunidad o fundador).

---

## 3. Servicios de Aplicación (`Ludeka.Application`)

- **Radar de Sorteos:** [`IGiveawayService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IGiveawayService.cs) implementado en [`GiveawayService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Community/GiveawayService.cs).
- **Lanzamientos Semanales:** [`IWeeklyReleaseService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IWeeklyReleaseService.cs) implementado en [`WeeklyReleaseService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Community/WeeklyReleaseService.cs).
- **Consultorio de Reglas:** [`IRuleQAService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IRuleQAService.cs) implementado en [`RuleQAService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Community/RuleQAService.cs).
- **Motor de Composición Gráfica (Social Cards):** [`ISocialCardService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/ISocialCardService.cs) implementado en [`SocialCardService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Community/SocialCardService.cs):
  - Genera composiciones cuadradas 1:1 para Instagram con portada centrada, píldora identificativa destacada y pie de marca corporativo.

---

## 4. Componentes UI (`Ludeka.Web`)

- [`Radar.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/Radar.razor): Página pública con pestañas para sorteos activos y novedades de viernes.
- [`GiveawayCard.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/GiveawayCard.razor): Tarjeta visual con cuenta atrás y enlace directo al sorteo.
- [`RuleQAPanel.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/RuleQAPanel.razor): Sección interactiva de preguntas y respuestas en la ficha del juego.
- [`Transparency.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/Transparency.razor): Manifiesto de Transparencia de Fondos.
