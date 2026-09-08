# 03. Mesa Fundadora y Veredictos Editoriales

## 1. Visión General y Propósito
El Veredicto de la Mesa Fundadora es el distintivo editorial oficial de Ludeka. Aporta un análisis honesto y fundamentado con foco prioritario en la experiencia real en pareja (2 jugadores) y en familia con niños, respaldado por fotografías reales de partidas tomadas en salón propio para otorgar máxima credibilidad frente a renders promocionales.

---

## 2. Modelo de Dominio (`Ludeka.Core`)

### 2.1 Entidad `FoundingVerdict`
Ubicación: [`src/Ludeka.Core/Entities/FoundingVerdict.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/FoundingVerdict.cs)

- `Id` (Guid), `GameId` (Guid), `AuthorId` (string), `AuthorDisplayName` (string).
- `Recommendation`: Enum `FoundingRecommendation`:
  - `MustPlay` (⭐ Imprescindible de la Mesa)
  - `RecommendedWithAdaptations` (👍 Recomendado con adaptaciones)
  - `Skippable` (✋ Prescindible)
- `VerdictText`: Reseña editorial central y síntesis de la experiencia.
- `TwoPlayerExperience`: Análisis detallado del comportamiento y ritmo a 2 personas (tensión, entreturno, escalado del mapa/tablero).
- `FamilyExperience`: Análisis práctico con menores (edad real observada, reglas modificadas).
- `Photos`: Colección de [`FoundingPhoto`](file:///c:/repos/Ludeka/src/Ludeka.Core/ValueObjects/FoundingPhoto.cs) (`PhotoUrl`, `Caption`).
- `CreatedAt`, `UpdatedAt`.

---

## 3. Reglas de Negocio y Ciclo de Vida

1. **Prioridad Visual en la Ficha:**
   - La tarjeta del veredicto fundador se renderiza siempre en la posición superior de la ficha del juego, por encima de las opiniones abiertas de la comunidad.
   - Si el juego contaba con una síntesis inicial automatizada de IA, el Veredicto Fundador la reemplaza de inmediato.
2. **Autorización:**
   - Exclusivamente usuarios con rol `FoundingTeam` o `Moderator` tienen acceso al formulario de alta y edición.

---

## 4. Capa de Aplicación e Infraestructura

- **Servicio:** [`FoundingVerdictService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Founding/FoundingVerdictService.cs)
- **Repositorio:** [`IFoundingVerdictRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IFoundingVerdictRepository.cs) implementado en [`SqliteFoundingVerdictRepository.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Data/SqliteFoundingVerdictRepository.cs).

---

## 5. Componentes UI (`Ludeka.Web`)

- [`FoundingVerdictCard.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/FoundingVerdictCard.razor): Renderizado editorial con sello de recomendación, desplegable fotográfico con zoom accesible y pestañas de experiencia en pareja y familiar.
