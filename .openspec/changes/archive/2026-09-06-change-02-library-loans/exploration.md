# Exploration: change-02-library-loans (Incremento 2: Ludoteca Personal, Colección en 4 Estados y Préstamos)

## Current State

### Estado Actual del Código y Arquitectura
- **Solución y Plataforma:**
  - Solución .NET 10 (`src/Ludeca.slnx`) con arquitectura limpia en 4 capas (`Ludeca.Core`, `Ludeca.Application`, `Ludeca.Infrastructure`, `Ludeca.Web`) y tests en `tests/Ludeca.UnitTests`.
  - Entorno de desarrollo verificado y operativo con 27 pruebas unitarias e integración en verde (100% pasando).
- **Capa de Dominio (`Ludeca.Core`):**
  - Implementado el agregado raíz `Game` con Value Objects de ADN lúdico, semáforo de escalabilidad (1 a 7+ jugadores), accesibilidad y fundas.
  - Actualmente, **no existe ningún concepto de usuario, colección personal, préstamos de juegos ni reseñas/valoraciones de usuarios**.
- **Capa de Aplicación (`Ludeca.Application`):**
  - Existen `ICatalogService`, `IGameRepository`, `IBggClient` y DTOs orientados exclusivamente al catálogo público (`GameDetailDto`, `GameSummaryDto`, `GameFilterCriteria`).
  - No hay contratos ni servicios para gestionar el estado de un juego respecto a un usuario, registrar préstamos ni almacenar valoraciones rápidas.
- **Capa de Infraestructura (`Ludeca.Infrastructure`):**
  - `LudecaDbContext` gestiona únicamente la tabla `Games` en SQLite con mapeo JSON nativo (`OwnsMany().ToJson()`) para `Scalability` y `Sleeves`.
  - Repositorio `SqliteGameRepository` con consultas filtradas y ordenadas.
- **Capa Web (`Ludeca.Web`):**
  - Interfaz Blazor Web App interactiva con páginas `Home.razor` (catálogo y filtros) y `GameDetail.razor` (ficha con badges, semáforo y fundas).
  - La ficha de juego actual es de sólo lectura: carece de la barra de acción inferior para el pulgar con los 4 estados, carece de acción de préstamo y carece de formulario de valoración o tarjeta de opinión propia.
  - No existe la página `/mi-ludoteca` ni navegación hacia ella.
- **Alcance Funcional según Especificación Maestra (`docs/specs/LUDIST_SPEC_FUNCIONAL_MVP.md` - Sección 7):**
  - 7.1 Colección en 4 estados: *En mi ludoteca* (verde), *Jugado* (azul), *Deseado* (amarillo) y *Quiero comprar* (rojo).
  - 7.2 Modo préstamo integrado ("¿A quién se lo dejé?"): registrar destinatario y fecha desde juegos en propiedad; listado privado y devolución en 1 toque.
  - 7.3 Formulario modular de valoración en 45 segundos: puntuación 1-10, micro-reseña máx. 280 caracteres, chips de comensales 1J-7J+ con semáforo personal, experiencia infantil opcional y contexto de juego.
  - Tarjeta de valoración propia destacada con edición rápida.

---

## Affected Areas

### 1. Dominio (`src/Ludeca.Core`)
- **Nuevas Entidades y Agregados:**
  - `UserCollectionItem`: Representa la vinculación de un usuario con un juego en uno de los 4 estados lúdicos (`InCollection`, `Played`, `Wishlist`, `WantToBuy`). Registra fechas de alta y modificación.
  - `GameLoan`: Representa un préstamo de un juego en propiedad a una persona u organización (`BorrowerName`, `LoanDate`, `Notes`, `IsReturned`, `ReturnedDate`). Invariante: solo se pueden prestar juegos que estén en `InCollection`.
  - `UserGameReview`: Representa la micro-valoración de un usuario (`Score` 1.0–10.0, `MicroReview` máx. 280 caracteres, lista de votos de semáforo por comensal, experiencia infantil/familiar opcional, contexto de partida).
- **Nuevos Enums y Value Objects:**
  - `CollectionStatus`: `InCollection` (En mi ludoteca), `Played` (Jugado), `Wishlist` (Deseado), `WantToBuy` (Quiero comprar).
  - `PlayContextType`: `Owned` (Propiedad), `ClubOrAssociation` (Club/Asociación), `Friends` (Amigos), `BoardGameBar` (Bar lúdico), `Bga` (Board Game Arena / Digital).
  - `UserPlayerCountVote`: Value Object con `PlayerCount` (1 a 7+) y `Status` (`ScalabilityStatus`: MustPlay / Recommended / NotRecommended).
  - `UserFamilyExperienceVote`: Value Object con `PlayedWithChildren`, `SuggestedMinAge` (4, 6, 8, 10, 12, 14) y `IsAdaptedRules` (booleano o enum de reglas oficiales vs adaptadas).
- **Modificaciones a Entidades Existentes:**
  - `Game`: Método de dominio para recalcular o actualizar su `LudistRating` tras la inserción o actualización de valoraciones comunitarias.

### 2. Aplicación (`src/Ludeca.Application`)
- **Contratos e Interfaces:**
  - `IUserCollectionRepository`: Métodos para obtener ítems de colección por usuario, consultar el estado de un juego específico, añadir, actualizar estado o eliminar de colección.
  - `IGameLoanRepository`: Métodos para obtener préstamos activos e históricos de un usuario, registrar nuevo préstamo y marcar devolución.
  - `IUserReviewRepository`: Métodos para consultar reseña propia de un usuario para un juego, listar reseñas de un juego, crear/actualizar reseña y calcular estadísticas comunitarias (nota media y votos de escalabilidad).
  - `IUserLibraryService`: Servicio orquestador de alto nivel para gestionar colección, préstamos y valoraciones con lógica de negocio desacoplada de la UI.
  - `ICurrentUserService`: Abstracción ligera de usuario actual (`UserId`, `UserName`) que provee un usuario predeterminado local mientras el sistema OAuth completo entra en los incrementos posteriores.
- **DTOs y Modelos de Entrada/Salida:**
  - `UserCollectionItemDto`, `GameLoanDto`, `UserReviewDto`, `UserLibrarySummaryDto`.
  - Solicitudes (`Requests`): `SetCollectionStatusRequest`, `CreateLoanRequest`, `SubmitReviewRequest`.

### 3. Infraestructura (`src/Ludeca.Infrastructure`)
- **Persistencia con EF Core 10 y SQLite:**
  - Actualización de `LudecaDbContext`: adición de `DbSet<UserCollectionItem>`, `DbSet<GameLoan>`, `DbSet<UserGameReview>`.
  - Mapeo relacional e índices:
    - Índice compuesto único en `UserCollectionItem(UserId, GameId)`.
    - Índice compuesto en `GameLoan(UserId, IsReturned)`.
    - Índice compuesto único en `UserGameReview(UserId, GameId)`.
    - Mapeo JSON (`ToJson()`) para `UserGameReview.PlayerCountRatings` y `ComplexProperty` o JSON para `FamilyExperience`.
  - Implementaciones de repositorio: `SqliteUserCollectionRepository`, `SqliteGameLoanRepository`, `SqliteUserReviewRepository`.

### 4. Interfaz Web (`src/Ludeca.Web`)
- **Componentes Razor Nuevos en `Components/Shared`:**
  - `CollectionActionBar.razor`: Barra de acción ergonómica tipo mobile-first con los 4 botones de estado (🟢 En mi ludoteca, 🔵 Jugado, 🟡 Deseado, 🔴 Quiero comprar) con feedback táctil inmediato.
  - `LoanModal.razor`: Modal / panel emergente para registrar un préstamo con nombre de prestatario, fecha y notas; o mostrar estado de préstamo activo con botón de devolución rápida.
  - `ReviewBottomSheet.razor`: Formulario interactivo modular diseñado para completarse en 45 segundos (selector numérico 1-10, textarea de 280 caracteres con contador en vivo, chips de comensales 1J-7J+ con semáforo, toggle infantil y selector de contexto).
  - `UserReviewCard.razor`: Tarjeta visual destacada en la ficha que muestra la valoración del usuario autenticado con botón `[ ✏️ Editar mi valoración ]`.
- **Modificación de Páginas Existentes:**
  - `GameDetail.razor`: Integración de la barra de acciones ergonómica, visualización de estado actual del juego, apertura del modal de préstamo y del formulario de valoración, y presentación de la tarjeta de valoración del usuario encima del bloque de reseñas.
  - `MainLayout.razor` y Cabecera: Añadir enlace de navegación a "Mi Ludoteca" con badge o contador.
- **Nueva Página:**
  - `Pages/MyLibrary.razor` (`/mi-ludoteca`): Vista completa de la ludoteca personal organizada en pestañas reactivas:
    - *En mi ludoteca (N)*
    - *Jugados (N)*
    - *Deseados (N)*
    - *Quiero comprar (N)*
    - *Préstamos activos (N)* con botón de devolución en 1 clic.

### 5. Pruebas Unitarias (`tests/Ludeca.UnitTests`)
- Pruebas de dominio:
  - Invariantes de `UserCollectionItem` (cambio de estados, validaciones).
  - Invariantes de `GameLoan` (sólo prestar juegos propios, validación de fechas y nombres, marcación de devolución).
  - Invariantes de `UserGameReview` (rango de puntuación 1-10, longitud de micro-reseña <= 280 caracteres, votos de escalabilidad).
- Pruebas de aplicación:
  - Lógica de `UserLibraryService` al alternar estados, disparar sugerencia de reseña al marcar `Played` o `InCollection`, registrar préstamos y calcular agregados.
- Pruebas de persistencia:
  - Inserción y consulta en `LudecaDbContext` con SQLite en memoria, verificando serialización JSON de votos de jugadores y restricciones de unicidad.

---

## Approaches

### Enfoque 1: Arquitectura DDD Vertical Slice con Persistencia SQLite y Entidades Dedicadas (Recomendado)
- **Descripción:** Crear entidades de dominio independientes y ricas (`UserCollectionItem`, `GameLoan`, `UserGameReview`) en `Ludeca.Core`. Conectar a través de interfaces de repositorio en `Ludeca.Application` y tablas dedicadas en `LudecaDbContext` con soporte de índices y `.ToJson()` para votos dinámicos.
- **Pros:**
  - Mantiene pureza absoluta de Clean Architecture y DDD.
  - Altamente testeable mediante pruebas unitarias aisladas sin bases de datos.
  - Preparado para migrar a PostgreSQL o SQL Server sin alterar el dominio.
  - Escalable para el sistema de OAuth futuro (`UserId` ya desacoplado).
  - Consultas indexadas de alto rendimiento (`UserId`, `GameId`, `IsReturned`).
- **Cons:**
  - Requiere creación de repositorios dedicados y configuración EF Core explícita.
- **Esfuerzo:** Medio.

### Enfoque 2: Embebido Directo en la Entidad `Game` (Descartado)
- **Descripción:** Tratar la colección y préstamos como listas embebidas dentro de la entidad `Game`.
- **Pros:** Menos archivos iniciales.
- **Cons:**
  - Viola principios DDD: `Game` es un agregado del catálogo público, no debe conocer la colección privada de miles de usuarios.
  - Colisiones de concurrencia y problemas masivos de rendimiento.
- **Esfuerzo:** Alto a largo plazo por refactorización inevitable.

---

## Recommendation

Se recomienda firmemente el **Enfoque 1 (DDD Vertical Slice con Entidades Dedicadas)**:
1. Diseñar `UserCollectionItem`, `GameLoan` y `UserGameReview` como entidades con invariantes de negocio estrictas.
2. Usar un `ICurrentUserService` que provea un `UserId` por defecto (ej. `"usuario-demo-fundador"` o guid local persistente) para permitir operar inmediatamente en el navegador sin bloquearse por la falta de OAuth.
3. Integrar la barra de acción con micro-interacciones ágiles y el formulario en 45 segundos con Tailwind CSS y componentes Razor nativos.

---

## Risks

| Riesgo | Severidad | Mitigación |
|---|---|---|
| Experiencia de usuario en móviles con modales | Media | Diseñar `ReviewBottomSheet` y `LoanModal` con diseño ergonómico mobile-first para evitar desbordamientos en pantallas pequeñas. |
| Rendimiento al consultar estado de colección para múltiples juegos | Baja | Índices compuestos en SQLite (`UserId`, `GameId`) y métodos optimizados de carga por lote si fuera necesario. |
| Validación de límite de 280 caracteres en cliente y servidor | Baja | Doble validación: contador en tiempo real en UI y guardián en la entidad de dominio. |

---

## Ready for Proposal
**Sí.** El análisis está completado con claridad sobre las entidades, servicios, persistencia y componentes necesarios. Estamos listos para generar la propuesta formal (`proposal.md`) del Incremento 2: `change-02-library-loans`.
