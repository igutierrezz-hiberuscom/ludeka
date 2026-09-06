# Diseño Técnico: Incremento 3 — Panel y Veredicto de la Mesa Fundadora (`change-03-founding-verdict`)

## Enfoque Técnico
Implementación del ciclo de vida editorial del veredicto lúdico, control de roles de moderación (`FoundingTeam`, `Moderator`), galería fotográfica de mesa real y algoritmo de sustitución visual de IA por veredicto humano. Se sigue estrictamente la arquitectura limpia en 4 capas (.NET 10 y C# 13) y los principios de UI anti-plantillas con Tailwind CSS.

---

## Decisiones de Arquitectura

| Decisión | Opción Elegida | Alternativas Evaluadas | Justificación |
|---|---|---|---|
| **Modelo del Veredicto** | Agregado `FoundingVerdict` independiente vinculado por `GameId` | Campos sueltos dentro de `Game` | Mantiene el catálogo desacoplado del contenido editorial, facilitando permisos diferenciados, historial y autoría sin sobrecargar el agregado base. |
| **Almacenamiento de Fotos Reales** | Value Object inmutable `FoundingPhoto` serializado como JSON en SQLite (`OwnsMany().ToJson()`) | Tabla relacional normalizada de fotos | Límite acotado de 1 a 3 fotos por juego; el mapeo nativo JSON de EF Core 10 es atómico, rápido y evita joins innecesarios. |
| **Ciclo de Vida de IA (Fase 1 a 2)** | Sustitución condicional determinista en capa de servicio y UI (`FoundingVerdict != null`) | Flags booleanos manuales en base de datos | Cero deuda técnica: la mera existencia de un veredicto humano revoca automáticamente el resumen de IA sin inconsistencias de sincronización. |
| **Control de Roles en MVP** | `ICurrentUserService` con colección en memoria de roles y conmutador visual para demo | Bloquear desarrollo hasta conectar OAuth Google/Discord | Permite auditar y verificar permisos de inmediato en local; la interfaz `ICurrentUserService` se conectará a `AuthenticationStateProvider` en el Incremento de OAuth sin cambiar los consumidores. |
| **Ponderación en `LudistRating`** | Peso equivalente a 3 votos comunitarios asignados al sello fundador | Reemplazar totalmente la nota comunitaria | Refleja la autoridad del criterio fundador mientras respeta la voz de la comunidad lúdica. |

---

## Flujo de Datos

```
[Usuario / Moderador]
       │ (1. Abre /juegos/{slug})
       ▼
[GameDetail.razor] ──── Consulta ICurrentUserService.IsFoundingTeam
       │
       ├─► Si es FoundingTeam ──► Muestra botón [ 🛡️ Gestionar Veredicto Fundador ]
       │
       ├─► Si existe FoundingVerdict ──► Renderiza <FoundingVerdictCard> (Fase 2)
       │                                 (Sello oficial + Análisis 2J y Familias + Fotos)
       │
       └─► Si NO existe veredicto ─────► Renderiza <AiSummaryCard> (Fase 1)
                                         (Resumen objetivo generado por IA)
       │
[Moderador pulsa Gestionar]
       ▼
[FoundingVerdictModal] (Blazor Server interactivo)
       │ (Redacta análisis, selecciona sello, añade 1-3 fotos reales)
       ▼
[FoundingVerdictService] (Ludeca.Application)
       │ (Valida rol FoundingTeam y reglas de negocio)
       ▼
[SqliteFoundingVerdictRepository & LudecaDbContext] (Ludeca.Infrastructure)
       │ (Persiste en tabla FoundingVerdicts con fotos en JSON)
       ▼
[Game.UpdateLudistRating()] (Actualiza nota media con peso editorial ponderado)
```

---

## Cambios en Archivos y Componentes

| Archivo | Acción | Capa | Descripción |
|---|---|---|---|
| `src/Ludeca.Core/Enums/FoundingRecommendation.cs` | Crear | `Core` | Enum con sellos: `MustPlay`, `RecommendedWithAdaptations`, `Skippable` |
| `src/Ludeca.Core/ValueObjects/FoundingPhoto.cs` | Crear | `Core` | VO inmutable con `PhotoUrl` y `Caption` descriptivo |
| `src/Ludeca.Core/Entities/FoundingVerdict.cs` | Crear | `Core` | Agregado de veredicto con invariantes (máx. 3 fotos, análisis 2J y familiar obligatorios) |
| `src/Ludeca.Application/Contracts/ICurrentUserService.cs` | Modificar | `Application` | Añadir `Roles`, `IsFoundingTeam`, `IsInRole(role)` y `SwitchRole(role)` |
| `src/Ludeca.Application/Contracts/IFoundingVerdictRepository.cs` | Crear | `Application` | Contrato de persistencia para veredictos fundadores |
| `src/Ludeca.Application/Contracts/IFoundingVerdictService.cs` | Crear | `Application` | Contrato del servicio de veredictos y resúmenes de IA |
| `src/Ludeca.Application/DTOs/FoundingVerdictDtos.cs` | Crear | `Application` | DTOs de veredicto, fotos, petición de guardado y resumen de IA |
| `src/Ludeca.Application/Features/Founding/FoundingVerdictService.cs` | Crear | `Application` | Implementación de orquestación editorial y sustitución de IA |
| `src/Ludeca.Infrastructure/Data/LudecaDbContext.cs` | Modificar | `Infrastructure` | Adición de `DbSet<FoundingVerdict>` y mapeo JSON de fotos con índice único |
| `src/Ludeca.Infrastructure/Data/SqliteFoundingVerdictRepository.cs` | Crear | `Infrastructure` | Implementación de repositorio SQLite |
| `src/Ludeca.Infrastructure/Services/DefaultCurrentUserService.cs` | Modificar | `Infrastructure` | Soporte de roles mutable en memoria con conmutación en caliente |
| `src/Ludeca.Infrastructure/Seeding/CatalogSeeder.cs` | Modificar | `Infrastructure` | Precarga de veredictos fundadores con fotos reales para *Ark Nova*, *Wingspan* y *Carcassonne* |
| `src/Ludeca.Web/Components/Shared/FoundingVerdictCard.razor` | Crear | `Web` | Tarjeta editorial con sello prominente, pestañas de análisis y galería fotográfica |
| `src/Ludeca.Web/Components/Shared/AiSummaryCard.razor` | Crear | `Web` | Tarjeta de síntesis de IA para títulos en Fase 1 |
| `src/Ludeca.Web/Components/Shared/FoundingVerdictModal.razor` | Crear | `Web` | Modal ergonómico para redactar y editar veredictos y fotos reales |
| `src/Ludeca.Web/Components/Pages/GameDetail.razor` | Modificar | `Web` | Inyección de servicio, botón editorial y sustitución visual de IA |
| `src/Ludeca.Web/Components/Layout/MainLayout.razor` | Modificar | `Web` | Conmutador de rol para pruebas y demostración |
| `src/Ludeca.Web/Program.cs` | Modificar | `Web` | Registro de `IFoundingVerdictRepository` y `IFoundingVerdictService` en DI |
| `tests/Ludeca.UnitTests/FoundingVerdictTests.cs` | Crear | `Tests` | Suite de tests unitarios de dominio, persistencia, servicio y roles |

---

## Modelo de Dominio Detallado

```csharp
namespace Ludeca.Core.Entities;

public class FoundingVerdict
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid GameId { get; private set; }
    public string AuthorUserId { get; private set; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;
    public FoundingRecommendation Recommendation { get; private set; }
    public string OverallVerdict { get; private set; } = string.Empty;
    public string TwoPlayerVerdict { get; private set; } = string.Empty;
    public string FamilyVerdict { get; private set; } = string.Empty;
    public List<FoundingPhoto> Photos { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    
    // Invariantes de dominio: máx. 3 fotos, textos obligatorios
}
```

---

## Estrategia de Pruebas

1. **Dominio:** Validar que `FoundingVerdict` rechace textos vacíos, que impida añadir más de 3 fotos y que asigne correctamente el sello de recomendación.
2. **Repositorio:** Validar inserción, lectura por `GameId`, actualización y serialización JSON de `Photos` en SQLite en memoria.
3. **Servicio y Permisos:**
   - Validar que un usuario sin rol `FoundingTeam` no pueda publicar un veredicto.
   - Validar que al guardar un veredicto se actualice el `LudistRating` del juego.
   - Validar que para un juego sin veredicto se devuelva `AiGameSummaryDto` completo.
4. **Verificación Visual:** Inspección en vivo del comportamiento en navegador de la ficha con y sin veredicto fundador y del conmutador de roles en la cabecera.
