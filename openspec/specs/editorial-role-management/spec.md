# Especificación: editorial-role-management

## Propósito
Definir la gestión de roles de usuario en la capa de aplicación, habilitando la distinción entre usuarios estándar y miembros del equipo fundador (`FoundingTeam`) o moderadores (`Moderator`), e incorporando un conmutador de rol demostrativo en la interfaz para facilitar la auditoría y verificación.

## Requerimientos

### Requerimiento: Abstracción de Roles en `ICurrentUserService`
El contrato `ICurrentUserService` DEBE exponer información de roles del usuario autenticado:
- Lista de roles asignados (`Roles`).
- Helper booleano `IsFoundingTeam`.
- Método de consulta `IsInRole(string role)`.

#### Escenario: Usuario con rol FoundingTeam
- DADO un usuario configurado con el rol `"FoundingTeam"`
- CUANDO se consulta `IsInRole("FoundingTeam")` o `IsFoundingTeam`
- ENTONCES el resultado DEBE ser `true`.

#### Escenario: Usuario estándar
- DADO un usuario configurado como usuario regular (`"User"`)
- CUANDO se consulta `IsInRole("FoundingTeam")` o `IsInRole("Moderator")`
- ENTONCES el resultado DEBE ser `false`.

---

### Requerimiento: Conmutador de Roles en Tiempo de Ejecución
Para posibilitar la verificación manual y demostración fluida en el MVP sin requerir un servidor OAuth externo, el servicio DEBE permitir alternar el rol activo del usuario en caliente (`SwitchRole`).

#### Escenario: Cambio dinámico de rol en la barra de navegación
- DADO un usuario navegando por la aplicación en modo `"User"`
- CUANDO pulsa el conmutador de la cabecera seleccionando `"🛡️ Modo Mesa Fundadora"`
- ENTONCES el rol activo del servicio DEBE pasar a `"FoundingTeam"`
- Y los componentes reactivos DEBEN actualizar su visualización inmediatamente.
