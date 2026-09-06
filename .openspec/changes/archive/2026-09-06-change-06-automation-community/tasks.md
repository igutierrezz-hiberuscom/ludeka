# Tareas de Implementación: change-06-automation-community (Incremento 6: Automatización Omnicanal, Radar de Sorteos y Comunidad)

## Fase 1: Dominio y Entidades Puras (`src/Ludeka.Core`)
- [x] 1.1 Crear enum `GiveawayPlatform` en `Enums/GiveawayPlatform.cs` (`Instagram`, `TwitterX`, `YouTube`, `Community`, `Other`).
- [x] 1.2 Crear entidad `Giveaway` en `Entities/Giveaway.cs` con propiedades, invariantes, método `MergeCollaborator` y propiedad `IsExpired`.
- [x] 1.3 Crear entidad `WeeklyRelease` en `Entities/WeeklyRelease.cs` con propiedades y distinción de reimpresión vs novedad.
- [x] 1.4 Crear entidades `RuleQuestion`, `RuleAnswer` y `RuleVote` en `Entities/` con métodos de voto (+1 / desvoto) y marcado de `IsAccepted` autorizado.
- [x] 1.5 Crear pruebas unitarias de dominio en `tests/Ludeka.UnitTests/Domain/`:
  - `GiveawayTests.cs` (fusión de colaboradores, expiración y validación de fechas).
  - `RuleQATests.cs` (votos, alternancia de votos y autorización de respuesta aceptada).

## Fase 2: Contratos y Lógica de Aplicación (`src/Ludeka.Application`)
- [x] 2.1 Crear DTOs en `DTOs/CommunityDtos.cs`:
  - `GiveawayDto`, `CreateGiveawayRequest`, `WeeklyReleaseDto`, `CreateWeeklyReleaseRequest`.
  - `RuleQuestionDto`, `RuleAnswerDto`, `CreateRuleQuestionRequest`, `CreateRuleAnswerRequest`.
  - `SocialCardDataDto`, `GeneratedSocialCardDto`.
- [x] 2.2 Crear interfaces de repositorio en `Contracts/`:
  - `IGiveawayRepository.cs`
  - `IWeeklyReleaseRepository.cs`
  - `IRuleQARepository.cs`
- [x] 2.3 Crear interfaces de servicio en `Contracts/`:
  - `IGiveawayService.cs`
  - `IWeeklyReleaseService.cs`
  - `IRuleQAService.cs`
  - `ISocialCardService.cs`
- [x] 2.4 Implementar `GiveawayService` en `Features/Community/GiveawayService.cs` con detección y fusión automática de colaboraciones.
- [x] 2.5 Implementar `WeeklyReleaseService` en `Features/Community/WeeklyReleaseService.cs`.
- [x] 2.6 Implementar `RuleQAService` en `Features/Community/RuleQAService.cs` (gestión de dudas, respuestas, control de votos y solución aceptada).
- [x] 2.7 Implementar `SocialCardService` en `Features/Community/SocialCardService.cs` (composición SVG 1:1 1080x1080 y generador de copy para Instagram).
- [x] 2.8 Crear pruebas unitarias de aplicación en `tests/Ludeka.UnitTests/Application/`:
  - `GiveawayServiceTests.cs`
  - `RuleQAServiceTests.cs`
  - `SocialCardServiceTests.cs`

## Fase 3: Persistencia y Sembrado EF Core 10 (`src/Ludeka.Infrastructure`)
- [x] 3.1 Actualizar `LudekaDbContext.cs` con `DbSet` e índices para `Giveaway`, `WeeklyRelease`, `RuleQuestion`, `RuleAnswer` y `RuleVote`.
- [x] 3.2 Implementar `SqliteGiveawayRepository` en `Data/SqliteGiveawayRepository.cs`.
- [x] 3.3 Implementar `SqliteWeeklyReleaseRepository` en `Data/SqliteWeeklyReleaseRepository.cs`.
- [x] 3.4 Implementar `SqliteRuleQARepository` en `Data/SqliteRuleQARepository.cs`.
- [x] 3.5 Actualizar `CatalogSeeder.cs` para sembrar sorteos activos, lanzamientos del viernes y preguntas de reglas resueltas.
- [x] 3.6 Crear pruebas de integración en `tests/Ludeka.UnitTests/Infrastructure/`:
  - `SqliteCommunityRepositoriesTests.cs` (persistencia, transacciones y consultas de sorteos y Q&A).

## Fase 4: Interfaz de Usuario Blazor (`src/Ludeka.Web`)
- [x] 4.1 Crear componente `GiveawayCard.razor` en `Components/Shared/` con visualización de countdown, badges y colaboradores fusionados.
- [x] 4.2 Crear página `Radar.razor` en `Components/Pages/` (`/radar` y `/sorteos`) con selector de pestañas (Sorteos / Novedades del Viernes) y modal de registro rápido.
- [x] 4.3 Crear página `Transparency.razor` en `Components/Pages/` (`/transparencia`) con diseño editorial para el Manifiesto de Transparencia de Fondos, enlaces de mecenazgo y Discord.
- [x] 4.4 Crear componente `SocialCardModal.razor` en `Components/Shared/` con previsualización del SVG 1:1, selector de opciones, copia de texto y descarga.
- [x] 4.5 Crear componente `RuleQuestionsSection.razor` en `Components/Shared/` con listado de dudas de reglamento, votos, respuestas y marcado de solución aceptada.
- [x] 4.6 Integrar `SocialCardModal` y `RuleQuestionsSection` en `Components/Pages/GameDetail.razor`.
- [x] 4.7 Actualizar `Components/Layout/MainLayout.razor` con navegación hacia el Radar, Transparencia, Discord y footer actualizado del MVP completado.
- [x] 4.8 Registrar los nuevos servicios y repositorios en `Program.cs`.

## Fase 5: Verificación Integral, Cierre y Archivo
- [x] 5.1 Ejecutar suite completa de pruebas unitarias e integración en verde (`dotnet test src/Ludeka.slnx`).
- [x] 5.2 Verificar compilación y arranque de servidor sin advertencias ni errores.
- [x] 5.3 Generar informe de verificación `verify-report.md`.
- [x] 5.4 Promover especificaciones a `specs/` y archivar el cambio bajo `openspec/changes/archive/`.
