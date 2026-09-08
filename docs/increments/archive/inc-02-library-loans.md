# Incremento 2: Ludoteca Personal, Colección en 4 Estados y Préstamos

- **Identificador SDD:** `change-02-library-loans`
- **Puntos del MVP cubiertos:** 7.1, 7.2, 7.3 (Colección, Préstamos y Formulario modular de valoración).
- **Estado:** ✅ **Completado y Archivado** (Commit `e4d777e`).

---

## 1. Alcance Funcional Entregado

1. **Barra de acción interactiva en la ficha del juego con 4 estados:**
   - 🟢 *En mi ludoteca* (físico propio).
   - 🔵 *Jugado* (asociación, bar, amigos).
   - 🟡 *Deseado* (radar de interés).
   - 🔴 *Quiero comprar* (lista de seguimiento de ofertas).
2. **Módulo privado de préstamos ("¿A quién se lo dejé?"):**
   - Registrar nombre de la persona o asociación y fecha de entrega.
   - Listado privado en el perfil con botón de devolución en 1 clic.
3. **Formulario modular de valoración rápida en 45 segundos:**
   - Puntuación 1 a 10 y micro-reseña de máximo 280 caracteres.
   - Chips interactivos de comensales (1J a 7J+) para votar el semáforo personal.
   - Experiencia infantil opcional (edad mínima sugerida y adaptación de reglas).
4. **Edición de valoración propia:**
   - Tarjeta destacada encima de las opiniones públicas con botón de edición rápida.

---

## 2. Artefactos y Componentes Clave

- **Dominio:** [`UserCollectionItem`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/UserCollectionItem.cs), [`GameLoan`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/GameLoan.cs), [`UserGameReview`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/UserGameReview.cs), [`UserPlayerCountVote`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/UserPlayerCountVote.cs).
- **Aplicación:** [`UserLibraryService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Library/UserLibraryService.cs), [`IUserCollectionRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IUserCollectionRepository.cs), [`IGameLoanRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IGameLoanRepository.cs), [`IUserReviewRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IUserReviewRepository.cs).
- **Infraestructura:** Repositorios SQLite correspondientes.
- **Web UI:** [`MyLibrary.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/MyLibrary.razor), [`ReviewBottomSheet.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/ReviewBottomSheet.razor), [`LoanModal.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/LoanModal.razor).

---

## 3. Verificación

- Pruebas unitarias de cambios de estado, ciclo de vida de préstamos y votación por número de comensales en [`tests/Ludeka.UnitTests`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests).
