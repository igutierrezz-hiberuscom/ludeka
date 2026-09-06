# Informe de Verificación: Incremento 2 (change-02-library-loans)

**Fecha de Verificación:** 2026-09-06  
**Cambio Evaluado:** `change-02-library-loans` (Ludoteca Personal, Colección en 4 Estados y Préstamos)  
**Estado:** Superado exitosamente  
**Veredicto:** **PASS**

---

## Resumen de Ejecución y Estado de Pruebas

Se ejecutó la suite completa de pruebas unitarias y de integración sobre la solución `src/Ludeca.slnx` con el SDK de .NET 10 (`net10.0`, C# 13):

```powershell
$env:DOTNET_ROOT = "$HOME\.dotnet"; $env:PATH = "$HOME\.dotnet;$env:PATH"; dotnet test src/Ludeca.slnx
```

### Resultados de la Ejecución
- **Total de pruebas ejecutadas:** 54 (27 previas + 27 nuevas de este incremento)
- **Pruebas superadas (Passed):** 54
- **Pruebas fallidas (Failed):** 0
- **Pruebas omitidas (Skipped):** 0
- **Tiempo de ejecución:** ~1.0 s
- **Código de salida:** 0 (Éxito)

---

### Desglose por Componente y Pruebas Nuevas

1. **Dominio de Colección (`UserCollectionItemTests` - 5 pruebas):**
   - Instanciación correcta con validación de identificadores no vacíos.
   - Rechazo de identificadores nulos o en blanco arrojando `ArgumentException`.
   - Transición de estados con actualización de fecha `UpdatedAt`.
   - Idempotencia al reasignar el mismo estado sin alterar marcas temporales.

2. **Dominio de Préstamos (`GameLoanTests` - 3 pruebas):**
   - Creación de préstamo con prestatario, fecha y notas opcionales.
   - Validación obligatoria de prestatario no vacío.
   - Marcación de devolución mediante `MarkAsReturned()` registrando fecha de retorno e `IsReturned = true`.

3. **Dominio de Micro-Valoraciones (`UserGameReviewTests` - 4 pruebas):**
   - Creación de reseña con nota, texto, votos de escalabilidad, experiencia infantil y contexto de partida.
   - Validación de rango estricto de nota `[1.0, 10.0]` arrojando `ArgumentOutOfRangeException`.
   - Validación de longitud estricta de micro-reseña (rechazo a más de 280 caracteres).
   - Actualización de reseña con reasignación de comensales y sellado de tiempo.

4. **Casos de Uso de Ludoteca (`UserLibraryServiceTests` - 6 pruebas):**
   - Alta de estado en colección cuando el juego no estaba registrado.
   - Mecánica de desmarcado (toggle off) al volver a seleccionar el estado activo.
   - Creación de préstamo exitosa para títulos en posesión (`InCollection`).
   - Invariante de dominio: rechazo de préstamo si el juego no está en posesión física (`InvalidOperationException`).
   - Retorno de préstamo actualizando estado de préstamo activo.
   - Emisión de valoración con recálculo dinámico automático del consenso local `LudistRating`.

5. **Persistencia SQLite EF Core 10 (`SqliteLibraryPersistenceTests` - 3 pruebas):**
   - Inserción y consulta de ítems de colección vinculados con la tabla `Games`.
   - Inserción y filtrado de préstamos activos e históricos con devolución en base de datos.
   - Persistencia de objetos de valor JSON dinámicos (`.OwnsMany().ToJson()`) para votos de comensales y experiencia infantil con cálculo de promedios.

6. **Arranque y Verificación en Vivo de `Ludeca.Web`:**
   - La aplicación Blazor Server compiló y ejecutó `EnsureCreatedAsync()`, creando con éxito en SQLite las tablas `Games`, `GameLoans`, `UserCollectionItems` y `UserGameReviews` con todos sus índices únicos.
   - Sembrado automático del catálogo de 25 títulos top en español.
   - Servidor escuchando en `http://localhost:5000` con respuesta inmediata.

---

## Estado de Tareas en `tasks.md`

Todas las 24 tareas de las 6 fases de `tasks.md` se completaron satisfactoriamente:
- [x] **Fase 1:** Dominio Lúdico y Reglas de Negocio (1.1 a 1.6)
- [x] **Fase 2:** Contratos, DTOs y Servicios de Aplicación (2.1 a 2.4)
- [x] **Fase 3:** Persistencia SQLite EF Core 10 (3.1 a 3.5)
- [x] **Fase 4:** Pruebas Unitarias y Persistencia (4.1 a 4.4)
- [x] **Fase 5:** Interfaz Web Blazor y Experiencia Mobile-First (5.1 a 5.8)
- [x] **Fase 6:** Verificación Final y Documentación (6.1 a 6.3)

---

## Matriz de Conformidad de Capacidades y Especificaciones

| Capacidad | Tipo | Escenarios Clave Validados | Estado |
|---|---|---|---|
| `personal-collection` | Nueva | 4 estados lúdicos, cambio de estado, desmarcado y barra interactiva | **CONFORME** |
| `game-loans` | Nueva | Préstamo de juego propio, indicador activo en ficha y devolución en 1 clic | **CONFORME** |
| `modular-reviews` | Nueva | Nota 1–10, límite de 280 caracteres, chips 1J–7J+, toggle infantil y tarjeta propia | **CONFORME** |
| `user-library-view` | Nueva | Página `/mi-ludoteca` con 5 pestañas, contadores en tiempo real y devolución directa | **CONFORME** |
| `core-catalog` | Modificada (Delta) | Recálculo dinámico de `LudistRating` y enlace directo a Mi Ludoteca | **CONFORME** |

---

## Conclusión

El **Incremento 2: Ludoteca Personal, Colección en 4 Estados y Préstamos** (`change-02-library-loans`) satisface al 100% las especificaciones funcionales y de arquitectura, con una cobertura total de 54 pruebas unitarias e integración en verde. Queda listo para ser archivado en el histórico del proyecto.
