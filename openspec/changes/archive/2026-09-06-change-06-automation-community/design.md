# Diseño Técnico: change-06-automation-community (Incremento 6: Automatización Omnicanal, Radar de Sorteos y Comunidad)

## 1. Visión General de la Arquitectura

Siguiendo los principios de **Clean Architecture** y **Vertical Slices** en .NET 10 y C# 13, el Incremento 6 añade las capacidades finales del MVP para automatización omnicanal, dinamización comunitaria y transparencia sin acoplamientos indeseados.

```mermaid
graph TD
    subgraph "src/Ludeka.Web (Blazor InteractiveServer)"
        RadarPage[Radar.razor - /radar y /sorteos]
        TranspPage[Transparency.razor - /transparencia]
        GameDetailPage[GameDetail.razor - /juegos/{Slug}]
        SocialModal[SocialCardModal.razor]
        QASect[RuleQuestionsSection.razor]
        GivCard[GiveawayCard.razor]
    end

    subgraph "src/Ludeka.Application (Casos de Uso)"
        GiveawaySvc[GiveawayService : IGiveawayService]
        WeeklySvc[WeeklyReleaseService : IWeeklyReleaseService]
        RuleQASvc[RuleQAService : IRuleQAService]
        SocialSvc[SocialCardService : ISocialCardService]
        RepoContracts[IGiveawayRepository / IWeeklyReleaseRepository / IRuleQARepository]
    end

    subgraph "src/Ludeka.Infrastructure (Persistencia y Datos)"
        DbContext[LudekaDbContext - EF Core 10]
        SqliteGiv[SqliteGiveawayRepository]
        SqliteRel[SqliteWeeklyReleaseRepository]
        SqliteQA[SqliteRuleQARepository]
        Seeder[CatalogSeeder con Sorteos, Novedades y Q&A]
    end

    subgraph "src/Ludeka.Core (Dominio Puro)"
        GiveawayEnt[Giveaway]
        WeeklyEnt[WeeklyRelease]
        RuleQEnt[RuleQuestion]
        RuleAEnt[RuleAnswer]
        RuleVEnt[RuleVote]
        PlatEnum[GiveawayPlatform]
    end

    RadarPage --> GiveawaySvc
    RadarPage --> WeeklySvc
    GameDetailPage --> SocialSvc
    GameDetailPage --> RuleQASvc
    SocialModal --> SocialSvc
    QASect --> RuleQASvc
    GiveawaySvc --> RepoContracts
    WeeklySvc --> RepoContracts
    RuleQASvc --> RepoContracts
    SqliteGiv --> DbContext
    SqliteRel --> DbContext
    SqliteQA --> DbContext
    DbContext --> GiveawayEnt
    DbContext --> WeeklyEnt
    DbContext --> RuleQEnt
    DbContext --> RuleAEnt
    DbContext --> RuleVEnt
```

---

## 2. Modelo de Dominio (`src/Ludeka.Core`)

### 2.1 Entidad `Giveaway`
- **Ruta:** `src/Ludeka.Core/Entities/Giveaway.cs`
- **Propiedades:**
  - `Guid Id` (PK)
  - `string Title` (Nombre del sorteo)
  - `string Organizer` (Editorial o tienda promotora)
  - `string? Collaborator` (Creador o influencer asociado)
  - `string Url` (Enlace a la publicación original)
  - `GiveawayPlatform Platform` (Instagram, TwitterX, YouTube, Community)
  - `DateTimeOffset DeadlineAt` (Fecha límite)
  - `Guid? GameId` (FK opcional al catálogo)
  - `string? GameTitle` (Título libre si no está en catálogo)
  - `string? ThumbnailUrl` (Carátula o imagen)
  - `bool IsCommunityExclusive` (Sorteo exclusivo Ludeka)
  - `DateTimeOffset CreatedAt`
- **Propiedades Calculadas y Métodos de Negocio:**
  - `bool IsExpired => DeadlineAt < DateTimeOffset.UtcNow;`
  - `void MergeCollaborator(string collaborator)`: Actualiza o concatena el colaborador para consolidar colaboraciones y evitar tarjetas duplicadas.
  - `void ExtendDeadline(DateTimeOffset newDeadline)`: Actualiza la fecha límite con validación (`newDeadline > DeadlineAt`).

### 2.2 Enum `GiveawayPlatform`
- **Ruta:** `src/Ludeka.Core/Enums/GiveawayPlatform.cs`
- **Valores:** `Instagram = 1`, `TwitterX = 2`, `YouTube = 3`, `Community = 4`, `Other = 5`.

### 2.3 Entidad `WeeklyRelease`
- **Ruta:** `src/Ludeka.Core/Entities/WeeklyRelease.cs`
- **Propiedades:**
  - `Guid Id` (PK)
  - `string Title` (Título del juego en tiendas)
  - `string Publisher` (Editorial que lo publica en España)
  - `DateOnly ReleaseDate` (Viernes de puesta a la venta)
  - `Guid? GameId` (FK opcional)
  - `string? CoverImageUrl`
  - `decimal? EstimatedPvp` (PVP orientativo en €)
  - `bool IsReprint` (true = reimpresión, false = novedad absoluta)
  - `string? Notes` (Información adicional)
  - `DateTimeOffset CreatedAt`

### 2.4 Entidades de Consultorio de Reglas: `RuleQuestion`, `RuleAnswer`, `RuleVote`
- **Rutas:**
  - `src/Ludeka.Core/Entities/RuleQuestion.cs`
  - `src/Ludeka.Core/Entities/RuleAnswer.cs`
  - `src/Ludeka.Core/Entities/RuleVote.cs`
- **Propiedades `RuleQuestion`:**
  - `Guid Id` (PK)
  - `Guid GameId` (FK obligatoria a `Games`)
  - `string UserId`
  - `string UserName`
  - `string Title` (Duda resumida)
  - `string Body` (Explicación de la situación de juego)
  - `int VotesCount`
  - `Guid? AcceptedAnswerId`
  - `DateTimeOffset CreatedAt`
  - `List<RuleAnswer> Answers`
  - **Métodos de Negocio:**
    - `void Upvote()` / `void Downvote()`
    - `void MarkAcceptedAnswer(Guid answerId, string requestingUserId, bool isModerator)`: Valida que solo el autor de la pregunta o un moderador/FoundingTeam pueda marcar una solución como oficial.
- **Propiedades `RuleAnswer`:**
  - `Guid Id` (PK)
  - `Guid QuestionId` (FK)
  - `string UserId`
  - `string UserName`
  - `string Body` (Solución)
  - `string? OfficialRuleReference` (ej. "Pág. 12, punto 4")
  - `int VotesCount`
  - `bool IsAccepted`
  - `DateTimeOffset CreatedAt`
  - **Métodos de Negocio:**
    - `void Upvote()` / `void Downvote()`
    - `void SetAccepted(bool accepted)`
- **Propiedades `RuleVote`:**
  - `Guid Id` (PK)
  - `string UserId`
  - `Guid? QuestionId`
  - `Guid? AnswerId`
  - `DateTimeOffset CreatedAt`

---

## 3. Capa de Aplicación (`src/Ludeka.Application`)

### 3.1 Contratos de Repositorio
- `IGiveawayRepository`:
  - `Task<IReadOnlyList<Giveaway>> GetActiveGiveawaysAsync(bool includeExpired = false, CancellationToken ct = default);`
  - `Task<Giveaway?> GetByIdAsync(Guid id, CancellationToken ct = default);`
  - `Task<Giveaway?> FindDuplicateOrCollaborativeAsync(string title, string organizer, DateTimeOffset deadline, CancellationToken ct = default);`
  - `Task AddAsync(Giveaway giveaway, CancellationToken ct = default);`
  - `Task UpdateAsync(Giveaway giveaway, CancellationToken ct = default);`
- `IWeeklyReleaseRepository`:
  - `Task<IReadOnlyList<WeeklyRelease>> GetReleasesAsync(DateOnly? fromDate = null, CancellationToken ct = default);`
  - `Task AddAsync(WeeklyRelease release, CancellationToken ct = default);`
- `IRuleQARepository`:
  - `Task<IReadOnlyList<RuleQuestion>> GetQuestionsByGameIdAsync(Guid gameId, CancellationToken ct = default);`
  - `Task<RuleQuestion?> GetQuestionWithAnswersAsync(Guid questionId, CancellationToken ct = default);`
  - `Task<RuleVote?> GetUserVoteAsync(string userId, Guid? questionId, Guid? answerId, CancellationToken ct = default);`
  - `Task AddQuestionAsync(RuleQuestion question, CancellationToken ct = default);`
  - `Task AddAnswerAsync(RuleAnswer answer, CancellationToken ct = default);`
  - `Task AddVoteAsync(RuleVote vote, CancellationToken ct = default);`
  - `Task RemoveVoteAsync(RuleVote vote, CancellationToken ct = default);`
  - `Task UpdateQuestionAsync(RuleQuestion question, CancellationToken ct = default);`
  - `Task UpdateAnswerAsync(RuleAnswer answer, CancellationToken ct = default);`

### 3.2 Servicios y Lógica de Negocio
- `IGiveawayService`:
  - Listado con cálculo de tiempo restante y badge.
  - Registro de sorteo con detección y fusión automática de colaboraciones.
- `IWeeklyReleaseService`:
  - Listado de novedades ordenadas por fecha de lanzamiento.
- `IRuleQAService`:
  - `CreateQuestionAsync`, `AddAnswerAsync`, `ToggleVoteQuestionAsync`, `ToggleVoteAnswerAsync`, `MarkAcceptedAnswerAsync`.
- `ISocialCardService`:
  - `GenerateSvgCard(SocialCardDataDto data)`: Genera un string SVG 1:1 (1080x1080px) perfectamente formateado y válido.
  - `GenerateInstagramCopy(SocialCardDataDto data)`: Genera el texto optimizado con emojis y hashtags curados.

---

## 4. Capa de Infraestructura (`src/Ludeka.Infrastructure`)

### 4.1 Configuración de EF Core 10 (`LudekaDbContext.cs`)
- `DbSet<Giveaway> Giveaways`
- `DbSet<WeeklyRelease> WeeklyReleases`
- `DbSet<RuleQuestion> RuleQuestions`
- `DbSet<RuleAnswer> RuleAnswers`
- `DbSet<RuleVote> RuleVotes`
- Relaciones e Índices:
  - `Giveaway`: Índice en `DeadlineAt`, `Platform`, `IsCommunityExclusive`.
  - `WeeklyRelease`: Índice en `ReleaseDate`.
  - `RuleQuestion`: Índice en `GameId`, `CreatedAt`. Cascada al borrar `Game`.
  - `RuleAnswer`: Índice en `QuestionId`, `IsAccepted`. Cascada al borrar `RuleQuestion`.
  - `RuleVote`: Índice compuesto único `(UserId, QuestionId)` y `(UserId, AnswerId)`.

### 4.2 Sembrado de Datos Iniciales (`CatalogSeeder.cs`)
- 4 Sorteos de ejemplo (Devir, Maldito Games con Análisis Parálisis fusionado, Zacatrus, Sorteo Exclusivo de Comunidad Ludeka).
- 4 Novedades del viernes en tiendas.
- 3 Preguntas de reglas reales con respuestas y soluciones aceptadas (ej. para *Brass: Birmingham* y *Terraforming Mars*).

---

## 5. Capa Web Blazor (`src/Ludeka.Web`)

1. **`Radar.razor` (`/radar` y `/sorteos`):**
   - Encabezado con selector de pestañas: `[ 🎁 Radar de Sorteos ]` y `[ 🚀 Novedades del Viernes ]`.
   - Tarjetas de sorteo con badges dinámicos ("Quedan 2 días", "Finaliza hoy", "Expirado") y colaboración destacada.
   - Formulario modal rápido para proponer/añadir un sorteo.
2. **`Transparency.razor` (`/transparencia`):**
   - Diseño editorial oscuro con alta legibilidad, explicando los 3 destinos de los fondos, botones de mecenazgo y Discord.
3. **`SocialCardModal.razor`:**
   - Modal interactivo activable desde la ficha del juego (`GameDetail.razor`).
   - Muestra el cartel 1:1 renderizado dinámicamente en SVG.
   - Botón para copiar el copy de Instagram al portapapeles y botón para descargar el SVG.
4. **`RuleQuestionsSection.razor`:**
   - Integrado en `GameDetail.razor`.
   - Listado de preguntas con votos (+1) y respuestas expandibles.
   - La respuesta aceptada se muestra con borde verde y badge oficial.
   - Formulario para publicar una nueva duda o respuesta.
5. **`MainLayout.razor`:**
   - Barra de navegación con enlace a "Radar" (`/radar`).
   - Pie de página con enlaces a `/radar`, `/transparencia`, Discord y resumen del MVP completado.
