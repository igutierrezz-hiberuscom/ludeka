# Propuesta: Incremento 3 — Panel y Veredicto de la Mesa Fundadora

## Intención
Implementar el ciclo de vida editorial del veredicto lúdico (resumen inicial de IA y sustitución prioritaria por el veredicto oficial de la mesa fundadora con análisis en pareja a 2J, familias/niños y fotos reales de partida), junto con el control de acceso por roles (`FoundingTeam`, `Moderator`) y el panel/modal de gestión editorial `[ 🛡️ Gestionar Veredicto Fundador ]`.

## Alcance

### Dentro del Alcance
- **Ciclo de vida del veredicto en 3 fases (Punto 4.1 de la Spec):**
  - *Fase 1 (Arranque con IA):* Bloque identificado como `🤖 Resumen generado por IA` con síntesis de escalabilidad, edad real y huella en mesa para evitar fichas vacías.
  - *Fase 2 (Fase Editorial Fundadora):* Sustitución prioritaria por el bloque `👤 Veredicto de la Mesa Fundadora` con Sello de Recomendación oficial (*"Imprescindible de la Mesa"*, *"Recomendado con adaptaciones"*, *"Prescindible"*), análisis en pareja a 2J y en familia/niños.
  - *Fase 3 (Fase Comunitaria y Ponderación):* Fijación prioritaria en la parte superior de la ficha y mayor peso ponderado en el algoritmo del ranking local (`LudistRating`).
- **Galería fotográfica de mesa real:**
  - Subida y renderizado de 1 a 3 fotografías reales tomadas en mesa propia con pie de foto descriptivo y visualizador modal/ampliación.
- **Módulo y Panel de Gestión Editorial (Punto 4.2 de la Spec):**
  - Botón de acceso contextual en la ficha de juego: `[ 🛡️ Gestionar Veredicto Fundador ]`.
  - Modal/formulario ergonómico `FoundingVerdictModal.razor` para redactar y editar veredictos, asignar sellos y gestionar fotos reales de partida.
- **Autenticación y Roles de Usuario:**
  - Ampliación de `ICurrentUserService` para soportar roles (`FoundingTeam`, `Moderator`, `User`).
  - Conmutador interactivo de roles en la barra superior (`MainLayout.razor`) para alternar fácilmente entre la perspectiva de la mesa fundadora y la de un usuario común sin fricción.
- **Persistencia SQLite EF Core 10:**
  - Nueva entidad `FoundingVerdict` con índice único por `GameId` y almacenamiento JSON embebido para las fotos de mesa.
  - Datos semillados enriquecidos en `CatalogSeeder` con veredictos fundadores y fotos para juegos emblemáticos, manteniendo otros títulos con resumen de IA para verificar la sustitución.

### Fuera del Alcance
- Hub multimedia segregado de YouTube e Instagram (Incremento 4).
- Importador masivo de colecciones BGG XMLAPI2 (Incremento 5).
- Consultorio de reglas Q&A y motor omnicanal de sorteos (Incremento 6).

## Capacidades

### Nuevas Capacidades
- `founding-verdict-core`: Entidad de dominio `FoundingVerdict`, Value Object `FoundingPhoto`, enum `FoundingRecommendation` (Imprescindible, Recomendado con adaptaciones, Prescindible), análisis estructurado (general, 2 jugadores / pareja, familias / niños) y galería de fotos de mesa real.
- `verdict-lifecycle-ai-fallback`: Algoritmo de ciclo de vida del veredicto: renderizado del bloque `🤖 Resumen generado por IA` en Fase 1 y sustitución prioritaria por la tarjeta editorial `👤 Veredicto de la Mesa Fundadora` en Fase 2. Ponderación reforzada en el cálculo de `LudistRating`.
- `founding-editorial-panel`: Botón `[ 🛡️ Gestionar Veredicto Fundador ]` y modal reactivo para redactar, editar y publicar veredictos de la casa en tiempo real.
- `editorial-role-management`: Soporte de roles de usuario (`FoundingTeam`, `Moderator`, `User`) en la capa de aplicación con conmutador rápido para pruebas y auditoría visual.

### Capacidades Modificadas
- `core-catalog`: Ficha de juego `GameDetail.razor` enriquecida con la tarjeta del veredicto fundador / resumen de IA, botón contextual de gestión y actualización de la nota media `LudistRating`.

## Enfoque Arquitectónico
- **Dominio (`Ludeca.Core`)**: Entidad `FoundingVerdict`, Value Object inmutable `FoundingPhoto`, enum `FoundingRecommendation` con invariantes estrictas (máximo 3 fotos, textos descriptivos obligatorios).
- **Aplicación (`Ludeca.Application`)**: Repositorio `IFoundingVerdictRepository`, servicio `IFoundingVerdictService`, ampliación de `ICurrentUserService` y DTOs específicos (`FoundingVerdictDto`, `AiGameSummaryDto`, `SaveFoundingVerdictRequest`).
- **Infraestructura (`Ludeca.Infrastructure`)**: Mapeo relacional en `LudecaDbContext` con `ToJson()` para fotos, implementación de repositorio `SqliteFoundingVerdictRepository`, soporte de roles en `DefaultCurrentUserService` y semillas enriquecidas en `CatalogSeeder`.
- **UI Blazor (`Ludeca.Web`)**: Componentes editoriales con Tailwind CSS `FoundingVerdictCard.razor`, `AiSummaryCard.razor`, `FoundingVerdictModal.razor` y selector de rol de demostración en `MainLayout.razor`.

## Áreas Afectadas
| Área | Impacto | Descripción |
|---|---|---|
| `Ludeca.Core` | Nuevo | Entidad `FoundingVerdict`, VO `FoundingPhoto`, enum `FoundingRecommendation` |
| `Ludeca.Application` | Nuevo / Modificado | Contratos `IFoundingVerdictRepository`, `IFoundingVerdictService`, extensión de `ICurrentUserService` y DTOs |
| `Ludeca.Infrastructure` | Modificado | Tabla `FoundingVerdicts` en `LudecaDbContext`, repositorio SQLite, roles y datos de semillas |
| `Ludeca.Web` | Nuevo / Modificado | Componentes de veredicto, modal editorial, resumen de IA y botón de gestión en `GameDetail.razor` |
| `tests/Ludeca.UnitTests` | Nuevo | Tests de invariantes de dominio, persistencia, casos de uso de veredictos y visualización condicional |

## Riesgos y Mitigaciones
| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Carga de imágenes en local sin CDN propia | Media | Soporte de URLs seguras externas y almacenamiento estático ligero en `wwwroot/uploads` con fallback |
| Inconsistencia de datos en la sustitución de IA | Baja | Algoritmo determinista: la presencia de un `FoundingVerdict` en base de datos desactiva inmediatamente el bloque de IA para ese juego |
| Complejidad de prueba de roles sin autenticación OAuth | Baja | Conmutador interactivo de roles en la interfaz para alternar entre mesa fundadora y usuario público con 1 clic |

## Plan de Rollback
- Revertir los cambios del incremento 3 en git.
- Restaurar `LudecaDbContext` eliminando el DbSet `FoundingVerdicts` y regenerar la base de datos `ludeca.db`.

## Dependencias
- Catálogo base y ficha inteligente (`change-01-core-catalog`).
- Ludoteca personal, colecciones y valoraciones (`change-02-library-loans`).
