# Incremento 6: Automatización Omnicanal, Radar de Sorteos y Comunidad

- **Identificador SDD:** `change-06-automation-community`
- **Puntos del MVP cubiertos:** 6.1 a 6.3, 10.1 a 10.5, 11.1, 11.2 (Sorteos, Novedades, Plantillas de marca, Q&A de reglas y Transparencia).
- **Estado:** ✅ **Completado y Archivado** (Commit `f71e98f`, 146 tests en verde).

---

## 1. Alcance Funcional Entregado

1. **Radar de Sorteos Externos y Carruseles:**
   - Publicación de sorteos con fecha límite (`DeadlineAt`) y expiración automática.
   - Fusión visual de colaboraciones (editorial + divulgador) en tarjeta única.
2. **Novedades de Tiendas de los Viernes:**
   - Sección de lanzamientos y reimpresiones semanales.
3. **Motor de Composición de Imágenes de Marca (1:1 / Instagram):**
   - Servicio `ISocialCardService` para generar activos gráficos con portada del juego, píldora de evento (`🎁 SORTEO`, `🚀 NOVEDAD`) y pie de marca corporativo.
4. **Consultorio de Reglas Q&A (Estilo StackOverflow):**
   - Sistema por juego de preguntas, respuestas, votos comunitarios y respuesta marcada como oficial/aceptada.
5. **Página del Manifiesto de Transparencia:**
   - Compromiso ético público en `/transparencia` sobre el destino de los fondos y afiliación.

---

## 2. Artefactos y Componentes Clave

- **Dominio:** [`Giveaway`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/Giveaway.cs), [`WeeklyRelease`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/WeeklyRelease.cs), [`RuleQA`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/RuleQA.cs).
- **Aplicación:** [`IGiveawayService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IGiveawayService.cs), [`IWeeklyReleaseService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IWeeklyReleaseService.cs), [`IRuleQAService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IRuleQAService.cs), [`ISocialCardService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/ISocialCardService.cs).
- **Infraestructura:** Repositorios SQLite correspondientes.
- **Web UI:** [`Radar.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/Radar.razor), [`GiveawayCard.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/GiveawayCard.razor), [`RuleQAPanel.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Shared/RuleQAPanel.razor), [`Transparency.razor`](file:///c:/repos/Ludeka/src/Ludeka.Web/Components/Pages/Transparency.razor).

---

## 3. Verificación

- Pruebas unitarias de expiración de sorteos, votación en reglas y generación de tarjetas en [`tests/Ludeka.UnitTests`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests).
