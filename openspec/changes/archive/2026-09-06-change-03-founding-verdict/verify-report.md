# Informe de Verificación: Incremento 3 (change-03-founding-verdict)

**Fecha de Verificación:** 2026-09-06  
**Cambio Evaluado:** `change-03-founding-verdict` (Panel y Veredicto de la Mesa Fundadora)  
**Estado:** Superado exitosamente  
**Veredicto:** **PASS**

---

## Resumen de Ejecución y Estado de Pruebas

Se ejecutó la suite completa de pruebas unitarias y de integración sobre la solución `src/Ludeca.slnx` con el SDK de .NET 10 (`net10.0`, C# 13):

```powershell
$env:DOTNET_ROOT = "$HOME\.dotnet"; $env:PATH = "$HOME\.dotnet;$env:PATH"; dotnet test src/Ludeca.slnx
```

### Resultados de la Ejecución
- **Total de pruebas ejecutadas:** 71 (54 previas + 17 nuevas de este incremento)
- **Pruebas superadas (Passed):** 71
- **Pruebas fallidas (Failed):** 0
- **Pruebas omitidas (Skipped):** 0
- **Tiempo de ejecución:** ~1.0 s
- **Código de salida:** 0 (Éxito)

---

### Desglose por Componente y Pruebas Nuevas

1. **Dominio de Veredicto Fundador (`FoundingVerdictTests` - 5 pruebas):**
   - Instanciación correcta de `FoundingVerdict` con sello `MustPlay`, textos estructurados y fotos.
   - Validación de longitud mínima (al menos 10 caracteres) para análisis general, análisis 2J y análisis familiar arrojando `ArgumentException`.
   - Límite estricto de máximo 3 fotos de mesa real arrojando `InvalidOperationException`.
   - Remoción de fotos por índice y actualización de marcas de tiempo.
   - Puntuación equivalente según sello (`MustPlay` -> 10.0, `RecommendedWithAdaptations` -> 7.5, `Skippable` -> 4.0).

2. **Persistencia SQLite EF Core 10 (`SqliteFoundingVerdictRepositoryTests` - 3 pruebas):**
   - Inserción y consulta por `GameId` persistiendo fotos serializadas en JSON (`OwnsMany().ToJson()`) de forma transparente.
   - Actualización completa de veredicto y colección de fotos.
   - Eliminación física de registros en base de datos.

3. **Casos de Uso y Autorización Editorial (`FoundingVerdictServiceTests` - 4 pruebas):**
   - Generación de `AiGameSummaryDto` estructurado para juegos sin veredicto (Fase 1).
   - Control de acceso por rol: rechazo de publicación con `UnauthorizedAccessException` cuando el usuario no tiene rol `FoundingTeam` o `Moderator`.
   - Publicación de veredicto por miembro fundador y recálculo ponderado del `LudistRating` (peso triple).
   - Conmutador dinámico de roles de usuario (`DefaultCurrentUserService.SwitchRole`).

4. **Arranque y Verificación en Vivo de `Ludeca.Web`:**
   - La aplicación Blazor Web App se ejecutó en `http://localhost:5081`.
   - Se verificó mediante peticiones HTTP reales:
     - `/juegos/wingspan`: renderiza la tarjeta editorial `FoundingVerdictCard` con sello de recomendación oficial, análisis desglosado y fotos reales en mesa.
     - `/juegos/cascadia`: renderiza la tarjeta `AiSummaryCard` con el badge `🤖 Resumen generado por IA` y el banner didáctico del ciclo de vida lúdico.
     - La cabecera `MainLayout` incluye el conmutador de rol `[ 🛡️ Mesa Fundadora ] / [ 👤 Usuario ]`.
