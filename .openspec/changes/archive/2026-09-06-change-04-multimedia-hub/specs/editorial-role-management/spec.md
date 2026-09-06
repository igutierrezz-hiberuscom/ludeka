# Especificación Delta: editorial-role-management (Acceso al Panel de Moderación)

## Propósito
Extender los accesos de navegación según roles para proveer enlace directo al panel móvil de moderación `/moderacion/multimedia` a los usuarios con permisos editoriales.

## Requerimientos Modificados

### Requerimiento: Acceso Rápido de Moderación en Navegación
La cabecera de la aplicación (`MainLayout.razor`) DEBE mostrar un botón o enlace `[ 🎬 Moderar Medios ]` cuando el usuario activo posea el rol `FoundingTeam` o `Moderator`.

#### Escenario: Moderador visualiza acceso a moderación
- DADO un usuario activo con rol `Moderator` o `FoundingTeam`
- CUANDO visualiza cualquier pantalla en Ludeka
- ENTONCES en la barra superior DEBE estar visible el acceso directo `[ 🎬 Moderar Medios ]` que enlaza a `/moderacion/multimedia`.

#### Escenario: Usuario regular no visualiza acceso
- DADO un usuario con rol regular `User`
- CUANDO navega por la plataforma
- ENTONCES el enlace de moderación NO DEBE ser visible ni accesible en la barra superior.
