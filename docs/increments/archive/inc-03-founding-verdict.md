# Incremento 3: Panel y Veredicto de la Mesa Fundadora

- **Identificador SDD:** `change-03-founding-verdict`
- **Puntos del MVP cubiertos:** 4.1, 4.2 (Ciclo de vida del veredicto y panel editorial).
- **Estado:** ✅ **Completado y Archivado** (Commit `4404e6b`).

---

## 1. Alcance Funcional Entregado

1. **Autenticación y roles de usuario:** Soporte para el rol editorial `FoundingTeam` / `Moderator`.
2. **Acceso directo desde la ficha:** Botón contextual visible para editores `[ 🛡️ Gestionar Veredicto Fundador ]`.
3. **Análisis oficial de la casa con foco doble:**
   - Experiencia real a 2 personas (juego en pareja).
   - Experiencia familiar con niños (ritmo y adaptación de reglas).
4. **Galería fotográfica de mesa real:**
   - Subida y visualización de 1 a 3 fotos tomadas en mesa propia para máxima credibilidad visual frente a renders comerciales.
5. **Sello de recomendación oficial:**
   - *Imprescindible de la Mesa* (`MustPlay`), *Recomendado con adaptaciones* (`RecommendedWithAdaptations`), *Prescindible* (`Skippable`).
6. **Algoritmo de prioridad visual:** El veredicto de la mesa fundadora se renderiza en la parte superior de la ficha antes que las valoraciones abiertas y sustituye a la síntesis inicial de IA.

---

## 2. Artefactos y Componentes Clave

- **Dominio:** [`FoundingVerdict`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/FoundingVerdict.cs), [`FoundingPhoto`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/FoundingPhoto.cs), [`FoundingRecommendation`](file:///c:/repos/Ludeka/src/Ludeka.Core/Enums/FoundingRecommendation.cs).
- **Aplicación:** [`IFoundingVerdictService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IFoundingVerdictService.cs), [`FoundingVerdictService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Founding/FoundingVerdictService.cs).
- **Infraestructura:** [`SqliteFoundingVerdictRepository`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteFoundingVerdictRepository.cs).
- **Web UI:** [`FoundingVerdictCard.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/FoundingVerdictCard.razor), panel modal de edición editorial.

---

## 3. Verificación

- Pruebas unitarias de persistencia, restricciones de rol y prioridad visual en [`tests/Ludeka.UnitTests`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests).
