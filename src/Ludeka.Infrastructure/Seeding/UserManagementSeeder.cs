using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Seeding;

/// <summary>
/// Sembrador inicial de usuarios del sistema y bitácora demostrativa de auditoría.
/// </summary>
public static class UserManagementSeeder
{
    public static async Task SeedUsersAndAuditAsync(LudekaDbContext db, CancellationToken ct = default)
    {
        await SeedUsersAsync(db, ct);
        await SeedAuditLogsAsync(db, ct);
    }

    private static async Task SeedUsersAsync(LudekaDbContext db, CancellationToken ct)
    {
        if (await db.AppUsers.AnyAsync(ct))
            return;

        var users = new List<AppUser>
        {
            new(
                id: "carlos_fundador",
                userName: "Carlos Fundador",
                email: "carlos@ludeka.es",
                role: UserRole.FoundingTeam,
                createdAt: DateTimeOffset.UtcNow.AddMonths(-6)
            ),
            new(
                id: "usuario-fundador-ludeka",
                userName: "Mesa Fundadora (Demo)",
                email: "mesa@ludeka.es",
                role: UserRole.FoundingTeam,
                createdAt: DateTimeOffset.UtcNow.AddMonths(-5)
            ),
            new(
                id: "laura_mod",
                userName: "Laura Moderadora",
                email: "laura@ludeka.es",
                role: UserRole.Moderator,
                permissions: ModeratorPermission.CanEditGames | ModeratorPermission.CanUploadImages,
                createdAt: DateTimeOffset.UtcNow.AddMonths(-3)
            ),
            new(
                id: "pablo_editoriales",
                userName: "Pablo Gestor de Industria",
                email: "pablo@ludeka.es",
                role: UserRole.Moderator,
                permissions: ModeratorPermission.CanManagePublishers | ModeratorPermission.CanManageCreators | ModeratorPermission.CanManageStoreLinks,
                createdAt: DateTimeOffset.UtcNow.AddMonths(-2)
            ),
            new(
                id: "marta_media",
                userName: "Marta Audiovisual",
                email: "marta@ludeka.es",
                role: UserRole.Moderator,
                permissions: ModeratorPermission.CanApproveMedia | ModeratorPermission.CanResolveReports,
                createdAt: DateTimeOffset.UtcNow.AddMonths(-1)
            ),
            new(
                id: "david_comunidad",
                userName: "David Jugador",
                email: "david@comunidad.es",
                role: UserRole.CommunityUser,
                createdAt: DateTimeOffset.UtcNow.AddDays(-20)
            ),
            new(
                id: "elena_ludica",
                userName: "Elena Tablero",
                email: "elena@comunidad.es",
                role: UserRole.CommunityUser,
                createdAt: DateTimeOffset.UtcNow.AddDays(-10)
            ),
            new(
                id: "susana_suspendida",
                userName: "Susana Cuenta Inactiva",
                email: "susana@suspendida.es",
                role: UserRole.Moderator,
                permissions: ModeratorPermission.CanEditGames,
                status: UserStatus.Suspended,
                createdAt: DateTimeOffset.UtcNow.AddMonths(-4)
            )
        };

        await db.AppUsers.AddRangeAsync(users, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedAuditLogsAsync(LudekaDbContext db, CancellationToken ct)
    {
        if (await db.AuditLogs.AnyAsync(ct))
            return;

        var logs = new List<AuditLogEntry>
        {
            new(
                userId: "carlos_fundador",
                userName: "Carlos Fundador",
                action: AuditAction.RoleChanged,
                entityType: AuditEntityType.User,
                entityId: "laura_mod",
                entityName: "Laura Moderadora",
                summary: "Ascendida a Moderadora con permisos de edición de juegos y carátulas",
                changes: [
                    new AuditFieldChange("Role", "CommunityUser", "Moderator"),
                    new AuditFieldChange("Permissions", "None", "CanEditGames, CanUploadImages")
                ],
                timestamp: DateTimeOffset.UtcNow.AddDays(-25)
            ),
            new(
                userId: "laura_mod",
                userName: "Laura Moderadora",
                action: AuditAction.Updated,
                entityType: AuditEntityType.Game,
                entityId: "catan",
                entityName: "Catan",
                summary: "Corrección de duración recomendada y errata en sinopsis",
                changes: [
                    new AuditFieldChange("Duration", "60-90 min", "45-75 min"),
                    new AuditFieldChange("BoxAge", "12+", "10+")
                ],
                timestamp: DateTimeOffset.UtcNow.AddDays(-18)
            ),
            new(
                userId: "pablo_editoriales",
                userName: "Pablo Gestor de Industria",
                action: AuditAction.Created,
                entityType: AuditEntityType.Publisher,
                entityId: "tranjis-games",
                entityName: "Tranjis Games",
                summary: "Alta de editorial nacional Tranjis Games y canales de YouTube",
                changes: [
                    new AuditFieldChange("Name", null, "Tranjis Games"),
                    new AuditFieldChange("Country", null, "España")
                ],
                timestamp: DateTimeOffset.UtcNow.AddDays(-12)
            ),
            new(
                userId: "marta_media",
                userName: "Marta Audiovisual",
                action: AuditAction.StatusChanged,
                entityType: AuditEntityType.Report,
                entityId: "rep-001",
                entityName: "Reporte: Wingspan",
                summary: "Incidencia comunitaria de carátula desactualizada resuelta con éxito",
                changes: [
                    new AuditFieldChange("Status", "Pending", "Resolved"),
                    new AuditFieldChange("ModeratorNotes", null, "Carátula de edición española en alta resolución aplicada.")
                ],
                timestamp: DateTimeOffset.UtcNow.AddDays(-5)
            )
        };

        await db.AuditLogs.AddRangeAsync(logs, ct);
        await db.SaveChangesAsync(ct);
    }
}
