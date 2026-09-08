# creators-directory Specification

> Especificación NUEVA (no existe spec previo de este dominio). Idioma: español castellano.
> Cobertura: directorio público de creadores de contenido, ficha con redes sociales, alta/edición con permiso, sembrado, alias `/autores`, diseñador de juego como texto plano en fichas de juego y reetiquetado Autores → Creadores.

## Propósito

Definir el comportamiento del directorio de **creadores de contenido** de Ludeka (`/creadores`): un listado público exclusivo de creadores de contenido con sus redes sociales (`SocialLinks`), gestión reservada al permiso `CanManageCreators`, sembrado sin diseñadores de juegos, alias retrocompatible `/autores` y fichas de juego que muestran el diseñador como texto plano sin enlazar al directorio.

## Requirements

### Requirement: Listado público solo de creadores de contenido

La página `/creadores` **DEBE** listar únicamente creadores de contenido y **NO DEBE** listar diseñadores de juegos. El listado **DEBE** mostrar el buscador y el grid de tarjetas existentes.

#### Scenario: Directorio sin diseñadores

- GIVEN el directorio sembrado y un visitante anónimo en `/creadores`
- WHEN la página se renderiza
- THEN aparecen únicamente los creadores de contenido sembrados (incluido "Análisis Parálisis")
- AND ningún diseñador de juego (p. ej. "Uwe Rosenberg", "Elizabeth Hargrave") aparece en el listado

### Requirement: Ficha con redes sociales

La ficha de creador (`/creadores/{slug}`) **DEBE** renderizar las redes sociales del creador (`SocialLinks`) mediante el componente de listado de redes reutilizable, con nombre e icono por plataforma. Un creador sin redes **DEBE** renderizar su ficha sin errores y sin sección de redes vacía defectuosa.

#### Scenario: Ficha muestra el link de YouTube

- GIVEN el creador "Análisis Parálisis" con un `SocialLink` de plataforma YouTube
- WHEN un visitante abre `/creadores/analisis-paralisis`
- THEN la ficha muestra el enlace de YouTube con la identidad amigable de la plataforma

#### Scenario: Ficha sin redes

- GIVEN un creador sin `SocialLinks`
- WHEN un visitante abre su ficha
- THEN la ficha renderiza sin errores y sin enlaces de redes

### Requirement: Alta y edición con permiso CanManageCreators

La creación y edición de creadores **DEBE** exigir el permiso granular `CanManageCreators`. Un usuario con el permiso **DEBE** poder dar de alta y editar un creador con sus redes sociales desde el modal de edición; sin el permiso, el sistema **NO DEBE** permitir la operación.

#### Scenario: Moderador crea creador con redes

- GIVEN un usuario con permiso `CanManageCreators`
- WHEN da de alta un creador con nombre y un link de Instagram vía el modal
- THEN el creador queda persistido con su red social y aparece en el directorio

#### Scenario: Usuario sin permiso no gestiona creadores

- GIVEN un usuario sin permiso `CanManageCreators`
- WHEN intenta crear o editar un creador
- THEN la operación se deniega sin modificar datos

### Requirement: Sembrado purgado y re-sembrado con creadores de contenido (D1)

El sembrado del directorio **NO DEBE** crear diseñadores de juegos. El sembrado **DEBE** crear únicamente creadores de contenido, tomando como padrón los creadores del registro estático de foco de canales (incluido Análisis Parálisis con su red de YouTube). La re-ejecución del sembrado **DEBE** ser idempotente (sin duplicados por slug único).

#### Scenario: Seed sin diseñadores y con creadores del padrón

- GIVEN la base de datos vacía de creadores
- WHEN se ejecuta el sembrado del directorio
- THEN existen creadores de contenido del padrón y cero diseñadores de juegos (ninguno de los 6 eliminados existe)

#### Scenario: Re-ejecución idempotente

- GIVEN el sembrado ya ejecutado una vez
- WHEN se vuelve a ejecutar el sembrado
- THEN no se duplican creadores (slug único respetado) y no aparecen diseñadores

### Requirement: Alias /autores operativo (D2)

Las rutas `/autores` y `/autores/{slug}` **DEBEN** seguir resolviendo el directorio y la ficha de creador con el mismo contenido que `/creadores` y `/creadores/{slug}`, sin redirecciones rotas.

#### Scenario: Bookmark antiguo al directorio

- GIVEN un visitante con bookmark a `/autores`
- WHEN navega a `/autores`
- THEN recibe el mismo directorio de creadores que en `/creadores`

#### Scenario: Bookmark antiguo a una ficha

- GIVEN un visitante con bookmark a `/autores/{slug}` de un creador existente
- WHEN navega a esa URL
- THEN recibe la misma ficha que en `/creadores/{slug}`

### Requirement: Diseñador de juego como texto plano en ficha de juego

La ficha de juego **DEBE** mostrar el diseñador como texto plano ("Diseñado por {Designer}") y **NO DEBE** enlazarlo a `/creadores/{slug}`. Por tanto, ninguna ficha de juego genera enlaces rotos al directorio.

#### Scenario: Diseñador sin link al directorio

- GIVEN una ficha de juego con `Designer` "Uwe Rosenberg"
- WHEN la ficha se renderiza
- THEN el diseñador aparece como texto plano "Diseñado por Uwe Rosenberg"
- AND no existe ningún `href` hacia `/creadores/uwe-rosenberg` ni hacia el directorio derivado del diseñador

### Requirement: Sin sección "Obras" ni cruce por Game.Designer

La ficha de creador **NO DEBE** renderizar la sección "Obras de {creador}". El sistema **NO DEBE** calcular conteos ni listados de juegos del creador por coincidencia con `Game.Designer`; el cruce juego↔creador queda reservado a creadores de contenido reales.

#### Scenario: Ficha sin obras

- GIVEN un creador de contenido existente
- WHEN su ficha se renderiza
- THEN no existe sección "Obras de" ni un conteo de juegos derivado de `Game.Designer`

#### Scenario: Servicio sin matching de diseñador

- GIVEN un creador cuyo nombre coincide con el `Designer` de algún juego del catálogo
- WHEN se consulta el detalle del creador
- THEN la respuesta no incluye listado ni conteo de juegos obtenidos por coincidencia de `Game.Designer`

### Requirement: Reetiquetado transversal Autores → Creadores

La interfaz **DEBE** usar "Creadores" (y "Creador de contenido" para el badge/formulario) en la navegación (cabecera y pie), títulos, `<PageTitle>`/h1 de directorio y ficha, textos del modal de edición y etiqueta de auditoría. No **DEBE** quedar la etiqueta visible "Autores" ni "Autor / Diseñador" (la URL `/autores` no es una etiqueta visible).

#### Scenario: Navegación y fichas reetiquetadas

- GIVEN la aplicación renderizada
- WHEN se inspecciona cabecera, pie, directorio, ficha, modal y auditoría
- THEN las etiquetas usan "Creadores"/"Creador de contenido" y no aparece "Autores" ni "Autor / Diseñador" como texto visible
