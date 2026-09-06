# Propuesta: change-06-automation-community (Incremento 6: Automatización Omnicanal, Radar de Sorteos y Comunidad)

## 1. Resumen Ejecutivo y Motivación

El **Incremento 6** es el último vertical slice del Roadmap del MVP de **Ludeka** (`ROADMAP_MVP_SLICES.md`) y cierra integralmente los puntos **6.1 a 6.3, 10.1 a 10.5, 11.1 y 11.2** de la especificación funcional maestra (`LUDIST_SPEC_FUNCIONAL_MVP.md`).

### ¿Por qué este incremento es crucial para el lanzamiento del MVP?
1. **Tracción Orgánica a Coste 0 € (Motor de Plantillas de Redes):** Permite a los fundadores y creadores generar en 1 clic carteles gráficos cuadrados (1:1) de alta resolución listos para publicar en Instagram con la portada oficial, ADN lúdico, badges y pie de marca de Ludeka, acompañados de copy editorial y hashtags curados.
2. **Utilidad Diaria en Mesa (Consultorio de Reglas Q&A):** Resuelve el problema común de *"duda a mitad de partida"* mediante un sistema ágil estilo StackOverflow indexado por juego, con respuestas comunitarias verificadas y marcadas como aceptadas.
3. **Radar Unificado (Sorteos y Novedades del Viernes):** Centraliza las oportunidades lúdicas dispersas en redes con fusión inteligente de colaboraciones (editorial + creador en una sola tarjeta sin duplicados) y expiración automática.
4. **Confianza y Ética (Manifiesto de Transparencia):** Establece el compromiso comunitario del proyecto con el desglose transparente del destino de cualquier ingreso por afiliación o mecenazgo.

---

## 2. Alcance Detallado

### 2.1 Motor de Automatización Gráfica Omnicanal (Punto 6 de la Spec)
- **Generación del Cartel Cuadrado (1:1):**
  - Composición vectorial/visual nativa SVG de 1080x1080px (exportable y renderizable en cliente/servidor) que incluye:
    - Fondo editorial oscuro elegante (#0B0F17) con gradiente radial sutil de marca (#F97316).
    - Portada oficial de BGG del juego con bordes redondeados y sombra de profundidad.
    - Título comercial en español en tipografía sans-serif audaz.
    - Píldora de ADN Lúdico / Escalabilidad (ej. *"⚔️ Eurogame • 🟢 Ideal 2 Jugadores"*).
    - Sello de recomendación de la Mesa Fundadora si existe (ej. *"🛡️ Sello Fundador: Imprescindible"*).
    - Pie de marca inferior: *"Ludeka • El Letterboxd de los juegos de mesa en español"*.
- **Generador de Copywriting para Instagram:**
  - Texto formateado con emojis, resumen de duración/comensales, llamada a la acción y bloques de hashtags: `#juegosdemesa #boardgames #ludeka #devir #malditogames #asmodee` etc.
- **Acciones Rápidas:**
  - Botón `[ 📋 Copiar Texto ]` para el portapapeles.
  - Botón `[ 📥 Descargar Imagen SVG/PNG ]`.

### 2.2 Radar de Sorteos y Fusión de Colaboraciones (Punto 10.1 y 10.3)
- **Entidad `Giveaway`:**
  - `Id` (Guid), `Title` (string), `Organizer` (string, ej. "Devir Iberia"), `Collaborator` (string opcional, ej. "El Rincón Legacy"), `Url` (string), `Platform` (Instagram, X, YouTube, Comunidad), `DeadlineAt` (DateTimeOffset), `GameId` (Guid opcional), `GameTitle` (string opcional), `ThumbnailUrl` (string opcional), `IsCommunityExclusive` (bool), `CreatedAt` (DateTimeOffset).
- **Reglas de Negocio:**
  - **Fusión de Colaboraciones:** Si se registra un sorteo conjunto entre una editorial y un influencer para el mismo juego, se unifican bajo la misma tarjeta destacando *"Organizado por Devir en colaboración con El Rincón Legacy"*, previniendo tarjetas duplicadas en el radar.
  - **Expiración Dinámica:** Propiedad calculada `IsExpired => DeadlineAt < DateTimeOffset.UtcNow`. En la interfaz, los sorteos vencidos muestran el badge *"Finalizado"* o se ocultan con el filtro activo.
  - **Badge de Cuenta Atrás:** *"⏳ Finaliza en 2 días"*, *"🔥 ¡Últimas horas!"*, *"Terminado"*.
  - **Sorteos Comunitarios Exclusivos:** Distintivo *"🎁 Exclusivo Comunidad Ludeka"*, destinado a usuarios activos (al menos 1 micro-reseña o 1 voto en el consultorio).

### 2.3 Radar de Lanzamientos Semanales (Punto 10.2)
- **Entidad `WeeklyRelease`:**
  - `Id` (Guid), `Title` (string), `Publisher` (string), `ReleaseDate` (DateOnly / DateTimeOffset), `GameId` (Guid opcional), `CoverImageUrl` (string opcional), `EstimatedPvp` (decimal opcional), `IsReprint` (bool, Distintivo *"Reimpresión"* vs *"Novedad en tiendas"*), `Notes` (string opcional).
- **Visualización:**
  - Carrusel o cuadrícula limpia con los lanzamientos del viernes de la semana en curso.

### 2.4 Consultorio de Reglas Q&A estilo StackOverflow (Punto 10.4)
- **Entidades `RuleQuestion`, `RuleAnswer` y `RuleVote`:**
  - `RuleQuestion`: `Id`, `GameId`, `UserId`, `UserName`, `Title`, `Body`, `VotesCount`, `AcceptedAnswerId`, `CreatedAt`.
  - `RuleAnswer`: `Id`, `QuestionId`, `UserId`, `UserName`, `Body`, `OfficialRuleReference`, `VotesCount`, `IsAccepted`, `CreatedAt`.
  - `RuleVote`: `Id`, `UserId`, `QuestionId?`, `AnswerId?`, `CreatedAt`.
- **Reglas de Negocio:**
  - Cualquier usuario registrado puede publicar una pregunta y responder.
  - Solo el autor de la pregunta o un miembro con rol `FoundingTeam`/`Moderator` puede marcar una respuesta como **Aceptada (`IsAccepted = true`)**.
  - Sistema de votos anti-spam (un usuario solo puede votar una vez cada pregunta o respuesta; un nuevo voto retira el anterior o suma +1).
  - Ordenación de respuestas: primero la respuesta aceptada (fijada al inicio con borde verde y badge `✓ Solución Aceptada`), seguida por orden de votos descendente.

### 2.5 Manifiesto de Transparencia de Fondos (Punto 11.2)
- **Página `/transparencia`:**
  - Declaración institucional de Ludeka como proyecto independiente sin publicidad invasiva.
  - Desglose claro de los tres destinos de fondos:
    1. Mantenimiento y hosting a coste eficiente.
    2. Copias para análisis exhaustivo a 2 jugadores y familias con fotos reales.
    3. Financiación de sorteos periódicos para la comunidad activa.
  - Botones de apoyo (Ko-fi / Mecenazgo) e invitación al servidor de Discord.
- **Pie de página global:** Enlaces a `/radar`, `/transparencia`, Discord y leyenda actualizada del MVP.

---

## 3. Arquitectura y Componentes a Implementar

### 3.1 Dominio (`src/Ludeka.Core`)
- `Entities/Giveaway.cs`
- `Entities/WeeklyRelease.cs`
- `Entities/RuleQuestion.cs`
- `Entities/RuleAnswer.cs`
- `Entities/RuleVote.cs`
- `Enums/GiveawayPlatform.cs`

### 3.2 Aplicación (`src/Ludeka.Application`)
- `Contracts/IGiveawayRepository.cs`
- `Contracts/IWeeklyReleaseRepository.cs`
- `Contracts/IRuleQARepository.cs`
- `Contracts/IGiveawayService.cs`
- `Contracts/IWeeklyReleaseService.cs`
- `Contracts/IRuleQAService.cs`
- `Contracts/ISocialCardService.cs`
- `DTOs/CommunityDtos.cs`
- `Features/Community/GiveawayService.cs`
- `Features/Community/WeeklyReleaseService.cs`
- `Features/Community/RuleQAService.cs`
- `Features/Community/SocialCardService.cs`

### 3.3 Infraestructura (`src/Ludeka.Infrastructure`)
- Actualización de `LudekaDbContext.cs` con los nuevos `DbSet` e índices relacionales.
- Implementación de repositorios SQLite:
  - `Data/SqliteGiveawayRepository.cs`
  - `Data/SqliteWeeklyReleaseRepository.cs`
  - `Data/SqliteRuleQARepository.cs`
- Actualización de `CatalogSeeder.cs` con sorteos vigentes (ej. Devir / Maldito Games), novedades del viernes y dudas de reglas resueltas.

### 3.4 Interfaz de Usuario Blazor (`src/Ludeka.Web`)
- `Components/Shared/SocialCardModal.razor` (Generador de plantillas 1:1 y copy).
- `Components/Shared/RuleQuestionsSection.razor` (Consultorio Q&A para la ficha).
- `Components/Shared/GiveawayCard.razor` (Tarjeta visual de sorteos con fusión y countdown).
- `Components/Pages/Radar.razor` (`/radar` y `/sorteos`).
- `Components/Pages/Transparency.razor` (`/transparencia`).
- Actualización de `GameDetail.razor` (botón de cartel para redes y sección Q&A).
- Actualización de `MainLayout.razor` (navegación y footer).

---

## 4. Criterios de Aceptación (RFC 2119 & Gherkin)

### Escenario 1: Fusión de colaboraciones en el Radar de Sorteos
```gherkin
Given un sorteo existente organizado por "Maldito Games" para el juego "Brass: Birmingham"
When un usuario o moderador registra un sorteo conjunto de "Análisis Parálisis" con "Maldito Games" para el mismo título
Then el sistema debe fusionar la entrada en una única tarjeta
And la tarjeta debe exhibir "Organizado por Maldito Games en colaboración con Análisis Parálisis"
And la plataforma no debe mostrar tarjetas duplicadas para la misma campaña
```

### Escenario 2: Expiración automática de sorteos por fecha límite
```gherkin
Given un sorteo cuya fecha límite es anterior a la fecha y hora UTC actual
When el usuario consulta el Radar de Sorteos con el filtro activo de sorteos vigentes
Then el sorteo caducado no debe aparecer entre los sorteos destacados
And si se consultan los sorteos históricos debe mostrar el distintivo "Finalizado"
```

### Escenario 3: Marcado de respuesta aceptada en el Consultorio de Reglas
```gherkin
Given una pregunta de reglas formulada por el usuario "Carlos"
When otro usuario aporta una respuesta explicando la regla de colocación
And el usuario "Carlos" o un moderador pulsa "Marcar como respuesta aceptada"
Then la respuesta debe quedar registrada con IsAccepted en verdadero
And la respuesta debe situarse en la primera posición visual destacada con borde verde
```

### Escenario 4: Generación de plantilla cuadrada 1:1 para redes
```gherkin
Given la ficha de un juego con ADN lúdico y carátula oficial
When el usuario pulsa "Generar Cartel para Redes"
Then el sistema debe componer la plantilla 1:1 con la portada, ADN lúdico, título y pie de marca Ludeka
And debe proporcionar un bloque de copy con hashtags en español y botón de copiado al portapapeles
```

---

## 5. Plan de Pruebas Unitarias y Cobertura
- Pruebas de Dominio: Creación de sorteos, fusión de colaboradores, cálculo de expiración, votos en preguntas/respuestas, marcado de solución aceptada.
- Pruebas de Aplicación: Casos de uso de `GiveawayService`, `WeeklyReleaseService`, `RuleQAService` y `SocialCardService`.
- Pruebas de Integración/Persistencia: Consultas SQLite en memoria verificando índices, cascadas y transacciones.
