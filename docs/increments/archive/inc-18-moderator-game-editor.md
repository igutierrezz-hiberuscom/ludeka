# Incremento 18: Editor Editorial de Fichas de Catálogo y Carga de Imágenes para Moderadores

- **Identificador SDD:** `change-18-moderator-game-editor`
- **Estado:** ✅ **Completado y Archivado (07/09/2026)**
- **Puntos del MVP cubiertos:** Gestión de catálogo, Herramientas editoriales internas y control de calidad multimedia.
- **Objetivo Principal:** Proporcionar al equipo fundador y moderadores la capacidad de editar en tiempo real cualquier ficha de juego (títulos, año, autores, rango de jugadores, duración, edad mínima, peso/complejidad, sinopsis) y cargar nuevas imágenes (subida de ficheros JPG/PNG/WebP con almacenamiento persistente local en `wwwroot/images/games` o especificación por URL remota), permitiendo subsanar incidencias de catálogo al instante.

---

## 1. Alcance Funcional Propuesto

1. **Permisos y Control de Acceso:**
   - Funcionalidad restringida a usuarios con rol `Moderator` o `FoundingTeam` a través de `ICurrentUserService`.
   - Botón visible exclusivamente para moderadores en `GameDetail.razor`: `[ ✏️ Editar Ficha ]`.
   - Acceso cruzado directo desde la bandeja de reportes de INC-17 mediante el botón `[ ✏️ Corregir Ficha y Resolver ]`.
2. **Formulario Editorial Completo (`GameEditorModal.razor`):**
   - **Metadatos Generales:** Título comercial en español, Título original, Diseñador/Autor, Editorial licenciataria y Año de publicación.
   - **Parámetros Técnicos y de Mesa:** Rango de jugadores (mín/máx), Duración de partida (mín/máx en minutos), Edad mínima oficial, Nivel de dependencia del idioma (`None`, `Low`, `Medium`, `High`), Estilo de juego (`Euro`, `Ameritrash`, `Party`, `Filler`, etc.) y Tipo de confrontación (`Competitive`, `Cooperative`, `SemiCooperative`, `TeamVsTeam`).
   - **Sinopsis y Notas:** Descripción del juego con formato de texto limpio en español.
3. **Carga y Reemplazo de Imágenes de Portada:**
   - **Subida Directa de Archivos:** Componente `InputFile` con soporte para `.jpg`, `.jpeg`, `.png` y `.webp`.
     - Límite de tamaño máximo seguro (5 MB).
     - Almacenamiento físico en `src/Ludeka.Web/wwwroot/images/games/` con nombre canónico higienizado basado en el slug (ej. `{slug}-cover.jpg` y `{slug}-thumb.jpg`).
   - **Opción de Enlace URL:** Permite especificar una URL externa válida (ej. BGG o web oficial de la editorial) con botón de verificación y vista previa antes de guardar.
   - **Actualización Dinámica:** Invalida la caché del catálogo y actualiza la ficha del juego en memoria inmediatamente sin necesidad de reiniciar la aplicación ni causar parpadeos visuales (Zero-FOUC).
4. **Resolución Integrada de Reportes Comunitarios:**
   - Si la edición se inicia desde un reporte de INC-17, al pulsar "Guardar y Resolver", el reporte asociado pasa automáticamente a estado `Resolved` con la nota del moderador indicando los cambios realizados.

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Moderador modifica datos erróneos de una ficha existente
  Dado un usuario autenticado con rol "Moderator"
  Cuando pulsa en "✏️ Editar Ficha" en el juego "Catan"
  Y modifica el número mínimo de jugadores de 3 a 2 (con variante) y actualiza la editorial
  Y pulsa "Guardar Cambios"
  Entonces la entidad Game en base de datos persiste los nuevos valores
  Y la vista del juego refleja los cambios al instante

Escenario: Moderador sube una nueva carátula para sustituir una imagen incorrecta
  Dado un moderador autenticado en la ficha de un juego con imagen placeholder o errónea
  Cuando abre el modal de edición y selecciona un archivo "caratula_hd.jpg" de 2 MB
  Y pulsa "Subir y Aplicar"
  Entonces el fichero se almacena en "wwwroot/images/games/{slug}-cover.jpg"
  Y la propiedad CoverImageUrl del juego se actualiza con la nueva ruta
  Y la ficha muestra la nueva carátula de inmediato

Escenario: Moderador corrige ficha y resuelve reporte comunitario en un clic
  Dado un reporte de incidencia de INC-17 para el juego "Wingspan" que indica "Falta autor"
  Cuando el moderador pulsa "Corregir Ficha y Resolver" desde la bandeja de reportes
  Y añade "Elizabeth Hargrave" en el campo Diseñador y pulsa "Guardar y Resolver"
  Entonces los datos del juego se actualizan
  Y el reporte de incidencia pasa automáticamente a estado "Resolved" con auditoría

Escenario: Usuario no autorizado intenta invocar la edición
  Dado un usuario con rol de comunidad general o invitado no autenticado
  Cuando intenta acceder a la acción de edición
  Entonces la interfaz no muestra el botón de edición
  Y cualquier intento directo por endpoint o servicio devuelve autorización denegada
```

---

## 3. Arquitectura y Componentes Clave

- **Dominio (`Ludeka.Core`):**
  - Métodos enriquecidos en `Game.cs`: `UpdateCatalogInformation(...)`, `UpdateImages(...)`.
  - Entidad de auditoría `GameEditLog.cs` para registrar el historial de cambios.
- **Aplicación (`Ludeka.Application`):**
  - Contratos `IGameEditorService.cs` e `IImageStorageService.cs`.
  - DTOs `UpdateGameDetailsCommand.cs`, `GameImageUploadResult.cs`.
  - Validaciones con reglas de negocio (ej. número mínimo de jugadores no puede ser mayor que el máximo).
- **Infraestructura (`Ludeka.Infrastructure`):**
  - Implementación `PhysicalFileImageStorageService.cs` para almacenamiento seguro y validación MIME.
  - Actualización transaccional en `SqliteGameRepository.cs` y repositorio `SqliteGameEditLogRepository.cs`.
- **Presentación Web (`Ludeka.Web`):**
  - Componente modal `GameEditorModal.razor`.
  - Integración en `GameDetail.razor` (botón contextual) y en `GameReportsModeration.razor` (botón de acción cruzada).

---

## 4. Sinergia con otros Incrementos

- **Incremento 17 (Reporte Comunitario de Errores):** Se complementan formando el circuito cerrado de calidad editorial: *Comunidad reporta -> Moderador revisa -> Moderador edita y sustituye imagen -> Reporte resuelto*.
