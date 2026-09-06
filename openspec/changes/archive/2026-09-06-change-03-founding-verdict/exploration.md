# Exploración: change-03-founding-verdict (Incremento 3: Panel y Veredicto de la Mesa Fundadora)

## 1. Estado Actual del Sistema

### 1.1 Solución y Arquitectura Base (.NET 10 y C# 13)
- **Estructura:** Arquitectura limpia en 4 capas (`Ludeca.Core`, `Ludeca.Application`, `Ludeca.Infrastructure`, `Ludeca.Web`) con suite de 54 pruebas unitarias e integración pasando al 100% en `tests/Ludeca.UnitTests`.
- **Incremento 1 (`change-01-core-catalog`):** Modelo de juego `Game` con ADN lúdico, semáforo dinámico de escalabilidad (1 a 7+ jugadores), edad real vs. legal, dependencia lingüística, duración por comensal, huella en mesa, guía de fundas y catálogo base con 15 títulos hispanos en SQLite.
- **Incremento 2 (`change-02-library-loans`):** Colección personal en 4 estados (`InCollection`, `Played`, `Wishlist`, `WantToBuy`), cuaderno de préstamos con devolución en 1 clic, formulario modular de valoración rápida en 45 segundos y vista de ludoteca personal `/mi-ludoteca`.
- **Identidad de Usuario Actual:** Existe la interfaz `ICurrentUserService` en `Ludeca.Application.Contracts` con implementación `DefaultCurrentUserService` en `Ludeca.Infrastructure.Services`, que provee un usuario predeterminado (`UserId`, `UserName`). Actualmente **no define roles** (`FoundingTeam`, `Moderator`, `User`), ni permisos de acceso para la mesa fundadora.
- **Visualización en Ficha de Juego (`GameDetail.razor`):** La ficha actual renderiza carátula, ratings (BGG vs Ludist), ADN lúdico, barra de colección/préstamos, tarjeta de valoración propia, semáforo de escalabilidad, guía de fundas y descripción oficial. Actualmente **no dispone de ningún bloque de síntesis de IA**, **no dispone de tarjeta de veredicto de la mesa fundadora**, **no dispone de galería de fotos de mesa real**, ni de botón o modal para gestionar dicho veredicto.

---

## 2. Requerimientos del Incremento 3 (según ROADMAP_MVP_SLICES y Especificación Maestra)

El Incremento 3 aborda los puntos **4.1** y **4.2** de `LUDIST_SPEC_FUNCIONAL_MVP.md`:

### 2.1 Ciclo de Vida del Veredicto en Tres Fases
1. **Fase 1 (Arranque con IA):**
   - Cuando un juego no tiene aún análisis humano de la mesa fundadora, se muestra un bloque visible con el distintivo `🤖 Resumen generado por IA`.
   - Síntesis estructurada y objetiva: evaluación de escalabilidad, edad real recomendada y huella en mesa basada en el consenso del hobby, evitando que las fichas queden vacías.
2. **Fase 2 (Fase Editorial Fundadora):**
   - En el momento en que el equipo fundador publica su análisis, el bloque de IA es **sustituido prioritariamente** por la tarjeta `👤 Veredicto de la Mesa Fundadora`.
   - Incorpora el **Sello de Recomendación Oficial** (*"Imprescindible de la Mesa"*, *"Recomendado con adaptaciones"*, *"Prescindible"*).
   - Incluye el análisis estructurado de la casa con doble foco:
     - **Foco en Pareja (2 jugadores):** Ritmo, tensión, adaptación del tablero, si requiere bot o si brilla de forma natural.
     - **Foco en Familias y Niños:** Edad real comunitaria, comprensión de reglas, adaptaciones caseras recomendadas y paciencia requerida.
   - Incorpora la **Galería de Fotos Reales de Partida:** Subida y renderizado de 1 a 3 fotos tomadas directamente en mesa física de salón o club (con pie de foto explicativo), aportando autenticidad frente a renders publicitarios.
3. **Fase 3 (Fase Comunitaria y Ponderación):**
   - La tarjeta del veredicto fundador se mantiene fija en la zona superior de la ficha (antes de opiniones abiertas de usuarios).
   - El veredicto de la mesa fundadora tiene un peso ponderado mayor en el algoritmo del ranking local (`LudistRating`).

### 2.2 Gestión Editorial y Roles
- **Acceso Restringido:** Solo usuarios con rol `FoundingTeam` o `Moderator` pueden ver el botón `[ 🛡️ Gestionar Veredicto Fundador ]` en la ficha del juego y acceder al modal/panel de edición.
- **Acceso en Modo Demostración:** Se proveerá un conmutador rápido de roles en la barra de navegación para permitir al revisor alternar entre modo usuario regular y modo fundador sin fricción de autenticación externa.

---

## 3. Áreas Afectadas y Diseño Preliminar

### 3.1 Dominio (`src/Ludeca.Core`)
- **Nueva Entidad `FoundingVerdict`:**
  - `Id` (Guid)
  - `GameId` (Guid, único por juego)
  - `AuthorUserId` (string)
  - `AuthorName` (string)
  - `Recommendation` (`FoundingRecommendation`: `MustPlay`, `RecommendedWithAdaptations`, `Skippable`)
  - `OverallVerdict` (string: análisis general de la casa)
  - `TwoPlayerVerdict` (string: análisis específico de juego en pareja a 2 comensales)
  - `FamilyVerdict` (string: análisis familiar y con niños)
  - `Photos` (`List<FoundingPhoto>`: 1 a 3 fotos reales)
  - `CreatedAt` y `UpdatedAt` (DateTimeOffset)
- **Nuevo Value Object `FoundingPhoto`:**
  - `Url` (string), `Caption` (string)
  - Invariante: URL válida, pie de foto descriptivo, máximo 3 fotos por veredicto.
- **Nuevo Enum `FoundingRecommendation`:**
  - `MustPlay` ("Imprescindible de la Mesa")
  - `RecommendedWithAdaptations` ("Recomendado con adaptaciones")
  - `Skippable` ("Prescindible")

### 3.2 Aplicación (`src/Ludeca.Application`)
- **Ampliación de `ICurrentUserService`:**
  - Propiedades o métodos para verificación de roles: `IReadOnlyList<string> Roles { get; }`, `bool IsFoundingTeam { get; }`, `bool IsInRole(string role)`.
- **Nuevo Repositorio `IFoundingVerdictRepository`:**
  - `GetByGameIdAsync(Guid gameId, CancellationToken ct)`
  - `AddAsync(FoundingVerdict verdict, CancellationToken ct)`
  - `UpdateAsync(FoundingVerdict verdict, CancellationToken ct)`
  - `DeleteAsync(Guid id, CancellationToken ct)`
  - `GetAllAsync(CancellationToken ct)`
- **Nuevo Servicio `IFoundingVerdictService`:**
  - `GetVerdictByGameIdAsync(Guid gameId, CancellationToken ct)`
  - `SaveVerdictAsync(SaveFoundingVerdictRequest request, CancellationToken ct)`
  - `GetAiSummaryAsync(Guid gameId, CancellationToken ct)`: Genera o suministra la síntesis inteligente de Fase 1 para juegos sin veredicto fundador.
- **DTOs:**
  - `FoundingVerdictDto`, `FoundingPhotoDto`, `SaveFoundingVerdictRequest`, `AiGameSummaryDto`.

### 3.3 Infraestructura (`src/Ludeca.Infrastructure`)
- **Persistencia EF Core 10 con SQLite (`LudecaDbContext`):**
  - Nueva tabla `FoundingVerdicts` con índice único sobre `GameId`.
  - Mapeo de `Photos` mediante `OwnsMany(x => x.Photos).ToJson()`.
  - Repositorio `SqliteFoundingVerdictRepository`.
- **Actualización de `DefaultCurrentUserService`:**
  - Soporte de roles configurables (con rol `FoundingTeam` asignado por defecto para la experiencia fundadora, pero permitiendo alternancia dinámica).
- **Semillado de Datos (`CatalogSeeder`):**
  - Carga de veredictos fundadores de ejemplo con fotos reales para títulos representativos (ej. *Ark Nova*, *Wingspan*, *Carcassonne*), dejando otros títulos en Fase 1 (resumen de IA) para validar la sustitución visual.

### 3.4 Web y UI Blazor (`src/Ludeca.Web`)
- **Componente `FoundingVerdictCard.razor`:**
  - Tarjeta con diseño editorial, sello de recomendación con código cromático (oro, azul/esmeralda, pizarra), pestañas o bloques para análisis general, pareja (2J) y familias/niños, más galería de fotos reales de partida con lightbox/ampliación.
- **Componente `AiSummaryCard.razor`:**
  - Tarjeta identificada claramente con `🤖 Resumen generado por IA`, sintetizando escalabilidad, edad y huella de forma inteligente cuando no hay veredicto fundador.
- **Botón y Modal de Edición `[ 🛡️ Gestionar Veredicto Fundador ]`:**
  - Botón visible exclusivamente para usuarios con rol `FoundingTeam` o `Moderator`.
  - Componente `FoundingVerdictModal.razor` para crear, editar y subir fotos de mesa real.
- **Selector de Rol en Barra de Navegación (`MainLayout.razor` o cabecera):**
  - Conmutador accesible `[ 👤 Rol: Mesa Fundadora / Usuario Regular ]` para permitir al usuario probar ambos estados sin necesidad de reconfigurar la base de datos.

---

## 4. Riesgos y Decisiones Técnicas

1. **Almacenamiento de Fotos Reales en MVP Offline-First:**
   - Para no incurrir en costes ni infraestructura externa compleja en el MVP (manteniendo la filosofía de 0 € de coste fijo), se admitirán URLs remotas directas (por ejemplo de servicios de hosting de imágenes o Unsplash/CDNs seguras) y datos embebidos o rutas locales en `wwwroot/uploads`.
2. **Compatibilidad con SQLite y DateTimeOffset:**
   - Como se descubrió en el Incremento 2, las consultas con ordenación por `DateTimeOffset` en SQLite deben realizarse en memoria con LINQ to Objects tras `ToListAsync()`.
3. **Ponderación de Rating Local (`LudistRating`):**
   - El veredicto de la mesa fundadora otorgará un peso equivalente a 3 votos comunitarios en el recálculo ponderado del `LudistRating`, reflejando la autoridad editorial de la plataforma.
