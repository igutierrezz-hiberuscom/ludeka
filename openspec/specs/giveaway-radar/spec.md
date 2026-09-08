# Especificación: giveaway-radar (Radar de Sorteos Activos y Fusión de Colaboraciones)

## 1. Propósito y Contexto
Esta especificación define el comportamiento del **Radar de Sorteos Activos**, centralizando los sorteos de juegos de mesa organizados por editoriales, tiendas y creadores de contenido, así como los sorteos exclusivos de la comunidad Ludeka. Resuelve el problema de la dispersión de oportunidades en redes sociales e implementa la regla de **fusión de colaboraciones** para evitar duplicados en el feed.

---

## 2. Requerimientos de Comportamiento (RFC 2119)

### REQ-GIV-01: Registro de Sorteos y Datos Mínimos
- El sistema **DEBE** permitir registrar un sorteo con los siguientes campos obligatorios:
  - `Title`: Título descriptivo del sorteo o lote lúdico.
  - `Organizer`: Nombre de la entidad organizadora principal (ej. "Devir Iberia", "Maldito Games", "Zacatrus").
  - `Url`: Enlace web directo a la publicación oficial (Instagram, X, YouTube, Discord, web).
  - `Platform`: Plataforma de origen (`Instagram`, `TwitterX`, `YouTube`, `Community`).
  - `DeadlineAt`: Fecha y hora límite de participación (`DateTimeOffset`).
- El sistema **PUEDE** asociar opcionalmente:
  - `GameId`: Identificador del juego en el catálogo local (`Guid?`).
  - `GameTitle`: Nombre del juego cuando no esté aún catalogado en Ludeka (`string?`).
  - `ThumbnailUrl`: URL remota de la imagen del sorteo (`string?`).
  - `Collaborator`: Nombre del creador o co-organizador en sorteos conjuntos (`string?`).
  - `IsCommunityExclusive`: Bandera booleana para sorteos financiados por Ludeka (`bool`).

### REQ-GIV-02: Fusión Inteligente de Colaboraciones (Evitar Duplicados)
- Cuando se intente dar de alta un sorteo que comparte el mismo juego o título similar y misma fecha límite:
  - Si una entrada ya existe para la editorial y se añade la del creador colaborador (o viceversa), el sistema **DEBE** permitir la fusión (`MergeCollaborator`) asignando el colaborador a la entrada existente en lugar de duplicarla.
  - La tarjeta visual en el radar **DEBE** renderizar la colaboración unificada: *"Organizado por {Organizer} en colaboración con {Collaborator}"*.

### REQ-GIV-03: Expiración Automática y Cálculo de Plazo Restante
- La entidad **DEBE** computar la propiedad `IsExpired => DeadlineAt < DateTimeOffset.UtcNow`.
- El servicio de consulta **DEBE** admitir un filtro para excluir sorteos expirados por defecto (`includeExpired = false`).
- Para los sorteos activos, el sistema **DEBE** clasificar el tiempo restante:
  - *"Finaliza hoy"* si queda menos de 24 horas.
  - *"Quedan X días"* si queda entre 1 y 30 días.
  - *"Finalizado"* para sorteos con fecha límite superada.

### REQ-GIV-04: Sorteos Exclusivos de la Comunidad
- Los sorteos marcados con `IsCommunityExclusive = true` **DEBEN** mostrar el distintivo visual *"🎁 Exclusivo Comunidad Ludeka"*.
- El sistema **DEBE** validar que para participar en sorteos comunitarios el usuario cuente con actividad mensual demostrable (al menos 1 micro-reseña o 1 voto en el consultorio de reglas).

---

## 3. Criterios de Aceptación (Escenarios Gherkin)

### Escenario 1: Fusión exitosa de colaboración editorial + creador
```gherkin
Given un sorteo registrado con título "Sorteo Brass Birmingham" y organizador "Maldito Games"
When un moderador registra una colaboración con "Análisis Parálisis" para dicho sorteo
Then la entrada resultante tiene organizador "Maldito Games" y colaborador "Análisis Parálisis"
And el radar muestra una única tarjeta con el texto "Maldito Games en colaboración con Análisis Parálisis"
```

### Escenario 2: Expiración en tiempo real
```gherkin
Given un sorteo cuya fecha límite es hace 1 hora respecto a DateTimeOffset.UtcNow
When se consulta la lista de sorteos activos
Then el sorteo tiene IsExpired igual a true
And no se muestra en el listado por defecto a menos que se active el filtro histórico
```

---

## 4. Requerimientos de la Página /sorteos: Renderizado y Enrutado (añadidos por INC-31)

### Requirement: Página de sorteos sin banner legacy

La página de sorteos **NO DEBE** renderizar el banner "¡Radar renovado!" bajo ninguna ruta de entrada, incluida la llegada desde un enlace legacy. La eliminación cubre tanto el bloque visual del banner como cualquier condición de ruta legacy que lo activara.

#### Scenario: Entrada por la ruta canónica

- GIVEN un visitante que navega a `/sorteos` (desde menú, portada o URL directa)
- WHEN la página se renderiza
- THEN no aparece el banner "¡Radar renovado!" ni sus enlaces asociados

#### Scenario: Entrada por la ruta legacy

- GIVEN un visitante que llega a la página de sorteos mediante la ruta legacy `/radar`
- WHEN la página se renderiza
- THEN no aparece el banner "¡Radar renovado!" bajo ninguna circunstancia

### Requirement: /radar como alias silencioso de /sorteos

La ruta `/radar` **DEBE** continuar resolviendo la página de sorteos con el mismo contenido que `/sorteos`, sin página de error y sin mensajes de migración o aviso visible al usuario (alias silencioso).

#### Scenario: Bookmark antiguo resuelto sin aviso

- GIVEN un visitante con un bookmark antiguo a `/radar`
- WHEN navega a `/radar`
- THEN recibe la página de sorteos completa, idéntica en contenido a `/sorteos`
- AND no se muestra ningún aviso de ruta antigua ni mensaje de migración
