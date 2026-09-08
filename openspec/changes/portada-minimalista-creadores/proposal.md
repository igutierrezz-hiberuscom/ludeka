# Propuesta: Portada minimalista y reorientación Autores → Creadores (`portada-minimalista-creadores`)

## Intención

Corregir de rumbo el producto sobre INC-19/INC-21: la portada es un dashboard editorial saturado y el directorio "Autores" está sesgado a diseñadores de juegos, cuando la visión real del producto es un directorio de **creadores de contenido** con redes sociales. Este incremento minimaliza el hero, corrige dos defectos de navegación (link de novedades y banner legacy) y termina la reorientación Creadores sin romper URLs.

## Problema

1. El hero anuncia "El Letterboxd…" (marca ajena) con badge decorativo, restando protagonismo al buscador y al acceso por píldoras.
2. BUG: "Ver todas las novedades" (`HomeDashboard.razor` L232) apunta a `/radar` (Sorteos) en vez de `/novedades` (News.razor).
3. El banner "¡Radar renovado!" (`Radar.razor` L14-33) sigue apareciendo porque el hero enlaza `/radar` (condición `IsLegacyRoute` L303).
4. El seed (`DirectorySeeder.SeedCreatorsAsync` L153-253) crea 6 diseñadores + 1 creador; `GameDetail.razor` L200-208 enlaza "Diseñado por" a `/creadores/{slug}` sin verificar existencia → enlaces rotos latentes y sistemáticos.

## Alcance

### Incluye
- **Hero minimalista** (`HomeDashboard.razor`): quitar badge (L14-16) y titular h1 (L18-20); conservar el párrafo actual (L22-24) y el buscador (L26-43). Remedio a11y WCAG 2.2 AA: **h1 visualmente oculto** (`sr-only`) con texto neutro propio ("Ludeka — Juegos de mesa en español"; texto final lo fija sdd-spec).
- **Píldoras finales (D4)**: Catálogo Completo → `/catalogo` · Sorteos (re-etiquetada desde "Radar & Sorteos", destino `/sorteos`) · Novedades → `/novedades` · Eventos → `/eventos`. Quedan fuera: Editoriales y Autores.
- **FIX bug novedades**: L232 `href="/radar"` → `href="/novedades"`.
- **Quitar banner "¡Radar renovado!"** (`Radar.razor` L14-33 y `IsLegacyRoute` L303). `/radar` **se mantiene como alias silencioso** de `/sorteos` (cero coste; `transparency-manifesto` enlaza `/radar`).
- **Purga y re-siembra (D1)**: borrar los 6 diseñadores del seed y sembrar solo creadores de contenido (los del padrón estático de `ChannelFocusProvider` L40-52, incluido Análisis Parálisis con sus redes YouTube).
- **Consecuencia D1 obligatoria**: en `GameDetail.razor` L200-208 el diseñador pasa a **texto plano sin link** al directorio; el cruce juego↔creador queda solo para creadores de contenido reales. Se retira la sección "Obras de X" de `CreatorDetail.razor` (L127-156) y el cálculo `GamesCount` por coincidencia con `Game.Designer` en `CreatorService.cs` (L52-54, L68): ese matching solo tenía sentido en un directorio de diseñadores.
- **Reetiquetado transversal** Autores → Creadores: `MainLayout.razor` (L33 header, L210 footer), títulos/PageTitle/h1 de `CreatorsDirectory.razor` y `CreatorDetail.razor`, badge "Autor / Diseñador" → "Creador de contenido", textos de `CreatorEditModal.razor`, `AuditService.cs` L122 ("Autor/Creador").
- **Alias `/autores` mantenido (D2)**: cero URLs rotas.
- **Roadmap**: el cambio se declara como **INC-31** en `docs/increments/ROADMAP.md` y `docs/specs/ROADMAP_MVP_SLICES.md` (la edición de archivos ocurre en `apply`; la propuesta solo lo registra en alcance).
- Tests: ajustar `DirectoryServicesTests.cs` (L259 `CreatorService_CRUD_And_CatalogMatching_Works`, L352-376 canal "Sergio AP"), `DirectoryDomainTests.cs` (L73, L142) y nuevos tests del seed/hero bajo strict TDD.

### Contra (fuera de alcance)
- **D5 — Scrapeo social nocturno de Instagram/TikTok**: queda anotado como trabajo futuro (candidato INC-32). Este incremento entrega modelo de redes + ficha + reetiquetado; el pipeline YouTube ya consume las redes vía `ChannelDirectoryProvider`.
- `<PageTitle>` del navegador (L8) no cambia (sin decisión de producto).
- Las features `/editoriales` y `/creadores` siguen existiendo; solo desaparecen del hero.
- Sin migraciones EF ni job/hosted service nuevo.

## Capacidades (contrato con sdd-spec)

### Nuevas
- `home-landing-hero`: hero minimalista de la portada (buscador, párrafo, h1 oculto, píldoras D4, link de novedades correcto).
- `creators-directory`: directorio de creadores de contenido (listado, ficha con `SocialLinks`, alta/edición, sembrado, alias `/autores`, diseñador de juego como texto plano en ficha de juego).

### Modificadas
- `giveaway-radar`: la página `/sorteos` no renderiza banner legacy; `/radar` queda como alias silencioso.

## Enfoque

Enfoque quirúrgico (exploración, Opción 2): cambios de markup en `HomeDashboard.razor` y `Radar.razor` + reetiquetado de texto en ~10 archivos + purga/re-siembra del seed. Se reutiliza ~80 % del código existente (entidad `Creator` + `SocialNetworkLink` + `SocialPlatform` ya modelados, modal con editor de redes, `SocialLinksList`, permiso `CanManageCreators`, `ChannelDirectoryProvider`). Sin migraciones: el DDL idempotente de `SqliteSchemaMigrator` ya soporta `SocialLinks` JSON.

## Áreas afectadas

| Área | Impacto | Descripción |
|------|---------|-------------|
| `src/Ludeka.Web/Components/Pages/HomeDashboard.razor` | Modificado | Hero minimalista, píldoras D4, fix link L232 |
| `src/Ludeka.Web/Components/Pages/Radar.razor` | Modificado | Banner L14-33 y `IsLegacyRoute` L303 eliminados |
| `src/Ludeka.Web/Components/Pages/GameDetail.razor` | Modificado | Diseñador como texto plano (L200-208) |
| `src/Ludeka.Web/Components/Pages/CreatorsDirectory.razor` | Modificado | Reetiquetado títulos/h1 |
| `src/Ludeka.Web/Components/Pages/CreatorDetail.razor` | Modificado | Reetiquetado + retirar sección "Obras" |
| `src/Ludeka.Web/Components/Shared/CreatorEditModal.razor` | Modificado | Textos del formulario |
| `src/Ludeka.Web/Components/Layout/MainLayout.razor` | Modificado | Etiquetas L33 y L210 |
| `src/Ludeka.Application/Features/Directory/CreatorService.cs` | Modificado | Retirar matching `Game.Designer` |
| `src/Ludeka.Application/Features/Admin/AuditService.cs` | Modificado | Etiqueta L122 |
| `src/Ludeka.Infrastructure/Seeding/DirectorySeeder.cs` | Modificado | Purga + re-siembra de creadores |
| `tests/Ludeka.UnitTests/**` | Modificado | `DirectoryServicesTests`, `DirectoryDomainTests`, tests nuevos |

## Riesgos

| Riesgo | Prob. | Mitigación |
|--------|-------|------------|
| Enlaces "Diseñado por" rotos tras purga | Alta | D1: diseñador como texto plano, sin link |
| Purga de diseñadores irreversible en DB local | Media | Seed idempotente; diseño re-ejecutable; datos solo de sembrado |
| Tests acoplados a semántica de diseñadores | Media | Listados en alcance; ajuste con strict TDD |
| Pérdida de h1 al minimalizar hero | Alta | H1 `sr-only` decidido en D3 |
| Bookmarks antiguos a `/autores` o `/radar` | Baja | Ambos alias se mantienen (D2 + alias silencioso) |

## Plan de rollback

`git revert` del PR único restaura markup, textos y seed; el arranque re-siembra los diseñadores al restaurar `DirectorySeeder`. Los alias `/autores` y `/radar` no se tocan, por lo que ningún enlace externo se rompe durante ni después del rollback.

## Dependencias

- Ninguna externa. Para ejecutar la suite: detener el proceso dev `Ludeka.Web` (el build falla con el proceso corriendo); alternativamente `dotnet test Ludeka.sln --no-build` tras un build previo.

## Estrategia de pruebas

- **strict_tdd: true** (config.yaml). Rojo → verde → refactor por cada tarea.
- Comando: `dotnet test Ludeka.sln` (nota: usar `--no-build` si el proceso `Ludeka.Web` dev corre).
- Base existente: `DirectoryServicesTests` (L259, L352-376), `DirectoryDomainTests`, `SqliteDirectoryRepositoriesTests`, `ChannelFocusProviderTests`, `GranularPermissionsTests`.

## Entrega y presupuesto

- `delivery_strategy`: **single-pr**. Estimación: **~300-380 líneas** (additions+deletions) → dentro del presupuesto de revisión de 400 líneas, riesgo Low.

## Criterios de éxito

- [ ] La portada muestra solo párrafo + buscador + 4 píldoras (D4); sin badge ni titular; el documento tiene h1 accesible.
- [ ] "Ver todas las novedades" lleva a `/novedades`; entrar a Sorteos no muestra el banner.
- [ ] `/creadores` (y `/autores`) lista solo creadores de contenido con ficha y redes; ningún link roto a "Creador no encontrado" desde fichas de juego.
- [ ] Suite `dotnet test Ludeka.sln` en verde; INC-31 registrado en el roadmap en `apply`.
