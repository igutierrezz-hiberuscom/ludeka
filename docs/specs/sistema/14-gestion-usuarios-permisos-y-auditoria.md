# 14. Gestión de Usuarios, Permisos Granulares de Moderación (RBAC) y Bitácora Universal de Auditoría Editorial

## 1. Visión General y Propósito
El subsistema de gestión de usuarios, roles RBAC y auditoría editorial proporciona la gobernanza integral de Ludeka, permitiendo a la **Mesa Fundadora (`FoundingTeam`)** administrar los privilegios de acceso y moderación de forma granular y mantener una trazabilidad inmutable de todas las mutaciones realizadas sobre el catálogo lúdico.

Sus tres pilares fundamentales son:
1. **Panel Centralizado de Usuarios (`/admin/usuarios`):** Exclusivo para la Mesa Fundadora, permite visualizar todos los usuarios de la comunidad, filtrar por rol y estado, ascender a usuarios comunitarios a moderadores o fundadores, suspender cuentas infractoras y asignar permisos específicos.
2. **Sistema Granular de Permisos de Moderación (`ModeratorPermission` con `[Flags]`):** Permite descomponer la capacidad de moderación en permisos atómicos independientes, de modo que cada moderador solo tenga acceso a las áreas del catálogo para las que ha sido autorizado.
3. **Bitácora Universal de Auditoría Editorial (`/admin/auditoria`):** Motor de registro (`IAuditService`) que captura de forma automática e indeleble cualquier acción de alta, modificación o borrado en juegos, imágenes, editoriales, creadores, tiendas, reportes o roles, almacenando un diff estructurado (*valor anterior* vs *valor nuevo*) para consulta visual e histórica.

---

## 2. Modelo de Dominio e Invariantes (`Ludeka.Core`)

### 2.1 Enumerados y Flags de Autorización
- **`UserRole` (`Enums/UserRole.cs`):**
  - `FoundingTeam`: Miembro de la Mesa Fundadora. Posee inherentemente todos los permisos de administración y moderación sin requerir flags explícitos.
  - `Moderator`: Moderador de la comunidad. Sus capacidades están delimitadas por la máscara de bits `Permissions`.
  - `CommunityUser`: Usuario estándar registrado. No posee privilegios de moderación.
- **`UserStatus` (`Enums/UserStatus.cs`):**
  - `Active`: Usuario activo en la plataforma.
  - `Suspended`: Cuenta suspendida. **Invariante de seguridad:** Si un usuario está suspendido, cualquier comprobación de permisos devuelve `false` de inmediato.
- **`ModeratorPermission` (`[Flags] Enums/ModeratorPermission.cs`):**
  - `None = 0`: Sin permisos especiales.
  - `CanEditGames = 1 << 0` (1): Editar fichas técnicas, metadatos y parámetros de mesa.
  - `CanUploadImages = 1 << 1` (2): Cargar o actualizar carátulas y fotos reales de juego.
  - `CanManagePublishers = 1 << 2` (4): Crear y editar perfiles de editoriales y sus redes.
  - `CanManageCreators = 1 << 3` (8): Crear y editar perfiles de autores/diseñadores.
  - `CanApproveMedia = 1 << 4` (16): Moderar y aprobar/rechazar contenidos multimedia en cola.
  - `CanResolveReports = 1 << 5` (32): Atender, reclasificar y resolver incidencias comunitarias (INC-17).
  - `CanManageStoreLinks = 1 << 6` (64): Crear y gestionar enlaces de compra y ofertas de afiliados (INC-11).
  - `All = 127`: Máscara completa con todos los permisos habilitados.

### 2.2 Eventos y Entidades de Auditoría
- **`AuditAction` (`Enums/AuditAction.cs`):** `Created`, `Updated`, `Deleted`, `StatusChanged`, `RoleChanged`, `PermissionsChanged`.
- **`AuditEntityType` (`Enums/AuditEntityType.cs`):** `Game`, `Publisher`, `Creator`, `Store`, `Media`, `Report`, `User`.
- **`AuditFieldChange` (`ValueObjects/AuditFieldChange.cs`):** Value Object inmutable que registra `FieldName` (campo modificado), `OldValue` (valor original como texto o JSON) y `NewValue` (nuevo valor).

### 2.3 Entidades Principales
- **`AppUser` (`Entities/AppUser.cs`):**
  - `Id`: Identificador canónico del usuario (slug o username único).
  - `UserName` y `Email`: Datos de identidad del usuario.
  - `Role`: `UserRole` asignado.
  - `Status`: `UserStatus` actual.
  - `Permissions`: Máscara de bits `ModeratorPermission`.
  - `CreatedAt` y `UpdatedAt`: Marcas temporales UTC.
  - Método `HasPermission(ModeratorPermission permission)`: Implementa la lógica de evaluación defensiva:
    1. Si `Status == UserStatus.Suspended` $\rightarrow$ `false`.
    2. Si `Role == UserRole.FoundingTeam` $\rightarrow$ `true`.
    3. Si `Role == UserRole.Moderator` $\rightarrow$ `(Permissions & permission) == permission`.
    4. En cualquier otro caso $\rightarrow$ `false`.
- **`AuditLogEntry` (`Entities/AuditLogEntry.cs`):**
  - `Id`: Identificador único (Guid).
  - `UserId` y `UserName`: Identidad del actor responsable de la mutación.
  - `Timestamp`: Marca temporal UTC de la operación.
  - `Action`: `AuditAction` ejecutada.
  - `EntityType`: Tipo de entidad impactada (`AuditEntityType`).
  - `EntityId`: Identificador de la entidad afectada.
  - `EntityName`: Nombre legible del elemento (ej. título del juego o nombre de la editorial).
  - `Summary`: Resumen conciso de la operación.
  - `Changes`: Colección de `AuditFieldChange` que almacena el diff estructurado.

---

## 3. Capa de Aplicación y Seguridad Defensiva (`Ludeka.Application`)

### 3.1 Contratos de Repositorio y Servicios
- **[`IUserRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IUserRepository.cs):** Acceso a datos para usuarios con filtrado por rol, estado y búsqueda textual.
- **[`IAuditLogRepository`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IAuditLogRepository.cs):** Persistencia y consulta paginada de registros de auditoría ordenados descendentemente.
- **[`IUserManagementService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IUserManagementService.cs):**
  - Gestión de usuarios, cambio de roles, suspensión/reactivación y actualización de permisos granulares.
  - **Barrera de seguridad:** Solo usuarios con `IsFoundingTeam == true` pueden invocar estos métodos; cualquier llamada no autorizada arroja `UnauthorizedAccessException`.
  - Registra automáticamente auditorías de tipo `RoleChanged`, `StatusChanged` y `PermissionsChanged`.
- **[`IAuditService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IAuditService.cs):**
  - `RecordChangeAsync(...)`: Registra operaciones de auditoría con diff de campos.
  - `GetAuditLogsAsync(...)`: Consultas filtradas por entidad, ID, acción, usuario y rango temporal.
- **[`ICurrentUserService`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/ICurrentUserService.cs):**
  - Ampliado con `HasPermission(ModeratorPermission permission)` y `SwitchUser(string userId)` mediante implementaciones por defecto (*default interface methods*) para mantener compatibilidad total con tests preexistentes.

### 3.2 Integración Defensiva en Casos de Uso (Defense-in-Depth)
Cada servicio de mutación editorial valida explícitamente el permiso granular correspondiente y genera su respectivo registro de auditoría:
- **`GameEditorService`:** Requiere `CanEditGames` (o `FoundingTeam`). Registra diffs de duración, comensales, edad, etc.
- **`PublisherService`:** Requiere `CanManagePublishers` para alta y edición de editoriales.
- **`CreatorService`:** Requiere `CanManageCreators` para alta y edición de autores.
- **`StoreService`:** Requiere `CanManageStoreLinks` para alta y edición de tiendas u ofertas.
- **`GameIssueReportService`:** Requiere `CanResolveReports` para resolver o descartar incidencias.
- **`MediaService`:** Requiere `CanApproveMedia` para moderar tutoriales y partidas en cola.

---

## 4. Persistencia e Infraestructura (`Ludeka.Infrastructure`)

- **Persistencia en SQLite (`LudekaDbContext.cs`):**
  - `DbSet<AppUser> AppUsers`: Mapeo con índice único por email y búsquedas por username.
  - `DbSet<AuditLogEntry> AuditLogs`: Configuración nativa `OwnsMany(..., b => b.ToJson())` para serializar la colección `Changes` (`AuditFieldChange`) como JSON nativo en una única columna de SQLite.
- **Migración Defensiva (`SqliteSchemaMigrator.cs`):**
  - Creación idempotente de tablas `AppUsers` e `AuditLogs` con índices por `UserId`, `EntityType`, `Action` y `Timestamp`.
- **Repositorios Implementados:**
  - `SqliteUserRepository.cs`: Consultas asíncronas con LINQ y paginación.
  - `SqliteAuditLogRepository.cs`: Recuperación y filtrado compuesto de la bitácora editorial.
- **Simulador de Sesión (`DefaultCurrentUserService.cs`):**
  - Soporta conmutación reactiva de usuario (`SwitchUser`) entre perfiles simulados (Fundador, Moderador de Juegos, Moderador de Comunidad y Usuario estándar) para pruebas y demostración.
- **Semillero Canónico (`UserManagementSeeder.cs`):**
  - Precarga de usuarios con roles y permisos diversificados y registros iniciales en la bitácora de auditoría.

---

## 5. Vistas Web Blazor y Control Reactivo de UI (`Ludeka.Web`)

- **Panel de Gestión de Usuarios (`/admin/usuarios` - `UserManagement.razor`):**
  - Tabla de usuarios con avatares, badges de rol (👑 Fundador, 🛡️ Moderador, 👤 Comunidad) y estado (🟢 Activo, 🔴 Suspendido).
  - Buscador reactivo por texto y filtros por rol y estado.
  - Modales para alternar rol, suspender con motivo y configurar permisos granulares.
- **Modal de Permisos Granulares (`UserPermissionsModal.razor`):**
  - Matriz accesible con interruptores/checkboxes para cada una de las banderas de `ModeratorPermission`.
- **Visor de Auditoría Editorial (`/admin/auditoria` - `AuditLogViewer.razor`):**
  - Historial cronológico con timeline visual de eventos editoriales.
  - Filtros por tipo de entidad, acción y usuario actor.
  - Vista expandible de detalle que renderiza el diff visual (*Valor previo* tachado en rojo $\rightarrow$ *Nuevo valor* en verde).
- **Protección Reactiva en Cascada:**
  - `MainLayout.razor`: Enlaces de administración (`Usuarios` y `Auditoría`) visibles exclusivamente para fundadores. Enlaces a `Reportes` y `Moderar Media` visibles solo si el moderador ostenta el permiso respectivo.
  - Ocultación de botones en `GameDetail.razor`, `PublishersDirectory.razor`, `CreatorsDirectory.razor`, `StoresDirectory.razor` y `CatalogQueuePanel.razor` según la presencia del flag correspondiente en el usuario en sesión.
