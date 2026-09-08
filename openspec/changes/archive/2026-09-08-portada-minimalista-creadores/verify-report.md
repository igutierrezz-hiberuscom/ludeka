```yaml
schema: gentle-ai.verify-result/v1
evidence_revision: sha256:cec88e20cfecc53b246427988347b82f3002d5709d9d715c1dca3ed4efda798d
verdict: pass
blockers: 0
critical_findings: 0
requirements: 15/15
scenarios: 22/22
test_command: dotnet test Ludeka.sln
test_exit_code: 0
test_output_hash: sha256:759001ce255dd28be5a09d7bf9ab170433c9ed6ff3f33d1c535092cc2e2fee15
build_command: dotnet build Ludeka.sln
build_exit_code: 0
build_output_hash: sha256:6e1680e689ebd37abb92c6f929358fc0ec8d6253747e8bee12a4ae0dd7efab7e
```

# Informe de Verificación — `portada-minimalista-creadores` (INC-31) — RE-EJECUCIÓN

**Cambio**: `portada-minimalista-creadores` · **Incremento**: INC-31 · **Modo**: Strict TDD · **Fase**: `sdd-verify` (re-ejecución)
**Artefactos verificados**: proposal.md · design.md · tasks.md (20/20) · specs (home-landing-hero, creators-directory, giveaway-radar) · apply-progress (Engram `sdd/portada-minimalista-creadores/apply-progress`)
**Commits de apply**: d9d3cfe · 0070ebd · 7cbd0e3 · 60f0b44 · b537bc0 · **Remediación**: f8ff077 (`test: cubrir busqueda rapida de portada y ficha de creador sin redes`)

> **Naturaleza de esta re-ejecución**: el verify anterior (`fail`, 20/22) falló EXCLUSIVAMENTE por 2 escenarios sin test automatizado (HLH·"Búsqueda rápida desde la portada" y CD·"Ficha sin redes"). El commit de remediación **f8ff077** agregó exactamente 2 tests (107 líneas, cero cambios en código de producción). Esta re-ejecución verifica el estado completo actual y eleva el veredicto a **PASS**: 22/22 escenarios COMPLIANT, 0 UNTESTED, 0 FAILING.

## Verificación Report

### Completitud
| Métrica | Valor |
|---|---|
| Tareas totales | 20 |
| Tareas completas | 20 |
| Tareas incompletas | 0 |

### Ejecución de comandos (Result Contract)
| Comando | Resultado |
|---|---|
| `dotnet test Ludeka.sln` (run 1, con build) | ✅ 723/723 superadas, 0 falladas, 0 omitidas, 11 s, exit 0 |
| `dotnet build Ludeka.sln` (sellado) | ✅ 0 Errores / 0 Advertencias, exit 0 → `build_output_hash` |
| `dotnet test Ludeka.sln --no-build` (sellado para hash) | ✅ 723/723 superadas, 12 s, exit 0 → `test_output_hash` |

`dotnet test Ludeka.sln`: ✅ 723/723 superadas (exit 0, 2 ejecuciones completas) · `dotnet build Ludeka.sln`: ✅ 0 Errores / 0 Advertencias (exit 0).
`evidence_revision` = sha256 de la concatenación de las salidas selladas de test y build (con delimitador `===BUILD===`).

**Nota sobre el flaky conocido** (`StoreStockServiceTests.GetStockAsync_WhenClientTimesOut`): NO apareció en ninguna de las 2 ejecuciones completas de esta re-ejecución — no fue necesario aislarlo ni re-ejecutar. El registro del informe anterior (WARNING-2, test de timing ajeno a INC-31) se mantiene como antecedente.

### Render de confirmación (acotado, puerto 5999)
Confirmación mínima posterior a f8ff077 (el código de producto NO cambió desde el verify anterior — HEAD=f8ff077 que solo agrega tests; el render completo de 7 URLs del informe anterior mantiene su validez). La app arrancó con `dotnet run --no-build`; PID guardado (24356), parentesco verificado con `Win32_Process.ParentProcessId` (hijos 16924 conhost, 20056 Ludeka.Web.exe), árbol matado únicamente de este arranque, puerto 5999 libre tras la limpieza.

| URL | HTTP | Verificación | Evidencia |
|---|---|---|---|
| `/` | 200 | ✅ VERIFICADO | "PORTADA EDITORIAL" AUSENTE; párrafo del hero presente; buscador presente (form + placeholder `Buscar por juego, editorial o dise…`); h1 sr-only ÚNICO del documento con texto exacto **verificado por code points** (`Ludeka — Juegos de mesa en español`: em dash U+2014 y ñ U+00F1 presentes); píldoras del hero EXACTAMENTE 4 en orden `href="/catalogo"`, `href="/sorteos"`, `href="/novedades"`, `href="/eventos"` (los `href="/editoriales"` y `href="/creadores"` detectados en el documento pertenecen al nav del MainLayout — legítimos y fuera del bloque hero); `<a href="/novedades"> Ver todas las novedades` PRESENTE; `href="/radar"` AUSENTE |
| `/sorteos` | 200 | ✅ VERIFICADO | "Radar renovado" AUSENTE; h1 "Sorteos de Juegos de Mesa" PRESENTE |
| `/creadores` | 200 | ✅ VERIFICADO | h1 "Creadores de Contenido" PRESENTE; PageTitle "Directorio de Creadores de Contenido" PRESENTE; **12 slugs únicos** = padrón completo (`analisis-paralisis`, `consola-y-tablero`, `el-agujero-de-hobbit`, `jugador-inicial`, `la-mazmorra-de-pacheco`, `la-taberna-de-dam`, `meepletopia`, `mesa-de-guerra`, `oceano-de-juegos`, `pareja-de-ases`, `rincon-de-jugetes`, `sentido-antihorario`); **0 diseñadores** (`uwe-rosenberg`, `elizabeth-hargrave`, `klaus-teuber`, `bruno-cathala`, `jacob-fryxelius`, `jamey-stegmaier` ausentes); sin etiqueta visible "Autores"; la única mención de "obras" es el CTA leftover "Ver obras diseñadas" (SUGGESTION-1, fuera de contrato) |
| `/juegos/brass-birmingham` | 500 | ⚠️ CONFIRMADO PRE-EXISTENTE | Bug de play-log de INC-30 (commit fc90de0), ajeno a INC-31 — ver WARNING-1. Sin cambios desde el verify anterior; NO cuenta como violación de INC-31 |

### TDD Compliance
| Check | Resultado | Detalles |
|---|---|---|
| Evidencia TDD reportada | ✅ | tasks.md documenta RED/GREEN/Done por tarea (T1-T6b) y apply-progress registra progresión de suites 705→709→709→717→721; remediación f8ff077 eleva a 723 |
| Todas las tareas tienen tests | ✅ | 5/5 WUs con tests + remediación f8ff077 con 2 tests |
| RED confirmado (tests existen) | ✅ | Tareas originales: 4 archivos verificados en codebase (verify anterior). Remediación: **RED probado experimentalmente** — 3 mutaciones transitorias revertidas (M1 rompió `/catalogo?q` → rojo en la aserción del spec; M2 rompió la rama de término vacío → rojo en el caso borde; M3 rompió `SocialLinks?` en `CreatorService.CreateAsync` L83 → ArgumentNullException detectada), restauración confirmada con git status |
| GREEN confirmado (pasan ahora) | ✅ | 723/723 en 2 ejecuciones completas (11 s y 12 s, exit 0) |
| Triangulación adecuada | ✅ | Quick search: escenario del spec (`azul` → `/catalogo?q=azul`) + caso borde (término vacío → `/catalogo`); ficha sin redes: alta + lectura del detalle (persistencia y consulta); sembrado: 3 casos + padrón; servicio: CRUD íntegro + 2 permisos; markup: 12 contratos |
| Safety net en archivos modificados | ✅ | La remediación ejecutó la suite completa tras cada ciclo (verificado: production code untouched); cada WU original ejecutó la suite completa (tasks.md 1.5/2.4/3.5/4.4/5.2) |

**TDD Compliance**: 6/6 checks sustantivos ✅

### Distribución de capas de prueba (incluye remediación)
| Capa | Tests | Archivos | Herramienta |
|---|---|---|---|
| Unit (Application) | DirectoryServicesTests (CRUD + permisos + ficha sin redes) + GranularPermissionsTests | `DirectoryServicesTests.cs`, `GranularPermissionsTests.cs` | xUnit 2.9.3 |
| Unit (Web, handler de componente sin bUnit) | HomeDashboardQuickSearchTests (1 fact: escenario del spec + caso borde) | `HomeDashboardQuickSearchTests.cs` | xUnit + `NavigationManager` fake (monitor de URI) |
| Integración de repositorio (SQLite en memoria) | T1-T3 | `DirectorySeederTests.cs` | xUnit + Microsoft.Data.Sqlite |
| Contrato de fuente | T4 + T6 (12 casos) | `ChannelFocusProviderTests.cs`, `WebMarkupContractTests.cs` | xUnit (lectura de fuentes) |
| E2E / DOM | 0 | — | no instalado (decisión de diseño documentada; render real confirmado en ambas ejecuciones de verify con `dotnet run`) |

## Matriz de cumplimiento de especificaciones

> Los 20 escenarios COMPLIANT del verify anterior mantienen su evidencia sin re-derivarla (mismos tests y renders ya ejecutados y documentados). Las 2 filas actualizadas por la remediación f8ff077 se marcan con **[REMEDIADO]**. Conteo autoritativo de specs: 15 requerimientos / 22 escenarios (HLH 5/6 · CD 8/13 · GR 2/3).

### home-landing-hero (5 requerimientos / 6 escenarios)
| Requerimiento | Escenario | Test / Evidencia | Resultado |
|---|---|---|---|
| Hero sin badge ni titular visible | Portada renderiza hero minimalista | `WebMarkupContractTests.Source_FulfillsMarkupContract` (HomeDashboard) + render real `/` (confirmación de esta re-ejecución) | ✅ COMPLIANT |
| Buscador rápido conservado | Búsqueda rápida desde la portada | **[REMEDIADO]** `HomeDashboardQuickSearchTests.QuickSearch_SubmitNavigatesToCatalogoWithQueryAndFallsBackWithoutTerm` (`tests/Ludeka.UnitTests/Web/HomeDashboardQuickSearchTests.cs`, nuevo en f8ff077): handler real `HandleQuickSearch` contra `NavigationManager` fake → `azul` resulta en `/catalogo?q=azul`; caso borde término vacío → `/catalogo`. RED probado con mutación M1 (rompió el formato `?q=` → aserción roja). + render real (form + input + botón presentes) | ✅ COMPLIANT |
| Píldoras de acceso finales (D4) | Píldoras exactas con destinos exactos | `HomeDashboard_Pills_AreExactlyTheFourD4PillsInOrder` (count=4, orden) + render real (bloque hero: 4 píldoras exactas, confirmado en esta re-ejecución) | ✅ COMPLIANT |
| Píldoras de acceso finales (D4) | Etiqueta de Sorteos sin herencia legacy | Render real (`Sorteos` → `/sorteos`; sin "Radar & Sorteos", sin "Autores") + teoría HomeDashboard (sin `href="/radar"`) | ✅ COMPLIANT |
| Link "Ver todas las novedades" corregido | Link de novedades apunta a su página | `HomeDashboard_VerTodasLasNovedades_LinkPointsToNovedades` + render real (anchor `<a href="/novedades"> Ver todas las novedades`, confirmado en esta re-ejecución) | ✅ COMPLIANT |
| Jerarquía de encabezados accesible (WCAG 2.2 AA) | Único h1 sr-only con texto propio | Teoría HomeDashboard (h1 exacto) + render real (total h1 = 1, sr-only, texto exacto por code points U+2014/U+00F1) | ✅ COMPLIANT |

### creators-directory (8 requerimientos / 13 escenarios)
| Requerimiento | Escenario | Test / Evidencia | Resultado |
|---|---|---|---|
| Listado público solo de creadores | Directorio sin diseñadores | `DirectorySeederTests` T1 (12 creators, 0 retirados) + render real `/creadores` (12 slugs del padrón, 0 diseñadores — confirmado en esta re-ejecución) | ✅ COMPLIANT |
| Ficha con redes sociales | Ficha muestra el link de YouTube | T5 (detalle expone SocialLinks) + render real `/creadores/analisis-paralisis` (YouTube PRESENTE, verify anterior) | ✅ COMPLIANT |
| Ficha con redes sociales | Ficha sin redes | **[REMEDIADO]** `DirectoryServicesTests.CreatorService_CreateWithoutSocialLinks_DetailExposesEmptyLinks` (`tests/Ludeka.UnitTests/Application/DirectoryServicesTests.cs`, nuevo en f8ff077): `CreateAsync` con `CreateCreatorDto` sin `SocialLinks` → persistido sin error → `GetBySlugAsync("creador-sin-redes")` retorna detalle con nombre y `SocialLinks` vacío. RED probado con mutación M3 (rompe `dto.SocialLinks?` en `CreateAsync` L83 → ArgumentNullException). Condición de render por fuente ya verificada (CreatorDetail L118: sección solo si `SocialLinks.Count > 0`) | ✅ COMPLIANT |
| Alta y edición con permiso CanManageCreators | Moderador crea creador con redes | T5 (CreateAsync con YouTube persistido) + `CreatorService_WithCanManageCreators_Succeeds` | ✅ COMPLIANT |
| Alta y edición con permiso CanManageCreators | Usuario sin permiso no gestiona creadores | `CreatorService_WithoutCanManageCreators_ThrowsUnauthorized` + fuente `EnsurePermission` (L190-198) | ✅ COMPLIANT |
| Sembrado purgado y re-sembrado (D1) | Seed sin diseñadores y con creadores del padrón | T1 (purga + 12 del padrón, link YouTube en Análisis Parálisis) | ✅ COMPLIANT |
| Sembrado purgado y re-sembrado (D1) | Re-ejecución idempotente | T2 (count estable en 2ª ejecución, sin diseñadores) | ✅ COMPLIANT |
| Alias /autores operativo (D2) | Bookmark antiguo al directorio | Teoría CreatorsDirectory (`@page "/autores"`) + render real (contenido ≡, verify anterior) | ✅ COMPLIANT |
| Alias /autores operativo (D2) | Bookmark antiguo a una ficha | Teoría CreatorDetail (`@page "/autores/{Slug}"`) + render real `/autores/analisis-paralisis` (200, contenido íntegro, verify anterior) | ✅ COMPLIANT |
| Diseñador de juego como texto plano | Diseñador sin link al directorio | Teoría GameDetail del contrato de fuente (sin `creadores/`, con span del designer) **PASÓ en runtime** (suite 723/723 de esta re-ejecución) + fuente L200-209; el DOM real sigue bloqueado por el 500 pre-existente (WARNING-1, ajeno al cambio) | ✅ COMPLIANT |
| Sin sección "Obras" ni cruce por Game.Designer | Ficha sin obras | Teoría CreatorDetail (sin "Obras de") + render real ficha (sin "Obras", verify anterior) | ✅ COMPLIANT |
| Sin sección "Obras" ni cruce por Game.Designer | Servicio sin matching de diseñador | T5 (sin Games/conteo; constructor sin `IGameRepository`) + fuente | ✅ COMPLIANT |
| Reetiquetado transversal | Navegación y fichas reetiquetadas | Teorías MainLayout/CreatorEditModal/AuditService/AuditLogViewer/UserPermissionsModal + render real (sin "Autores" visible en `/creadores` — confirmado en esta re-ejecución) | ✅ COMPLIANT |

### giveaway-radar (2 requerimientos / 3 escenarios)
| Requerimiento | Escenario | Test / Evidencia | Resultado |
|---|---|---|---|
| Página de sorteos sin banner legacy | Entrada por la ruta canónica | Teoría Radar (sin "¡Radar renovado!") + render real `/sorteos` (banner AUSENTE — confirmado en esta re-ejecución) | ✅ COMPLIANT |
| Página de sorteos sin banner legacy | Entrada por la ruta legacy | Render real `/radar` (banner AUSENTE, sin avisos, verify anterior) | ✅ COMPLIANT |
| /radar como alias silencioso | Bookmark antiguo resuelto sin aviso | Teoría Radar (ambas `@page`) + render real (contenido ≡ `/sorteos`, sin mensaje de migración, verify anterior) | ✅ COMPLIANT |

**Resumen de cumplimiento**: 22/22 escenarios COMPLIANT · 0 UNTESTED · 0 FAILING

### Correctness (evidencia estática — sin cambios desde el verify anterior)
| Requerimiento | Estado | Nota |
|---|---|---|
| HLH hero minimalista | ✅ Implementado | HomeDashboard.razor L12-55: h1 sr-only L14, sin badge/titular |
| HLH buscador | ✅ Implementado | form L22-36 + handler L376-386 (ahora con test de runtime) |
| HLH píldoras D4 | ✅ Implementado | L40-53: 4 píldoras exactas |
| HLH link novedades | ✅ Implementado | L226: `href="/novedades"` |
| HLH h1 sr-only | ✅ Implementado | L14, único h1 del documento |
| CD listado solo creadores | ✅ Implementado | Seeder purga (L159-175) + padrón (L181-209) |
| CD ficha con redes | ✅ Implementado | CreatorDetail L118-124 con `SocialLinksList` |
| CD permiso CanManageCreators | ✅ Implementado | CreatorService `EnsurePermission` L190-198 |
| CD sembrado D1 | ✅ Implementado | `RetiredSeedCreatorSlugs` = 6 slugs exactos (L159-163) |
| CD alias /autores (D2) | ✅ Implementado | `@page "/autores"` + `@page "/autores/{Slug}"` intactos |
| CD diseñador texto plano | ✅ Implementado | GameDetail L200-209: span plano, sin `<a>` |
| CD sin Obras ni cruce | ✅ Implementado | CreatorService sin `IGameRepository`; DTOs sin Games/GamesCount |
| CD reetiquetado transversal | ✅ Implementado | AD-7 completo en 10 archivos (tabla del diseño) |
| GR sin banner legacy | ✅ Implementado | Radar sin banner, sin `IsLegacyRoute`, sin inject NavigationManager |
| GR alias silencioso | ✅ Implementado | `@page "/sorteos"` + `@page "/radar"` |

### Coherencia (design.md — sin cambios desde el verify anterior)
| Decisión | ¿Seguida? | Nota |
|---|---|---|
| AD-1 purga por lista cerrada de slugs | ✅ Sí | `RetiredSeedCreatorSlugs` con los 6 slugs exactos del diseño |
| AD-2 padrón vía `GetStaticCreators()` | ✅ Sí | Única fuente; sin duplicar nombres |
| AD-3 markup del hero + fix L232 | ✅ Sí | Markup renderizado coincide con el diseño |
| AD-4 radar sin banner/`IsLegacyRoute`/inject | ✅ Sí | Las tres eliminaciones verificadas |
| AD-5 diseñador texto plano | ✅ Sí | Span plano, fallback "Desconocido", editoriales intactas |
| AD-6 desacope CreatorService | ✅ Sí | Constructor, DTOs y ripple tal como el diseño |
| AD-7 reetiquetado (tabla archivo→cadena) | ✅ Sí | Verificado por contrato de fuente y render |
| Estrategia: contrato de fuente + render en verify | ✅ Sí | Los 2 gaps de la estrategia quedaron remedios por f8ff077 (handler de quick search + ficha sin redes) |
| Desviaciones declaradas por apply | ✅ Aceptadas | Ripple AD-6 adelantado a WU2 (compilación) y "Ver todos los sorteos" L160 `/radar`→`/sorteos` — ambas documentadas y coherentes con los specs |

### Criterios de éxito del proposal
| Criterio | Estado | Evidencia |
|---|---|---|
| Portada = párrafo + buscador + 4 píldoras (D4); sin badge ni titular; h1 accesible | ✅ | Render real `/` (ambas verificaciones) + teorías HomeDashboard + test de navegación quick search |
| "Ver todas las novedades" → `/novedades`; Sorteos sin banner | ✅ | Render real `/` y `/sorteos`/`/radar` + `HomeDashboard_VerTodasLasNovedades_LinkPointsToNovedades` |
| `/creadores` (y `/autores`) solo creadores con ficha y redes; cero links rotos desde fichas de juego | ✅ | Render real + T1-T4 + T5 (incl. ficha sin redes de f8ff077) + GameDetail sin `<a>` al directorio (contrato + fuente) |
| Suite verde; INC-31 en roadmap | ✅ | 723/723 (exit 0, 2 ejecuciones); ROADMAP.md L46 y ROADMAP_MVP_SLICES.md L278 ("⏳ En progreso", formato coherente) |

## Hallazgos

**CRITICAL**: 0 (los 2 CRITICAL de cobertura del verify anterior quedan remediados y verificados por f8ff077)

**WARNING**:
1. **Bug pre-existente ajeno a INC-31, bloquea el render real de fichas de juego — FOLLOW-UP separado**: toda `/juegos/{slug}` devuelve HTTP 500 (`NotSupportedException: SQLite does not support expressions of type 'DateTimeOffset' in ORDER BY clauses`, `SqliteGamePlayLogRepository.GetByUserAndGameAsync` L56, vía `GameDetail.LoadUserDataAsync → RefreshPlaysCountAsync`). Introducido por la feature de play-log (commit fc90de0, INC-30) y sin cambios desde entonces; ningún commit de INC-31 (incluido f8ff077) toca play-log ni `LoadUserDataAsync`. Implicación: el DOM de la ficha de juego no puede capturarse; el escenario CD·diseñador-plano queda COMPLIANT por su contrato de fuente que pasó en runtime (723/723). **Rutear remediation separada** (ORDER BY sobre tipo soportable o LINQ-to-Objects).
2. **Test flaky ajeno a INC-31 (antecedente)**: `StoreStockServiceTests.GetStockAsync_WhenClientTimesOut_ShouldGracefullyDegradeToUnknown` falló 1/3 ejecuciones en el verify anterior; en esta re-ejecución pasó en ambas corridas completas. Test de timing con carrera, sin relación con los 6 commits de INC-31.

**SUGGESTION** (sin cambios desde el verify anterior):
1. **Leftover cosmético fuera de contrato** — `CreatorsDirectory.razor` L123: CTA "Ver obras diseñadas" en las tarjetas del directorio (confirmado presente en el render de esta re-ejecución; no viola ningún requerimiento del spec). Sugerir renombrar a "Ver ficha" en una limpieza futura.
2. **Leftover cosmético fuera de contrato** — `AuditLogViewer.razor` L49: la palabra "autores" (minúscula) en la microcopy de enumeración. La etiqueta de auditoría prohibida (L95) ya dice "✍️ Creador de Contenido".
3. `PageTitle` de la portada conserva "El Letterboxd de los juegos de mesa en español" — **explícitamente fuera de alcance por spec** (se documenta, no es desviación).
4. `LibraryStatsDashboard.razor` mantiene "Autores y Editoriales Favoritas" — **N/A por decisión AD-7** (estadística de catálogo derivada de `Game.Designer`, fuera del directorio de creadores).

## Roadmaps
| Archivo | Estado |
|---|---|
| `docs/increments/ROADMAP.md` (L46) | ✅ INC-31 "⏳ En progreso" con enlace al proposal |
| `docs/specs/ROADMAP_MVP_SLICES.md` (L278) | ✅ "## Incremento 31" con identificador SDD, objetivo y "⏳ **En progreso**" — formato idéntico a los slices 23-30 |

Coherencia: ambos en "En progreso" → correcto hasta que `sdd-archive` los pase a "✅ Archivado".

## Veredicto

**PASS**

- **15/15 requerimientos** implementados y evidenciados (Correctness ✅ en los 15; Coherencia de diseño ✅ en las 9 decisiones).
- **22/22 escenarios COMPLIANT** con test de cobertura que pasó en runtime: 20 con la evidencia del verify anterior + 2 remediados por f8ff077 (`QuickSearch_SubmitNavigatesToCatalogoWithQueryAndFallsBackWithoutTerm` y `CreatorService_CreateWithoutSocialLinks_DetailExposesEmptyLinks`), ambos con RED probado por mutaciones transitorias revertidas.
- Suite **723/723** (exit 0, 2 ejecuciones), build 0 errores / 0 advertencias, criterios de éxito del proposal **4/4**, renders de confirmación sin violación alguna del spec.
- Único riesgo remanente: el **bug pre-existente de play-log (500 en `/juegos/{slug}`)** de INC-30 — follow-up separado, fuera del alcance de INC-31. El cambio está listo para `sdd-archive`.
