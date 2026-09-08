# 10. Módulo de Síntesis con IA (Google Gemini / Heurística)

## 1. Visión General y Propósito
El Módulo de Síntesis con IA proporciona a Ludeka resúmenes inteligentes, objetivos y estructurados en español (`🤖 Resumen generado por IA`) para juegos recién catalogados o pendientes de análisis por la Mesa Fundadora. Responde a los tres ejes críticos del jugador moderno: escalabilidad y comensales ideales, accesibilidad y edad real, y huella en mesa/ritmo de juego.

---

## 2. Modelo de Dominio (`Ludeka.Core`)

### 2.1 Value Object `AiGameSummary`
Ubicación: [`src/Ludeka.Core/ValueObjects/AiGameSummary.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/AiGameSummary.cs)

- `GeneralVerdict`: Texto narrativo en español objetivo y conciso (2-3 oraciones).
- `ScalabilitySummary`: Rango de comensales óptimo y comportamiento del entreturno.
- `AgeSummary`: Comparativa entre edad legal de componentes (caja) y edad real observada.
- `FootprintSummary`: Superficie de salón requerida (cafetería, mesa estándar o monstruo de mesa) y duración por comensal.
- `Model`: Nombre del modelo o motor que produjo la síntesis (`"Google Gemini (gemini-2.5-flash)"`, `"Heurística Editorial"`, `"Heurística Editorial (Fallback)"`).
- `GeneratedAt`: Marca de tiempo UTC de generación.

### 2.2 Entidad `Game`
- Propiedad: `AiSummary` (`AiGameSummary?`).
- Mutador: `SetAiSummary(AiGameSummary summary)`.

---

## 3. Capa de Aplicación (`Ludeka.Application`)

### 3.1 Contrato `IAiGameSummaryService`
Ubicación: [`src/Ludeka.Application/Contracts/IAiGameSummaryService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IAiGameSummaryService.cs)

- `GenerateSummaryAsync(Game game, CancellationToken ct)`: Genera la síntesis estructurada.
- `EnsureSummaryForGameAsync(Guid gameId, CancellationToken ct)`: Recupera la síntesis persistida o la genera y persiste bajo demanda en el catálogo.
- `ProcessPendingSummariesBatchAsync(int batchSize, CancellationToken ct)`: Procesa por lotes (carga nocturna) juegos del catálogo sin resumen.

### 3.2 Desacoplo de la Ficha y Ejecución en Descarga BGG / Cargas Nocturnas
- **Navegación Ordinaria en Ficha (`FoundingVerdictService.GetAiSummaryForGameAsync`):** La carga de fichas es 100% libre de llamadas externas a la IA; únicamente lee de SQLite el `AiSummary` pre-existente. Si no existe, devuelve `null` sin latencia.
- **Botón de Moderación Bajo Demanda (`FoundingVerdictService.RequestAiSummaryGenerationAsync`):** Reservado para usuarios con rol `Moderator` o `FoundingTeam`. Permite generar o regenerar la síntesis de IA para una ficha específica bajo demanda.
- **Descargas desde BGG:**
  - [`BggCatalogQueueService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Bgg/BggCatalogQueueService.cs): Al procesar la cola comunitaria nocturna, genera y almacena la síntesis de IA de forma atómica.
  - [`BggSearchAssistedService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Bgg/BggSearchAssistedService.cs): Al incorporar un juego descargado en vivo de BGG, ejecuta la síntesis de IA y la persiste en el catálogo.
- **Cargas Nocturnas en Lote:** [`GeminiGameSummaryService.ProcessPendingSummariesBatchAsync`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Services/GeminiGameSummaryService.cs) itera por lotes sobre títulos no sintetizados en el catálogo.

---

## 4. Capa de Infraestructura (`Ludeka.Infrastructure`)

### 4.1 Opciones de Configuración (`GeminiOptions`)
Ubicación: [`src/Ludeka.Infrastructure/Services/GeminiOptions.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Services/GeminiOptions.cs)
- `Simulate`: Conmutador booleano (por defecto `true` para desarrollo seguro).
- `ApiKey`: Clave de API de Google AI Studio.
- `Model`: Identificador del modelo (por defecto `gemini-3.6-flash`). Admite valor `"auto"` o vacío para autoseleccionar el recomendado.
- `BaseUrl`: Endpoint REST de Google Gemini (`https://generativelanguage.googleapis.com/v1beta/`).
- `ShouldSimulate`: Propiedad evaluada (`Simulate || string.IsNullOrWhiteSpace(ApiKey)`).

### 4.2 Motor Heurístico Resiliente (`HeuristicGameSummaryGenerator`)
- Genera resúmenes deterministas en español analizando las características del juego sin depender de internet ni de tokens.

### 4.3 Servicio Conmutado, Autoselección y Tolerancia a Fallos (`GeminiGameSummaryService`)
- Si `ShouldSimulate == false`: Envía payload a la API REST de Google Gemini solicitando salida estructurada JSON (`application/json`).
- **Autoselección de Modelo:** Si no se especifica modelo o se indica `"auto"`, se selecciona de forma transparente el modelo canónico `gemini-3.6-flash`.
- **Autorrecuperación ante 404:** Si un modelo configurado resulta descontinuado, el cliente reintenta de forma automática con `gemini-3.6-flash`.
- Si `ShouldSimulate == true`: Ejecuta el generador heurístico de forma instantánea.
- **Zero-Crash Fallback:** Ante excepciones de red, timeouts o errores de cuota HTTP 429, conmuta de forma segura al motor heurístico con etiqueta `"Heurística Editorial (Fallback)"`.

### 4.4 Persistencia EF Core y SQLite
- `LudekaDbContext.cs`: Mapeo propio JSON `game.OwnsOne(g => g.AiSummary, b => b.ToJson());`.
- `SqliteSchemaMigrator.cs`: Reconciliación defensiva de la columna `AiSummary` (`TEXT NULL`) en la tabla `Games`.

---

## 5. Interfaz de Usuario (`Ludeka.Web`)

- Componente [`AiSummaryCard.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/AiSummaryCard.razor):
  - Badge principal `🤖 Síntesis generada por IA`.
  - Chip con el modelo utilizado (`Google Gemini (gemini-3.6-flash)` o `Heurística Editorial`).
  - Tres paneles visuales limpios para escalabilidad, edad y huella/tiempo.
  - Veredicto general y aviso editorial transparente.
- En [`GameDetail.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/GameDetail.razor):
  - Carga inmediata sin peticiones de IA para usuarios generales.
  - Botón de moderación `🤖 Generar con IA` / `🔄 Regenerar con IA` exclusivo para moderadores/fundadores.
  - Bloque editorial informativo con disparador manual cuando el juego carece de veredicto y de síntesis.
- En [`CatalogQueuePanel.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/CatalogQueuePanel.razor):
  - Botón administrativo `🌙 Carga Nocturna IA` para ejecutar el lote de enriquecimiento de catálogo bajo demanda con reporte de resultados en tiempo real.
