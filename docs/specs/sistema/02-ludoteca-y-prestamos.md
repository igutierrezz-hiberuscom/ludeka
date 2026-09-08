# 02. Ludoteca Personal, Colección y Préstamos

## 1. Visión General y Propósito
Este módulo gestiona la colección lúdica individual de cada usuario en 4 estados diferenciados, el registro privado de préstamos a amigos o asociaciones y el sistema de valoración rápida de 45 segundos con desglose por número de comensales.

---

## 2. Modelo de Dominio (`Ludeka.Core`)

### 2.1 Entidad `UserCollectionItem`
Ubicación: [`src/Ludeka.Core/Entities/UserCollectionItem.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/UserCollectionItem.cs)

- `Id` (Guid), `UserId` (string), `GameId` (Guid).
- `Status`: Enum `CollectionStatus`:
  - `InCollection` (🟢 En mi ludoteca - copia física en propiedad).
  - `Played` (🔵 Jugado - probado en club, asociación o bar).
  - `Wishlist` (🟡 Deseado - radar de interés).
  - `WantToBuy` (🔴 Quiero comprar - seguimiento comercial).
- `IsPendingCatalog`: booleano que indica si el título proviene de una importación de BGG aún en cola.
- `PendingBggId`: identificador BGG en espera de promoción.
- `AddedAt`, `UpdatedAt`.

### 2.2 Entidad `GameLoan` ("¿A quién se lo dejé?")
Ubicación: [`src/Ludeka.Core/Entities/GameLoan.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/GameLoan.cs)

- `Id` (Guid), `UserId` (string), `GameId` (Guid).
- `BorrowerName`: Nombre de la persona o asociación.
- `LoanedAt`: Fecha de salida.
- `ReturnedAt`: Fecha de devolución (`null` mientras esté activo).
- `Notes`: Anotaciones sobre componentes o fecha pactada.

### 2.3 Entidades de Reseña y Valoración
Ubicación: [`src/Ludeka.Core/Entities/UserGameReview.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/UserGameReview.cs) y [`UserPlayerCountVote.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/UserPlayerCountVote.cs)

- `Rating`: Valor numérico entre 1.0 y 10.0 (con incrementos de 0.5).
- `ShortReview`: Micro-reseña con límite de 280 caracteres.
- `PlayerCountVotes`: Colección de votos por número de jugadores (`PlayerCount`, `Status` MustPlay/Recommended/NotRecommended).
- `FamilyExperience`: Indicadores opcionales si se jugó con niños (edad mínima real y adaptación de reglas).
- `PlayContext`: Selector de entorno (Propiedad, Asociación, Bar, Amigos, BGA).

---

## 3. Capa de Aplicación (`Ludeka.Application`)

- **Servicio:** [`UserLibraryService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Library/UserLibraryService.cs)
  - `SetCollectionStatusAsync(userId, gameId, status, ct)`
  - `GetLibraryAsync(userId, filter, ct)`
  - `RecordLoanAsync(userId, gameId, borrower, date, ct)`
  - `ReturnLoanAsync(loanId, ct)`
  - `SubmitReviewAsync(reviewDto, ct)`
- **Contratos:** [`IUserCollectionRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IUserCollectionRepository.cs), [`IGameLoanRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IGameLoanRepository.cs), [`IUserReviewRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IUserReviewRepository.cs).

---

## 4. Componentes UI (`Ludeka.Web`)

- [`MyLibrary.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/MyLibrary.razor): Vista completa de la colección con filtros por estado, contador de prestados, panel de preferencias de tema y pestaña `🧬 ADN y Estadísticas`.
- [`ReviewBottomSheet.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/ReviewBottomSheet.razor): Modal/Bottom sheet táctil de valoración con slider numérico de 1 a 10 y chips de comensales.
- [`LoanModal.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/LoanModal.razor): Formulario accesible para registrar nuevo préstamo.

---

## 5. Estadísticas Avanzadas de Colección y ADN del Jugador (Incremento 15)

- **Servicio:** [`UserLibraryStatsService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Library/UserLibraryStatsService.cs) bajo contrato [`IUserLibraryStatsService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IUserLibraryStatsService.cs).
- **Métricas Calculadas en Tiempo Real (sobre títulos `InCollection`):**
  - **Horas Acumuladas en Estantería (`ShelfTimeStatsDto`):** Sumatorio de duración mínima y máxima convertidas a horas (`TotalMinHours` – `TotalMaxHours`).
  - **ADN Lúdico (`PlayerDnaDistributionDto`):** Proporción porcentual de los 5 estilos lúdicos (`Eurogames`, `Temáticos/Ameritrash`, `Party Games`, `Fillers/Abstractos`, `Campaña/Narrativos`) con identificación de estilo dominante, además de dimensiones transversales (% Cooperativos y % Solitario).
  - **Curva y Sweet Spot de Comensales (`ScalabilitySweetSpotDto`):** Distribución de títulos optimizados de 1 a 7+ jugadores identificando el tamaño de mesa con mayor cobertura.
  - **Radar de Fundas y Protección (`SleeveProtectionRadarDto`):** Cómputo global de cartas en la colección, cálculo automático de paquetes de 50 fundas requeridos y ranking de formatos de cartas más frecuentes.
  - **Top Diseñadores y Editoriales:** Parsing robusto de diseñadores múltiples con ordenación por presencia porcentual en la colección.
  - **Gamificación No Invasiva (`PlayerBadgeDto`):** Rango de coleccionista por volumen (Niveles 0 a 4: *Estantería en Blanco*, *Iniciado de Mesa*, *Explorador Lúdico*, *Veterano de Mesa*, *Mecenas Lúdico*) y rasgo distintivo lúdico (ej. *Cerebro Eurogamer 🧠*, *Héroe Temático ⚔️*, *Alma de la Fiesta 🎉*, *Espíritu Cooperativo 🤝*, *Maestro del Filler ⚡*, *Lobo Solitario 🐺*, *Paladar Ecléctico 🌈*).
- **Componentes y Vistas Blazor:**
  - [`LibraryStatsDashboard.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Features/Library/LibraryStatsDashboard.razor): Tablero interactivo con diseño editorial, barra de espectro continuo y gráficos de frecuencia con Tailwind CSS.
  - [`PublicProfile.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/PublicProfile.razor): Página de perfil público compartible en `/u/{UserId}` y `/perfil/{UserId}` con botón de copiado de enlace al portapapeles y vitrina de títulos físicos.
