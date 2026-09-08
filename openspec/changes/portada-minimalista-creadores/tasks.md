# Tasks: Portada minimalista y reorientaciÃ³n Autores â†’ Creadores (`portada-minimalista-creadores`)

> Idioma: espaÃ±ol castellano (regla suprema del repo). Fase: `sdd-tasks`. Modo: hybrid. Incremento: **INC-31**.
> EjecuciÃ³n **strict TDD**: cada tarea GREEN va precedida de su test RED (se escribe el test, se comprueba fallando con su filtro, y solo entonces se implementa; el check de la tarea implica ambos pasos). Cada WU = 1 commit convencional compilable con suite verde.
> Comando base: `dotnet test Ludeka.sln` â€” con el proceso dev `Ludeka.Web` corriendo el build falla: build previo y `--no-build`.
> Leyenda de specs: CD = `creators-directory` Â· HLH = `home-landing-hero` Â· GR = `giveaway-radar`.

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | **~520** (rango 480-540, adiciones+borraciones) |
| 400-line budget risk | High |
| Chained PRs recommended | Yes |
| Suggested split | PR 1: WU1 (~225) â†’ PR 2: WU2 (~85) â†’ PR 3: WU3 (~115) â†’ PR 4: WU4+WU5 (~95), apilados a main |
| Delivery strategy | single-pr (cacheado en preflight) |
| Chain strategy | pending |

Decision needed before apply: Yes
Chained PRs recommended: Yes
Chain strategy: pending
400-line budget risk: High

**DecisiÃ³n documentada (correcciÃ³n de la estimaciÃ³n del diseÃ±o).** El diseÃ±o declaraba ~415 lÃ­neas y riesgo Low-Medium, pero su WU1 (~120) es aritmÃ©ticamente inviable verificado contra el cÃ³digo real: el diff del seeder por sÃ­ solo es ~120 (âˆ’80 lÃ­neas de literales de diseÃ±adores en `SeedCreatorsAsync` L158-237, +40 de purga/siembra) mÃ¡s el accesor del padrÃ³n (+10) mÃ¡s los tests nuevos T1-T4 (~95: scaffold SQLite ~33 + T1-T3 ~60 + T4 ~12). Recalibrado: **~520**. Recorte honesto mÃ¡ximo sin perder cobertura de specs: consolidar T6 en una sola teorÃ­a (~70â†’~45) y compartir helper de siembra en T1-T4 (~95â†’~80) â‡’ ~475, aÃºn >400; recortar mÃ¡s exige borrar tests (code-golf prohibido por `work-unit-commits`). Tras un Ãºnico pase de slicing honesto no existe divisiÃ³n por unidad de trabajo que entre en 400 en PR Ãºnico â‡’ **antes de `sdd-apply` el mantenedor elige**: (a) **`size:exception`** â€” PR Ãºnico ~520 con aprobaciÃ³n explÃ­cita, o (b) **cadena de 4 PRs apilados a main** â€” el split de arriba, donde cada WU es revertible e independiente segÃºn la tabla de rollback del diseÃ±o. El desglose ordena los 5 WUs como commits apilados, vÃ¡lido para ambas vÃ­as sin re-trabajo.

### Suggested Work Units

Filtro compacto: `dotnet test Ludeka.sln --filter "FullyQualifiedName~X"`.

| Unit | Goal | Likely PR | Focused test command | Runtime harness | Rollback boundary |
|------|------|-----------|----------------------|-----------------|-------------------|
| WU1 | Purga + re-siembra desde el padrÃ³n (AD-1+AD-2), T1-T4 verdes | PR 1 | `~DirectorySeederTests`, `~ChannelFocusProviderTests` | N/A en apply â€” T1-T3 ya ejercitan el seeder real con SQLite; el arranque real se verifica en sdd-verify | Revert del commit; DB queda con creators vÃ¡lidos (seed aditivo, re-ejecutable) |
| WU2 | CreatorService sin cruce por `Game.Designer` (AD-6), T5 verde | PR 2 | `~DirectoryServicesTests`, `~GranularPermissionsTests` | N/A â€” capa Application, sin superficie de render | Revert; el matching por Designer vuelve sin tocar markup |
| WU3 | DiseÃ±ador texto plano + reetiquetado de fichas (AD-5+AD-7), T6a verde | PR 3 | `~WebMarkupContractTests` | N/A en apply â€” render real se verifica en sdd-verify (`dotnet run`) | Revert |
| WU4 | Hero minimalista + radar sin legacy (AD-3+AD-4), T6b verde | PR 4 | `~WebMarkupContractTests` | N/A en apply â€” render real en sdd-verify (portada y `/sorteos`) | Revert; alias `/sorteos` y `/radar` intactos |
| WU5 | INC-31 en roadmaps | PR 4 | N/A â€” docs | N/A â€” revisiÃ³n manual | Revert |

## Fase 1 Â· WU1 â€” Datos: padrÃ³n, purga y re-siembra (T1-T4) [~225 lÃ­neas]

- [x] 1.1 **RED T1-T3** â€” crear `tests/Ludeka.UnitTests/Infrastructure/DirectorySeederTests.cs` con SQLite en memoria (patrÃ³n de `tests/Ludeka.UnitTests/Infrastructure/CatalogSeederFullDatasetTests.cs` (read-only)): T1 `SeedCreatorsAsync_OnLegacyDbWithDesigners_PurgesAndSeedsOnlyContentCreators` (inserta a mano los 6 diseÃ±adores + Sergio, llama a `SeedDirectoryAsync`, afirma 0 slugs retirados / 12 creators / link YouTube en AnÃ¡lisis ParÃ¡lisis), T2 `..._IsIdempotent_NoDuplicatesOnSecondRun`, T3 `..._PreservesCreatorsOutsidePadron` (creator manual con slug ajeno sobrevive). Requisito: CDÂ·Sembrado D1 (esc. 1-2) + purga quirÃºrgica. Done: filtro `~DirectorySeederTests` en rojo.
- [x] 1.2 **RED T4** â€” aÃ±adir `GetStaticCreators_ReturnsTwelveContentCreators` a `tests/Ludeka.UnitTests/Infrastructure/ChannelFocusProviderTests.cs`: 12 entradas, todas `Category.Creator`, incluye "AnÃ¡lisis ParÃ¡lisis". Requisito: CDÂ·Sembrado D1 (padrÃ³n). Done: rojo (el accesor aÃºn no existe).
- [x] 1.3 **GREEN T4** â€” aÃ±adir `GetStaticCreators()` a `src/Ludeka.Infrastructure/YouTube/ChannelFocusProvider.cs` (AD-2: `Channels.Where(Category == Creator)`). Done: filtro `~ChannelFocusProviderTests` verde.
- [x] 1.4 **GREEN T1-T3** â€” reescribir `SeedCreatorsAsync` en `src/Ludeka.Infrastructure/Seeding/DirectorySeeder.cs` (AD-1): array privado `RetiredSeedCreatorSlugs` (los 6 slugs exactos), purga (SELECT por slug âˆˆ lista â†’ `RemoveRange` + `SaveChanges`) y re-siembra aditiva desde `GetStaticCreators()` (`Name`, `Slug = Game.GenerateSlug`, `Bio`, `SocialLinks` de YouTube), sin updates sobre existentes. Requisito: CDÂ·Sembrado D1 + CDÂ·Listado sin diseÃ±adores. Done: T1-T3 verdes.
- [x] 1.5 **VerificaciÃ³n WU1** â€” suite completa verde (`dotnet test Ludeka.sln`); commit `feat: sembrar solo creadores de contenido con purga de diseÃ±adores retirados (INC-31)`.

## Fase 2 Â· WU2 â€” Desacope de CreatorService (T5) [~85 lÃ­neas]

- [x] 2.1 **RED T5** â€” reescribir `CreatorService_CRUD_And_CatalogMatching_Works` â†’ `CreatorService_CRUD_Works` en `tests/Ludeka.UnitTests/Application/DirectoryServicesTests.cs` (L259): sin `FakeGameRepository` ni asserts de `GamesCount`/`Games`; CRUD Ã­ntegro y detalle que expone `SocialLinks`. Rojo por compilaciÃ³n (los DTOs aÃºn tienen los campos). Requisito: CDÂ·Sin Obras ni cruce (esc. Servicio sin matching). Done: rojo confirmado.
- [x] 2.2 **GREEN T5** â€” en `src/Ludeka.Application/Features/Directory/CreatorService.cs` (AD-6): constructor sin `IGameRepository`, `GetAllAsync` sin conteos, `GetBySlugAsync`/`GetByIdAsync` sin `gameSummaries`, `UpdateAsync` sin re-cuento, `MapToDto`/`MapToDetailDto` sin juegos; y en `src/Ludeka.Application/DTOs/DirectoryDtos.cs`: `CreatorDto` sin `GamesCount`, `CreatorDetailDto` sin `Games`. Nota: DI sin cambios (Program.cs L183 resuelve el ctor Ãºnico). Requisito: CDÂ·Ficha con redes + CDÂ·Sin cruce. Done: filtro `~DirectoryServicesTests` verde. **Nota apply:** las remociones estructurales del ripple AD-6 (badge `GamesCount` en CreatorsDirectory y sección `Obras` en CreatorDetail) se adelantaron a WU2 para mantener la solución compilable; los textos quedan en WU3.
- [x] 2.3 **GREEN (accesorio T5)** â€” textos de auditorÃ­a en `src/Ludeka.Application/Features/Directory/CreatorService.cs` (L127/L140/L175/L202/L214 â†’ "creador de contenido") y ajuste mecÃ¡nico en `tests/Ludeka.UnitTests/Application/GranularPermissionsTests.cs` (L164/L177: `new CreatorService(creatorRepo, currentUser)`). Requisito: CDÂ·Reetiquetado (auditorÃ­a). Done: filtro `~GranularPermissionsTests` verde.
- [x] 2.4 **VerificaciÃ³n WU2** â€” suite verde; commit `refactor: desacoplar creatorservice del cruce por game.designer (INC-31)`.

## Fase 3 Â· WU3 â€” Fichas Web: diseÃ±ador plano y reetiquetado (T6a) [~115 lÃ­neas]

> Guard: `LibraryStatsDashboard.razor` **no se toca** (decisiÃ³n del diseÃ±o AD-7: es estadÃ­stica de catÃ¡logo derivada de `Game.Designer`).

- [x] 3.1 **RED T6a** â€” crear `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs`: teorÃ­as `(archivo, debeContener[], noDebeContener[])` que leen los fuentes resolviendo la raÃ­z del repo desde `AppContext.BaseDirectory`: GameDetail (sin `<a>` al directorio por designer; con "DiseÃ±ado por"), CreatorDetail (sin "Obras de"; con "Creador de contenido"; conserva `@page` del alias `/autores/{slug}`), CreatorsDirectory (PageTitle nuevo; sin badge de obras; conserva `@page` del alias `/autores`), MainLayout (sin "Autores", con "Creadores"), AuditLogViewer y UserPermissionsModal (sin "Autor / DiseÃ±ador"). Requisito: CDÂ·DiseÃ±ador texto plano + CDÂ·Reetiquetado + CDÂ·Alias D2. Done: rojo.
- [x] 3.2 **GREEN T6a (GameDetail)** â€” `src/Ludeka.Web/Components/Pages/GameDetail.razor` L200-208 (AD-5): "DiseÃ±ado por" con designer como `<span>` plano (fallback "Desconocido"), sin `<a>` al directorio; bloque editoriales intacto. Requisito: CDÂ·DiseÃ±ador texto plano. Done: subconjunto GameDetail del contrato verde.
- [x] 3.3 **GREEN T6a (fichas)** â€” en `src/Ludeka.Web/Components/Pages/CreatorDetail.razor` (AD-7): eliminar secciÃ³n "Obras" (L127-156), badge L72 â†’ "ðŸŽ¬ Creador de contenido", PageTitle L11 y textos L17/L26; en `src/Ludeka.Web/Components/Pages/CreatorsDirectory.razor`: PageTitle L10, badge L17, h1 L19-21, pÃ¡rrafo L23 y eliminar badge "@creator.GamesCount obras" (L107-109). Requisito: CDÂ·Sin Obras + CDÂ·Reetiquetado + CDÂ·Ficha con redes (componente de redes existente, sin tocar). Done: verde.
- [x] 3.4 **GREEN T6a (transversal)** â€” textos exactos de AD-7 en `src/Ludeka.Web/Components/Shared/CreatorEditModal.razor` (L22/L43/L46/L86), `src/Ludeka.Web/Components/Layout/MainLayout.razor` (L33/L210), `src/Ludeka.Application/Features/Admin/AuditService.cs` (L122), `src/Ludeka.Web/Components/Pages/AuditLogViewer.razor` (L95) y `src/Ludeka.Web/Components/Shared/UserPermissionsModal.razor` (L143). Requisito: CDÂ·Reetiquetado (no queda "Autores"/"Autor / DiseÃ±ador" visible). Done: T6a verde completo.
- [x] 3.5 **VerificaciÃ³n WU3** â€” suite verde; commit `feat: fichas con diseÃ±ador en texto plano y reetiquetado a creadores (INC-31)`.

## Fase 4 Â· WU4 â€” Hero minimalista + radar sin legacy (T6b) [~80 lÃ­neas]

- [ ] 4.1 **RED T6b** â€” extender `tests/Ludeka.UnitTests/Infrastructure/WebMarkupContractTests.cs`: HomeDashboard (contiene `sr-only` y el h1 exacto `Ludeka â€” Juegos de mesa en espaÃ±ol`; no contiene "PORTADA EDITORIAL"; hrefs de pÃ­ldoras exactos `href="/catalogo"`, `href="/sorteos"`, `href="/novedades"`, `href="/eventos"` en orden; no contiene `href="/radar"`; "Ver todas las novedades" con `href="/novedades"`), Radar (no "Radar renovado" ni `IsLegacyRoute`; conserva ambas directivas `@page`). Requisito: HLH completo + GR completo + alias D2. Done: rojo.
- [ ] 4.2 **GREEN T6b (hero)** â€” `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` (AD-3): eliminar badge L14-16 y h1 visible L18-20; aÃ±adir `<h1 class="sr-only">Ludeka â€” Juegos de mesa en espaÃ±ol</h1>` (Ãºnico h1 del documento); pÃ­ldoras D4 exactas (CatÃ¡logo Completo, Sorteos, Novedades, Eventos con sus destinos; fuera la pÃ­ldora de Autores y la de `/editoriales`); fix L232: "Ver todas las novedades" de `href="/radar"` â†’ `href="/novedades"`. Buscador (L26-43) y pÃ¡rrafo (L22-24) intactos. Requisito: HLHÂ·Sin badge ni titular + HLHÂ·Buscador conservado + HLHÂ·PÃ­ldoras D4 + HLHÂ·Link novedades + HLHÂ·h1 sr-only. Done: verde.
- [ ] 4.3 **GREEN T6b (radar)** â€” `src/Ludeka.Web/Components/Pages/Radar.razor` (AD-4): eliminar el bloque del banner (L15-33), la propiedad `IsLegacyRoute` (L303) y el inject de `NavigationManager` (L9); conservar `@page "/sorteos"` y `@page "/radar"` (alias silencioso). Requisito: GRÂ·Sin banner legacy + GRÂ·Alias silencioso. Done: T6b verde completo.
- [ ] 4.4 **VerificaciÃ³n WU4** â€” suite verde; commit `feat: hero minimalista en portada y radar sin banner legacy (INC-31)`.

## Fase 5 Â· WU5 â€” Docs y verificaciÃ³n final [~15 lÃ­neas]

- [ ] 5.1 **INC-31 en roadmaps** â€” aÃ±adir fila INC-31 "â³ En progreso" en `docs/increments/ROADMAP.md` (tras INC-30) y la entrada/slice INC-31 en `docs/specs/ROADMAP_MVP_SLICES.md` (mismo formato que el slice de INC-30). Requisito: regla 5 de `AGENTS.md` (sincronizaciÃ³n de roadmap). Done: ambos archivos referencian INC-31; commit `docs: registrar inc-31 en roadmaps`.
- [ ] 5.2 **VerificaciÃ³n final** â€” `dotnet build Ludeka.sln` limpio + `dotnet test Ludeka.sln` completo en verde (proceso dev detenido, o `--no-build` tras build previo); criterios de Ã©xito del proposal (`openspec/changes/portada-minimalista-creadores/proposal.md` (read-only)) demostrados: hero â†’ T6b; link de novedades + sin banner â†’ T6b; `/creadores` y `/autores` solo creadores con ficha y redes, sin links rotos desde fichas de juego â†’ T1-T4 + T6a; suite verde â†’ este paso. Done: checklist del proposal listo para `sdd-verify`.
