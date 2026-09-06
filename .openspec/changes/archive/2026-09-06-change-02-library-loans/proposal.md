# Propuesta: Incremento 2 — Ludoteca Personal, Colección en 4 Estados y Préstamos

## Intención
Implementar la gestión de ludoteca personal en 4 estados lúdicos, el registro privado de préstamos ("¿A quién se lo dejé?") con devolución en 1 clic, y el formulario modular de valoración rápida en 45 segundos con tarjeta de opinión propia y vista de biblioteca en `/mi-ludoteca`.

## Alcance

### Dentro del Alcance
- Barra de acción inferior interactiva ergonómica en `/juegos/{slug}` con 4 estados: *En mi ludoteca* (verde), *Jugado* (azul), *Deseado* (amarillo) y *Quiero comprar* (rojo).
- Módulo de préstamos para juegos en propiedad: prestatario, fecha, notas, indicador de préstamo activo y devolución en 1 clic.
- Formulario de valoración en 45 s: nota 1–10, micro-reseña (máx. 280 caracteres), chips de comensales 1J–7J+ con semáforo personal, experiencia familiar/infantil y contexto.
- Tarjeta de valoración propia fijada sobre el bloque de opiniones con edición directa.
- Vista de ludoteca personal `/mi-ludoteca` organizada en pestañas reactivas por estado y préstamos activos.
- Persistencia SQLite EF Core 10 con mapeo JSON nativo e índices compuestos.
- Abstracción `ICurrentUserService` para desacoplar identidad de usuario previo a OAuth.

### Fuera del Alcance
- Veredicto y panel de la mesa fundadora con fotos reales (Incremento 3).
- Hub multimedia de vídeos YouTube y posts Instagram (Incremento 4).
- Importador masivo de colecciones BGG XMLAPI2 (Incremento 5).

## Capacidades

### Nuevas Capacidades
- `personal-collection`: Estado lúdico del usuario sobre cada título (En mi ludoteca, Jugado, Deseado, Quiero comprar) con barra táctil inferior y persistencia.
- `game-loans`: Cuaderno privado de préstamos para títulos en propiedad con registro de destinatario, fecha y devolución en 1 toque.
- `modular-reviews`: Formulario rápido en 45 s con nota 1–10, micro-reseña de 280 caracteres, chips de 1J a 7J+ con semáforo personal, experiencia infantil y contexto.
- `user-library-view`: Página `/mi-ludoteca` con navegación por pestañas filtradas por estado de colección y préstamos activos.

### Capacidades Modificadas
- `core-catalog`: Ficha de juego enriquecida con barra de acción, indicador de préstamo activo, tarjeta de valoración propia y recálculo de consenso Ludist.

## Enfoque Arquitectónico
- **Dominio (`Ludeca.Core`)**: Nuevas entidades ricas `UserCollectionItem`, `GameLoan`, `UserGameReview` y Value Objects inmutables con validaciones estrictas.
- **Aplicación (`Ludeca.Application`)**: Repositorios `IUserCollectionRepository`, `IGameLoanRepository`, `IUserReviewRepository`, servicio `IUserLibraryService` y abstracción `ICurrentUserService`.
- **Infraestructura (`Ludeca.Infrastructure`)**: Mapeo relacional e índices en `LudecaDbContext`, serialización JSON para votos de comensales y repositorios SQLite.
- **UI (`Ludeca.Web`)**: Componentes Blazor interactivos `CollectionActionBar`, `LoanModal`, `ReviewBottomSheet`, `UserReviewCard` y página `MyLibrary`.

## Áreas Afectadas
| Área | Impacto | Descripción |
|---|---|---|
| `Ludeca.Core` | Nuevo / Modificado | Entidades `UserCollectionItem`, `GameLoan`, `UserGameReview` y Value Objects de votos |
| `Ludeca.Application` | Nuevo | Interfaces de repositorio, DTOs y `UserLibraryService` |
| `Ludeca.Infrastructure` | Modificado | DbSets en `LudecaDbContext`, índices y repositorios SQLite |
| `Ludeca.Web` | Nuevo / Modificado | Página `/mi-ludoteca`, barra de acción en `GameDetail.razor` y modales de préstamo y reseña |
| `tests/Ludeca.UnitTests` | Nuevo | Tests unitarios de dominio, casos de uso y persistencia en memoria |

## Riesgos y Mitigaciones
| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Usabilidad móvil de formularios de reseña | Media | Diseño ergonómico tipo Bottom Sheet con chips de 1 toque y contador en tiempo real de 280 caracteres |
| Inconsistencia al prestar juegos no propios | Baja | Invariante de dominio: `GameLoan` exige que el juego esté en estado `InCollection` |
| Identidad de usuario sin autenticación OAuth | Baja | Proveedor `ICurrentUserService` predeterminado local listo para conectarse a OAuth en futuros incrementos |

## Plan de Rollback
- Revertir commits del incremento 2.
- Restaurar `LudecaDbContext` a la versión previa y migrar o regenerar `ludeca.db`.

## Dependencias
- Catálogo base y ficha inteligente (`change-01-core-catalog`).
- EF Core 10 SQLite y motor de componentes Blazor Server.

## Criterios de Éxito
- [ ] 100% de tests unitarios y de integración pasando en xUnit.
- [ ] Barra interactiva operativa con los 4 estados reflejados al instante en la ficha del juego.
- [ ] Préstamo de juego en propiedad registrado con éxito y devuelto en 1 clic.
- [ ] Formulario de valoración en 45 s validado (nota 1–10, límite 280 caracteres, chips de comensales).
- [ ] Página `/mi-ludoteca` operativa mostrando los conteos y juegos por cada estado.
